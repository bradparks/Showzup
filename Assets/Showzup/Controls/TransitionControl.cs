using System;
using Silphid.Sequencit;
using UniRx;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class TransitionControl : PresenterControlBase
    {
        #region Fields

        private GameObject _sourceContainer;
        private GameObject _targetContainer;
        private IView _sourceView;
        private IView _targetView;

        #endregion

        #region Properties

        public GameObject Container1;
        public GameObject Container2;
        public Transition DefaultTransition;

        #endregion

        #region Injected properties

        [Inject(Optional = true)] internal ITransitionResolver TransitionResolver { get; set; }

        #endregion

        #region Life-time

        internal void Start()
        {
            Container1.SetActive(false);
            Container2.SetActive(false);

            if (HistoryContainer)
                HistoryContainer.SetActive(false);
        }

        #endregion

        #region Virtual and abstract overrides

        protected override IObservable<Unit> Present(IView view, Options options)
        {
            var transition = ResolveTransition();
            var duration = ResolveDuration(transition, options);
            return PerformTransition(transition, duration, options);
        }

        protected override void OnViewReady(IView view)
        {
            base.OnViewReady(view);

            _sourceView = View.Value;
            _targetView = view;
        }

        #endregion

        #region Implementation

        protected Transition ResolveTransition() => TransitionResolver?.Resolve(_sourceView, _targetView) ?? DefaultTransition;

        protected float ResolveDuration(Transition transition, Options options) => options?.Duration ?? transition.Duration;

        protected IObservable<Unit> PerformTransition(Transition transition, float duration, Options options)
        {
            PrepareContainers(_targetView, transition, options.GetDirection());

            return Sequence.Create(seq =>
                {
                    transition.Perform(_sourceContainer, _targetContainer, options.GetDirection(),
                            duration)
                        .In(seq);

                    seq.AddAction(() => CompleteTransition(transition));
                })
                .DoOnError(ex => Debug.LogException(
                    new Exception($"Failed to transition from {_sourceView} to {_targetView}.", ex)));
        }

        private void PrepareContainers(IView targetView, Transition transition, Direction direction)
        {
            // Lazily initialize containers
            _sourceContainer = _sourceContainer ?? Container1;
            _targetContainer = _targetContainer ?? Container2;

            // Swap containers
            var temp = _targetContainer;
            _targetContainer = _sourceContainer;
            _sourceContainer = temp;

            transition.Prepare(_sourceContainer, _targetContainer, direction);
            ReplaceView(_targetContainer, targetView);

            _sourceContainer.SetActive(true);
            _targetContainer.SetActive(true);
            if (targetView != null)
                targetView.IsActive = true;
        }

        protected virtual void CompleteTransition(Transition transition)
        {
            if (_sourceView != null)
                _sourceView.IsActive = false;
            View.Value = _targetView;
            RemoveAllViews(_sourceContainer);
            _sourceContainer.SetActive(false);
            transition.Complete(_sourceContainer, _targetContainer);
        }

        #endregion
    }
}