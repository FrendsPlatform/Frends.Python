using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Frends.Python.ExecuteScript.Definitions;
using NUnit.Framework;

namespace Frends.Python.ExecuteScript.Tests;

[TestFixture]
public class UnitTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");

    private static readonly string PrepScriptExtension =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".ps1" : ".sh";

    private static readonly string CustomErrorMessage = "error message";

    private static Input DefaultInput() => new()
    {
        IsPreparationNeeded = true,
        PreparationScriptPath = Path.Combine(TestDataDir, $"prepScript{PrepScriptExtension}"),
        ExecutionMode = ExecutionMode.File,
        ScriptPath = Path.Combine(TestDataDir, "script.py"),
        Code = null,
        Arguments = [],
    };

    private static Options DefaultOptions() => new() { ThrowErrorOnFailure = true, ErrorMessageOnFailure = null };

    [Test]
    public void CustomErrorMessageIsUsed()
    {
        var input = DefaultInput();
        input.ScriptPath = Path.Combine(TestDataDir, "invalid.py");
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await Python.ExecuteScript(input, options, CancellationToken.None));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message == CustomErrorMessage);
    }

    [Test]
    public void ErrorThrownOnFailure()
    {
        var input = DefaultInput();
        input.ScriptPath = Path.Combine(TestDataDir, "invalid.py");
        Assert.ThrowsAsync<Exception>(async () =>
            await Python.ExecuteScript(input, DefaultOptions(), CancellationToken.None));
    }

    [Test]
    public async Task ResultWithFailureReturnedOnFailure()
    {
        var input = DefaultInput();
        input.ScriptPath = Path.Combine(TestDataDir, "invalid.py");
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = await Python.ExecuteScript(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task ScriptExecutedWithPreparation()
    {
        var input = DefaultInput();
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = await Python.ExecuteScript(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.ExitCode, Is.EqualTo(0));
        Assert.That(result.StandardOutput, Does.Contain("Preparing environment..."));
    }

    [Test]
    public async Task InlineScriptExecuted()
    {
        var input = DefaultInput();
        input.ExecutionMode = ExecutionMode.Inline;
        input.Code = "print('Hello, World!')";
        var result = await Python.ExecuteScript(input, DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.ExitCode, Is.EqualTo(0));
        Assert.That(result.StandardOutput, Does.Contain("Hello, World!"));
    }

    [Test]
    public async Task ScriptFromFileExecuted()
    {
        var result = await Python.ExecuteScript(DefaultInput(), DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.ExitCode, Is.EqualTo(0));
        Assert.That(result.StandardOutput, Does.Contain("Hello, World!"));
    }

    [Test]
    public async Task FileScriptWithArgumentsExecuted()
    {
        var input = DefaultInput();
        input.ScriptPath = Path.Combine(TestDataDir, "scriptWithArgs.py");
        input.Arguments = ["testName"];
        var result = await Python.ExecuteScript(input, DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.ExitCode, Is.EqualTo(0));
        Assert.That(result.StandardOutput, Does.Contain("Hello, testName!"));
    }

    [Test]
    public async Task InlineScriptWithArgumentsExecuted()
    {
        var input = DefaultInput();
        input.ExecutionMode = ExecutionMode.Inline;
        input.Code = "import sys; print(f'Hello, {sys.argv[1]}!')";
        input.Arguments = ["testName"];
        var result = await Python.ExecuteScript(input, DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.ExitCode, Is.EqualTo(0));
        Assert.That(result.StandardOutput, Does.Contain("Hello, testName!"));
    }

    [Test]
    public async Task ScriptWithImportedPackageRunsCorrectly()
    {
        var input = DefaultInput();
        input.ScriptPath = Path.Combine(TestDataDir, "scriptWithImport.py");
        var result = await Python.ExecuteScript(input, DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.ExitCode, Is.EqualTo(0));
        Assert.That(result.StandardOutput, Does.Contain("Sum: 15"));
    }

    [Test]
    public async Task ResultContainsStandardOutput()
    {
        var result = await Python.ExecuteScript(DefaultInput(), DefaultOptions(), CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.ExitCode, Is.EqualTo(0));
        Assert.That(result.StandardOutput, Is.Not.Empty);
    }

    [Test]
    public async Task ResultContainsStandardError()
    {
        var input = DefaultInput();
        input.ScriptPath = Path.Combine(TestDataDir, "invalid.py");
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = await Python.ExecuteScript(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.ExitCode, Is.Not.EqualTo(0));
        Assert.That(result.StandardError, Is.Not.Empty);
    }
}