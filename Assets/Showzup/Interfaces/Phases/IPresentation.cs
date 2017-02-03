using System;

namespace Silphid.Showzup
{
    public interface IPresentation
    {
        object ViewModel { get; }
        IView SourceView { get; }
        IView TargetView { get; set; }
        Type SourceViewType { get; }
        Type TargetViewType { get; }
        Options Options { get; }
    }
}