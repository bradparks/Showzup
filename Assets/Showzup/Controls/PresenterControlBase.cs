using Silphid.Extensions;
using Silphid.Sequencit;
using UniRx;
using Zenject;

namespace Silphid.Showzup
{
    public abstract class PresenterControlBase : Control, IPresenter
    {
        #region State enum

        protected enum State
        {
            Ready,
            Loading,
            Presenting
        }

        #endregion

        #region PendingRequest inner-class

        private class PendingRequest
        {
            public readonly object Input;
            public readonly Options Options;
            public readonly Subject<IView> Subject = new Subject<IView>();

            public PendingRequest(object input, Options options)
            {
                Input = input;
                Options = options;
            }
        }

        #endregion

        #region Injected properties

        [Inject] internal IViewLoader ViewLoader { get; set; }

        #endregion

        #region Config properties

        public string[] Variants;

        #endregion

        #region Private fields

        private readonly Subject<Unit> _loadCancellations = new Subject<Unit>();
        private State _state;
        private PendingRequest _pendingRequest;

        protected readonly ReactiveProperty<IView> _view = new ReactiveProperty<IView>();
        public ReadOnlyReactiveProperty<IView> View { get; }

        #endregion

        protected PresenterControlBase()
        {
            View = _view.ToReadOnlyReactiveProperty();
        }

        #region IPresenter members

        public virtual IObservable<IView> Present(object input, Options options = null)
        {
            if (_state == State.Ready)
                return PresentNow(input, options);

            if (_state == State.Loading)
            {
                CancelLoading();
                return PresentNow(input, options);
            }

            // State.Presenting
            return PresentLater(input, options);
        }

        #endregion

        #region Implementation

        private IObservable<IView> PresentNow(object input, Options options)
        {
            _state = State.Loading;
            return Observable
                .Defer(() => LoadView(input, options))
                .DoOnError(_ => _state = State.Ready)
                .ContinueWith(view =>
                {
                    _state = State.Presenting;
                    return Present(view, options)
                        .ThenReturn(view);
                })
                .DoOnCompleted(CompleteRequest);
        }

        private IObservable<IView> PresentLater(object input, Options options)
        {
            // Complete any pending request without fulling it (we only allow a single pending request)
            _pendingRequest?.Subject.OnCompleted();

            // Prepare new pending request
            _pendingRequest = new PendingRequest(input, options);
            return _pendingRequest.Subject;
        }

        private void CancelLoading()
        {
            _state = State.Ready;
            _loadCancellations.OnNext(Unit.Default);
        }

        private void CompleteRequest()
        {
            _state = State.Ready;

            if (_pendingRequest != null)
            {
                PresentNow(_pendingRequest.Input, _pendingRequest.Options)
                    .SubscribeAndForget(_pendingRequest.Subject);

                _pendingRequest = null;
            }
        }

        protected IObservable<IView> LoadView(object input, Options options)
        {
            var cancellationDisposable = new BooleanDisposable();
            var cancellationToken = new CancellationToken(cancellationDisposable);
            var cancellations = _loadCancellations.Do(_ => cancellationDisposable.Dispose());

            return ViewLoader
                .Load(input, cancellationToken, options.WithExtraVariants(Variants))
                .TakeUntil(cancellations)
                .Do(OnViewReady);
        }

        #endregion

        #region Virtual and abstract members

        protected abstract IObservable<Unit> Present(IView view, Options options);

        protected virtual void OnViewReady(IView view)
        {
        }

        #endregion
    }
}