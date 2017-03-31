using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Silphid.Showzup
{
    public class PhaseCancelledException : Exception
    {
        public PhaseCancelledException()
        {
        }

        public PhaseCancelledException(string message) : base(message)
        {
        }

        protected PhaseCancelledException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PhaseCancelledException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}