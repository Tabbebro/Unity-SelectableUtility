using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UISelectable {
    public class TweenGroup
    {
        readonly List<Tween> _tweens = new();

        public void Add(Tween tween) { 
            _tweens.Add(tween);
        }

        public void Kill() {
            foreach (var tween in _tweens) {
                tween?.Kill();
            }

            _tweens.Clear();
        }
    }
}
