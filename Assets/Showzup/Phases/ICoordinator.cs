using UniRx;

namespace Silphid.Showzup
{
    public interface ICoordinator
    {
        IObservable<Phase> Coordinate(IRequest request);
    }
}