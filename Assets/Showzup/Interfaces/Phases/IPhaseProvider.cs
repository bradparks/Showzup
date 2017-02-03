using System;
using Silphid.Sequencit;
using UniRx;

namespace Silphid.Showzup
{
    public interface IPhaseProvider
    {
        Func<IObservable<Unit>> DeconstructionPhase { get; }
        Func<IObservable<Unit>> LoadPhase { get; }
        Func<IObservable<Unit>> TransitionPhase { get; }
        Func<IObservable<Unit>> ConstructionPhase { get; }
        Func<IObservable<Unit>> GetCustomPhase(
            Func<Parallel, Phase> createPhase,
            Func<Phase, IObservable<Unit>> phaseOperation = null);
    }
}