using System;
using System.Collections;
using System.Collections.Generic;
using FableLabs.Util;
using UnityEngine;

public interface IIdentifiable
{
    string GetId();
}

namespace FableLabs.Anim
{

    public abstract class BaseTween : IDisposable, IIdentifiable
    {
        private static long _nextId = 1;
        private string _id;

        public enum States { Pause = 0, Play, Loop, PingPong, Infinite, Stop };
        public delegate float EasingFunction(float start, float end, float value);
        //public static ILogSender Log = new UnityLogSender();

        public float Age
        {
            get { return _age; }
            set
            {
                _age = value;

                if (_state != States.Infinite && ((_speed >= 0 && _age > Duration) || (_speed < 0 && _age < 0))) { End(); }
                else if (_age >= 0 && _age <= Duration) { Update(); }
            }
        }

        public virtual float Duration
        {
            get { return _duration; }
            set 
            { 
                _duration = value;
                if (_age >= 0 && _age <= _duration) { Update(); }
            }
        }

        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        /// <summary>
        /// Normalized Age (0.0f - 1.0f, unclamped)
        /// </summary>
        public float Life
        {
            get { return _age / Duration; }
            set 
            { 
                _age = value * Duration;
                if (_age >= 0 && _age <= _duration) { Update(); }
            }
        }

        protected float _age;
        protected float _duration;
        protected float _speed = 1; // timescale
        protected States _state = States.Play;

        protected Func<float, float> _curve;
        public Action onComplete;
        public Action onStart;
        public Action<float> onUpdate;

        public List<string> tags;
        protected bool _nullCheck;
        protected bool _persistent;
        protected float _position;
        protected float _lastPosition;
        public object Target;
        public int LoopCount;
        protected int _loopsSoFar = -1; // also tracks started or not
        public int channel;

        public string Name;

        // NOTE: Added thread safety here, might be overkill?
        // NOTE: Needed GetId for WeightTable / avoiding JIT compiler on iOS
        public string GetId()
        {
            if (_id == null)
            {
                _id = System.Threading.Interlocked.Increment(ref _nextId).ToString();
            }
            return _id;
        }

        public float AbsoluteSpeed
        {
            get { return Mathf.Abs(_speed); }
        }

        public float DurationRemaining
        {
            get { if (_speed >= 0) { return Duration - _age; } else { return _age; } }
        }

        // where the VALUE is between the two ends
        public float Position
        {
            get { return _position; }
            set { _position = value; } // warning - not eased //inverse of the ease
        }

        private EasingFunction easing
        {
            set
            {
                if (value == null) { _curve = null; return; }
                EasingFunction e = value;
                _curve = f => e(0, 1, f);
            }
        }

        public BaseTween() { tags = new List<string>(); }
        public void AddTag(string t) { tags.Add(t); }
        public void RemoveTag(string t) { tags.Remove(t); }
        public bool HasTag(string t) { return (tags.IndexOf(t) != -1); }

        public virtual void Delta(float del)
        {
            //if (_state == States.Pause) { return; }
            if (_state == States.Pause) { del = 0; }

            float delta = del * _speed;
            float newage = _age + delta;

            bool endCondition = false;
            if (_state != States.Infinite)
            {
                bool afterEnd, beforeBeginning;
                if (_speed >= 0) { afterEnd = (_age >= Duration); beforeBeginning = (newage < 0); endCondition = (newage >= Duration); }
                else { afterEnd = (_age < 0); beforeBeginning = (newage >= Duration); endCondition = (newage <= 0); }

                if (afterEnd)
                {
                    Stop();
                    return;
                }
                _age = newage;
                if (beforeBeginning) { return; }
            }
            else
            {
                _age = newage;
            }
            Update();
            if (endCondition) { End(); }
            //else { Apply(); }
        }

        protected virtual void Apply()
        {
            if (_loopsSoFar == -1) { _loopsSoFar = 0; RaiseOnStart(); }
        }

        protected virtual void Update()
        {
            _lastPosition = _position;
            if (Duration <= 0) { _position = 0; }
            else { _position = (_curve != null) ? _curve(_age / Duration) : _age / Duration; }
            if (_state != States.Infinite) { _position = Mathf.Clamp01(_position); }
            Apply();
        }

        public virtual void Start()
        {
            _age = (_speed >= 0) ? 0 : Duration;
            Update();
        }
        protected virtual void RaiseOnComplete() { if (onComplete != null) { onComplete(); } }
        protected virtual void RaiseOnStart() { if (onStart != null) { onStart(); } }
        public virtual void End()
        {
            _age = (_speed >= 0) ? Duration : 0;
            Update();
            RaiseOnComplete();
            if (_state == States.Play) { onComplete = null; _loopsSoFar = -1; return; }
            if (LoopCount > 0 && ++_loopsSoFar >= LoopCount) { return; }
            if (_state == States.Loop) { _age = 0; }
            else if (_state == States.PingPong) { Reverse(); }
        }
        public void PlayForward() { _speed = Mathf.Abs(_speed); _state = States.Play; }
        public void PlayBackward() { _speed = -Mathf.Abs(_speed); _state = States.Play; }
        public void Reverse() { _speed = -_speed; }
        public void Rewind() { Start(); _state = States.Pause; _loopsSoFar = -1; }
        public void Pause() { _state = States.Pause; }
        public void Play() { _state = States.Play; _loopsSoFar = -1; }

        public void Stop()
        {
            if (_state == States.Stop) { return; }
            if (_age > Duration || _speed < 0) { End(); }
            if (_state == States.Play)
            {
                _state = States.Stop;
            }
        }

        public virtual void Dispose(bool disposeAll) {}
        public void Dispose() { Dispose(true); }
        public virtual bool isDisposable { get { return !_persistent && (((_speed >= 0) ? _age >= Duration : _age <= 0) && onComplete == null && _state != States.Infinite); } }

        // property chaining
        public BaseTween Tag(string t) { tags.Add(t); Tween.AddTag(this,t); return (this); }
        public BaseTween Channel(int c) { Tween.RemoveFromTargetChannel(this); channel = c; Tween.AddToTargetChannel(this); return (this); }
        public BaseTween OnStart(Action a) { onStart = a; return (this); }
        public BaseTween OnComplete(Action a) { onComplete = a; return (this); }
        public BaseTween OnCompleteTarget(Action<object> a) { onComplete = () => a(Target); return (this); }
        public BaseTween OnUpdate(Action<float> a) { onUpdate = a; return (this); }

        public BaseTween Easing(EasingFunction f) { easing = f; return (this); }
        public BaseTween Easing(EaseFunction f) { easing = Mathfl.GetEaseFunction(f); return (this); }
        public BaseTween Easing(EaseShape s, EaseMode t) { easing = Mathfl.GetEaseFunction(s,t); return (this); }
        public BaseTween Curve(Func<float, float> cur) { _curve = cur; return (this); }
        public BaseTween Delay(float t) { _age -= t; return (this); }
        public BaseTween AtSpeed(float t) { _speed = t; return (this); }
        public BaseTween Loop(int count = 0) { _state = States.Loop; LoopCount = count; _loopsSoFar = -1; return (this); }
        public BaseTween PingPong(int count = 0) { _state = States.PingPong; LoopCount = count*2; _loopsSoFar = -1; return (this); }
        public BaseTween Infinite() { _state = States.Infinite; return (this); }
        public BaseTween Persist() { _persistent = true; return (this); }

        public BaseTween NullCheck(bool val = true) { _nullCheck = val; return (this); }
        public IEnumerator Block()
        {
            while (!isDisposable)
            {
                yield return null;
            }
        }
    }
}
