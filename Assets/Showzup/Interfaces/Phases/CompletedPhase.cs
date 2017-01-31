namespace Silphid.Showzup
{
    public struct CompletedPhase
    {
        public Present Present { get; }
        public PhaseId Id { get; }

        public CompletedPhase(Present present, PhaseId id)
        {
            Present = present;
            Id = id;
        }
    }
}