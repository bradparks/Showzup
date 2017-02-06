using System;
using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class Phase : IPhase
    {
        protected readonly IPresentation _presentation;

        public ISequenceable Parallel { get; }
        public float? Duration { get; }

        public Phase(IPresentation presentation, ISequenceable parallel, float? duration = null)
        {
            _presentation = presentation;
            Parallel = parallel;
            Duration = duration;
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