using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Frends.Python.Install.Definitions;
using Frends.Python.Install.Helpers;

namespace Frends.Python.Install;

/// <summary>
/// Task Class for Python operations.
/// </summary>
public static class Python
{
    /// <summary>
    /// Task to install Python on machine
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Python-Install)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">x.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Output, object Error { string Message, Exception AdditionalInfo } }</returns>
    // TODO: Remove Connection parameter if the task does not make connections
    public static Result Install(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Do something with connection parameters, e.g., connect to a service.
            _ = connection.ConnectionString;

            // Cancellation token should be provided to methods that support it
            // and checked during long-running operations, e.g., loops
            cancellationToken.ThrowIfCancellationRequested();

            if (input.Repeat < 0)
                throw new Exception("Repeat count cannot be negative.");

            var output = string.Join(options.Delimiter, Enumerable.Repeat(input.Content, input.Repeat));

            return new Result
            {
                Success = true,
                Output = output,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}
