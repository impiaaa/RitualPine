using System;
using System.Collections.Generic;
using System.Globalization;
using FableLabs;
using FableLabs.Anim;
using FableLabs.Collections;
using FableLabs.Util;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class AudioBankEntry
{
    public string Name;
    public AudioClip Clip;
    public float Pitch; // -1 to 1
    public float Volume; // -1 to 1
    public float Weight; // -1 to 1

    public float SplitNormalized(float v, float min, float mid, float max)
    {
        if (v < 0) { return Mathf.Lerp(mid, min, -v); }
        return Mathf.Lerp(mid, max, v);
    }

    public float NormalizedVolume { get { return SplitNormalized(Volume, 0.0f, 0.5f, 1.0f); } }
    public float NormalizedPitch { get { return SplitNormalized(Pitch, 0.10f, 1f, 10.0f); } }
    public float NormalizedWeight { get { return SplitNormalized(Weight, 0.10f, 1f, 10.0f); } }

    public void Reset()
    {
        Pitch = 0;
        Volume = 0;
        Weight = 0;
    }
}

public enum AudioBankMode
{
    Indexed,
    Cycle,
    Random,
}


public class AudioBank : MonoBehaviour
{
    public List<AudioBankEntry> Entries = new List<AudioBankEntry>();
    public int MaxVoices = 1;
    public float CrossFadeOverlap = 0.5f;
    public float CrossFadeTime = 0.0f;
    public bool Loop;
    public GameObject Source;
    public AudioBankMode Mode;

    private WeightTable<int> _weights = new WeightTable<int>();
    private int _lastIndex;
    private List<AudioSource> _sources = new List<AudioSource>();

    [UsedImplicitly]
    private void Awake()
    {
        _weights.IdentityExtractor = i => i.ToString(CultureInfo.InvariantCulture);
        //transform.DestroyAllChildren();
        for (int i = transform.childCount - 1;i>=0 ; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public bool Contains(string file) { return Get(file) != null; }
    public AudioBankEntry Get(string file, int index = 0)
    {
        int nth = 0;
        for (int i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].Name == file)
            {
                if (nth++ == index)
                {
                    return Entries[i];
                }
            }
        }
        return null;
    }

    public void Refresh()
    {
        _weights.Clear();
        for (int i = 0; i < Entries.Count; i++)
        {
            _weights.Add(i, Entries[i].NormalizedWeight);
        }
    }

    public void Update()
    {
        if (Loop)
        {
            AudioSource a;
            while ((a = GetInactiveAudioSource()) != null) { PlaySource(a, a.volume); }
        }
    }

    private AudioSource GetYoungestAudioSource() { return GetBestAudioSource(a => !a.isPlaying ? 0 : a.time); }
    private AudioSource GetOldestAudioSource() { return GetBestAudioSource(a => !a.isPlaying ? 0 : a.time); }
    private AudioSource GetBestAudioSource(Func<AudioSource, float> heuristic, bool inv = false)
    {
        float best = float.NegativeInfinity;
        AudioSource src = null;
        int count = Math.Min(_sources.Count, MaxVoices);
        for (int i = 0; i < count; i++)
        {
            var t = heuristic(_sources[i]);
            if (inv && t < best) { src = _sources[i];}
            if (t > best) { src = _sources[i]; best = t; }
        }
        return src;
    }

    private AudioSource GetInactiveAudioSource() { return GetFirstAudioSource(a => !a.isPlaying); }
    private AudioSource GetFirstAudioSource(Func<AudioSource, bool> heuristic)
    {
        int count = Math.Min(_sources.Count, MaxVoices);
        for (int i = 0; i < count; i++)
        {
            if (heuristic(_sources[i])) { return _sources[i]; }
        }
        return null;
    }

    private AudioSource InstantiateAudioSource()
    {
        Debug.Log("Instantiating");
        if (Source == null) { Debug.LogError("Cannot play without a source prefab"); return null; }
        GameObject inst = Instantiate(Source);
        if (inst == null) { Debug.LogError("Error instantiating audio source"); return null; }
        inst.transform.SetParent(transform);
        AudioSource src = inst.GetComponent<AudioSource>();
        if (src == null) { Debug.LogError("Audio source prefab must contain an AudioSource Component"); return null; }
        return src;
    }

    public void Play(string file, int index = 0) { Play(file, Vector3.zero, index); }
    public void Play(string file, Vector3 pos, int index = 0)
    {
        AudioBankEntry entry;
        switch (Mode)
        {
            case AudioBankMode.Indexed: entry = Get(file, index); break;
            case AudioBankMode.Cycle: entry = Entries[_lastIndex++%Entries.Count]; break;
            case AudioBankMode.Random: entry = Entries[_weights.GetRandom()]; break;
            default: throw new ArgumentOutOfRangeException();
        }
        if (entry == null) { Debug.LogWarning("Audio clip not found in bank: " + file); return; }
        AudioSource src = GetInactiveAudioSource();
        if (src == null)
        {
            if (_sources.Count < MaxVoices)
            {
                src = InstantiateAudioSource();
                _sources.Add(src);
            }
            else
            {
                src = GetYoungestAudioSource();
                if (src == null) { return; }
                if (CrossFadeTime > 0)
                {
                    var deadsrc = src;
                    //Tween.To(deadsrc, "volume", 0.0f, CrossFadeTime).OnComplete(() => Destroy(deadsrc.gameObject));
                    deadsrc.name = "(fadeout) " + file;
                    _sources.Remove(deadsrc);
                    src = InstantiateAudioSource();
                    _sources.Add(src);
                }
                else
                {
                    src.Stop();
                }
            } 
        }
        src.clip = entry.Clip;
        src.name = _sources.FindIndex(s => s == src) + " " + file;
        src.pitch = entry.NormalizedPitch;
        src.transform.position = pos;
        PlaySource(src, entry.NormalizedVolume);
    }

    private void PlaySource(AudioSource src, float volume)
    {
        if (CrossFadeTime > 0)
        {
            src.volume = 0;
            Tween.FromTo(src, f => src.volume = f, 0.0f, volume, CrossFadeTime).Delay((1.0f - CrossFadeOverlap) * CrossFadeTime);
        }
        else
        {
            src.volume = volume;
        }
        src.Play();
    }

    public string GetUniqueName(string attempt)
    {
        var a = attempt;
        int suf = 1;
        for (int i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].Name == a) { a = attempt + (suf++); }
        }
        return a;
    }

    public static AudioBank FindBank(string name)
    {
        var ab = FindObjectsOfType<AudioBank>();
        for (int i = 0; i < ab.Length; i++)
        {
            if (ab[i].name == name) { return ab[i]; }
        }
        return null;
    }

    public static AudioBank FindBankByFile(string file)
    {
        var ab = FindObjectsOfType<AudioBank>();
        for (int i = 0; i < ab.Length; i++)
        {
            if (ab[i].Contains(file)) { return ab[i]; }
        }
        return null;
    }

    public static void PlayGlobal(string file) { PlayGlobal(file, Vector3.zero); }
    public static void PlayGlobal(string file, Vector3 pos)
    {
        var b = FindBankByFile(file);
        if (b != null) { b.Play(file, pos); }
    }
}
