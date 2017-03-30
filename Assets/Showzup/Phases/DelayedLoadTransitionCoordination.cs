using System;
using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class DelayedLoadTransitionCoordination : CoordinationBase
    {
        public DelayedLoadTransitionCoordination(Presentation presentation) : base(presentation)
        {
        }

        protected override IDisposable CoordinateInternal()
        {
            var present = CreatePerformer(PhaseId.Present);
            var construct = CreatePerformer(PhaseId.Construct);
            var deconstruct = CreatePerformer(PhaseId.Deconstruct);
            var show = CreatePerformer(PhaseId.Show);
            var hide = CreatePerformer(PhaseId.Hide);
            var load = CreatePerformer(PhaseId.Load);
            var transition = CreatePerformer(PhaseId.Transition);

            return Sequence.Start(seq =>
            {
                seq.AddAction(() =>
                {
                    present.Start();
                    hide.Start();
                    deconstruct.Start();
                });
                seq.AddWaitUntil(deconstruct.Completed);
                seq.AddAction(() => show.Start());
                seq.Add(() => load.Perform());
                seq.Add(() => transition.Perform());
                seq.AddAction(() => hide.Complete());
                seq.Add(() => construct.Perform());
                seq.AddAction(() =>
                {
                    show.Complete();
                    present.Complete();
                    Observer.OnCompleted();
                });
            });
        }
    }
}