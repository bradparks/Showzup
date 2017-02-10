using System;
using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class Phase
    {
        public Presentation Presentation { get; }
        public Parallel Parallel { get; }
        public float? Duration { get; }

        public Phase(Presentation presentation, Parallel parallel, float? duration = null)
        {
            Presentation = presentation;
            Parallel = parallel;
            Duration = duration;
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

        public override string ToString() =>
            $"{nameof(Duration)}: {Duration}, {nameof(TargetViewModel)}: {TargetViewModel}, {nameof(SourceView)}: {SourceView}, {nameof(TargetView)}: {TargetView}, {nameof(SourceViewType)}: {SourceViewType}, {nameof(TargetViewType)}: {TargetViewType}, {nameof(Options)}: {Options}";
    }
}