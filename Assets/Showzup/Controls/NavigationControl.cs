using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UniRx;
using Zenject;

namespace Silphid.Showzup
{
    public class NavigationControl : TransitionControl, INavigationPresenter
    {
        #region Fields

        private readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();
        private ReadOnlyReactiveProperty<bool> _canPop;

        #endregion

        public bool CanPopTopLevelView;

        #region Life-time

        [Inject]
        public void Inject()
        {
            History.PairWithPrevious().Skip(1).Subscribe(DisposeDroppedViews).AddTo(this);
        }

        #endregion

        #region INavigationPresenter members

        public ReadOnlyReactiveProperty<bool> CanPop =>
            _canPop ?? (_canPop = History
                .Select(x => x.Count > (CanPopTopLevelView ? 0 : 1))
                .DistinctUntilChanged()
                .CombineLatest(IsReady, (x, y) => x && y)
                .ToReadOnlyReactiveProperty());

        public ReadOnlyReactiveProperty<IView> View => _view.ToReadOnlyReactiveProperty();

        public ReactiveProperty<List<IView>> History { get; } =
            new ReactiveProperty<List<IView>>(new List<IView>());

        #region IPresenter members

        public override IObservable<IView> Present(object input, Options options = null)
        {
            AssertCanPresent();

            return PresentInternal(
                input,
                options,
                presentation =>
                    GetNewHistory(presentation.TargetView, presentation.Options.GetPushMode()));
        }

        #endregion

        private IObservable<IView> PresentInternal(object input, Options options,
            Func<Presentation, List<IView>> getHistory) =>
            TransitionPresenter.Present(input, _view.Value, Variants, DefaultTransition, options,
                PrePresentation,
                phase => PerformTransition((TransitionPhase) phase),
                presentation =>
                {
                    PostPresentation(presentation);
                    History.Value = getHistory(presentation);
                });

        protected void PrePresentation(Presentation presentation)
        {
            _view.Value = null;
        }

        protected void PostPresentation(Presentation presentation)
        {
            _view.Value = presentation.TargetView;
        }

        private List<IView> GetNewHistory(IView view, PushMode pushMode) =>
            pushMode == PushMode.Child
                ? History.Value.Append(view).ToList()
                : pushMode == PushMode.Sibling
                    ? History.Value.Take(History.Value.Count - 1).Append(view).ToList()
                    : new List<IView> {view};

        public IObservable<IView> Pop()
        {
            AssertCanPop();

            var view = History.Value.Count >= 2
                ? History.Value[History.Value.Count - 2]
                : null;

            //Debug.Log($"#Nav# Pop({view})");
            var history = History.Value.Take(History.Value.Count - 1).ToList();

            return PopInternal(view, history);
        }

        public IObservable<IView> PopToRoot()
        {
            AssertCanPop();

            var view = History.Value.First();
            //Debug.Log($"#Nav# PopToRoot({view})");
            var history = History.Value.Take(1).ToList();

            return PopInternal(view, history);
        }

        public IObservable<IView> PopTo(IView view)
        {
            //Debug.Log($"#Nav# PopTo({view})");
            var viewIndex = History.Value.IndexOf(view);
            AssertCanPopTo(view, viewIndex);
            var history = History.Value.Take(viewIndex + 1).ToList();

            return PopInternal(view, history);
        }

        private IObservable<IView> PopInternal(IView view, List<IView> history) =>
            PresentInternal(view, new Options {Direction = Direction.Backward}, _ => history);

        #endregion

        #region Implementation

        private void AssertCanPresent()
        {
            if (!IsReady.Value)
                throw new InvalidOperationException("Cannot present at this moment");
        }

        private void AssertCanPop()
        {
            if (!CanPop.Value)
                throw new InvalidOperationException("Cannot pop at this moment");
        }

        // ReSharper disable once UnusedParameter.Local
        private void AssertCanPopTo(IView view, int viewIndex)
        {
            AssertCanPop();

            if (viewIndex == -1)
                throw new InvalidOperationException($"History does not contain view {view}");
            if (viewIndex == History.Value.Count - 1)
                throw new InvalidOperationException($"Cannot pop to view {view} because it is already current view");
        }

        private void DisposeDroppedViews(Tuple<List<IView>, List<IView>> tuple)
        {
            tuple.Item1
                .Where(x => !tuple.Item2.Contains(x))
                .ForEach(DisposeView);
        }

        private void DisposeView(IView view)
        {
            if (view == null)
                return;

            Destroy(view.GameObject);
            (view as IDisposable)?.Dispose();
        }

        #endregion
    }
}