﻿using DG.Tweening;
using Silphid.Sequencit;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class ZoomTransition : CrossfadeTransition
    {
        public float StartScale = 0.8f;
        public float EndScale = 1.2f;

        public override void Prepare(GameObject sourceContainer, GameObject targetContainer, Direction direction)
        {
            base.Prepare(sourceContainer, targetContainer, direction);

            if (sourceContainer != null)
                ((RectTransform) sourceContainer.transform).localScale = Vector3.one;

            var scale = direction == Direction.Forward ? StartScale : EndScale;
            ((RectTransform) targetContainer.transform).localScale = Vector3.one * scale;
        }

        public override IObservable<Unit> Perform(GameObject sourceContainer, GameObject targetContainer,
            Direction direction, float duration)
        {
            return Parallel.Create(parallel =>
            {
                base.Perform(sourceContainer, targetContainer, direction, duration)
                    .In(parallel);

                if (sourceContainer != null)
                {
                    var scale = direction == Direction.Forward ? EndScale : StartScale;
                    ((RectTransform) sourceContainer.transform)
                        .DOScale(scale, duration)
                        .SetEase(Ease)
                        .SetAutoKill()
                        .In(parallel);
                }

                ((RectTransform) targetContainer.transform)
                    .DOScale(1f, duration)
                    .SetEase(Ease)
                    .SetAutoKill()
                    .In(parallel);
            });
        }

        public override void Complete(GameObject sourceContainer, GameObject targetContainer)
        {
            base.Complete(sourceContainer, targetContainer);

            if (sourceContainer != null)
                ((RectTransform) sourceContainer.transform).localScale = Vector3.one;

            ((RectTransform) targetContainer.transform).localScale = Vector3.one;
        }
    }
}