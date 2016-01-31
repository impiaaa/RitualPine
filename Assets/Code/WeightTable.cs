using System;
using System.Collections;
using System.Collections.Generic;
using FableLabs.Util;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FableLabs.Collections
{

    // NOTE: this class used to not require T : IIdentifiable and
    // there was only one dictionary where T is the key. but we run into problems
    // with JIT compiling on iOS device so we added a separate layer of lookup
    // to avoid that issue.

    public class WeightTable<T> : IEnumerable<T>
    {
        public float totalWeight;
        public float Average { get { return (Count > 0) ? totalWeight / Count : 0; } }
        public float Highest = float.MinValue;
        public float Lowest = float.MaxValue;

        public Dictionary<string, T> lookup;
        public Dictionary<string, float> weights;
        public Func<T, string> IdentityExtractor;
        private float _lastThreshold;
        private float _lastThresholdTotal;

        public WeightTable()
        {
            totalWeight = 0.0f;
            lookup = new Dictionary<string, T>();
            weights = new Dictionary<string, float>();
        }

        private void RecalculateStatistics()
        {
            Lowest = float.MaxValue;
            Highest = float.MinValue;
            foreach (KeyValuePair<string, float> pair in weights)
            {
                float f = pair.Value;
                if (f < Lowest) { Lowest = f; }
                if (f > Highest) { Highest = f; }
            }
        }

        public float GetThresholdTotal(float threshold)
        {
            float total = 0;
            foreach (KeyValuePair<string, float> weight in weights)
            {
                if (weight.Value >= threshold) { total += weight.Value; }
            }
            return total;
        }

        public int GetThresholdCount(float threshold)
        {
            int count = 0;
            foreach (KeyValuePair<string, float> weight in weights)
            {
                if (weight.Value >= threshold) { count++; }
            }
            return count;
        }

        public float GetThresholdAverage(float threshold)
        {
            int count = GetThresholdCount(threshold);
            if (count == 0) { return 0; }
            return GetThresholdTotal(threshold) / count;
        }

        public T GetAtIndex(float index, float threshold = 0)
        {
            float accum = 0.0f;
            foreach (KeyValuePair<string, float> weight in weights)
            {
                float w = weight.Value;
                if (w < threshold) { continue; }
                if (index >= accum && index < accum + w)
                {
                    return lookup[weight.Key];
                }
                accum += w;
            }
            return default(T);
        }

        public float GetWeight(T item) { return GetWeight(GetId(item)); }
        public float GetWeight(string id)
        {
            float f;
            weights.TryGetValue(id, out f);
            return f;
        }

        private string GetId(T item)
        {
            IIdentifiable ident = item as IIdentifiable;
            if (ident != null) { return ident.GetId(); }
            if (IdentityExtractor != null) { return IdentityExtractor(item); }
            return item.ToString();
        }

        public T GetRandom()
        {
            float index = Random.value * totalWeight;
            return GetAtIndex(index);
        }

        public T GetRandom(float threshold)
        {
            if (Mathf.Abs(_lastThreshold - threshold) > Mathf.Epsilon || true)
            {
                _lastThresholdTotal = GetThresholdTotal(threshold);
                _lastThreshold = threshold;
            }
            return GetAtIndex(_lastThresholdTotal * Random.value, _lastThreshold);
        }

        public void Multiply(T item, float factor = 1)
        {
            string key = GetId(item);
            if (!weights.ContainsKey(key)) { return; } // error
            float current = weights[key];
            float modified = current * factor;
            totalWeight += modified - current;
            weights[key] = modified;
        }

        public void Add(T item, float weight = 1, string ikey = "")
        {
            string key = ikey;
            if (string.IsNullOrEmpty(ikey))
                key = GetId(item);

            if (!weights.ContainsKey(key))
            {
                lookup.Add(key, item);
                weights.Add(key, 0);
            }
            totalWeight += weight;
            weights[key] += weight;
            if (weight < Lowest) { Lowest = weight; }
            if (weight > Highest) { Highest = weight; }
        }

        public void Remove(T item)
        {
            string key = GetId(item);
            if (!weights.ContainsKey(key)) { return; }
            totalWeight -= weights[key];
            lookup.Remove(key);
            weights.Remove(key);
            lookup.Remove(key);
            RecalculateStatistics();
        }

        public void Clear()
        {
            totalWeight = 0;
            weights.Clear();
            lookup.Clear();
            RecalculateStatistics();
        }

        public bool Contains(T item)
        {
            return lookup.ContainsKey(GetId(item));
        }

        public int Count
        {
            get { return weights.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return lookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }

}
