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
    /// Pythones the input string the specified number of times.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Python-ExecuteScript)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Output, object Error { string Message, dynamic AdditionalInfo } }</returns>
    public static async Task<Result> ExecuteScript(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        var exitCode = 0;
        var stdError = string.Empty;
        var stdOutput = string.Empty;
        try
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var command = GenerateCommand(isWindows, input);
            var argumentsPrefix = isWindows ? "/C" : "-c";
            var psi = new ProcessStartInfo
            {
                FileName = isWindows ? "cmd" : "/bin/bash",
                Arguments = $"{argumentsPrefix} {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var process = new Process();
            process.StartInfo = psi;
            process.Start();

            stdOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            stdError = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

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
            return ErrorHandler.Handle(e, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure, exitCode,
                stdError, stdOutput);
        }
    }

    private static string GenerateCommand(bool isWindows, Input input)
    {
        string pythonCommand;
        var args = input.Arguments != Array.Empty<string>() ? string.Join(" ", input.Arguments) : string.Empty;
        switch (input.ExecutionMode)
        {
            case ExecutionMode.Script:
                pythonCommand = $"python {input.ScriptPath} {args}";
                break;
            case ExecutionMode.Inline:
                pythonCommand = $"python -c \"{input.Code}\" {args}";
                break;
            default:
                throw new Exception("Execution mode must be defined");
        }

        string fullCommand;
        if (input.IsPreparationNeeded)
        {
            fullCommand = isWindows
                ? $"powershell -File {input.PreparationScriptPath} && {pythonCommand}"
                : $"source '{input.PreparationScriptPath}' && {pythonCommand}";
        }
        else
        {
            fullCommand = pythonCommand;
        }

        return fullCommand;
    }
}