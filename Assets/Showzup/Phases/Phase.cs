﻿using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class Phase
    {
        public PhaseId Id { get; }
        public Presentation Presentation { get; }
        public Step Step { get; }

        public Phase(PhaseId id, Presentation presentation)
        {
            Id = id;
            Presentation = presentation;
            Step = new Step();
        }
    }
}