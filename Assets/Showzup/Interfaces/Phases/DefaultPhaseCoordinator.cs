using JetBrains.Annotations;
using Silphid.Sequencit;
using UniRx;

namespace Silphid.Showzup
{
    public class DefaultPhaseCoordinator : IPhaseCoordinator
    {
        [Pure]
        public IObservable<Unit> Coordinate(IPhaseProvider provider) =>
            Sequence
                .Create(seq =>
                {
                    seq.Add(provider.DeconstructionPhase);
                    seq.Add(provider.LoadPhase);
                    seq.Add(provider.TransitionPhase);
                    seq.Add(provider.ConstructionPhase);
                });
    }
}