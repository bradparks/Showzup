﻿using Silphid.Sequencit;

namespace Silphid.Showzup
{
    public class Nav
    {
        public IView Source { get; }
        public IView Target { get; }
        public Parallel Parallel { get; set; }
        public Transition Transition { get; }
        public float Duration { get; }

        public Nav(IView source, IView target, Transition transition, float duration)
        {
            Source = source;
            Target = target;
            Transition = transition;
            Duration = duration;
        }

        public override string ToString() => $"Source: {Source}, Target: {Target}";
    }}