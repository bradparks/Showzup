using System;
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

        public void SelectNext()
        {
            var currentIndex = Views.IndexOf(SelectedView.Value);
            currentIndex++;

            SelectedView.Value = currentIndex > Views.Count - 1 ? null : Views[currentIndex];
        }

        public void SelectPrevious()
        {
            var currentIndex = Views.IndexOf(SelectedView.Value);
            currentIndex--;

            SelectedView.Value = currentIndex < 0 ? null : Views[currentIndex];
        }
    }
}