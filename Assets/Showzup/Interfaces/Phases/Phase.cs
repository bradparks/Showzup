using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public struct Phase
    {
        public Presentation Presentation { get; }
        public PhaseId Id { get; }
        public float? Duration { get; }
        public ISequenceable Parallel { get; }

        public Phase(Presentation presentation, PhaseId id, float? duration, ISequenceable parallel)
        {
            Presentation = presentation;
            Id = id;
            Duration = duration;
            Parallel = parallel;
        }
    }
}