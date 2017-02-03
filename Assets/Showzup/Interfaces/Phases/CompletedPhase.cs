namespace Silphid.Showzup
{
    public struct CompletedPhase
    {
        public Presentation Presentation { get; }
        public PhaseId Id { get; }

        public CompletedPhase(Presentation presentation, PhaseId id)
        {
            Presentation = presentation;
            Id = id;
        }
    }
}