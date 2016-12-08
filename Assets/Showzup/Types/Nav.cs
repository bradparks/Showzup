using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class Nav
    {
        public IView Source { get; }
        public IView Target { get; }
        public Parallel Parallel { get; }

        public Nav(IView source, IView target, Parallel parallel)
        {
            Source = source;
            Target = target;
            Parallel = parallel;
        }

        public override string ToString() => $"Source: {Source}, Target: {Target}";
    }}