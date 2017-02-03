using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public interface IPhase : IPresentation
    {
        PhaseId Id { get; }
        ISequenceable Parallel { get; }
        float? Duration { get; }
    }
}