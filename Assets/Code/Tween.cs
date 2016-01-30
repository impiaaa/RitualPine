#if JETBRAINS
using JetBrains.Annotations;
#endif
#if FL_OPERATOR
using MiscUtil;
#endif
using FableLabs.Util;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace FableLabs.Anim
{       
    /*
    * TODO: Wishlist
    * public static AnimationClip ExportAnimation() {} *
    * public static List<BaseTween> tweensWithTarget
    * public static List<BaseTween> tweensWithTag
    * public static void killAllTweensWithTarget( object target )
    * Euler/Quaternion fix for rotations
    * Nearest Angle / Cycles
    * Nearest Location from list
    * Evaluators for Groups
    * Shake. maybe.
    * Splines. maybe.
    */

    [ExecuteInEditMode]
    public partial class Tween : MonoBehaviour
    {
        #region ***        PUBLIC SINGLETON INSTANCE      ***
        public float Speed = 1.0f;
        public float MaxDelta = 1.0f / 15.0f;
        public float SystemLife;
#if JETBRAINS
        [UsedImplicitly] 
#endif
        public int ActiveTweenCount; // readonly: used for debug in inspector only 
        public int TweensCreated;
        public Func<float> DeltaSource;
        public static Func<float> DefaultDeltaSource; 

        public override string ToString()
        {
            return string.Format("Tween Engine (tweens:{0} speed:{1} uptime:{2} maxdelta:{3}", Tweens.Count, Speed, SystemLife, MaxDelta);
        }

#if JETBRAINS
        [UsedImplicitly]
#endif
        public virtual void Awake()
        {
            Speed = 1.0f;
            DeltaSource = () => Math.Min(Time.deltaTime * Speed, MaxDelta);
            //DeltaSource = DefaultDeltaSource;
            DontDestroyOnLoad(gameObject);
        }

        public void Pulse(float delta)
        {
            SystemLife += delta;
            if (Tweens.Count <= 0) { return; }
            for (int i = Tweens.Count - 1; i >= 0; i--)
            {
                Tweens[i].Delta(delta);
            }
            for (int i = Tweens.Count - 1; i >= 0; i--)
            {
                BaseTween tween = Tweens[i];
                if (tween.isDisposable)
                {
                    RemoveAt(i);
                    tween.Dispose();
                }
            }
            ActiveTweenCount = Count;
        }

#if JETBRAINS
        [UsedImplicitly]
#endif
        public void Update()
        {
            if (DeltaSource == null) { return; }
            float delta = DeltaSource();
            //float delta = Math.Min(DeltaSource() * Speed, MaxDelta);
            Pulse(delta);
        }
        #endregion

        #region ***     INITIALIZATION AND MAINTENANCE    ***
        private static Tween _master;
        private static readonly List<BaseTween> Tweens = new List<BaseTween>();
        private static readonly Dictionary<object, List<BaseTween>> TweensByTarget = new Dictionary<object, List<BaseTween>>();
        private static readonly Dictionary<object, Dictionary<int, BaseTween>> TweenByTargetChannel = new Dictionary<object, Dictionary<int, BaseTween>>();
        private static readonly Dictionary<string, List<BaseTween>> TweensByTag = new Dictionary<string, List<BaseTween>>();

        private static void InitializeDefaultInterpolators()
        {
            //poor man's specialization
            //ActionTween<float>.Interpolator = Mathf.Lerp;
            ActionTween<float>.Interpolator = (frm, to, t) => ((to - frm) * t + frm);
            ActionTween<int>.Interpolator = (frm, to, t) => (int)(((float)to - (float)frm) * t + (float)frm);
            ActionTween<Vector2>.Interpolator = Vector2.Lerp;
            ActionTween<Vector3>.Interpolator = Vector3.Lerp;
            //ActionTween<Vector3>.Interpolator = (frm, to, t) => frm + ((to - frm) * t);
            ActionTween<Vector4>.Interpolator = Vector4.Lerp;
            ActionTween<Color>.Interpolator = Color.Lerp;
            ActionTween<Quaternion>.Interpolator = Quaternion.Lerp;
            ActionTween<Rect>.Interpolator = (frm, to, t) => new Rect(
                (to.x - frm.x) * t + frm.x,
                (to.y - frm.y) * t + frm.y,
                (to.width - frm.width) * t + frm.width,
                (to.height - frm.height) * t + frm.height);
            /*
            ActionTween<Material>.Interpolator = (frm, to, t) =>
            {
                Material m = new Material(frm); // not very optimal
                frm.Lerp(frm, to, t);
                return m;
            };*/
        }

        public static void UpdateMaster() { _master.Update(); }
        public static void Init() { Init("TweenMaster"); }
        public static void Init(string masterObjectName)
        {
            if (_master != null) { return; }
            InitializeDefaultInterpolators();
            _master = FindObjectOfType<Tween>() ?? new GameObject(masterObjectName).AddComponent<Tween>();
            _master.DeltaSource = () => Math.Min(Time.deltaTime * _master.Speed, _master.MaxDelta);
        }
        #endregion

        #region ***           STATIC OPERATIONS           ***

        private static void AddToTargetList(BaseTween tween)
        {
            object target = tween.Target;
            if (target == null) { return; }
            List<BaseTween> tweens;
            if (!TweensByTarget.TryGetValue(target, out tweens)) { tweens = TweensByTarget[target] = new List<BaseTween>(); }
            tweens.Add(tween);

            if (target is UnityEngine.Object)
            {
                tween.NullCheck();
            }
        }

        public static void AddToTargetChannel(BaseTween tween)
        {
            if (tween.Target == null || tween.channel == 0) { return; }
            Dictionary<int, BaseTween> channels;
            if (!TweenByTargetChannel.TryGetValue(tween.Target, out channels))
            {
                channels = TweenByTargetChannel[tween.Target] = new Dictionary<int, BaseTween>();
            }
            BaseTween existing;
            if (channels.TryGetValue(tween.channel, out existing))
            {
                existing.Dispose();
            }
            channels[tween.channel] = tween;
        }

        public static void RemoveFromTargetChannel(BaseTween tween)
        {
            if (tween.Target == null || tween.channel == 0) { return; }
            Dictionary<int, BaseTween> channels;
            if (!TweenByTargetChannel.TryGetValue(tween.Target, out channels)) { return; }
            channels.Remove(tween.channel);
        }

        public static bool IsChannelInUse(object target, int channel)
        {
            Dictionary<int, BaseTween> channels;
            if (!TweenByTargetChannel.TryGetValue(target, out channels))
            {
                return false;
            }
            return channels.ContainsKey(channel);
        }

        private static void AddToTagLists(BaseTween tween)
        {
            for (int i = 0; i < tween.tags.Count; i++)
            {
                AddTag(tween,tween.tags[i]);
            }
        }
        
        private static void RemoveFromTagLists(BaseTween tween)
        {
            for (int i = 0; i < tween.tags.Count; i++)
            {
                RemoveTag(tween,tween.tags[i]);
            }
        }

        private static void RemoveFromTargetList(BaseTween tween)
        {
            object target = tween.Target;
            if (target == null) { return; }
            if (!TweensByTarget.ContainsKey(target)) { return; }
            TweensByTarget[target].Remove(tween);
            if (TweensByTarget[target].Count == 0) { TweensByTarget.Remove(target); }
        }

        public static List<BaseTween> GetTweensOf(object target)
        {
            if (!TweensByTarget.ContainsKey(target)) { return null; }
            return TweensByTarget[target];
        }

        public static void Remove(BaseTween tw)
        {
            Tweens.Remove(tw);
            RemoveFromTagLists(tw);
            RemoveFromTargetList(tw);
            RemoveFromTargetChannel(tw);
        }

        private static void RemoveAt(int index)
        {
            Remove(Tweens[index]);
        }

        private static void Add(BaseTween tw)
        {
            Tweens.Add(tw);
            AddToTargetChannel(tw);
            AddToTargetList(tw);
            AddToTagLists(tw);
            _master.TweensCreated++;
        }

        public static void EndAllTweens()
        {
            foreach (BaseTween tween in Tweens) { tween.End(); }
        }

        private void OnDestroy() { KillAllTweens(true); }

        public static void KillAllTweens(bool endFirst = false)
        {
            foreach (BaseTween tween in Tweens)
            {
                if (endFirst) { tween.End(); }
                tween.Pause();
                tween.Dispose();
            }

            //Tweens.Clear();
            //TweensByTarget.Clear();
        }

        public static void KillTweensWithTag(string t, bool endFirst = false)
        {
            KillTheseTweens(FindTweensWithTag(t), endFirst);
        }

        public static void PauseAllTweensWithTag(string t)
        {
            foreach (BaseTween tween in FindTweensWithTag(t))
                tween.Pause();
        }

        public static void ResumeAllTweensWithTag(string t)
        {
            foreach (BaseTween tween in FindTweensWithTag(t))
                tween.Play();
        }

        public static void KillTweensOf(object target, bool endFirst = false)
        {
            if (target == null || TweensByTarget == null) { return; }
            if (!TweensByTarget.ContainsKey(target)) { return; }
            foreach (BaseTween tween in TweensByTarget[target].ToArray())
            {
                if (endFirst) { tween.End(); }
                tween.Dispose();
                Remove(tween);
            }
        }

        public static void KillTheseTweens(List<BaseTween> list, bool endFirst = false)
        {
            foreach (BaseTween tween in list)
            {
                if (endFirst) { tween.End(); }
                tween.Pause();
                tween.Dispose();
            }
        }

        public static List<BaseTween> FindTweensWithTag(string t)
        {
            List<BaseTween> tws = new List<BaseTween>();
            foreach (BaseTween tween in Tweens) { if (tween.HasTag(t)) { tws.Add(tween); } }
            return tws;
        }

        public static void AddTag(BaseTween tw, string tag)
        {
            List<BaseTween> tweens;
            if (!TweensByTag.TryGetValue(tag, out tweens)) { tweens = TweensByTag[tag] = new List<BaseTween>(); }
            tweens.Add(tw);
        }

        public static void RemoveTag(BaseTween tw, string tag)
        {
            List<BaseTween> tweens;
            if (!TweensByTag.TryGetValue(tag, out tweens)) { return; }
            tweens.Remove(tw);
        }

        public static int Count { get { return Tweens.Count; } }

        public static int CountOfTweensWithTag(string t)
        {
            int count = 0;
            foreach (BaseTween tween in Tweens) { if (tween.HasTag(t)) { count++; } }
            return count;
        }

        public static float LongestTweensWithTag(string t)
        {
            float longest = 0;
            foreach (BaseTween tween in Tweens)
            {
                if (tween.HasTag(t))
                {
                    float len = tween.DurationRemaining * tween.AbsoluteSpeed;
                    if (len > longest) { longest = len; }
                }
            }
            return longest;
        }

        #endregion
        public static BaseTween.EasingFunction DefaultEasing = Mathfl.easeInOutCubic;

        #region ***        STATIC FACTORY METHODS         ***


        /// <summary>
        /// Interval does nothing but can be used to get a callback at a regular interval with OnComplete
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static ActionTween<float> Interval(float duration) { return Wait(duration).Loop() as ActionTween<float>; }
        public static ActionTween<float> Interval(float duration, Action onInterval) { return Wait(duration).Loop().OnComplete(onInterval) as ActionTween<float>; }

        /// <summary>
        /// Wait does nothing but can be used to get a callback 'coroutine' after some time by using OnComplete
        /// </summary>
        /// <param name="duration">Duration to wait in seconds</param>
        /// <returns></returns>
        public static ActionTween<float> Wait(float duration) { return Raw().Tween(0, 1, duration); }
        public static ActionTween<float> Wait(float duration, Action onComplete) { return Raw().Tween(0, 1, duration).OnComplete(onComplete) as ActionTween<float>; }

        // TODO: test this
        public static ActionTween<float> WaitFrames(int frames)
        {
            ActionTween<float> timer = Forever();
            int framesLeft = frames;
            timer.OnUpdate(f =>
            {
                if (--framesLeft <= 0) { timer.Dispose(); }
            });
            return timer;
        }

        public static ActionTween<float> WaitUntil(Func<bool> condition)
        {
            ActionTween<float> timer = Forever();
            timer.action = f => { if (condition()) { timer.End(); timer.Dispose(); } };
            return timer;
        }

        /// <summary>
        /// Get a tween that tweens between 0 and 1 but does nothing with it
        /// </summary>
        /// <returns></returns>
        private static ActionTween<float> Raw()
        {
            Init();
            ActionTween<float> timer = new ActionTween<float>();
            Add(timer);
            return timer;
        }

        public static ActionTween<float> Forever() { return Raw().Infinite() as ActionTween<float>; }

        // Main Friendly Convenience Function
        //public static ActionTween<T> FromTo<T>(object target, string prop, T from, T to, float duration)
        //{
        //    return FromTo(target, Resolve.Setter<T>(target, prop), from, to, duration);
        //}
        //public static ActionTween<T> From<T>(object target, string prop, T val, float duration) { return FromTo(target, prop, val, Resolve.Get<T>(target, prop), duration); }
        //public static ActionTween<T> To<T>(object target, string prop, T val, float duration) { return FromTo(target, prop, Resolve.Get<T>(target, prop), val, duration); }

        //public static DynamicTween<T> From<T>(object target, string prop, T val, float duration) { return FromTo(target, prop, val, Resolve.Getter<T>(target, prop), duration); }
        //public static DynamicTween<T> To<T>(object target, string prop, T val, float duration) { return FromTo(target, prop, Resolve.Getter<T>(target, prop), val, duration); }
        //public static DynamicTween<T> From<T>(object target, Action<T> setter, Func<T> getter, T val, float duration) { return FromTo(target, setter, val, getter, duration); }
        //public static DynamicTween<T> To<T>(object target, Action<T> setter, Func<T> getter, T val, float duration) { return FromTo(target, setter, getter, val, duration); }
        
        // Note: Do-nothing Main Meatball of type T - just add action and start!
        public static ActionTween<T> FromTo<T>(object target, T from, T to, float duration)
        {
            Init();
            if (ActionTween<T>.Interpolator == null) { throw new ArgumentException(string.Format("ActionTween of Type ({0}) has no Interpolator defined", typeof(T))); }
            ActionTween<T> tween = new ActionTween<T>();
            tween.Easing(DefaultEasing);
            tween.Tween(from, to, duration);
            tween.Target = target;
            Add(tween);
            return tween;
        }

        //public static DynamicTween<T> FromTo<T>(object target, Action<T> prop, Func<T> from, Func<T> to, float duration)
        //{
        //    return Dynamic(target, prop).Tween(from, to, duration);
        //}
        //public static DynamicTween<T> FromTo<T>(object target, Action<T> prop, Func<T> from, T to, float duration)
        //{
        //    return Dynamic(target, prop).Tween(from, to, duration);
        //}
        //public static DynamicTween<T> FromTo<T>(object target, Action<T> prop, T from, Func<T> to, float duration)
        //{
        //    return Dynamic(target, prop).Tween(from, to, duration);
        //}
        //public static DynamicTween<T> FromTo<T>(object target, string prop, Func<T> from, Func<T> to, float duration)
        //{
        //    return Dynamic<T>(target, prop).Tween(from, to, duration);
        //}
        //public static DynamicTween<T> FromTo<T>(object target, string prop, Func<T> from, T to, float duration)
        //{
        //    return Dynamic<T>(target, prop).Tween(from, to, duration);
        //}
        //public static DynamicTween<T> FromTo<T>(object target, string prop, T from, Func<T> to, float duration)
        //{
        //    return Dynamic<T>(target, prop).Tween(from, to, duration);
        //}

        //public static DynamicTween<T> Dynamic<T>(object target)
        //    //Func<T> from, Func<T> to, float duration, bool dynamicFrom, bool dynamicTo)
        //{
        //    Init();
        //    if (ActionTween<T>.Interpolator == null) { throw new ArgumentException(string.Format("ActionTween of Type ({0}) has no Interpolator defined", typeof(T))); }
        //    DynamicTween<T> tween = new DynamicTween<T>();
        //    tween.Easing(DefaultEasing);
        //    tween.Target = target;
        //    Add(tween);
        //    return tween;
        //}

        //public static DynamicTween<T> Dynamic<T>(object target, Action<T> setter)
        //{
        //    DynamicTween<T> tween = Dynamic<T>(target);
        //    tween.action = setter;
        //    return tween;
        //}

        //public static DynamicTween<T> Dynamic<T>(object target, string prop)
        //{
        //    DynamicTween<T> tween = Dynamic<T>(target);
        //    tween.action = Resolve.Setter<T>(target, prop);
        //    return tween;
        //}

        // Simple Variant - Supply your own action 
        public static ActionTween<T> FromTo<T>(object target, Action<T> setter, T from, T to, float duration)
        {
            ActionTween<T> tween = FromTo(target, from, to, duration);
            tween.action = setter;
            tween.Start();
            return tween;
        }
        
        // Advanced Variant - action receives a tween object
        public static ActionTween<T> FromTo<T>(object target, Action<BaseTween> action, T from, T to, float duration)
        {
            ActionTween<T> tw = FromTo(target, from, to, duration);
            tw.advancedAction = action;
            tw.Start();
            return tw;
        }

#if FL_OPERATOR
        public static Action<BaseTween> GetAdditiveSetter<T>(object target, string prop)
        {
            Func<T> getter = Resolve.Getter<T>(target, prop);
            Action<T> setter = Resolve.Setter<T>(target, prop);
            return tw =>
            {
                // if this fails you're using the wrong type of setter action for the tween
                ActionTween<T> at = (ActionTween<T>)tw;
                T diff = Operator<T>.Subtract(at.value, at.lastValue);
                setter(Operator<T>.Add(getter(), diff));
            };
        }
        // Additive Variant - setter is applied additively (currently non-functional)
        public static ActionTween<T> FromToAdditive<T>(object target, string prop, T from, T to, float duration)
        {
            ActionTween<T> tw = FromTo(target, from, to, duration);
            tw.advancedAction = GetAdditiveSetter<T>(target, prop);
            tw.Start();
            return tw;
        }
#endif
        #endregion

    }

}
