using System;

namespace Silphid.Showzup
{
    public class Presentation : IPresentation
    {
        public object ViewModel { get; }
        public IView SourceView { get; }
        public IView TargetView { get; set; }
        public Type SourceViewType { get; }
        public Type TargetViewType { get; }
        public Options Options { get; }

        public Presentation(object viewModel, IView sourceView, Type targetViewType, Options options)
        {
            ViewModel = viewModel;
            SourceView = sourceView;
            SourceViewType = sourceView?.GetType();
            TargetViewType = targetViewType;
            Options = options;
        }
    }
}