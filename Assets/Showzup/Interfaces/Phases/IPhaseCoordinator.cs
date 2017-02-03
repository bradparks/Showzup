using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface IPhaseCoordinator
    {
        [Pure] IObservable<Unit> Coordinate(IPhaseProvider provider);
    }
}