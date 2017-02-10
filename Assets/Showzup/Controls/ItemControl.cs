using System;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class ItemControl : Control, IPresenter
    {
        #region Fields

        private readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();

        #endregion

        #region Properties

        [Inject] internal IViewResolver ViewResolver { get; set; }
        [Inject] internal IViewLoader ViewLoader { get; set; }
        public GameObject Container;
        public string[] Variants;

        #endregion

        #region Life-time

        internal void Start()
        {
            if (Container == null)
                throw new InvalidOperationException($"Must specify ContentContainer property of ContentControl {gameObject}");
        }

        #endregion

        #region IContentControl members

        [Pure]
        public IObservable<IView> Present(object input, Options options = null)
        {
            Debug.Log($"#Content# Present({input}, {options})");

            var viewInfo = ViewResolver.Resolve(input, options.WithExtraVariants(Variants));

            return ViewLoader
                .Load(viewInfo)
                .Do(view =>
                {
                    ReplaceView(Container, view);
                    view.IsActive = true;
                    _view.Value = view;
                });
        }

    #endregion
    }
}