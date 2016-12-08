using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface IViewLoader
    {
        [Pure] IObservable<IView> Load(object input, Options options = null);
    }
}