using JetBrains.Annotations;
using Silphid.Sequencit;
using UniRx;

namespace Silphid.Showzup
{
    public class DefaultPhaseCoordinator : IPhaseCoordinator
    {
        [Pure]
        public IObservable<Unit> Coordinate(IPresentation presentation, IPhaseProvider provider) =>
            Sequence
                .Create(seq =>
                {
                    seq.Add(provider.DeconstructionPhase);
                    seq.Add(provider.LoadPhase);
                    seq.Add(provider.TransitionPhase);
                    seq.Add(provider.ConstructionPhase);
//                    seq.Add(provider.GetCustomPhase(
//                        parallel => new Phase(presentation, PhaseId.Deconstruction, parallel),
//                        phase => Observable.ReturnUnit()
//                    ));
                });
    }
}