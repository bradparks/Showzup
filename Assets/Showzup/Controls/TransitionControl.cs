using System;
using Silphid.Extensions;
using Silphid.Sequencit;
using UniRx;
using Unity.Linq;
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
                .Where(view => CheckCancellation(view, options))
                .Do(OnViewReady);

        private bool CheckCancellation(IView view, Options options)
        {
            if (!(options?.CancellationToken.IsCancellationRequested ?? false))
                return true;

            view.GameObject.Destroy();
            return false;
        }

        protected void OnViewReady(IView view)
        {
//            Debug.Log($"#Transition# View ready: {view}");
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
                    PreHide(_sourceView, options, seq);
                    Deconstruct(_sourceView, options, seq);
                    PreShow(_targetView, options, seq);

                    transition.Perform(_sourceContainer, _targetContainer, options.GetDirection(),
                            duration)
                        .In(seq);

                    PostHide(_sourceView, options, seq);
                    Construct(_targetView, options, seq);
                    PostShow(_targetView, options, seq);
                    seq.AddAction(() => CompleteTransition(transition));
                })
                .DoOnError(ex => Debug.LogException(
                    new Exception($"Failed to transition from {_sourceView} to {_targetView}.", ex)));
        }

        private void PreHide(IView view, Options options, ISequenceable seq)
        {
            var preHide = view as IPreHide;
            if (preHide != null)
                seq.AddAction(() => preHide.OnPreHide(options));
        }

        private void PreShow(IView view, Options options, ISequenceable seq)
        {
            var preShow = view as IPreShow;
            if (preShow != null)
                seq.AddAction(() => preShow.OnPreShow(options));
        }

        private void Deconstruct(IView view, Options options, ISequenceable seq)
        {
            var deconstructable = view as IDeconstructable;
            if (deconstructable != null)
                seq.Add(() => deconstructable.Deconstruct(options));
        }

        private void Construct(IView view, Options options, ISequenceable seq)
        {
            var constructable = view as IConstructable;
            if (constructable != null)
                seq.Add(() => constructable.Construct(options));
        }

        private void PostShow(IView view, Options options, ISequenceable seq)
        {
            var postShow = view as IPostShow;
            if (postShow != null)
                seq.AddAction(() => postShow.OnPostShow(options));
        }

        private void PostHide(IView view, Options options, ISequenceable seq)
        {
            var postHide = view as IPostHide;
            if (postHide != null)
                seq.AddAction(() => postHide.OnPostHide(options));
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