using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public class SelectionControl : ListControl
    {
        private bool _isItemOrViewChanging;

        public ReactiveProperty<object> SelectedItem { get; } = new ReactiveProperty<object>();
        public ReactiveProperty<IView> SelectedView { get; } = new ReactiveProperty<IView>();

        public SelectionControl()
        {
            SelectedItem.Subscribe(x =>
                {
                    if (_isItemOrViewChanging)
                        return;

                    _isItemOrViewChanging = true;
                    SelectedView.Value = GetViewForViewModel(x);
                    _isItemOrViewChanging = false;
                })
                .AddTo(this);

            SelectedView.Subscribe(x =>
                {
                    if (_isItemOrViewChanging)
                        return;

                    _isItemOrViewChanging = true;
                    SelectedItem.Value = x.ViewModel;
                    _isItemOrViewChanging = false;
                })
                .AddTo(this);

            SubscribeSelectionUpdate(SelectedItem);
            SubscribeSelectionUpdate(SelectedView);
        }

        private void SubscribeSelectionUpdate<T>(IObservable<T> observable)
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
    }
}