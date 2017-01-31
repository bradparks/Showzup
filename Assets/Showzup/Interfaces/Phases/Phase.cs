using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public struct Phase
    {
        public Present Present { get; }
        public PhaseId Id { get; }
        public float? Duration { get; }
        public ISequenceable Parallel { get; }

        public Phase(Present present, PhaseId id, float? duration, ISequenceable parallel)
        {
            Present = present;
            Id = id;
            Duration = duration;
            Parallel = parallel;
        }
    }
}