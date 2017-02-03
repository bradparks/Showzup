using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Silphid.Extensions;
using Silphid.Sequencit;
using UniRx;
using Zenject;

namespace Silphid.Showzup
{
    public class PhasedPresenterImpl
    {
        #region Fields

        private readonly Subject<Presentation> _presentationStartingSubject = new Subject<Presentation>();
        private readonly Subject<Phase> _phaseStartingSubject = new Subject<Phase>();
        private readonly Subject<CompletedPhase> _phaseCompletedSubject = new Subject<CompletedPhase>();
        private readonly Subject<Presentation> _presentationCompletedSubject = new Subject<Presentation>();

        #endregion

        [Inject] internal IViewResolver ViewResolver { get; set; }
        [Inject] internal IViewLoader ViewLoader { get; set; }
        [Inject] internal IPhaseCoordinator PhaseCoordinator { get; set; }
        [Inject(Optional = true)] internal ITransitionResolver TransitionResolver { get; set; }

        public IObservable<Presentation> PresentationStarting => _presentationStartingSubject;
        public IObservable<Phase> PhaseStarting => _phaseStartingSubject;
        public IObservable<CompletedPhase> PhaseCompleted => _phaseCompletedSubject;
        public IObservable<Presentation> PresentationCompleted => _presentationCompletedSubject;

        public IObservable<IView> Present(object input, IView sourceView, IList<string> presenterVariants,
            Transition defaultTransition, Options options, Func<IView, IView, Transition, float, Options, IObservable<Unit>> transitionOperation)
        {
            IView targetView = null;

            var viewInfo = ResolveView(input, presenterVariants, options);
            var presentation = CreatePresentation(viewInfo, sourceView, options);
            var transition = ResolveTransition(sourceView?.GetType(), viewInfo.ViewType, defaultTransition);
            var duration = ResolveDuration(transition, options);

            var phaseProvider = new PhaseProvider(
                () => PerformDeconstructionPhase(presentation, sourceView),
                () => PerformLoadPhase(presentation, viewInfo)
                    .Do(view => targetView = view)
                    .AsSingleUnitObservable(),
                () => PerformTransitionPhase(presentation, sourceView, targetView, input, transition, duration, options,
                    transitionOperation),
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
        protected virtual IObservable<Unit> PerformTransitionPhase(Presentation presentation, IView sourceView,
            IView targetView, object input, Transition transition, float duration, Options options,
            Func<IView, IView, Transition, float, Options, IObservable<Unit>> phaseOperation) =>
            PerformPhase(presentation, PhaseId.Transition, duration,
                () => phaseOperation(sourceView, targetView, transition, duration, options));

        protected virtual IObservable<Unit> PerformConstructionPhase(Presentation presentation, IView view)
        {
            var constructable = view as IConstructable;
            return PerformPhase(presentation, PhaseId.Construction, null,
                constructable != null
                    ? () => constructable.Construct()
                    : (Func<IObservable<Unit>>) null);
        }

        protected virtual IObservable<Unit> PerformCustomPhase(Presentation presentation, PhaseId phaseId,
            Func<IObservable<Unit>> func) =>
            PerformPhase(presentation, phaseId, null, func);

        protected virtual void OnPresentationCompleted(Presentation presentation)
        {
            _presentationCompletedSubject.OnNext(presentation);
        }

        protected virtual ISequenceable PerformPhase(Presentation presentation, PhaseId phaseId, float? duration,
            Func<IObservable<Unit>> phaseOperation = null) =>
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

        protected Presentation CreatePresentation(ViewInfo viewInfo, object sourceView, Options options) =>
            new Presentation
            {
                ViewModel = viewInfo.ViewModel,
                SourceViewType = sourceView?.GetType(),
                TargetViewType = viewInfo.ViewType,
                Options = options
            };

        protected ViewInfo ResolveView(object input, IList<string> presenterVariants, Options options) =>
            ViewResolver.Resolve(input, options.WithExtraVariants(presenterVariants));

        protected Transition ResolveTransition(Type sourceViewType, Type targetViewType,
            Transition defaultTransition) =>
            TransitionResolver?.Resolve(sourceViewType, targetViewType) ?? defaultTransition;

        protected float ResolveDuration(Transition transition, Options options) =>
            options?.TransitionDuration ?? transition.Duration;

        #endregion
    }
}