using System;
using UniRx;

namespace Silphid.Showzup
{
    public class PhaseProvider : IPhaseProvider
    {
        private readonly Func<PhaseId, Func<IObservable<Unit>>, Func<IObservable<Unit>>> _getCustomPhase;

        public PhaseProvider(
            Func<IObservable<Unit>> deconstructionPhase,
            Func<IObservable<Unit>> loadPhase,
            Func<IObservable<Unit>> transitionPhase,
            Func<IObservable<Unit>> constructionPhase,
            Func<PhaseId, Func<IObservable<Unit>>, Func<IObservable<Unit>>> getCustomPhase)
        {
            DeconstructionPhase = deconstructionPhase;
            LoadPhase = loadPhase;
            TransitionPhase = transitionPhase;
            ConstructionPhase = constructionPhase;
            _getCustomPhase = getCustomPhase;
        }

        public Func<IObservable<Unit>> DeconstructionPhase { get; }
        public Func<IObservable<Unit>> LoadPhase { get; }
        public Func<IObservable<Unit>> TransitionPhase { get; }
        public Func<IObservable<Unit>> ConstructionPhase { get; }

        public Func<IObservable<Unit>> GetCustomPhase(PhaseId phaseId, Func<IObservable<Unit>> phaseOperation = null) =>
            _getCustomPhase(phaseId, phaseOperation);
    }
}