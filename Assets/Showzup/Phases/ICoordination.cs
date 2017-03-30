using System;
using UniRx;

namespace Silphid.Showzup
{
    public interface ICoordination : IDisposable
    {
        IDisposable Coordinate(IObserver<PhaseEvent> observer);
    }
}