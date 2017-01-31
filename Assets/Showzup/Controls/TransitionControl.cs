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
        private IView _sourceView;
        private IView _targetView;
        private readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();

        private readonly ReactiveProperty<bool> _isPresenting = new ReactiveProperty<bool>();
        private readonly ReactiveProperty<bool> _isLoading = new ReactiveProperty<bool>();
        private readonly Subject<Present> _presentStartingSubject = new Subject<Present>();
        private readonly Subject<Phase> _phaseStartingSubject = new Subject<Phase>();
        private readonly Subject<CompletedPhase> _phaseCompletedSubject = new Subject<CompletedPhase>();
        private readonly Subject<Present> _presentCompletedSubject = new Subject<Present>();

        #endregion

        #region Properties

        [Inject] internal IViewResolver ViewResolver { get; set; }
        [Inject] internal IViewLoader ViewLoader { get; set; }
        [Inject(Optional = true)] internal ITransitionResolver TransitionResolver { get; set; }

        public GameObject Container1;
        public GameObject Container2;
        public Transition DefaultTransition;
        public string[] Variants;

        #endregion

        #region IPhasedPresenter members

        public ReadOnlyReactiveProperty<bool> IsPresenting { get; }
        public ReadOnlyReactiveProperty<bool> IsLoading { get; }
        public IObservable<Present> PresentStarting => _presentStartingSubject;
        public IObservable<Phase> PhaseStarting => _phaseStartingSubject;
        public IObservable<CompletedPhase> PhaseCompleted => _phaseCompletedSubject;
        public IObservable<Present> PresentCompleted => _presentCompletedSubject;

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
            var viewInfo = ResolveView(input, options);
            var present = GetPresent(viewInfo, options);
            var transition = ResolveTransition();
            var duration = ResolveDuration(transition, options);
            var sourceView = _view.Value;
            IView targetView = null;

            return Sequence
                .Create(seq =>
                {
                    seq.AddAction(() => OnPresentStarting(present));
                    seq.Add(() => OnDeconstruction(present, sourceView));
                    seq.Add(() => OnLoadView(present, viewInfo).Do(view => targetView = view));
                    seq.Add(() => OnTransition(present, input, transition, duration, options));
                    seq.Add(() => OnConstruction(present, targetView));
                    seq.AddAction(() => OnPresentCompleted(present));
                })
                .ThenReturn(targetView);
        }

        #endregion

        #region Virtual members

        protected virtual void OnPresentStarting(Present present)
        {
            _presentStartingSubject.OnNext(present);
        }

        protected virtual IObservable<Unit> OnDeconstruction(Present present, IView view)
        {
            var deconstructable = view as IDeconstructable;
            return StartPhase(present, PhaseId.Deconstruction, null,
                deconstructable != null
                    ? () => deconstructable.Deconstruct()
                    : (Func<IObservable<Unit>>) null);
        }

        [Pure]
        protected virtual IObservable<IView> OnLoadView(Present present, ViewInfo viewInfo)
        {
            IView view = null;
            return StartPhase(present, PhaseId.Load, null, () =>
                    LoadView(viewInfo)
                        .Do(x => view = x)
                        .AsUnitObservable())
                .ThenReturn(view);
        }

        [Pure]
        protected virtual IObservable<Unit> OnTransition(Present present, object input, Transition transition, float duration, Options options) =>
            StartPhase(present, PhaseId.Transition, duration, () =>
            {
                PrepareContainers(_targetView, transition, options.GetDirection());
                return transition
                    .Perform(_sourceContainer, _targetContainer, options.GetDirection(), duration)
                    .DoOnCompleted(() => CompleteTransition(transition));
            });

        protected virtual IObservable<Unit> OnConstruction(Present present, IView view)
        {
            var constructable = view as IConstructable;
            return StartPhase(present, PhaseId.Construction, null,
                constructable != null
                    ? () => constructable.Construct()
                    : (Func<IObservable<Unit>>) null);
        }

        protected virtual void OnPresentCompleted(Present present)
        {
            _presentCompletedSubject.OnNext(present);
        }

        protected virtual ISequenceable StartPhase(Present present, PhaseId phaseId, float? duration, Func<IObservable<Unit>> func = null) =>
            Sequence.Create(seq =>
            {
                seq.AddParallel(parallel =>
                {
                    _phaseStartingSubject.OnNext(
                        new Phase(present, phaseId, duration, parallel));

                    if (func != null)
                        parallel.Add(func);
                });

                seq.AddAction(() =>
                    _phaseCompletedSubject.OnNext(
                        new CompletedPhase(present, phaseId)));
            });

        protected virtual void CompletePhase(Present present, PhaseId phaseId)
        {
            _phaseCompletedSubject.OnNext(
                new CompletedPhase(present, phaseId));
        }

        #endregion

        #region Implementation

        protected Present GetPresent(ViewInfo viewInfo, Options options) =>
            new Present
            {
                ViewModel = viewInfo.ViewModel,
                SourceViewType = _view.Value?.GetType(),
                TargetViewType = viewInfo.ViewType,
                Options = options
            };

        protected ViewInfo ResolveView(object input, Options options) =>
            ViewResolver
                .Resolve(input, options.WithExtraVariants(Variants));

        protected IObservable<IView> LoadView(ViewInfo viewInfo) =>
            ViewLoader
                .Load(viewInfo)
                .Do(OnViewReady);

        protected void OnViewReady(IView view)
        {
            Debug.Log($"#Transition# View ready: {view}");
            _sourceView = _view.Value;
            _targetView = view;
        }

        protected Transition ResolveTransition() =>
            TransitionResolver?.Resolve(_sourceView, _targetView)
            ?? DefaultTransition;

        protected float ResolveDuration(Transition transition, Options options) =>
            options?.TransitionDuration
            ?? transition.Duration;

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
            _view.Value = _targetView;
            RemoveAllViews(_sourceContainer);
            _sourceContainer.SetActive(false);
            transition.Complete(_sourceContainer, _targetContainer);
        }

        #endregion
    }
}