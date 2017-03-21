using System;
using Silphid.Extensions;
using Silphid.Showzup.Navigation;
using UniRx;

namespace Silphid.Showzup
{
    public class SelectionControl : ListControl, INavigatable
    {
        private bool _isSynching;

        public ReactiveProperty<object> SelectedItem { get; } = new ReactiveProperty<object>();
        public ReactiveProperty<IView> SelectedView { get; } = new ReactiveProperty<IView>();
        public ReactiveProperty<int?> SelectedIndex { get; } = new ReactiveProperty<int?>();
        public NavigationOrientation SupportedNavigationOrientations;

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

        public virtual bool CanHandle(NavigationCommand command)
        {
            return (SupportedNavigationOrientations & command.Orientation) != 0 &&
                   SelectedIndex.Value + command.Offset >= 0 &&
                   SelectedIndex.Value + command.Offset < Views.Count;
        }

        public virtual void Handle(NavigationCommand command)
        {
            if (CanHandle(command))
                SelectedIndex.Value += command.Offset;
        }
    }
}