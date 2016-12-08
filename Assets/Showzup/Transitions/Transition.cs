﻿using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class Transition : MonoBehaviour
    {
        public abstract void Prepare(GameObject sourceContainer, GameObject targetContainer, Direction direction);
        public abstract IObservable<Unit> Perform(GameObject sourceContainer, GameObject targetContainer, Direction direction, float duration);
        public virtual void Complete(GameObject sourceContainer, GameObject targetContainer) {}
    }
}