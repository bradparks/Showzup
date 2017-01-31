namespace Silphid.Showzup
{
    public class PhaseId
    {
        public static readonly PhaseId Deconstruction = new PhaseId(nameof(Deconstruction));
        public static readonly PhaseId Load = new PhaseId(nameof(Load));
        public static readonly PhaseId Transition = new PhaseId(nameof(Transition));
        public static readonly PhaseId Unload = new PhaseId(nameof(Unload));
        public static readonly PhaseId Construction = new PhaseId(nameof(Construction));

        public string Name { get; }

        public PhaseId(string name)
        {
            Name = name;
        }
    }
}