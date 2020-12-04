using System;
using System.Management.Automation;
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

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public ScriptBlock Process { get; set; }

        protected override void BeginProcessing()
        {
            this.scriptsSessionState = (SessionState)typeof(ScriptBlock)
                .GetProperty("SessionState", System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(this.Process, null);
            this.oldDollarUnder = this.scriptsSessionState.PSVariable.GetValue("_");
        }

        protected override void ProcessRecord()
        {
            this.scriptsSessionState.PSVariable.Set("_", this.InputObject);
            try
            {
                foreach (var result in this.SessionState.InvokeCommand.InvokeScript(this.scriptsSessionState, this.Process, new object[0]))
                    this.WriteObject(result);
            }
            catch (Exception ex)
            {
                this.WriteError(new ErrorRecord(ex, "ScriptBlock.Exception", ErrorCategory.OperationStopped, this.InputObject));
            }
        }

        protected override void EndProcessing()
        {
            this.scriptsSessionState.PSVariable.Set("_", this.oldDollarUnder);
        }
    }
}