using UniRx;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class TransitionControl : Control, ITransitionPresenter
    {
        #region Fields

        private GameObject _sourceContainer;
        private GameObject _targetContainer;
        private IView _view;

        #endregion

        #region Properties

        [Inject] protected TransitionPresenterImpl TransitionPresenter { get; set; }

        public GameObject Container1;
        public GameObject Container2;
        public Transition DefaultTransition;
        public string[] Variants;

        #endregion

        #region IPhasedPresenter members

        public ReadOnlyReactiveProperty<bool> IsReady => TransitionPresenter.IsReady;
        public ReadOnlyReactiveProperty<bool> IsLoading => TransitionPresenter.IsLoading;
        public IObservable<Presentation> Presenting => TransitionPresenter.PresentationStarting;
        public IObservable<Phase> Phases => TransitionPresenter.PhaseStarting;
        public IObservable<CompletedPhase> CompletedPhases => TransitionPresenter.PhaseCompleted;
        public IObservable<Presentation> Presented => TransitionPresenter.PresentationCompleted;

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

        #region IPresenter members

        public virtual IObservable<IView> Present(object input, Options options = null) =>
            TransitionPresenter.Present(input, _view, Variants, DefaultTransition, options,
                null, phase => PerformTransition((TransitionPhase) phase), null);

        #endregion

        protected IObservable<Unit> PerformTransition(TransitionPhase phase)
        {
            PrepareContainers(phase);
            return phase.Transition
                .Perform(_sourceContainer, _targetContainer, phase.Options.GetDirection(), phase.Duration.Value)
                .DoOnCompleted(() => CompleteTransition(phase));
        }

        private void PrepareContainers(TransitionPhase phase)
        {
            // Lazily initialize containers
            _sourceContainer = _sourceContainer ?? Container1;
            _targetContainer = _targetContainer ?? Container2;

            // Swap containers
            var temp = _targetContainer;
            _targetContainer = _sourceContainer;
            _sourceContainer = temp;

            phase.Transition.Prepare(_sourceContainer, _targetContainer, phase.Options.GetDirection());
            ReplaceView(_targetContainer, phase.TargetView);

            _sourceContainer.SetActive(true);
            _targetContainer.SetActive(true);
            if (phase.TargetView != null)
                phase.TargetView.IsActive = true;
        }

        protected virtual void CompleteTransition(TransitionPhase phase)
        {
            if (phase.SourceView != null)
                phase.SourceView.IsActive = false;
            _view = phase.TargetView;
            RemoveAllViews(_sourceContainer);
            _sourceContainer.SetActive(false);
            phase.Transition.Complete(_sourceContainer, _targetContainer);
        }
    }
}