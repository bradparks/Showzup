using System;

namespace Silphid.Showzup
{
    public struct Present
    {
        public object ViewModel { get; set; }
        public Type SourceViewType { get; set; }
        public Type TargetViewType { get; set; }
        public Options Options { get; set; }
    }
}