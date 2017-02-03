using System;
using JetBrains.Annotations;
using Silphid.Extensions;
using Silphid.Sequencit;
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
        private readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();

        private readonly ReactiveProperty<bool> _isPresenting = new ReactiveProperty<bool>();
        private readonly ReactiveProperty<bool> _isLoading = new ReactiveProperty<bool>();
        private readonly Subject<Presentation> _presentationStartingSubject = new Subject<Presentation>();
        private readonly Subject<Phase> _phaseStartingSubject = new Subject<Phase>();
        private readonly Subject<CompletedPhase> _phaseCompletedSubject = new Subject<CompletedPhase>();
        private readonly Subject<Presentation> _presentationCompletedSubject = new Subject<Presentation>();

        #endregion

        #region Properties

        [Inject] internal IViewResolver ViewResolver { get; set; }
        [Inject] internal IViewLoader ViewLoader { get; set; }
        [Inject] internal IPhaseCoordinator PhaseCoordinator { get; set; }
        [Inject(Optional = true)] internal ITransitionResolver TransitionResolver { get; set; }

        public GameObject Container1;
        public GameObject Container2;
        public Transition DefaultTransition;
        public string[] Variants;

        #endregion

        #region IPhasedPresenter members

        public ReadOnlyReactiveProperty<bool> IsPresenting { get; }
        public ReadOnlyReactiveProperty<bool> IsLoading { get; }
        public IObservable<Presentation> PresentStarting => _presentationStartingSubject;
        public IObservable<Phase> PhaseStarting => _phaseStartingSubject;
        public IObservable<CompletedPhase> PhaseCompleted => _phaseCompletedSubject;
        public IObservable<Presentation> PresentCompleted => _presentationCompletedSubject;

        #endregion

        #region Life-time

        public TransitionControl()
        {
            IsPresenting = _isPresenting.ToReadOnlyReactiveProperty();
            IsLoading = _isLoading.ToReadOnlyReactiveProperty();
        }

        internal void Start()
        {
            Container1.SetActive(false);
            Container2.SetActive(false);

            if (HistoryContainer)
                HistoryContainer.SetActive(false);
        }

        #endregion

        #region IPresenter members

        public virtual IObservable<IView> Present(object input, Options options = null)
        {
            var sourceView = _view.Value;
            IView targetView = null;

            var viewInfo = ResolveView(input, options);
            var presentation = CreatePresentation(viewInfo, options);
            var transition = ResolveTransition(sourceView?.GetType(), viewInfo.ViewType);
            var duration = ResolveDuration(transition, options);

            var phaseProvider = new PhaseProvider(
                () => PerformDeconstructionPhase(presentation, sourceView),
                () => PerformLoadPhase(presentation, viewInfo)
                    .Do(view => targetView = view)
                    .AsSingleUnitObservable(),
                () => PerformTransitionPhase(presentation, sourceView, targetView, input, transition, duration, options),
                () => PerformConstructionPhase(presentation, targetView),
                (phaseId, func) => () => PerformCustomPhase(presentation, phaseId, func));

            return Sequence
                .Create(seq =>
                {
                    seq.AddAction(() => OnPresentationStarting(presentation));
                    seq.Add(() => PhaseCoordinator.Coordinate(phaseProvider));
                    seq.AddAction(() => OnPresentationCompleted(presentation));
                })
                .ThenReturn(targetView);
        }

        #endregion

        #region Virtual members

        protected virtual void OnPresentationStarting(Presentation presentation)
        {
            _presentationStartingSubject.OnNext(presentation);
        }

        protected virtual IObservable<Unit> PerformDeconstructionPhase(Presentation presentation, IView view)
        {
            var deconstructable = view as IDeconstructable;
            return PerformPhase(presentation, PhaseId.Deconstruction, null,
                deconstructable != null
                    ? () => deconstructable.Deconstruct()
                    : (Func<IObservable<Unit>>) null);
        }

        [Pure]
        protected virtual IObservable<IView> PerformLoadPhase(Presentation presentation, ViewInfo viewInfo)
        {
            IView view = null;
            return PerformPhase(presentation, PhaseId.Load, null, () =>
                    ViewLoader.Load(viewInfo)
                        .Do(x => view = x)
                        .AsUnitObservable())
                .ThenReturn(view);
        }

        [Pure]
        protected virtual IObservable<Unit> PerformTransitionPhase(Presentation presentation, IView sourceView, IView targetView, object input, Transition transition, float duration, Options options) =>
            PerformPhase(presentation, PhaseId.Transition, duration, () =>
            {
                PrepareContainers(transition, targetView, options.GetDirection());
                return transition
                    .Perform(_sourceContainer, _targetContainer, options.GetDirection(), duration)
                    .DoOnCompleted(() => CompleteTransition(transition, sourceView, targetView));
            });

        protected virtual IObservable<Unit> PerformConstructionPhase(Presentation presentation, IView view)
        {
            var constructable = view as IConstructable;
            return PerformPhase(presentation, PhaseId.Construction, null,
                constructable != null
                    ? () => constructable.Construct()
                    : (Func<IObservable<Unit>>) null);
        }

        protected virtual IObservable<Unit> PerformCustomPhase(Presentation presentation, PhaseId phaseId, Func<IObservable<Unit>> func) =>
            PerformPhase(presentation, phaseId, null, func);

        protected virtual void OnPresentationCompleted(Presentation presentation)
        {
            _presentationCompletedSubject.OnNext(presentation);
        }

        protected virtual ISequenceable PerformPhase(Presentation presentation, PhaseId phaseId, float? duration, Func<IObservable<Unit>> phaseOperation = null) =>
            Sequence.Create(seq =>
            {
                seq.AddParallel(parallel =>
                {
                    if (phaseOperation != null)
                        parallel.Add(phaseOperation);

                    _phaseStartingSubject.OnNext(
                        new Phase(presentation, phaseId, duration, parallel));
                });

                seq.AddAction(() => CompletePhase(presentation, phaseId));
            });

        protected virtual void CompletePhase(Presentation presentation, PhaseId phaseId) =>
            _phaseCompletedSubject.OnNext(new CompletedPhase(presentation, phaseId));

        #endregion

        #region Implementation

        protected Presentation CreatePresentation(ViewInfo viewInfo, Options options) =>
            new Presentation
            {
                ViewModel = viewInfo.ViewModel,
                SourceViewType = _view.Value?.GetType(),
                TargetViewType = viewInfo.ViewType,
                Options = options
            };

        protected ViewInfo ResolveView(object input, Options options) =>
            ViewResolver.Resolve(input, options.WithExtraVariants(Variants));

        protected Transition ResolveTransition(Type sourceViewType, Type targetViewType) =>
            TransitionResolver?.Resolve(sourceViewType, targetViewType) ?? DefaultTransition;

        protected float ResolveDuration(Transition transition, Options options) =>
            options?.TransitionDuration ?? transition.Duration;

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
            _view.Value = targetView;
            RemoveAllViews(_sourceContainer);
            _sourceContainer.SetActive(false);
            transition.Complete(_sourceContainer, _targetContainer);
        }

        #endregion
    }
}