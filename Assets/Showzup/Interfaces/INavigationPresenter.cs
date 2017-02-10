using System.Collections.Generic;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface INavigationPresenter : ITransitionPresenter
    {
        ReadOnlyReactiveProperty<bool> CanPop { get; }
        ReadOnlyReactiveProperty<IView> View { get; }
        ReactiveProperty<List<IView>> History { get; }

        [Pure] IObservable<IView> Pop();
        [Pure] IObservable<IView> PopToRoot();
        [Pure] IObservable<IView> PopTo(IView view);
    }
}