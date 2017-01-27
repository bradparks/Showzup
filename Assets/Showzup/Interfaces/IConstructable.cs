﻿using UniRx;

namespace Silphid.Showzup
{
    public interface IConstructable
    {
        IObservable<Unit> Construct();
    }
}