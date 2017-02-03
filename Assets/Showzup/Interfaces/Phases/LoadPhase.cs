using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class LoadPhase : Phase
    {
        public LoadPhase(IPresentation presentation, ISequenceable parallel)
            : base(presentation, PhaseId.Load, parallel)
        {
        }
    }
}