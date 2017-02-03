namespace Silphid.Showzup
{
    public struct CompletedPhase
    {
        public IPresentation Presentation { get; }
        public PhaseId Id { get; }

        public CompletedPhase(IPresentation presentation, PhaseId id)
        {
            Presentation = presentation;
            Id = id;
        }
    }
}