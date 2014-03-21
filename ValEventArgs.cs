//-----------------------------------------------------------------------
// <copyright file="ValEventArgs.cs" company="Mike Fourie">(c) Mike Fourie, 2012. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuildExplorer
{
    using System;

    /// <summary>
    /// ValEventArgs
    /// </summary>
    public class ValEventArgs : EventArgs
    {
        public ValEventArgs(string validationmessage, int indentation)
        {
            this.Message = validationmessage;
            this.Indentation = indentation;
            this.Type = Helper.MessageType.Info;
        }

        internal ValEventArgs(string validationmessage, int indentation, Helper.MessageType messType)
        {
            this.Message = validationmessage;
            this.Indentation = indentation;
            this.Type = messType;
        }

        public string Message { get; set; }

        public int Indentation { get; set; }

        internal Helper.MessageType Type { get; set; }
    }
}
