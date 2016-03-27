// --------------------------------------------------
// RgssDecrypter - OptionException.cs
// --------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RgssDecrypter.Options
{
    [Serializable]
    public class OptionException : Exception
    {
        public string OptionName { get; }

        public OptionException() {}

        public OptionException(string message, string optionName)
            : base(message)
        {
            OptionName = optionName;
        }

        public OptionException(string message, string optionName, Exception innerException)
            : base(message, innerException)
        {
            OptionName = optionName;
        }

        protected OptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            OptionName = info.GetString("OptionName");
        }
#pragma warning disable 618 // SecurityPermissionAttribute is obsolete
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
#pragma warning restore 618
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("OptionName", OptionName);
        }
    }
}
