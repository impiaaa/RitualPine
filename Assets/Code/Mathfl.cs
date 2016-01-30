/*
TERMS OF USE - EASING EQUATIONS
Open source under the BSD License.
Copyright (c)2001 Robert Penner
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using FableLabs.Anim;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FableLabs.Util
{


    public enum EaseFunction
    {
        InOutQuad,
        InOutCubic,
        InOutQuartic,
        InOutQuintic,
        InOutSine,
        InOutExpo,
        InOutCircular,
        InOutBounce,
        InOutBack,
        InOutElastic,

        InQuad,
        InCubic,
        InQuartic,
        InQuintic,
        InSine,
        InExpo,
        InCircular,
        InBounce,
        InBack,
        InElastic,

        OutQuad,
        OutCubic,
        OutQuartic,
        OutQuintic,
        OutSine,
        OutExpo,
        OutCircular,
        OutBounce,
        OutBack,
        OutElastic,

        None
    }

    public enum EaseShape
    {
        Quad,
        Cubic,
        Quartic,
        Quintic,
        Sine,
        Expo,
        Circular,
        Bounce,
        Back,
        Elastic
    }

    public enum EaseMode
    {
        InOut,
        In,
        Out,
        None,
    }


    public static class Mathfl
    {
        static public EaseFunction MakeEaseFunction(EaseShape shape, EaseMode mode = EaseMode.InOut)
        {
            if (mode == EaseMode.None) { return EaseFunction.None; }
            string[] shapes = Enum.GetNames(typeof (EaseShape));
            return (EaseFunction) ((int) shape + shapes.Length * (int)mode);
        }

        static public BaseTween.EasingFunction GetEaseFunction(EaseShape shape, EaseMode mode) { return GetEaseFunction(MakeEaseFunction(shape, mode)); }
        static public BaseTween.EasingFunction GetEaseFunction(EaseFunction func)
        {
            switch (func)
            {
                case EaseFunction.InQuad: return easeInQuad;
                case EaseFunction.InCubic: return easeInCirc;
                case EaseFunction.InQuartic: return easeInQuart;
                case EaseFunction.InQuintic: return easeInQuint;
                case EaseFunction.InSine: return easeInSine;
                case EaseFunction.InExpo: return easeInExpo;
                case EaseFunction.InCircular: return easeInCirc;
                case EaseFunction.InBounce: return easeInBounce;
                case EaseFunction.InBack: return easeInBack;
                case EaseFunction.InElastic: return easeInElastic;
                case EaseFunction.OutQuad: return easeOutQuad;
                case EaseFunction.OutCubic: return easeOutCubic;
                case EaseFunction.OutQuartic: return easeOutQuart;
                case EaseFunction.OutQuintic: return easeOutQuint;
                case EaseFunction.OutSine: return easeOutSine;
                case EaseFunction.OutExpo: return easeOutExpo;
                case EaseFunction.OutCircular: return easeOutCirc;
                case EaseFunction.OutBounce: return easeOutBounce;
                case EaseFunction.OutBack: return easeOutBack;
                case EaseFunction.OutElastic: return easeOutElastic;
                case EaseFunction.InOutQuad: return easeInOutQuad;
                case EaseFunction.InOutCubic: return easeInOutCubic;
                case EaseFunction.InOutQuartic: return easeInOutQuart;
                case EaseFunction.InOutQuintic: return easeInOutQuint;
                case EaseFunction.InOutSine: return easeInOutSine;
                case EaseFunction.InOutExpo: return easeInOutExpo;
                case EaseFunction.InOutCircular: return easeInOutCirc;
                case EaseFunction.InOutBounce: return easeInOutBounce;
                case EaseFunction.InOutBack: return easeInOutBack;
                case EaseFunction.InOutElastic: return easeInOutElastic;
                case EaseFunction.None: return null;
                default:
                    throw new ArgumentOutOfRangeException("func");
            }
        }

        static public Func<float, float> ToCurve(this BaseTween.EasingFunction f) { return v => f(0, 1, v); }
        static public BaseTween.EasingFunction ToEasingFunction(this Func<float, float> f) { return (s, e, v) => f(v) * (e-s) + s; }

        static public Vector3 ballisticLaunchSpeed(Vector3 origin, Vector3 destination, Vector3 gravity, float duration)
        {
            return (destination-origin)/duration-(gravity*duration)/2;
        }

        static public Vector3 isoToScreen(Vector3 iso, Vector3 unit)
        {
            Vector3 scr = new Vector3(
                unit.x * (-iso.x + -iso.z),
                unit.y * (-iso.x + iso.z) + iso.y * 0.5f, 
                unit.z * (iso.x + -iso.z));
            return scr;
        }

        static public Vector3 screenToIso(Vector3 scr, Vector3 unit, float yOff = 0.0f) 
        {
            Vector3 iso = new Vector3(
                (scr.y / unit.y + scr.x / unit.x) * -0.5f + yOff,
                yOff,
                (scr.y / unit.y - scr.x / unit.x ) * 0.5f - yOff);
            return iso;
        }

        static public float updateIsoZ(Vector3 scr, Vector3 unit, float yOff = 0.0f) 
        {
            Vector3 iso = screenToIso( scr, unit, yOff );
            return isoToScreen( iso, unit ).z;
        }

        static public float safeInverse(float f)
        {
            return (f > 0.0f) ? 1.0f/f : 1.0f;
        }

        static public float oscCos(float f) 
        {
            return Mathf.Cos(f * Mathf.PI * 2) * 0.5f + 0.5f;
        }

        static public float oscSin(float f)
        {
            return Mathf.Sin(f * Mathf.PI * 2) * 0.5f + 0.5f;
        }
	
        static public float oscTri(float f)
        {
            f = f%1;
            return (f >= 0.5f) ? 1 - ((f - 0.5f) * 2) : f*2;
        }

        static public float solid(float f)
        {
            return 1;
        }

        static public float fadeInOut(float f, float intime, Func<float,float> inf, float outtime, Func<float,float> outf)
        {
            return (f < intime) ? (inf ?? linear)(Mathf.InverseLerp(0, intime, f))
                : (f > (1 - outtime)) ? (outf ?? linear)(Mathf.InverseLerp(1, 1 - outtime, f))
                : 1;
        }
        static public float fadeInOut(float f, float intime, BaseTween.EasingFunction inf, float outtime, BaseTween.EasingFunction outf)
        {
            return fadeInOut(f, intime, inf != null ? inf.ToCurve() : null, outtime, outf != null ? outf.ToCurve() : null);
        }

        static public float oscInverseSaw(float f)
        {
            f = 1 - f % 1;
            return f;
        }

        static public float oscSaw(float f)
        {
            f = f%1;
            return f;
        }
	
        static public float oscSqr(float f,float pw=0.5f)
        {
            f = f%1;
            return (f > pw) ? 1 : 0;
        }
	
        static public float oscNoise(float f)
        {
            return Random.value;
        }
	
        static public float oscPerlin(float f,float row=0.0f)
        {
            return Mathf.PerlinNoise(f,row);
        }

        static public float easeSin(float s,float e,float f)
        { 
            return oscSin(f);
        }

        /// <summary>
        /// Used for overshoot-and-return
        /// https://www.wolframalpha.com/input/?i=f%28x%29+%3D+sin%28x%5E0.5+*+PI%29+x+from+0+to+1
        /// </summary>
        /// <param name="str">Exponent Speedup strength: Less than 1 for lean right, Greater than 1 for lean left</param>
        /// <param name="cycles">Number of times to cross the 0-line</param>
        /// <returns></returns>
        static public Func<float, float> MakeLeaningSin(float str = 1.0f, float cycles = 1.0f)
        {
            return f => Mathf.Sin(Mathf.Pow(f * (Mathf.Pow(cycles, str)), 1/str) * Mathf.PI);
        }

        static public float linear(float value)
        {
            return value;
        }

        static public float linear(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value);
        }

        static public float Remap(float outfrom, float outto, float infrom, float into, float inval)
        {
            return Mathf.Lerp(outfrom, outto, Mathf.InverseLerp(infrom, into, inval));
        }

        static public int RingMod(int x, int size)
        {
            return (x + size * (Math.Abs(x) / size + 1)) % size;
        }
        static public float RingMod(float x, float size)
        {
            return (x + size * Mathf.Ceil(Math.Abs(x) / size)) % size;
        }

        static public int DivCeilInt(int x, int y)
        {
            return x % y != 0 ? x / y + 1 : x / y;
        }
	
        static public float clerp(float start, float end, float value)
        {
            float min = 0.0f;
            float max = 360.0f;
            float half = Mathf.Abs((max - min) / 2.0f);
            float retval = 0.0f;
            float diff = 0.0f;
            if ((end - start) < -half){
                diff = ((max - start) + end) * value;
                retval = start + diff;
            }else if ((end - start) > half){
                diff = -((max - end) + start) * value;
                retval = start + diff;
            }else retval = start + (end - start) * value;
            return retval;
        }

        static public float spring(float start, float end, float value)
        {
            value = Mathf.Clamp01(value);
            value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
            return start + (end - start) * value;
        }

        static public float easeInQuad(float start, float end, float value)
        {
            end -= start;
            return end * value * value + start;
        }

        static public float easeOutQuad(float start, float end, float value)
        {
            end -= start;
            return -end * value * (value - 2) + start;
        }

        static public float easeInOutQuad(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end / 2 * value * value + start;
            value--;
            return -end / 2 * (value * (value - 2) - 1) + start;
        }

        static public float easeInCubic(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value + start;
        }

        static public float easeOutCubic(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value + 1) + start;
        }

        static public float easeInOutCubic(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end / 2 * value * value * value + start;
            value -= 2;
            return end / 2 * (value * value * value + 2) + start;
        }

        static public float easeInQuart(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value + start;
        }

        static public float easeOutQuart(float start, float end, float value)
        {
            value--;
            end -= start;
            return -end * (value * value * value * value - 1) + start;
        }

        static public float easeInOutQuart(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end / 2 * value * value * value * value + start;
            value -= 2;
            return -end / 2 * (value * value * value * value - 2) + start;
        }

        static public float easeInQuint(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value * value * value + start;
        }

        static public float easeOutQuint(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value * value * value + 1) + start;
        }

        static public float easeInOutQuint(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end / 2 * value * value * value * value * value + start;
            value -= 2;
            return end / 2 * (value * value * value * value * value + 2) + start;
        }

        static public float easeInSine(float start, float end, float value)
        {
            end -= start;
            return -end * Mathf.Cos(value / 1 * (Mathf.PI / 2)) + end + start;
        }

        static public float easeOutSine(float start, float end, float value)
        {
            end -= start;
            return end * Mathf.Sin(value / 1 * (Mathf.PI / 2)) + start;
        }

        static public float easeInOutSine(float start, float end, float value)
        {
            end -= start;
            return -end / 2 * (Mathf.Cos(Mathf.PI * value / 1) - 1) + start;
        }

        static public float easeInExpo(float start, float end, float value)
        {
            end -= start;
            return end * Mathf.Pow(2, 10 * (value / 1 - 1)) + start;
        }

        static public float easeOutExpo(float start, float end, float value)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
        }

        static public float easeInOutExpo(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end / 2 * Mathf.Pow(2, 10 * (value - 1)) + start;
            value--;
            return end / 2 * (-Mathf.Pow(2, -10 * value) + 2) + start;
        }

        static public float easeInCirc(float start, float end, float value)
        {
            end -= start;
            return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
        }

        static public float easeOutCirc(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * Mathf.Sqrt(1 - value * value) + start;
        }

        static public float easeInOutCirc(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return -end / 2 * (Mathf.Sqrt(1 - value * value) - 1) + start;
            value -= 2;
            return end / 2 * (Mathf.Sqrt(1 - value * value) + 1) + start;
        }

        static public float easeInBounce(float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            return end - easeOutBounce(0, end, d-value) + start;
        }
	
        static public float easeOutBounce(float start, float end, float value)
        {
            value /= 1f;
            end -= start;
            if (value < (1 / 2.75f)){
                return end * (7.5625f * value * value) + start;
            }else if (value < (2 / 2.75f)){
                value -= (1.5f / 2.75f);
                return end * (7.5625f * (value) * value + .75f) + start;
            }else if (value < (2.5 / 2.75)){
                value -= (2.25f / 2.75f);
                return end * (7.5625f * (value) * value + .9375f) + start;
            }else{
                value -= (2.625f / 2.75f);
                return end * (7.5625f * (value) * value + .984375f) + start;
            }
        }
	
        static public float easeInOutBounce(float start, float end, float value)
        {
            end -= start;
            float d = 1f;
            if (value < d/2) return easeInBounce(0, end, value*2) * 0.5f + start;
            else return easeOutBounce(0, end, value*2-d) * 0.5f + end*0.5f + start;
        }
	
        static public float easeInBack(float start, float end, float value)
        {
            end -= start;
            value /= 1;
            float s = 1.70158f;
            return end * (value) * value * ((s + 1) * value - s) + start;
        }

        static public float easeOutBack(float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value = (value / 1) - 1;
            return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
        }

        static public float easeInOutBack(float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value /= .5f;
            if ((value) < 1){
                s *= (1.525f);
                return end / 2 * (value * value * (((s) + 1) * value - s)) + start;
            }
            value -= 2;
            s *= (1.525f);
            return end / 2 * ((value) * value * (((s) + 1) * value + s) + 2) + start;
        }

        static public float punch(float amplitude, float value)
        {
            float s = 9;
            if (value == 0){
                return 0;
            }
            if (value == 1){
                return 0;
            }
            float period = 1 * 0.3f;
            s = period / (2 * Mathf.PI) * Mathf.Asin(0);
            return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
        }
	
        static public float easeInElastic(float start, float end, float value)
        {
            end -= start;
		
            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;
		
            if (value == 0) return start;
		
            if ((value /= d) == 1) return start + end;
		
            if (a == 0f || a < Mathf.Abs(end)){
                a = end;
                s = p / 4;
            }else{
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }
		
            return -(a * Mathf.Pow(2, 10 * (value-=1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        }		
	
        static public float easeOutElastic(float start, float end, float value)
        {
            end -= start;
		
            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;
		
            if (value == 0) return start;
		
            if ((value /= d) == 1) return start + end;
		
            if (a == 0f || a < Mathf.Abs(end)){
                a = end;
                s = p / 4;
            }else{
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }
		
            return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
        }		
	
        static public float easeInOutElastic(float start, float end, float value)
        {
            end -= start;
		
            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;
		
            if (value == 0) return start;
		
            if ((value /= d/2) == 2) return start + end;
		
            if (a == 0f || a < Mathf.Abs(end)){
                a = end;
                s = p / 4;
            }else{
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }
		
            if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value-=1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
            return a * Mathf.Pow(2, -10 * (value-=1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }
    }
}
