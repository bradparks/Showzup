using Silphid.Extensions;
using Silphid.Sequencit;
using UniRx;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class TransitionControl : Control, IPresenter
    {
        #region Fields

        private GameObject _sourceContainer;
        private GameObject _targetContainer;
        private IView _sourceView;
        private IView _targetView;
        private readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();

        #endregion

        #region Properties

        [Inject] internal IViewLoader ViewLoader { get; set; }
        [Inject(Optional = true)] internal ITransitionResolver TransitionResolver { get; set; }

        public GameObject Container1;
        public GameObject Container2;
        public Transition DefaultTransition;
        public string[] Variants;

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

        public virtual IObservable<IView> Present(object input, Options options = null)
        {
            return LoadView(input, options)
                .ContinueWith(view =>
                {
                    var transition = ResolveTransition();
                    var duration = ResolveDuration(transition, options);
                    return PerformTransition(transition, duration, options)
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

        protected IObservable<Unit> PerformTransition(Transition transition, float duration, Options options)
        {
            PrepareContainers(_targetView, transition, options.GetDirection());

            return Sequence.Create(seq =>
            {
                (_sourceView as IShowable)?.Hide().In(seq);

                transition.Perform(_sourceContainer, _targetContainer, options.GetDirection(),
                    duration).In(seq);

                (_targetView as IShowable)?.Show().In(seq);
                seq.AddAction(() => CompleteTransition(transition));
            });
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