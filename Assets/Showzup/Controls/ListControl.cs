﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class ListControl : Control, IPresenter
    {
        [Inject]
        internal IViewLoader ViewLoader { get; set; }

        public bool SizeToContent;
        public GameObject Container;
        public string[] Variants;

        private readonly List<IView> _views = new List<IView>();

        public ReadOnlyCollection<IView> Views { get; }

        public ListControl()
        {
            Views = _views.AsReadOnly();
        }

        public IView GetViewForViewModel(object viewModel) => Views.FirstOrDefault(x => x.ViewModel == viewModel);

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
            _views.Clear();

            RemoveAllViews(Container);

            return LoadViews(items, options)
                .Do(view =>
                {
                    _views.Add(view);
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

            return items.Cast<object>().ToObservable()
                .SelectMany(x => ViewLoader.Load(x, options.WithExtraVariants(Variants)));
        }
    }
}