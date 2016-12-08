using System;
using UniRx;

namespace Silphid.Showzup
{
    public static class IObservableNavExtensions
    {
        #region IObservable<object>

        public static IDisposable BindTo(this IObservable<object> This, IPresenter target) =>
            This.Subscribe(x => target.Present(x));

        #endregion

        #region IObservable<Nav>

        public static IObservable<TView> From<TView>(this IObservable<Nav> This) =>
            This.Where(x => x.Source is TView).Select(x => (TView) x.Source);

        public static IObservable<TView> To<TView>(this IObservable<Nav> This) =>
            This.Where(x => x.Target is TView).Select(x => (TView) x.Target);

        public static IObservable<Nav> Between<TSource, TTarget>(this IObservable<Nav> This) =>
            This.Where(x => x.Source is TSource && x.Target is TTarget);

        #endregion
    }
}