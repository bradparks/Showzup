using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class ListControl : Control, IPresenter
    {
        [Inject] internal IViewResolver ViewResolver { get; set; }
        [Inject] internal IViewLoader ViewLoader { get; set; }

        public bool SizeToContent;
        public GameObject Container;
        public string[] Variants;

        [Pure]
        public IObservable<IView> Present(object input, Options options = null)
        {
            if (!(input is IEnumerable))
                input = new[] {input};

            return PresentInternal((IEnumerable) input, options);
        }

        [Pure]
        private IObservable<IView> PresentInternal(IEnumerable items, Options options = null)
        {
            RemoveAllViews(Container);

            return LoadViews(items, options)
                .Do(view => AddView(Container, view));
        }

        [Inject]
        internal void PostInject()
        {
            RemoveAllViews(Container);
        }

        private IObservable<IView> LoadViews(IEnumerable items, Options options)
        {
            if (items == null)
                return Observable.Empty<IView>();

            return items.Cast<object>().ToObservable()
                .SelectMany(x =>
                {
                    var viewInfo = ViewResolver.Resolve(x, options.WithExtraVariants(Variants));
                    return ViewLoader.Load(viewInfo);
                });
        }
    }
}