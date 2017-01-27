using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public struct Phase
    {
        public object Input { get; set; }
        public Options Options { get; set; }
        public IView Source { get; set; }
        public IView Target { get; set; }
        public Parallel Parallel { get; set; }
    }
}