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
        private readonly Subject<Phase> _prePresentationPhaseSubject = new Subject<Phase>();
        private readonly Subject<Phase> _deconstructionPhaseSubject = new Subject<Phase>();
        private readonly Subject<Phase> _loadPhaseSubject = new Subject<Phase>();
        private readonly Subject<Phase> _preTransitionPhaseSubject = new Subject<Phase>();
        private readonly Subject<Phase> _transitionPhaseSubject = new Subject<Phase>();
        private readonly Subject<Phase> _postTransitionPhaseSubject = new Subject<Phase>();
        private readonly Subject<Phase> _unloadPhaseSubject = new Subject<Phase>();
        private readonly Subject<Phase> _constructionPhaseSubject = new Subject<Phase>();
        private readonly Subject<Phase> _postPresentationPhaseSubject = new Subject<Phase>();

        #endregion

        #region Properties

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
        public IObservable<Phase> PrePresentationPhase => _prePresentationPhaseSubject;
        public IObservable<Phase> DeconstructionPhase => _deconstructionPhaseSubject;
        public IObservable<Phase> LoadPhase => _loadPhaseSubject;
        public IObservable<Phase> PreTransitionPhase => _preTransitionPhaseSubject;
        public IObservable<Phase> TransitionPhase => _transitionPhaseSubject;
        public IObservable<Phase> PostTransitionPhase => _postTransitionPhaseSubject;
        public IObservable<Phase> UnloadPhase => _unloadPhaseSubject;
        public IObservable<Phase> ConstructionPhase => _constructionPhaseSubject;
        public IObservable<Phase> PostPresentationPhase => _postPresentationPhaseSubject;

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
            return LoadView(input, options)
                .ContinueWith(view =>
                {
                    var transition = ResolveTransition();
                    var duration = ResolveDuration(transition, options);
                    return PerformTransition(input, transition, duration, options)
                        .ThenReturn(view);
                });
        }

        #endregion

        #region Implementation

        protected IObservable<IView> LoadView(object input, Options options) =>
            ViewLoader
                .Load(input, options.WithExtraVariants(Variants))
                .Do(OnViewReady);

        protected void OnViewReady(IView view)
        {
            Debug.Log($"#Transition# View ready: {view}");
            _sourceView = _view.Value;
            _targetView = view;
        }

        protected Transition ResolveTransition() => TransitionResolver?.Resolve(_sourceView, _targetView) ?? DefaultTransition;

        protected float ResolveDuration(Transition transition, Options options) => options?.Duration ?? transition.Duration;

        protected IObservable<Unit> PerformTransition(object input, Transition transition, float duration, Options options)
        {
            return Sequence.Create(seq =>
            {
                Phase(_prePresentationPhaseSubject, input, options, _sourceView, _targetView).In(seq);

                Deconstruct(_sourceView, seq);

                PrepareContainers(_targetView, transition, options.GetDirection());
                transition.Perform(_sourceContainer, _targetContainer, options.GetDirection(),
                    duration).In(seq);

                Construct(_targetView, seq);
                seq.AddAction(() => CompleteTransition(transition));

                Phase(_postPresentationPhaseSubject, input, options, _sourceView, _targetView).In(seq);
            });
        }

        [Pure]
        private IObservable<Unit> Phase(Subject<Phase> subject, object input, Options options, IView source, IView target) =>
            Parallel.Create(parallel =>
                subject.OnNext(new Phase
                {
                    Input = input,
                    Options = options,
                    Source = source,
                    Target = target,
                    Parallel = parallel
                }));

        private void Deconstruct(IView view, ISequenceable seq)
        {
            var deconstructable = view as IDeconstructable;
            if (deconstructable != null)
                seq.Add(() => deconstructable.Deconstruct());
        }

        private void Construct(IView view, ISequenceable seq)
        {
            var constructable = view as IConstructable;
            if (constructable != null)
                seq.Add(() => constructable.Construct());
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
            _view.Value = _targetView;
            RemoveAllViews(_sourceContainer);
            _sourceContainer.SetActive(false);
            transition.Complete(_sourceContainer, _targetContainer);
        }

        #endregion
    }
}