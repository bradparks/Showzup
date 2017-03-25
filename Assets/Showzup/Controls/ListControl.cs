using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class ListControl : Control, IListPresenter
    {
        public ReadOnlyReactiveProperty<ReadOnlyCollection<IView>> Views { get; }

        [Inject]
        internal IViewLoader ViewLoader { get; set; }

        public bool SizeToContent;
        public GameObject Container;
        public string[] Variants;

        protected readonly List<IView> _views = new List<IView>();
        private readonly ReactiveProperty<ReadOnlyCollection<IView>> _reactiveViews;

        public ListControl()
        {
            _reactiveViews = new ReactiveProperty<ReadOnlyCollection<IView>>(_views.AsReadOnly());
            Views = _reactiveViews.ToReadOnlyReactiveProperty();
        }

        public IView GetViewForViewModel(object viewModel) =>
            _views.FirstOrDefault(x => x.ViewModel == viewModel);

        public int? IndexOfView(IView view)
        {
            if (view == null)
                return null;

            int index = _views.IndexOf(view);
            if (index == -1)
                return null;

            return index;
        }

        public IView GetViewAtIndex(int? index) =>
            index.HasValue ? _views[index.Value] : null;

        [Pure]
        public virtual IObservable<IView> Present(object input, Options options = null)
        {
            if (!(input is IEnumerable))
                input = new[] {input};

            return PresentInternal((IEnumerable) input, options);
        }

        [Pure]
        private IObservable<IView> PresentInternal(IEnumerable items, Options options = null)
        {
            _views.Clear();
            _reactiveViews.Value = _views.AsReadOnly();

            RemoveAllViews(Container);

            return LoadViews(items, options)
                .Do(view =>
                {
                    _views.Add(view);
                    _reactiveViews.Value = _views.AsReadOnly();
                    AddView(Container, view);
                });
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

            var optionsWithExtraVariants = options.WithExtraVariants(Variants);
            return items.Cast<object>().ToObservable()
                .SelectMany(x => LoadView(x, optionsWithExtraVariants));
        }

        protected virtual IObservable<IView> LoadView(object item, Options options) =>
            ViewLoader.Load(item, CancellationToken.Empty, options);
    }
}