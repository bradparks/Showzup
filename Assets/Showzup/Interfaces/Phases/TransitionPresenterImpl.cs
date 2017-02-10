using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Silphid.Extensions;
using Silphid.Sequencit;
using UniRx;

namespace Silphid.Showzup
{
    public class TransitionPresenterImpl
    {
        #region Fields

        private ReadOnlyReactiveProperty<bool> _isReady;
        private ReadOnlyReactiveProperty<bool> _isLoading;
        private readonly IViewResolver _viewResolver;
        private readonly IViewLoader _viewLoader;
        private readonly IPhaseCoordinator _phaseCoordinator;
        private readonly ITransitionResolver _transitionResolver;
        private readonly Subject<Presentation> _presentationStartingSubject = new Subject<Presentation>();
        private readonly Subject<Phase> _phaseStartingSubject = new Subject<Phase>();
        private readonly Subject<CompletedPhase> _phaseCompletedSubject = new Subject<CompletedPhase>();
        private readonly Subject<Presentation> _presentationCompletedSubject = new Subject<Presentation>();

        #endregion

        public ReadOnlyReactiveProperty<bool> IsReady =>
            _isReady ?? (_isReady =
                PresentationStarting
                    .Select(_ => false)
                    .Merge(PresentationCompleted
                        .Select(_ => true))
                    .ToReadOnlyReactiveProperty(true));

        public ReadOnlyReactiveProperty<bool> IsLoading =>
            _isLoading ?? (_isLoading =
                PhaseStarting
                    .OfPhase<LoadPhase>()
                    .Select(_ => true)
                    .Merge(PhaseCompleted.OfPhase<LoadPhase>().Select(_ => false))
                    .ToReadOnlyReactiveProperty(true));

        public IObservable<Presentation> PresentationStarting => _presentationStartingSubject;
        public IObservable<Phase> PhaseStarting => _phaseStartingSubject;
        public IObservable<CompletedPhase> PhaseCompleted => _phaseCompletedSubject;
        public IObservable<Presentation> PresentationCompleted => _presentationCompletedSubject;

        public TransitionPresenterImpl(IViewResolver viewResolver, IViewLoader viewLoader, IPhaseCoordinator phaseCoordinator, ITransitionResolver transitionResolver = null)
        {
            _viewResolver = viewResolver;
            _viewLoader = viewLoader;
            _phaseCoordinator = phaseCoordinator;
            _transitionResolver = transitionResolver;
        }

        public IObservable<IView> Present(object input, IView sourceView, IList<string> presenterVariants,
            Transition defaultTransition, Options options,
            Action<Presentation> prePresentationAction,
            Func<Phase, IObservable<Unit>> transitionOperation,
            Action<Presentation> postPresentationAction)
        {
            var viewInfo = ResolveView(input, presenterVariants, options);
            var presentation = CreatePresentation(viewInfo, sourceView, options);
            var transition = ResolveTransition(presentation, defaultTransition);
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
                    seq.AddAction(() =>
                    {
                        prePresentationAction?.Invoke(presentation);
                        OnPresentationStarting(presentation);
                    });
                    seq.Add(() => _phaseCoordinator.Coordinate(presentation, phaseProvider));
                    seq.AddAction(() =>
                    {
                        postPresentationAction?.Invoke(presentation);
                        OnPresentationCompleted(presentation);
                    });
                })
                .ThenReturn(presentation.TargetView);
        }

        #region Virtual members

        protected virtual void OnPresentationStarting(Presentation presentation)
        {
            _presentationStartingSubject.OnNext(presentation);
        }

        protected virtual IObservable<Unit> PerformDeconstructionPhase(Presentation presentation) =>
            PerformPhase(
                parallel => new DeconstructionPhase(presentation, parallel),
                phase => (phase.SourceView as IDeconstructable)?.Deconstruct() ?? Observable.ReturnUnit());

        [Pure]
        protected virtual IObservable<Unit> PerformLoadPhase(Presentation presentation, ViewInfo viewInfo) =>
            PerformPhase(
                parallel => new LoadPhase(presentation, parallel),
                phase => _viewLoader.Load(viewInfo)
                    .Do(x => ((LoadPhase) phase).TargetView = x)
                    .AsUnitObservable());

        [Pure]
        protected virtual IObservable<Unit> PerformTransitionPhase(Presentation presentation,
            Transition transition, float duration, Options options,
            Func<Phase, IObservable<Unit>> phaseOperation) =>
            PerformPhase(
                parallel => new TransitionPhase(presentation, parallel, transition, duration),
                phaseOperation);

        protected virtual IObservable<Unit> PerformConstructionPhase(Presentation presentation) =>
            PerformPhase(
                parallel => new ConstructionPhase(presentation, parallel),
                phase => (phase.TargetView as IConstructable)?.Construct() ?? Observable.ReturnUnit());

        protected virtual IObservable<Unit> PerformPhase(
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

        protected virtual void OnPresentationCompleted(Presentation presentation)
        {
            _presentationCompletedSubject.OnNext(presentation);
        }

        #endregion

        #region Implementation

        protected Presentation CreatePresentation(ViewInfo viewInfo, IView sourceView, Options options) =>
            new Presentation(viewInfo.ViewModel, sourceView, viewInfo.ViewType, options);

        protected ViewInfo ResolveView(object input, IList<string> presenterVariants, Options options) =>
            _viewResolver.Resolve(input, options.WithExtraVariants(presenterVariants));

        protected Transition ResolveTransition(Presentation presentation, Transition defaultTransition) =>
            _transitionResolver?.Resolve(presentation) ?? defaultTransition;

        protected float ResolveDuration(Transition transition, Options options) =>
            options?.TransitionDuration ?? transition.Duration;

        #endregion
    }
}