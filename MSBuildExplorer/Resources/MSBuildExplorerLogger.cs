//-----------------------------------------------------------------------
// <copyright file="MSBuildExplorerLogger.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuildExplorer
{
    using System;
    using System.Globalization;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public class MSBuildExplorerLogger : Logger
    {
        private EventHandler handler;
        private int indent;
        private int warnings;
        private int errors;
        private DateTime startTime;

        public event EventHandler OnMessage;

        public override void Initialize(IEventSource eventSource)
        {
            this.handler = this.OnMessage;
            eventSource.BuildFinished += this.BuildFinished;
            eventSource.BuildStarted += this.BuildStarted;
            eventSource.ErrorRaised += this.ErrorRaised;
            eventSource.WarningRaised += this.WarningRaised;

            if (this.Verbosity != LoggerVerbosity.Quiet)
            {
                eventSource.MessageRaised += this.MessageRaised;
                eventSource.ProjectStarted += this.ProjectStarted;
                eventSource.ProjectFinished += this.ProjectFinished;
            }

            if (this.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                eventSource.TargetStarted += this.TargetStarted;
                eventSource.TargetFinished += this.TargetFinished;
            }

            if (this.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
            {
                eventSource.TaskStarted += this.TaskStarted;
                eventSource.TaskFinished += this.TaskFinished;
            }
        }

        public void ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            // BuildErrorEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters
            string line = string.Format("{3}: Error {0}({1},{2}): ", e.File, e.LineNumber, e.ColumnNumber, e.Message);
            this.handler(this, new ValEventArgs(line, this.indent));
            this.errors++;
        }

        public override void Shutdown()
        {
        }

        private void WarningRaised(object sender, BuildWarningEventArgs e)
        {
            // BuildWarningEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters
            string line = string.Format("{3}: Warning {0}({1},{2}): ", e.File, e.LineNumber, e.ColumnNumber, e.Message);
            this.handler(this, new ValEventArgs(line, this.indent));
            this.warnings++;
        }

        private void MessageRaised(object sender, BuildMessageEventArgs e)
        {
            this.handler(this, new ValEventArgs(e.Message, this.indent));
        }

        private void ProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            this.handler(this, new ValEventArgs(e.Message, this.indent));
        }

        private void TaskStarted(object sender, TaskStartedEventArgs e)
        {
            this.indent++;
            this.handler(this, new ValEventArgs(e.Message, this.indent));
        }

        private void TaskFinished(object sender, TaskFinishedEventArgs e)
        {
            this.indent--;
            this.handler(this, new ValEventArgs(e.Message, this.indent));
        }

        private void TargetStarted(object sender, TargetStartedEventArgs e)
        {
            this.indent++;
            this.handler(this, new ValEventArgs(string.Format(CultureInfo.InvariantCulture, "Target {0}:", e.TargetName), this.indent));
        }

        private void TargetFinished(object sender, TargetFinishedEventArgs e)
        {
            this.indent--;
            this.handler(this, new ValEventArgs(string.Format(CultureInfo.InvariantCulture, "Done building target \"{0}\" in project \"{1}\"", e.TargetName, e.ProjectFile), this.indent));
        }

        private void ProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            this.indent--;
            this.handler(this, new ValEventArgs(e.Message, this.indent));
        }

        private void BuildFinished(object sender, BuildFinishedEventArgs e)
        {
            this.handler(this, this.errors == 0 ? new ValEventArgs(e.Message, this.indent, Helper.MessageType.Success) : new ValEventArgs(e.Message, this.indent, Helper.MessageType.Error));

            this.handler(this, new ValEventArgs(string.Format(CultureInfo.InvariantCulture, "{0} Warning(s) ", this.warnings), this.indent));
            this.handler(this, new ValEventArgs(string.Format(CultureInfo.InvariantCulture, "{0} Error(s) ", this.errors) + Environment.NewLine + Environment.NewLine, this.indent));

            TimeSpan s = DateTime.UtcNow - this.startTime;
            this.handler(this, new ValEventArgs(string.Format(CultureInfo.InvariantCulture, "Time Elapsed {0}", s), this.indent));
        }

        private void BuildStarted(object sender, BuildStartedEventArgs e)
        {
            this.startTime = DateTime.UtcNow;
            string line = string.Format(CultureInfo.InvariantCulture, "{0} {1}", e.Message, e.Timestamp);
            this.handler(this, new ValEventArgs(line, this.indent));
        }
    }
}
