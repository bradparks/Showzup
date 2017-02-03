using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class ConstructionPhase : Phase
    {
        public ConstructionPhase(IPresentation presentation, ISequenceable parallel)
            : base(presentation, PhaseId.Construction, parallel)
        {
        }
    }
}