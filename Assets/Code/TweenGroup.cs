using UnityEngine;
using System.Collections.Generic;
using Sys = System;

namespace FableLabs.Anim
{
    /// <summary>
    /// Base class for TweenGroup, TweenParallel, and TweenSequence
    /// </summary>
    public abstract class BaseTweenGroup : BaseTween
    {
        public readonly List<BaseTween> Tweens = new List<BaseTween>();

        protected BaseTweenGroup()
        {
            _position = _lastPosition = 0;
            _age = 0;
            _speed = 1;
            _state = States.Play;
        }

        public override void Dispose(bool disposeAll)
        {
            if (disposeAll) { onComplete = null; }
            for (int i = 0; i < Tweens.Count; i++)
            {
                Tweens[i].Dispose();
            }
        }

        public override bool isDisposable
        {
            get
            {
                if (_persistent) { return false; }
                for (int i = 0; i < Tweens.Count; i++)
                {
                    if (!Tweens[i].isDisposable) { return false; }
                }
                return true;
            }
        }

        public override void Start()
        {
            base.Start();
            for (int i = 0; i < Tweens.Count; i++)
            {
                BaseTween t = Tweens[i];
                if (_speed >= 0) { t.Start(); }
                else { t.End(); }
            }
        }

        public override void End()
        {
            base.End();
            for (int i = 0; i < Tweens.Count; i++)
            {
                BaseTween t = Tweens[i];
                if (_speed >= 0) { t.End(); }
                else { t.Start(); }
            }
        }

        public BaseTweenGroup Add(params BaseTween[] tw)
        {
            for (int i = 0; i < tw.Length; i++) { Add(tw[i]); }
            return this;
        }

        public virtual BaseTweenGroup Add(BaseTween tw)
        {
            // pluck from static group
            Tween.Remove(tw);
            Tweens.Add(tw);
            return this;
        }

        public BaseTweenGroup Remove(BaseTween tw)
        {
            Tweens.Remove(tw);
            return this;
        }

        public BaseTweenGroup Merge(BaseTweenGroup grp)
        {
            for (int i = 0; i < grp.Tweens.Count; i++)
            {
                Tweens.Add(grp.Tweens[i]);
            }
            return this;
        }

        public float MaxDurationRemaining
        {
            get
            {
                float maxDurationRemaining = 0f;
                for (int i = 0; i < Tweens.Count; i++)
                {
                    maxDurationRemaining = Mathf.Max(Tweens[i].DurationRemaining, maxDurationRemaining);
                }
                return maxDurationRemaining;
            }
        }
    }

    public class TweenGroup : BaseTweenGroup
    {
        public override BaseTweenGroup Add(BaseTween tw)
        {
            //tw.Curve(null);
            return base.Add(tw);
        }

        protected override void Apply()
        {
            base.Apply();
            for (int i = 0; i < Tweens.Count; i++)
            {
                Tweens[i].Life = Position;
            }
        }

        public void ClearChildCurves()
        {
            for (int i = 0; i < Tweens.Count; i++)
            {
                Tweens[i].Curve(null);
            }
        }
    }

    /// <summary>
    /// A group of Tweens that run in parallel
    /// Example: Tween.Parallel().Add(Tween.MoveFromTo(obj,f,t,d)).Add(Tween.FadeOut(m,d))
    /// </summary>
    public class TweenParallel : BaseTweenGroup
    {
        private float _durationSum;
        
        public override float Duration
        {
            get { return (_duration > 0) ? _duration : _durationSum; }
            set { _duration = value; }
        }

        public void RecalculateDuration()
        {
            _durationSum = 0;
            for (int i = 0; i < Tweens.Count; i++)
            {
                BaseTween tw = Tweens[i];
                _durationSum = Mathf.Max(_durationSum, tw.Duration);
            }
        }

        public override BaseTweenGroup Add(BaseTween tw)
        {
            _durationSum = Mathf.Max(_durationSum, tw.Duration);
            return base.Add(tw);
        }

        protected override void Apply()
        {
            base.Apply();
            for (int i = 0; i < Tweens.Count; i++)
            {
                Tweens[i].Age = Age * ((_duration > 0) ? _durationSum/_duration : 1);
            }
        }
    }

    /// <summary>
    /// A group of Tweens that run in sequence. The next will start when the last completes
    /// Example: Tween.Sequence().Add(Tween.MoveFromTo(obj,f,t,d)).Add(Tween.FadeOut(m,d))
    /// </summary>
    public class TweenSequence : BaseTweenGroup
    {
        private float _durationSum;
        private int _index;
        
        public BaseTween Current
        {
            get { if (_index < 0 || _index >= Tweens.Count) { return null; } return Tweens[_index]; }
        }

        public override float Duration
        {
            get { return (_duration > 0) ? _duration : _durationSum; }
            set { _duration = value; }
        }

        public void RecalculateDuration()
        {
            _durationSum = 0;
            for (int i = 0; i < Tweens.Count; i++)
            {
                BaseTween tw = Tweens[i];
                _durationSum = Mathf.Max(_durationSum, tw.Duration);
            }
        }

        public override BaseTweenGroup Add(BaseTween tw)
        {
            _durationSum += tw.Duration;
            base.Add(tw);
            return this;
        }

        public BaseTweenGroup Unshift(BaseTween tw)
        {
            _durationSum += tw.Duration;
            Tween.Remove(tw);
            Tweens.Insert(0, tw);
            return this;
        }

        public void Advance()
        {
            /*
            _index += 1;
            if (_index >= Tweens.Count) { End(); }
            else { Current.Start(); }
             */
        }

        public void Wait(float time)
        {
            ActionTween<float> timer = new ActionTween<float> { Duration = time };
            Add(timer);
        }

        protected override void Apply()
        {
            base.Apply();
            float ax = 0;
            float scrub = _position*_durationSum;
            for (int i = 0; i < Tweens.Count; i++)
            {
                BaseTween t = Tweens[i];
                float norm = scrub - ax;
                if (norm >= 0 && norm <= 1)
                {
                    /*
                    if (_index != i)
                    {
                        Current.End();
                        _index = i; 
                    }*/
                    _index = i;
                }
                t.Age = norm;
                ax += t.Duration;
            }
        }
    }


    public partial class Tween
    {
        private static BaseTweenGroup BaseGroup(BaseTweenGroup grp, bool start)
        {
            Init();
            Tweens.Add(grp);
            if (start) { grp.Start(); }
            return grp;
        }

        public static TweenSequence Sequence(bool start = true)
        {
            return (TweenSequence) BaseGroup(new TweenSequence(), start);
        }

        public static TweenParallel Parallel(bool start = true)
        {
            return (TweenParallel)BaseGroup(new TweenParallel(), start);
        }

        public static TweenGroup Group(float duration, bool start = true)
        {
            TweenGroup grp = (TweenGroup) BaseGroup(new TweenGroup(), start);
            grp.Duration = duration;
            return grp;
        }
    }
    
}
