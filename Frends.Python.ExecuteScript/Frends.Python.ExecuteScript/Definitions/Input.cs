using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Python.ExecuteScript.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Defines whether the preparation script is needed to run before the main script.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool IsPreparationNeeded { get; set; }

    /// <summary>
    /// Path to a script that should be run before a main script. Script can be .ps1 on Windows or .sh on Linux.
    /// </summary>
    /// <example>/path/to/script.sh</example>
    [UIHint(nameof(IsPreparationNeeded), "", true)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string PreparationScriptPath { get; set; }

    /// <summary>
    /// Defines how script is provided.
    /// </summary>
    /// <example>File</example>
    [DefaultValue(ExecutionMode.File)]
    public ExecutionMode ExecutionMode { get; set; }

    /// <summary>
    /// Path to the python script file.
    /// </summary>
    /// <example>/path/to/script.py</example>
    [UIHint(nameof(ExecutionMode), "", ExecutionMode.File)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ScriptPath { get; set; }

    /// <summary>
    /// Inline python script to execute.
    /// </summary>
    /// <example>import sys; print(f'Hello, {sys.argv[1]}!')</example>
    [UIHint(nameof(ExecutionMode), "", ExecutionMode.Inline)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string Code { get; set; }

    /// <summary>
    /// Arguments provided to the script.
    /// </summary>
    /// <example>['MyName']</example>
    [DefaultValue("")]
    public string[] Arguments { get; set; } = [];
}