using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Python.ExecuteScript.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    public bool IsPreparationNeeded { get; set; }
    public string PreparationScriptPath { get; set; } //flag true only
    public ExecutionMode ExecutionMode { get; set; }
    public string ScriptPath { get; set; } //script mode only
    public string Code { get; set; } //inline mode only
    public string[] Arguments { get; set; } = [];
}
