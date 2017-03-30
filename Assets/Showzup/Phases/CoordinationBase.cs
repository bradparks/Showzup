using System;
using UniRx;

namespace Silphid.Showzup
{
    public abstract class CoordinationBase : ICoordination
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private bool _used;

        protected readonly Presentation Presentation;
        protected IObserver<PhaseEvent> Observer { get; private set; }

        protected CoordinationBase(Presentation presentation)
        {
            Presentation = presentation;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        protected PhasePerformer CreatePerformer(PhaseId id)
        {
            var info = new PhasePerformer(new Phase(PhaseId.Present, Presentation), Observer);
            _disposables.Add(info);
            return info;
        }

        public IDisposable Coordinate(IObserver<PhaseEvent> observer)
        {
            if (_used)
                throw new InvalidOperationException("Coordinator can be used only once.");
            _used = true;

            Observer = observer;
            _disposables.Add(CoordinateInternal());
            return _disposables;
        }

        protected abstract IDisposable CoordinateInternal();
    }
}