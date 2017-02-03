using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class TransitionPhase : Phase
    {
        public Transition Transition { get; }

        public TransitionPhase(IPresentation presentation, ISequenceable parallel, Transition transition, float duration)
            : base(presentation, PhaseId.Transition, parallel, duration)
        {
            Transition = transition;
        }
    }
}