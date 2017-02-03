using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class DeconstructionPhase : Phase
    {
        public DeconstructionPhase(IPresentation presentation, ISequenceable parallel)
            : base(presentation, PhaseId.Deconstruction, parallel)
        {
        }
    }
}