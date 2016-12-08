using UnityEngine;

namespace Silphid.Showzup
{
    public interface IView
    {
        object ViewModel { get; set; }
        GameObject GameObject { get; }
    }
}