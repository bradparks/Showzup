using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class DeconstructPhase : Phase
    {
        public DeconstructPhase(Presentation presentation, ISequenceable parallel)
            : base(presentation, PhaseId.Deconstruction, parallel)
        {
        }
    }
}