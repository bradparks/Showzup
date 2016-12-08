using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Silphid.Showzup
{
    public static class OptionsExtensions
    {
        public static Direction GetDirection(this Options This) => This?.Direction ?? Direction.Default;
        public static PushMode GetPushMode(this Options This) => This?.PushMode ?? PushMode.Default;
        public static IEnumerable<string> GetVariants(this Options This) =>
            This?.Variants ?? Enumerable.Empty<string>();

        public static Options WithExtraVariants(this Options This, IList<string> variants)
        {
            if (variants.IsEmpty())
                return This;

            if (This == null)
                This = new Options();

            This.Variants = This.Variants.Concat(variants);

            return This;
        }
    }
}