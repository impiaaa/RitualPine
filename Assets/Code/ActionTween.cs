using System;

namespace FableLabs.Anim
{
    public class ActionTween<T> : BaseTween
    {
        public static Func<T, T, float, T> Interpolator;

        private Action<T> _action;
        private Action<BaseTween> _advancedAction;
        protected T _from;
        protected T _to;

        public virtual T From { get { return _from; } }
        public virtual T To { get { return _to; } }

        public Action<T> action
        {
            get { return _action; }
            set { _action = value; }
        }

        public Action<BaseTween> advancedAction
        {
            get { return _advancedAction; }
            set { _advancedAction = value; }
        }

        // the current value used to apply
        public T value { get { return Lerp(_position); } }

        public T lastValue
        {
            get { return Lerp(_lastPosition); }
        }

        public ActionTween() { }

        /*
         * Convenience function to set up a tween the common way
         */
        public ActionTween<T> Tween(T f, T t, float d)
        {
            _from = f;
            _to = t;
            _duration = d;
            _position = _lastPosition = 0;
            _age = 0;
            _speed = 1;
            _state = States.Play;
            return this;
        }

        private T Lerp(float t) { return Interpolator(From, To, t); }

        protected override void Apply()
        {
            base.Apply();
            if (_nullCheck && (Target as UnityEngine.Object) == null) { Dispose(); return; }
            Apply(Lerp(_position));
        }

        protected void Apply(T v)
        {
            try
            {
                //if (_nullCheck && (Target as UnityEngine.Object) == null) { Dispose(); return; }
                if (_advancedAction != null) { _advancedAction(this); }
                if (_action != null) { _action(v); }
                if (onUpdate != null) { onUpdate(_position); }
            }
            catch (Exception e)
            {
                string errMsg = "Error applying tween with tags: ";
                if (tags != null)
                {
                    for (int i = 0; i < +tags.Count; i++)
                    {
                        errMsg += tags[i] + ",";
                    }
                }
                //Log.Warning(string.Format("Error Applying Tween {0}: {1}", this, e));
                //Log.Error(string.Format("Error Applying Tween {0}: {1}", this, e));
            }
        }

        /*
        public override void Start()
        {
            //age = (speed > 0) ? 0 : duration;
            _position = (_speed > 0) ? 0 : 1.0f;
            //Apply((speed > 0) ? from : to);
            _age = Position * _duration;
            Apply();
        }

        public override void End()
        {
            //Apply((_speed > 0) ? _to : _from); // What does this break again?
            base.End();
            Apply();
        }
        */

        public override void Dispose(bool disposeAll)
        {
            _age = _duration;
            if (!disposeAll) { return; }
            _action = null;
            _advancedAction = null;
            onComplete = null;
        }
    }
    
}
