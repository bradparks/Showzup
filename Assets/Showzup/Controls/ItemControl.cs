using System;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class ItemControl : PresenterControlBase
    {
        public GameObject Container;

        protected override IObservable<Unit> Present(IView view, Options options)
        {
            if (Container == null)
                throw new InvalidOperationException($"Must specify ContentContainer property of ContentControl {gameObject}");

            ReplaceView(Container, view);
            view.IsActive = true;
            View.Value = view;

            return Observable.ReturnUnit();
        }
    }
}