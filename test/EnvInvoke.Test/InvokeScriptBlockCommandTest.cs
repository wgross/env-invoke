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
        public void InvokeScripBlockCommand_passes_pipe_to_scriptblock_and_returns_result()
        {
            // ARRANGE

            var scriptBlock = ScriptBlock.Create("$_.Length");

            // ACT

            var result = this.PowerShell
                .AddCommand("Invoke-ScriptBlock").AddParameter("Process", scriptBlock)
                .Invoke(new[] { "one", "two" })
                .ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(new[] { 3, 3 }, result.Select(pso => (int)pso.BaseObject));
        }

        [Fact]
        public void InvokeScripBlockCommand_passes_pipe_to_scriptblock_and_returns_multiple_results()
        {
            // ARRANGE

            var scriptBlock = ScriptBlock.Create("$_.Length; $_");

            // ACT

            var result = this.PowerShell
                .AddCommand("Invoke-ScriptBlock").AddParameter("Process", scriptBlock)
                .Invoke(new[] { "one", "two" })
                .ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(new object[] { 3, "one", 3, "two" }, result.Select(pso => pso.BaseObject));
        }

        [Fact]
        public void InvokeScripBlockCommand_catches_error()
        {
            // ARRANGE

            var scriptBlock = ScriptBlock.Create("throw \"fail:$_\"");

            // ACT

            var result = this.PowerShell
                .AddCommand("Invoke-ScriptBlock").AddParameter("Process", scriptBlock)
                .Invoke(new[] { "one", "two" })
                .ToArray();

            // ASSERT

            Assert.True(this.PowerShell.HadErrors);
            Assert.Equal(2, this.PowerShell.Streams.Error.Count);
            Assert.Equal("fail:one", this.PowerShell.Streams.Error.First().Exception.Message);
            Assert.Equal("fail:two", this.PowerShell.Streams.Error.Last().Exception.Message);
            Assert.Empty(result);
        }
    }
}