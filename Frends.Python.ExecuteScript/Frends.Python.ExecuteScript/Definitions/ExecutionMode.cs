namespace Frends.Python.ExecuteScript.Definitions;

/// <summary>
/// Execution mode enum.
/// </summary>
public enum ExecutionMode
{
    /// <summary>
    /// Script is provided as a file path.
    /// </summary>
    File = 1,

    /// <summary>
    /// Script is provided as an inline script string.
    /// </summary>
    Inline = 2,
}