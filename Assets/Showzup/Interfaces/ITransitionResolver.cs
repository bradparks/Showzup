using System;

namespace Silphid.Showzup
{
    public interface ITransitionResolver
    {
        Transition Resolve(Type sourceViewType, Type targetViewType);
    }
}