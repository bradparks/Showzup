using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Silphid.Extensions;
using Silphid.Sequencit;
using UniRx;

namespace Silphid.Showzup
{
    public class PhasedPresenterImpl
    {
        #region Fields

        private readonly IViewResolver _viewResolver;
        private readonly IViewLoader _viewLoader;
        private readonly IPhaseCoordinator _phaseCoordinator;
        private readonly ITransitionResolver _transitionResolver;
        private readonly Subject<IPresentation> _presentationStartingSubject = new Subject<IPresentation>();
        private readonly Subject<IPhase> _phaseStartingSubject = new Subject<IPhase>();
        private readonly Subject<CompletedPhase> _phaseCompletedSubject = new Subject<CompletedPhase>();
        private readonly Subject<IPresentation> _presentationCompletedSubject = new Subject<IPresentation>();

        #endregion

        public IObservable<IPresentation> PresentationStarting => _presentationStartingSubject;
        public IObservable<IPhase> PhaseStarting => _phaseStartingSubject;
        public IObservable<CompletedPhase> PhaseCompleted => _phaseCompletedSubject;
        public IObservable<IPresentation> PresentationCompleted => _presentationCompletedSubject;

        public PhasedPresenterImpl(IViewResolver viewResolver, IViewLoader viewLoader, IPhaseCoordinator phaseCoordinator, ITransitionResolver transitionResolver = null)
        {
            _viewResolver = viewResolver;
            _viewLoader = viewLoader;
            _phaseCoordinator = phaseCoordinator;
            _transitionResolver = transitionResolver;
        }

        public IObservable<IView> Present(object input, IView sourceView, IList<string> presenterVariants,
            Transition defaultTransition, Options options, Func<Phase, IObservable<Unit>> transitionOperation)
        {
            var viewInfo = ResolveView(input, presenterVariants, options);
            var presentation = CreatePresentation(viewInfo, sourceView, options);
            var transition = ResolveTransition(sourceView?.GetType(), viewInfo.ViewType, defaultTransition);
            var duration = ResolveDuration(transition, options);

            var phaseProvider = new PhaseProvider(
                () => PerformDeconstructionPhase(presentation),
                () => PerformLoadPhase(presentation, viewInfo),
                () => PerformTransitionPhase(presentation, transition, duration, options, transitionOperation),
                () => PerformConstructionPhase(presentation),
                (createPhase, phaseOperation) => () => PerformPhase(createPhase, phaseOperation));

            return Sequence
                .Create(seq =>
                {
                    seq.AddAction(() => OnPresentationStarting(presentation));
                    seq.Add(() => _phaseCoordinator.Coordinate(presentation, phaseProvider));
                    seq.AddAction(() => OnPresentationCompleted(presentation));
                })
                .ThenReturn(presentation.TargetView);
        }

        #region Virtual members

        protected virtual void OnPresentationStarting(IPresentation presentation)
        {
            _presentationStartingSubject.OnNext(presentation);
        }

        protected virtual IObservable<Unit> PerformDeconstructionPhase(IPresentation presentation) =>
            PerformPhase(
                parallel => new DeconstructionPhase(presentation, parallel),
                phase => (phase.SourceView as IDeconstructable)?.Deconstruct() ?? Observable.ReturnUnit());

        [Pure]
        protected virtual IObservable<Unit> PerformLoadPhase(IPresentation presentation, ViewInfo viewInfo) =>
            PerformPhase(
                parallel => new LoadPhase(presentation, parallel),
                phase => _viewLoader.Load(viewInfo)
                    .Do(x => ((LoadPhase) phase).TargetView = x)
                    .AsUnitObservable());

        [Pure]
        protected virtual IObservable<Unit> PerformTransitionPhase(IPresentation presentation,
            Transition transition, float duration, Options options,
            Func<Phase, IObservable<Unit>> phaseOperation) =>
            PerformPhase(
                parallel => new TransitionPhase(presentation, parallel, transition, duration),
                phaseOperation);

        protected virtual IObservable<Unit> PerformConstructionPhase(IPresentation presentation) =>
            PerformPhase(
                parallel => new ConstructionPhase(presentation, parallel),
                phase => (phase.TargetView as IConstructable)?.Construct() ?? Observable.ReturnUnit());

        protected virtual ISequenceable PerformPhase(
            Func<Parallel, Phase> createPhase,
            Func<Phase, IObservable<Unit>> phaseOperation = null) =>
            Sequence.Create(seq =>
            {
                var phase = createPhase(Parallel.Create());

                if (phaseOperation != null)
                    phase.Parallel.Add(() => phaseOperation(phase));

                seq.AddAction(() => _phaseStartingSubject.OnNext(phase));
                seq.Add(phase.Parallel);
                seq.AddAction(() => _phaseCompletedSubject.OnNext(
                    new CompletedPhase(phase)));
            });

        protected virtual void OnPresentationCompleted(IPresentation presentation)
        {
            _presentationCompletedSubject.OnNext(presentation);
        }

        #endregion

        #region Implementation

        protected Presentation CreatePresentation(ViewInfo viewInfo, IView sourceView, Options options) =>
            new Presentation(viewInfo.ViewModel, sourceView, viewInfo.ViewType, options);

        protected ViewInfo ResolveView(object input, IList<string> presenterVariants, Options options) =>
            _viewResolver.Resolve(input, options.WithExtraVariants(presenterVariants));

        protected Transition ResolveTransition(Type sourceViewType, Type targetViewType,
            Transition defaultTransition) =>
            _transitionResolver?.Resolve(sourceViewType, targetViewType) ?? defaultTransition;

        protected float ResolveDuration(Transition transition, Options options) =>
            options?.TransitionDuration ?? transition.Duration;

        #endregion
    }
}