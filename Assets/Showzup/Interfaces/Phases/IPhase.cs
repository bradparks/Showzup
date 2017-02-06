using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public interface IPhase : IPresentation
    {
        ISequenceable Parallel { get; }
        float? Duration { get; }
    }
}