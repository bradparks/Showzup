using System;

namespace Silphid.Showzup.Navigation
{
    [Flags]
    public enum NavigationOrientation
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = 3
    }
}