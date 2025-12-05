using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Frends.Python.ExecuteScript.Definitions;
using Frends.Python.ExecuteScript.Helpers;

namespace Frends.Python.ExecuteScript;

/// <summary>
/// Task class.
/// </summary>
public static class Python
{
    /// <summary>
    /// Task to execute Python scripts.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Python-ExecuteScript)
    /// </summary>
    /// <param name="input">Information about an executed script</param>
    /// <param name="options">Exception settings.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, int ExitCode, object Error { string Message, Exception AdditionalInfo }, string StandardOutput, string StandardError }</returns>
    public static async Task<Result> ExecuteScript(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        var exitCode = 0;
        var stdError = string.Empty;
        var stdOutput = string.Empty;
        using var process = new Process();

        try
        {
            await ValidatePythonAvailability(process, cancellationToken);

            var command = GenerateCommand(IsWindows(), input);
            var psi = new ProcessStartInfo
            {
                FileName = IsWindows() ? "cmd" : "/bin/bash",
                Arguments = $"{GetArgumentsPrefix()} {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            process.StartInfo = psi;
            process.Start();

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);
            stdOutput = await stdoutTask.ConfigureAwait(false);
            stdError = await stderrTask.ConfigureAwait(false);

            await process.WaitForExitAsync(cancellationToken);
            exitCode = process.ExitCode;

            if (exitCode != 0)
            {
                throw new Exception($"Process failed with error: \n{stdError}");
            }

            return new Result
            {
                Success = true,
                Error = null,
                ExitCode = exitCode,
                StandardOutput = stdOutput,
                StandardError = stdError,
            };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ErrorHandler.Handle(
                e,
                options.ThrowErrorOnFailure,
                options.ErrorMessageOnFailure,
                exitCode,
                stdError,
                stdOutput);
        }
        finally
        {
            if (!process.HasExited)
                process.Kill(true);
        }
    }

    private static string GenerateCommand(bool isWindows, Input input)
    {
        var args = input.Arguments != Array.Empty<string>() ? string.Join(" ", input.Arguments) : string.Empty;
        var pythonCommand = input.ExecutionMode switch
        {
            ExecutionMode.File => $"python {input.ScriptPath} {args}",
            ExecutionMode.Inline => isWindows
                ? $"python -c \"{input.Code}\" {args}"
                : $"python -c \\\"{input.Code}\\\" {args}",
            _ => throw new Exception("Execution mode must be defined")
        };

        string fullCommand;
        if (input.IsPreparationNeeded)
        {
            fullCommand = isWindows
                ? $"powershell -File {input.PreparationScriptPath} && {pythonCommand}"
                : $"\"source '{input.PreparationScriptPath}' && {pythonCommand}\"";
        }
        else
        {
            fullCommand = isWindows ? pythonCommand : $"\"{pythonCommand}\"";
        }

        return fullCommand;
    }

    private static async Task ValidatePythonAvailability(Process process, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = IsWindows() ? "cmd" : "/bin/bash",
            Arguments = $"{GetArgumentsPrefix()} python --version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        process.StartInfo = psi;
        process.Start();

        await process.WaitForExitAsync(cancellationToken);
        var exitCode = process.ExitCode;
        if (exitCode != 0) throw new Exception($"Python is not installed nor added to PATH. Exit code: {exitCode}.");
    }

    private static string GetArgumentsPrefix() => IsWindows() ? "/C" : "-c";

    private static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
