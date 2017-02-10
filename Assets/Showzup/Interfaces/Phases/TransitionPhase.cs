using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class TransitionPhase : Phase
    {
        public Transition Transition { get; }

        public TransitionPhase(Presentation presentation, Parallel parallel, Transition transition, float duration)
            : base(presentation, parallel, duration)
        {
            Transition = transition;
        }
    }
}