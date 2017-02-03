using UniRx;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class TransitionControl : Control, IPhasedPresenter
    {
        #region Fields

        private GameObject _sourceContainer;
        private GameObject _targetContainer;
        private IView _view;

        #endregion

        #region Properties

        [Inject] protected PhasedPresenterImpl PhasedPresenter { get; set; }

        public GameObject Container1;
        public GameObject Container2;
        public Transition DefaultTransition;
        public string[] Variants;

        #endregion

        #region IPhasedPresenter members

        public IObservable<Presentation> PresentationStarting => PhasedPresenter.PresentationStarting;
        public IObservable<Phase> PhaseStarting => PhasedPresenter.PhaseStarting;
        public IObservable<CompletedPhase> PhaseCompleted => PhasedPresenter.PhaseCompleted;
        public IObservable<Presentation> PresentationCompleted => PhasedPresenter.PresentationCompleted;

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
            PhasedPresenter.Present(input, _view, Variants, DefaultTransition, options, PerformTransition);

        #endregion

        private IObservable<Unit> PerformTransition(IView sourceView, IView targetView, Transition transition,
            float duration, Options options)
        {
            PrepareContainers(transition, targetView, options.GetDirection());
            return transition
                .Perform(_sourceContainer, _targetContainer, options.GetDirection(), duration)
                .DoOnCompleted(() => CompleteTransition(transition, sourceView, targetView));
        }

        private void PrepareContainers(Transition transition, IView targetView, Direction direction)
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

        protected virtual void CompleteTransition(Transition transition, IView sourceView, IView targetView)
        {
            if (sourceView != null)
                sourceView.IsActive = false;
            _view = targetView;
            RemoveAllViews(_sourceContainer);
            _sourceContainer.SetActive(false);
            transition.Complete(_sourceContainer, _targetContainer);
        }
    }
}