namespace Frends.Python.ExecuteScript.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// ExitCode returned by the process. 0 indicates success.
    /// </summary>
    /// <example>0</example>
    public int ExitCode { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }

    /// <summary>
    /// Output written to standard output (stdout) by the process.
    /// </summary>
    /// <example>Hello, World!</example>
    public string StandardOutput { get; set; }

    /// <summary>
    /// Error output written to standard error (stderr) by the process.
    /// </summary>
    /// <example>File not found</example>
    public string StandardError { get; set; }
}