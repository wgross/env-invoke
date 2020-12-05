using System.Linq;
using System.Management.Automation;
using Xunit;

namespace EnvInvoke.Test
{
    public class InvokeScriptBlockCommandTest
    {
        protected PowerShell PowerShell { get; } = PowerShell.Create();

        public InvokeScriptBlockCommandTest()
        {
            this.PowerShell
                .AddCommand("Import-Module").AddParameter("Name", "./envinvoke.dll")
                .Invoke();

            if (this.PowerShell.HadErrors)
                throw new PSInvalidOperationException("test initialization fails");

            this.PowerShell.Commands.Clear();
        }

        [Fact]
        public void InvokeScriptBlockCommand_passes_pipe_to_scriptblock_and_returns_result()
        {
            // ARRANGE

            var scriptBlock = ScriptBlock.Create("$_.Length");

            // ACT

            var result = this.PowerShell
                .AddCommand("Invoke-ScriptBlock").AddParameter("ScriptBlock", scriptBlock)
                .Invoke(new[] { "one", "two" })
                .ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(new[] { 3, 3 }, result.Select(pso => (int)pso.BaseObject));
        }

        [Fact]
        public void InvokeScriptBlockCommand_passes_pipe_to_scriptblock_and_sets_variable_in_current_session_and_returns_result()
        {
            // ARRANGE

            var scriptBlock = ScriptBlock.Create("$_.Length; $len = $_.Length");

            // ACT

            var result = this.PowerShell
                .AddCommand("Invoke-ScriptBlock").AddParameter("ScriptBlock", scriptBlock)
                .AddStatement()
                .AddScript("$len")
                .Invoke(new[] { "one", "two" })
                .ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(new[] { 3, 3, 3 }, result.Select(pso => (int)pso.BaseObject));
        }

        [Fact]
        public void InvokeScriptBlockCommand_passes_pipe_and_arguments_and_returns_results()
        {
            // ARRANGE

            var scriptBlock = ScriptBlock.Create("param($p, $s) $p+$_+$s");

            // ACT

            var result = this.PowerShell
                .AddCommand("Invoke-ScriptBlock")
                    .AddParameter("ScriptBlock", scriptBlock)
                    .AddParameter("ArgumentList", new[] { "prefix-", "-suffix" })
                .Invoke(new[] { "one", "two" })
                .ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(new[] { "prefix-one-suffix", "prefix-two-suffix" }, result.Select(pso => (string)pso.BaseObject));
        }

        [Fact]
        public void InvokeScriptBlockCommand_passes_pipe_to_scriptblock_and_returns_multiple_results()
        {
            // ARRANGE

            var scriptBlock = ScriptBlock.Create("$_.Length; $_");

            // ACT

            var result = this.PowerShell
                .AddCommand("Invoke-ScriptBlock").AddParameter("ScriptBlock", scriptBlock)
                .Invoke(new[] { "one", "two" })
                .ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(new object[] { 3, "one", 3, "two" }, result.Select(pso => pso.BaseObject));
        }
    }
}