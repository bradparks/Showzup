using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Sequencit;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class NavigationControl : TransitionControl, INavigationPresenter, IDisposable
    {
        #region Fields

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();
        private readonly ReactiveProperty<bool> _isNavigating = new ReactiveProperty<bool>(false);
        private readonly ReactiveProperty<bool> _isLoading = new ReactiveProperty<bool>(false);
        private readonly Subject<Nav> _navigating = new Subject<Nav>();
        private readonly Subject<Nav> _navigated = new Subject<Nav>();
        private ReadOnlyReactiveProperty<bool> _canPush;
        private ReadOnlyReactiveProperty<bool> _canPop;

        #endregion

        #region Life-time

        [Inject]
        public void Inject()
        {
            IsNavigating = _isNavigating.ToReadOnlyReactiveProperty();
            IsLoading = _isLoading.ToReadOnlyReactiveProperty();
            History.PairWithPrevious().Skip(1).Subscribe(DisposeDroppedViews).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        #endregion

        #region INavigationPresenter members

        public ReadOnlyReactiveProperty<bool> IsNavigating { get; private set; }
        public ReadOnlyReactiveProperty<bool> IsLoading { get; private set; }

        public ReadOnlyReactiveProperty<bool> CanPresent =>
            _canPush ?? (_canPush = IsNavigating.Negate().ToReadOnlyReactiveProperty());

        public ReadOnlyReactiveProperty<bool> CanPop =>
            _canPop ?? (_canPop = History
                .Select(x => x.Count >= 1)
                .DistinctUntilChanged()
                .CombineLatest(IsNavigating.Negate(), (x, y) => x && y)
                .ToReadOnlyReactiveProperty());

        public ReadOnlyReactiveProperty<IView> View => _view.ToReadOnlyReactiveProperty();

        public ReactiveProperty<List<IView>> History { get; } =
            new ReactiveProperty<List<IView>>(new List<IView>());

        public IObservable<Nav> Navigating => _navigating;
        public IObservable<Nav> Navigated => _navigated;


        public override IObservable<IView> Present(object input, Options options = null)
        {
            Debug.Log($"#Nav# Present({input}, {options})");
            AssertCanPresent();

            StartChange();

            _isLoading.Value = true;
            return LoadView(input, options)
                .Do(_ => _isLoading.Value = false)
                .ContinueWith(view =>
                {
                    var transition = ResolveTransition();
                    var duration = ResolveDuration(transition, options);

                    var nav = StartNavigation(view, transition, duration);

                    return Observable
                        .WhenAll(
                            PerformTransition(transition, duration, options),
                            nav.Parallel)
                        .DoOnCompleted(() =>
                        {
                            History.Value = GetNewHistory(view, options.GetPushMode());
                            CompleteNavigation(nav);
                            CompleteChange();
                        })
                        .ThenReturn(view);
                });
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

        private IObservable<IView> PopInternal(IView view, List<IView> history)
        {
            AssertCanPresent();

            StartChange();

            OnViewReady(view);

            var options = new Options { Direction = Direction.Backward };
            var transition = ResolveTransition();
            var duration = ResolveDuration(transition, options);
            var nav = StartNavigation(view, transition, duration);

            return Observable
                .WhenAll(
                    PerformTransition(transition, duration, options),
                    nav.Parallel)
                .DoOnCompleted(() =>
                {
                    History.Value = history;
                    CompleteNavigation(nav);
                    CompleteChange();
                })
                .ThenReturn(view);
        }

        #endregion

        #region Implementation

        private void StartChange()
        {
            _isNavigating.Value = true;
        }

        private Nav StartNavigation(IView targetView, Transition transition, float duration)
        {
            var nav = new Nav(View.Value, targetView, new Parallel(), transition, duration);
            _navigating.OnNext(nav);
            _view.Value = null;
            return nav;
        }

        private void CompleteNavigation(Nav nav)
        {
            _view.Value = nav.Target;
            _navigated.OnNext(nav);
        }

        private void CompleteChange()
        {
            _isNavigating.Value = false;
            //Debug.Log("#Nav# Navigation complete");
        }

        private void AssertCanPresent()
        {
            if (!CanPresent.Value)
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
            Destroy(view.GameObject);
            (view as IDisposable)?.Dispose();
        }

        #endregion
    }
}