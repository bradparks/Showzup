using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    public class Options
    {
        public Direction Direction { get; set; }
        public PushMode PushMode { get; set; }
        public IEnumerable<string> Variants { get; set; } = Enumerable.Empty<string>();
        public Transition Transition { get; set; }
        public float? Duration { get; set; }

        public override string ToString() =>
            $"{nameof(Direction)}: {Direction}, {nameof(PushMode)}: {PushMode}, {nameof(Variants)}: {Variants?.ToDelimitedString(";")}, {nameof(Transition)}: {Transition}, {nameof(Duration)}: {Duration}";
    }
}