using System;

namespace Silphid.Showzup
{
    public interface IViewResolver
    {
        ViewMapping ResolveFromViewModelType(Type viewModelType, Options options = null);
        ViewMapping ResolveFromViewType(Type viewType, Options options = null);
    }
}