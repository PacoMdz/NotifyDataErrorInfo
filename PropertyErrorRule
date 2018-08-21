using System;

namespace Mx.CustomNotifyDataError
{
    public struct PropertyErrorRule
    {
        public PropertyErrorRule(Func<bool> failWith, string message)
        {
            FailWith = failWith;
            Message = message;
        }

        public Func<bool> FailWith { get; private set; }
        public string Message { get; private set; }
    }
}
