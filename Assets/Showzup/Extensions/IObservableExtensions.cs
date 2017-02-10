using System;
using Silphid.Sequencit;
using UniRx;

namespace Silphid.Showzup
{
    public static class IObservableExtensions
    {
        #region IObservable<object>

        public static IDisposable BindTo(this IObservable<object> This, IPresenter target) =>
            This.Subscribe(x => target.Present(x).SubscribeAndForget());

        #endregion
    }
}