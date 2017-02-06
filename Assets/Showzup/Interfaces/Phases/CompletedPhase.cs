using System;

namespace Silphid.Showzup
{
    public class CompletedPhase : IPresentation
    {
        private readonly IPresentation _presentation;

        public Type PhaseType { get; }

        public CompletedPhase(IPhase phase)
        {
            _presentation = phase;
            PhaseType = phase.GetType();
        }

        public object ViewModel => _presentation.ViewModel;
        public IView SourceView => _presentation.SourceView;
        public IView TargetView
        {
            get { return _presentation.TargetView; }
            set { _presentation.TargetView = value; }
        }
        public Type SourceViewType => _presentation.SourceViewType;
        public Type TargetViewType => _presentation.TargetViewType;
        public Options Options => _presentation.Options;
    }
}