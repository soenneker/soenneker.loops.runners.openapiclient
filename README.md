[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.loops.runners.openapiclient/build-and-test.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.loops.runners.openapiclient/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.loops.runners.openapiclient/daily-automatic-update.yml?style=for-the-badge&label=Daily%20Update)](https://github.com/soenneker/soenneker.loops.runners.openapiclient/actions/workflows/daily-automatic-update.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.loops.runners.openapiclient/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.loops.runners.openapiclient/actions/workflows/codeql.yml)

# Soenneker.Loops.Runners.OpenApiClient

Defines the file operations util contract.

> This is an automation runner, not a package intended for application consumption.

## What the runner does

- `IFileOperationsUtil.Process(cancellationToken)` — Runs the OpenAPI client regeneration workflow, including cleanup and post-processing.
- `Constants.Library` — The library.
- `ConsoleHostedService.StartAsync(cancellationToken)` — Runs the OpenAPI client update workflow and requests application shutdown when it finishes.
- `ConsoleHostedService.StopAsync(cancellationToken)` — Completes application-host shutdown after the client update workflow.

## What you get

- `IFileOperationsUtil` — Defines the file operations util contract.
- `Constants` — Represents the constants.
- `ConsoleHostedService` — Represents the console hosted service.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IFileOperationsUtil.Process(cancellationToken)` | Runs the OpenAPI client regeneration workflow, including cleanup and post-processing. | A task that completes when the full processing workflow has finished. |
| `ConsoleHostedService.StartAsync(cancellationToken)` | Runs the OpenAPI client update workflow and requests application shutdown when it finishes. | A task that completes when client regeneration has finished and shutdown has been requested. |
| `ConsoleHostedService.StopAsync(cancellationToken)` | Completes application-host shutdown after the client update workflow. | A task that completes when host shutdown has finished. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
