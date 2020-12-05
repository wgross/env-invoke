using System;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Reflection;

namespace EnvInvoke
{
    [Cmdlet(VerbsLifecycle.Invoke, nameof(ScriptBlock))]
    public sealed class InvokeScriptBlockCommand : PSCmdlet
    {
        private SessionState scriptsSessionState;
        private object oldDollarUnder;

        [Parameter(ValueFromPipeline = true)]
        public object InputObject { get; set; }

        [Parameter()]
        public object[] ArgumentList { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public ScriptBlock ScriptBlock { get; set; }

        private static readonly MethodInfo InvokeUsingCmdletMethod = typeof(ScriptBlock).GetMethod("InvokeUsingCmdlet", BindingFlags.NonPublic | BindingFlags.Instance);

        protected override void ProcessRecord()
        {
            InvokeUsingCmdletMethod.Invoke(this.ScriptBlock, new object[]
            {
                this, // contextCmdlet
                false, // useLocalScope
                1, // ErrorHandlingBehavior.WriteToCurrentErrorPipe = 1,
                this.InputObject, // dollarUnder
                new object[] { this.InputObject }, // input
                AutomationNull.Value, // scriptThis
                this.ArgumentList ?? Array.Empty<object>() // args
            });
        }
    }
}