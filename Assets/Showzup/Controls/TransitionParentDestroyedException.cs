using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Silphid.Showzup
{
    public class TransitionParentDestroyedException : Exception
    {
        public TransitionParentDestroyedException()
        {
        }

        public TransitionParentDestroyedException(string message) : base(message)
        {
        }

        protected TransitionParentDestroyedException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TransitionParentDestroyedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}