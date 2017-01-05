﻿using System;
using System.Collections;
using Silphid.Extensions;
using Silphid.Loadzup;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Silphid.Showzup
{
    public abstract class View<TViewModel> : MonoBehaviour, IView
    {
        #region IView members

        private object _viewModel;

        public bool IsActive
        {
            get { return enabled && gameObject.activeSelf; }
            set
            {
                gameObject.SetActive(value);
                enabled = value;
            }
        }

        object IView.ViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                _viewModel = value;
            }
        }

        public GameObject GameObject => gameObject;
        public TViewModel ViewModel => (TViewModel) _viewModel;

        [Inject] protected ILoader Loader;

        protected void Bind(Text text, string value)
        {
            if (text != null)
                text.text = value;
        }

        protected void Bind(IPresenter presenter, object content)
        {
            presenter?
                .Present(content)
                .AutoDetach()
                .Subscribe()
                .AddTo(this);
        }

        protected void Bind(ListControl listControl, IEnumerable items)
        {
            if (listControl != null)
                BindAsync(listControl, items)
                    .Subscribe()
                    .AddTo(this);
        }

        protected IObservable<Unit> BindAsync(ListControl listControl, IEnumerable items) =>
            listControl
                ?.Present(items)
                .AsSingleUnitObservable()
                .AutoDetach()
            ?? Observable.ReturnUnit();

        protected void Bind(Image image, Uri uri)
        {
            if (image != null)
                BindAsync(image, uri)
                    .Subscribe()
                    .AddTo(this);
        }

        protected IObservable<Unit> BindAsync(Image image, Uri uri, Loadzup.Options options = null)
        {
            if (image == null)
                return Observable.ReturnUnit();

            // Clear placeholder/previous image (if any) while real image is loading
            image.sprite = null;

            return Loader
                .Load<Sprite>(uri, options)
                .Do(x => image.sprite = x)
                .AutoDetach()
                .AsSingleUnitObservable();
        }

        #endregion
    }
}