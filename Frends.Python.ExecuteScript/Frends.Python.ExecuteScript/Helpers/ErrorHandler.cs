using System;
using Frends.Python.ExecuteScript.Definitions;

namespace Frends.Python.ExecuteScript.Helpers;

public static class ErrorHandler
{
    /// <summary>
    /// Handler for exceptions
    /// </summary>
    /// <param name="exception">Caught exception</param>
    /// <param name="throwOnFailure">Frends flag</param>
    /// <param name="errorMessage">Message to throw in error event</param>
    /// <returns>Throw exception if a flag is true, else return Result with Error info</returns>
    public static Result Handle(Exception exception, bool throwOnFailure, string errorMessageOnFailure, int exitCode,
        string stdError, string stdOutput)
    {
        if (throwOnFailure)
        {
            if (string.IsNullOrEmpty(errorMessageOnFailure))
                throw new Exception(exception.Message, exception);

            throw new Exception(errorMessageOnFailure, exception);
        }

        var errorMessage = !string.IsNullOrEmpty(errorMessageOnFailure)
            ? $"{errorMessageOnFailure}: {exception.Message}"
            : exception.Message;

        return new Result
        {
            Success = false,
            ExitCode = exitCode,
            Error = new Error
            {
                Message = errorMessage,
                AdditionalInfo = new
                {
                    Exception = exception,
                },
            },
            StandardError = stdError,
            StandardOutput = stdOutput
        };
    }
}