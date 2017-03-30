using System;
using Silphid.Sequencit;
using UniRx;
using Unity.Linq;
using UnityEngine;
using Zenject;

namespace Silphid.Showzup
{
    public class TransitionControl : PresenterControlBase
    {
        #region Fields

        private GameObject _sourceContainer;
        private GameObject _targetContainer;

        #endregion

        #region Properties

        public GameObject Container1;
        public GameObject Container2;
        public Transition DefaultTransition;

        #endregion

        #region Injected properties

        [Inject(Optional = true)]
        internal ITransitionResolver TransitionResolver { get; set; }

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

        #region Virtual and abstract overrides

        protected override IObservable<Unit> Present(Presentation presentation)
        {
            return PerformTransition(presentation);
        }

        #endregion

        #region Implementation

        protected override Presentation CreatePresentation(object viewModel, IView sourceView, Type targetViewType, Options options)
        {
            var presentation = base.CreatePresentation(viewModel, sourceView, targetViewType, options);
            presentation.Transition = ResolveTransition(presentation, DefaultTransition);
            presentation.TransitionDuration = ResolveDuration(presentation.Transition, options);
            return presentation;
        }

        protected Transition ResolveTransition(Presentation presentation, Transition defaultTransition) =>
            TransitionResolver?.Resolve(presentation) ?? defaultTransition;

        protected float ResolveDuration(Transition transition, Options options)
            => options?.TransitionDuration ?? transition.Duration;

        protected IObservable<Unit> PerformTransition(Presentation presentation)
        {
            var sourceView = presentation.SourceView;
            var targetView = presentation.TargetView;
            var options = presentation.Options;
            var transition = presentation.Transition;
            var duration = presentation.TransitionDuration;

            return Sequence.Create(seq =>
            {
                seq.AddAction(() => PrepareContainers(presentation));

                PreHide(sourceView, options, seq);
                Deconstruct(sourceView, options, seq);
                PreShow(targetView, options, seq);

                seq.Add(() => transition.Perform(_sourceContainer, _targetContainer, options.GetDirection(), duration));

                PostHide(sourceView, options, seq);
                Construct(targetView, options, seq);
                PostShow(targetView, options, seq);

                seq.AddAction(() => CompleteTransition(presentation));
            })
                .DoOnError(ex =>
                {
                    // ReSharper disable once MergeSequentialChecks
                    // ReSharper disable once UseNullPropagation
                    if (targetView != null && targetView.GameObject != null)
                        targetView.GameObject.Destroy();

                    Debug.LogException(
                        new Exception($"Failed to transition from {sourceView} to {targetView}.", ex));
                });
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

        private void PrepareContainers(Presentation presentation)
        {
            // Ensure parent has not been destroyed in the meantime
            if (Container1 == null)
                throw new TransitionParentDestroyedException();

            // Lazily initialize containers
            _sourceContainer = _sourceContainer ?? Container1;
            _targetContainer = _targetContainer ?? Container2;

            // Swap containers
            var temp = _targetContainer;
            _targetContainer = _sourceContainer;
            _sourceContainer = temp;

            var direction = presentation.Options.GetDirection();
            var targetView = presentation.TargetView;
            var transition = presentation.Transition;

            transition.Prepare(_sourceContainer, _targetContainer, direction);
            ReplaceView(_targetContainer, targetView);

            _sourceContainer.SetActive(true);
            _targetContainer.SetActive(true);
            if (targetView != null)
                targetView.IsActive = true;
        }

        protected virtual void CompleteTransition(Presentation presentation)
        {
            var sourceView = presentation.SourceView;
            var targetView = presentation.TargetView;
            var transition = presentation.Transition;

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