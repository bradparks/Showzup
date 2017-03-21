using System;

namespace Silphid.Showzup.Navigation
{
    public interface INavigationService : INavigatable
    {
        IDisposable Push(INavigatable navigatable);
    }
}