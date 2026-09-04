using Microsoft.Extensions.Logging;
using Soenneker.Extensions.String;
using Soenneker.Git.Util.Abstract;
using Soenneker.Loops.Runners.OpenApiClient.Utils.Abstract;
using Soenneker.Utils.Dotnet.Abstract;
using Soenneker.Utils.Environment;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Extensions.ValueTask;
using Soenneker.Kiota.Util.Abstract;
using Soenneker.OpenApi.Fixer.Abstract;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.File.Download.Abstract;
using System.Collections.Generic;

namespace Soenneker.Loops.Runners.OpenApiClient.Utils;

/// <inheritdoc cref="IFileOperationsUtil" />
public class FileOperationsUtil : IFileOperationsUtil
{
    private readonly ILogger<FileOperationsUtil> _logger;
    private readonly IGitUtil _gitUtil;
    private readonly IDotnetUtil _dotnetUtil;
    private readonly IKiotaUtil _kiotaUtil;
    private readonly IOpenApiFixer _openApiFixer;
    private readonly IFileDownloadUtil _fileDownloadUtil;
    private readonly IFileUtil _fileUtil;
    private readonly IDirectoryUtil _directoryUtil;

    public FileOperationsUtil(ILogger<FileOperationsUtil> logger, IGitUtil gitUtil, IDotnetUtil dotnetUtil,
        IFileDownloadUtil fileDownloadUtil, IFileUtil fileUtil, IDirectoryUtil directoryUtil, IKiotaUtil kiotaUtil, IOpenApiFixer openApiFixer)
    {
        _logger = logger;
        _gitUtil = gitUtil;
        _dotnetUtil = dotnetUtil;
        _kiotaUtil = kiotaUtil;
        _openApiFixer = openApiFixer;
        _fileDownloadUtil = fileDownloadUtil;
        _fileUtil = fileUtil;
        _directoryUtil = directoryUtil;
    }

    public async ValueTask Process(CancellationToken cancellationToken = default)
    {
        string gitDirectory = await _gitUtil.CloneToTempDirectory($"https://github.com/soenneker/{Constants.Library.ToLowerInvariantFast()}", cancellationToken: cancellationToken);

        string targetFilePath = Path.Combine(gitDirectory, "openapi.json");

        string fixedFilePath = Path.Combine(gitDirectory, "fixed.json");

        await _fileUtil.DeleteIfExists(targetFilePath, cancellationToken: cancellationToken);

        string? filePath = await _fileDownloadUtil.Download("https://app.loops.so/openapi.json",
            targetFilePath, fileExtension: ".json", cancellationToken: cancellationToken);

        if (filePath is null)
            throw new InvalidOperationException("Loops OpenAPI document download failed.");

        await _fileUtil.DeleteIfExists(fixedFilePath, cancellationToken: cancellationToken);
        await _openApiFixer.Fix(filePath, fixedFilePath, cancellationToken);


        await _kiotaUtil.EnsureInstalled(cancellationToken);

        string srcDirectory = Path.Combine(gitDirectory, "src", Constants.Library);

        await DeleteAllExceptCsproj(srcDirectory, cancellationToken).NoSync();

        await _kiotaUtil.Generate(fixedFilePath, "LoopsOpenApiClient", Constants.Library, gitDirectory, cancellationToken).NoSync();

        await BuildAndPush(gitDirectory, cancellationToken).NoSync();
    }

    /// <summary>
    /// Deletes generated files beneath the directory while preserving C# project files.
    /// </summary>
    /// <param name="directoryPath">Root directory whose generated contents should be removed.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes after the targeted files have been deleted.</returns>
    public async ValueTask DeleteAllExceptCsproj(string directoryPath, CancellationToken cancellationToken = default)
    {
        if (!(await _directoryUtil.Exists(directoryPath, cancellationToken)))
            throw new InvalidOperationException($"Generated source directory does not exist: {directoryPath}");

        List<string> files = await _directoryUtil.GetFilesByExtension(directoryPath, "", true, cancellationToken);
        foreach (string file in files)
        {
            if (file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                continue;

            await _fileUtil.Delete(file, ignoreMissing: true, log: false, cancellationToken).NoSync();
            _logger.LogInformation("Deleted file: {FilePath}", file);
        }

        List<string> dirs = await _directoryUtil.GetAllDirectoriesRecursively(directoryPath, cancellationToken);
        foreach (string dir in dirs.OrderByDescending(static directory => directory.Length))
        {
            List<string> dirFiles = await _directoryUtil.GetFilesByExtension(dir, "", false, cancellationToken);
            List<string> subDirs = await _directoryUtil.GetAllDirectories(dir, cancellationToken);
            if (dirFiles.Count == 0 && subDirs.Count == 0)
            {
                await _directoryUtil.Delete(dir, cancellationToken);
                _logger.LogInformation("Deleted empty directory: {DirectoryPath}", dir);
            }
        }
    }

    private async ValueTask BuildAndPush(string gitDirectory, CancellationToken cancellationToken)
    {
        string projFilePath = Path.Combine(gitDirectory, "src", Constants.Library, $"{Constants.Library}.csproj");

        await _dotnetUtil.Restore(projFilePath, cancellationToken: cancellationToken);

        bool successful = await _dotnetUtil.Build(projFilePath, true, "Release", false, cancellationToken: cancellationToken);

        if (!successful)
            throw new InvalidOperationException("Generated Loops client failed its Release build.");

        string gitHubToken = EnvironmentUtil.GetVariableStrict("GH__TOKEN");
        string name = EnvironmentUtil.GetVariableStrict("GIT__NAME");
        string email = EnvironmentUtil.GetVariableStrict("GIT__EMAIL");

        await _gitUtil.CommitAndPush(gitDirectory, "Automated update", gitHubToken, name, email, cancellationToken);
    }
}
