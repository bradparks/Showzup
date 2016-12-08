﻿using Silphid.Extensions;
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
        private Transition _currentTransition;
        private readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();

        #endregion

        #region Properties

        [Inject] internal IViewLoader ViewLoader { get; set; }
        [Inject(Optional = true)] internal ITransitionResolver TransitionResolver { get; set; }

        public float Duration = 0.4f;
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
                .ContinueWith(view => PerformTransition(options)
                    .ThenReturn(view));
        }

        #endregion

        #region Implementation

        protected IObservable<IView> LoadView(object input, Options options)
        {
            return ViewLoader
                .Load(input, options.WithExtraVariants(Variants))
                .Do(OnViewReady);
        }

        protected void OnViewReady(IView view)
        {
            Debug.Log($"#Transition# View ready: {view}");
            _sourceView = _view.Value;
            _targetView = view;
        }

        protected IObservable<Unit> PerformTransition(Options options)
        {
            // Resolve transition
            _currentTransition = TransitionResolver?.Resolve(_sourceView, _targetView) ?? DefaultTransition;

            PrepareContainers(_targetView, options.GetDirection());

            return Sequence.Create(seq =>
            {
                (_sourceView as IShowable)?.Hide().In(seq);

                _currentTransition.Perform(_sourceContainer, _targetContainer, options.GetDirection(),
                    options?.Duration ?? Duration).In(seq);

                (_targetView as IShowable)?.Show().In(seq);
                seq.AddAction(CompleteTransition);
            });
        }

        private void PrepareContainers(IView targetView, Direction direction)
        {
            // Lazily initialize containers
            _sourceContainer = _sourceContainer ?? Container1;
            _targetContainer = _targetContainer ?? Container2;

            // Swap containers
            var temp = _targetContainer;
            _targetContainer = _sourceContainer;
            _sourceContainer = temp;

            _sourceContainer.SetActive(true);
            _targetContainer.SetActive(true);

            _currentTransition.Prepare(_sourceContainer, _targetContainer, direction);
            ReplaceView(_targetContainer, targetView);
        }

        protected virtual void CompleteTransition()
        {
            _view.Value = _targetView;
            RemoveAllViews(_sourceContainer);
            _sourceContainer.SetActive(false);
            _currentTransition.Complete(_sourceContainer, _targetContainer);
        }

        #endregion
    }
}