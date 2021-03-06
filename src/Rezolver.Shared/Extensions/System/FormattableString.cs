﻿
#if NET451
// SHIM FOR FormattableStringFactory and FormattableString for Net451 builds
// Courtesy of http://stackoverflow.com/a/32077216/157701
namespace System.Runtime.CompilerServices
{
    internal class FormattableStringFactory
    {
        public static FormattableString Create(string messageFormat, params object[] args)
        {
            return new FormattableString(messageFormat, args);
        }
    }
}

namespace System
{
    internal class FormattableString : IFormattable
    {
        private readonly string messageFormat;
        private readonly object[] args;

        public FormattableString(string messageFormat, object[] args)
        {
            this.messageFormat = messageFormat;
            this.args = args;
        }

        public override string ToString()
        {
            return string.Format(this.messageFormat, this.args);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? this.messageFormat, this.args);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, this.messageFormat, this.args);
        }
    }
}
#endif