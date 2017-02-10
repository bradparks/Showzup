using System;

namespace Silphid.Showzup
{
    public class CompletedPhase
    {
        public Presentation Presentation { get; }
        public Type PhaseType { get; }

        public CompletedPhase(Phase phase)
        {
            Presentation = phase.Presentation;
            PhaseType = phase.GetType();
        }

        public object SourceViewModel => Presentation.SourceViewModel;
        public object TargetViewModel => Presentation.TargetViewModel;
        public IView SourceView => Presentation.SourceView;
        public IView TargetView
        {
            get { return Presentation.TargetView; }
            set { Presentation.TargetView = value; }
        }
        public Type SourceViewType => Presentation.SourceViewType;
        public Type TargetViewType => Presentation.TargetViewType;
        public Options Options => Presentation.Options;
    }
}