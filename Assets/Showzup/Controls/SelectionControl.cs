﻿using System;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public class SelectionControl : ListControl
    {
        private bool _isSynching;

        public ReactiveProperty<object> SelectedItem { get; } = new ReactiveProperty<object>();
        public ReactiveProperty<IView> SelectedView { get; } = new ReactiveProperty<IView>();
        public ReactiveProperty<int?> SelectedIndex { get; } = new ReactiveProperty<int?>();

        public virtual void Start()
        {
            SubscribeToUpdateSelectable(SelectedItem);
            SubscribeToUpdateSelectable(SelectedView);

            SubscribeToSynchOthers(SelectedItem, () =>
            {
                SelectedView.Value = GetViewForViewModel(SelectedItem.Value);
                SelectedIndex.Value = IndexOfView(SelectedView.Value);
            });

            SubscribeToSynchOthers(SelectedView, () =>
            {
                SelectedItem.Value = SelectedView.Value?.ViewModel;
                SelectedIndex.Value = IndexOfView(SelectedView.Value);
            });

            SubscribeToSynchOthers(SelectedIndex, () =>
            {
                SelectedView.Value = GetViewAtIndex(SelectedIndex.Value);
                SelectedItem.Value = SelectedView.Value?.ViewModel;
            });
        }

        private void SubscribeToUpdateSelectable<T>(IObservable<T> observable)
        {
            observable
                .PairWithPrevious()
                .Subscribe(x =>
                {
                    var previous = x.Item1 as ISelectable;
                    if (previous != null) previous.IsSelected.Value = false;

                    var current = x.Item2 as ISelectable;
                    if (current != null) current.IsSelected.Value = true;
                })
                .AddTo(this);
        }

        private void SubscribeToSynchOthers<T>(IObservable<T> observable, Action synchAction)
        {
            observable.Subscribe(x =>
                {
                    if (_isSynching)
                        return;

                    _isSynching = true;
                    synchAction();
                    _isSynching = false;
                })
                .AddTo(this);
        }

        public bool SelectNext()
        {
            if (SelectedView.Value == null)
                return false;

            var newIndex = Views.IndexOf(SelectedView.Value) + 1;
            if (newIndex >= Views.Count)
                return false;

            SelectedView.Value = Views[newIndex];
            return true;
        }

        public bool SelectPrevious()
        {
            if (SelectedView.Value == null)
                return false;

            var newIndex = Views.IndexOf(SelectedView.Value) - 1;
            if (newIndex < 0)
                return false;

            SelectedView.Value = Views[newIndex];
            return true;
        }
    }
}