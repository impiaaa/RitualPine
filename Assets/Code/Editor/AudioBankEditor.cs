using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioBank))]
public class AudioBankEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AudioBank c = target as AudioBank;
        if (c == null) { return; }

        c.Source = EditorGUILayout.ObjectField("Source", c.Source, typeof (GameObject), false) as GameObject;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Max Voices");
        c.MaxVoices = EditorGUILayout.IntField(c.MaxVoices);
        c.Loop = EditorGUILayout.Toggle(c.Loop, GUILayout.Width(20));
        GUILayout.Label("Loop", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
        c.Mode = (AudioBankMode)EditorGUILayout.EnumPopup("Select Mode", c.Mode);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Crossfade");
        c.CrossFadeTime = EditorGUILayout.FloatField(c.CrossFadeTime);
        c.CrossFadeOverlap = EditorGUILayout.Slider(c.CrossFadeOverlap, 0.0f, 1.0f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(34);
        GUILayout.Label("Name", GUILayout.Width(80));
        GUILayout.Label("Clip", GUILayout.ExpandWidth(true));
        GUILayout.Label("Pitch", GUILayout.Width(50));
        GUILayout.Label("Volume", GUILayout.Width(50));
        GUILayout.Label("Weight", GUILayout.Width(50));
        GUILayout.Space(46);
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < c.Entries.Count; i++)
        {
            var entry = c.Entries[i];
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(entry.Clip == null);
            if (GUILayout.Button("►", GUILayout.Width(30))) { if (entry != null) { c.Play(entry.Name); } }
            EditorGUI.EndDisabledGroup();

            entry.Name = EditorGUILayout.TextField(entry.Name, GUILayout.Width(80));

            var nc = EditorGUILayout.ObjectField(entry.Clip, typeof(AudioClip), false) as AudioClip;
            if (nc != entry.Clip) { entry.Clip = nc; if (entry.Name == null && nc != null) { entry.Name = c.GetUniqueName(nc.name.ToLower()); } }

            entry.Pitch = GUILayout.HorizontalSlider(entry.Pitch, -1f, 1f, GUILayout.Width(50));
            entry.Volume = GUILayout.HorizontalSlider(entry.Volume, -1f, 1f, GUILayout.Width(50));
            EditorGUI.BeginChangeCheck();
            entry.Weight = GUILayout.HorizontalSlider(entry.Weight, -1f, 1f, GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck()) { c.Refresh(); }
            if (GUILayout.Button("R", GUILayout.Width(20))) { c.Entries[i].Reset(); }
            if (GUILayout.Button("X", GUILayout.Width(20))) { c.Entries.RemoveAt(i); }
            
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+"))
        {
            c.Entries.Add(new AudioBankEntry());
            c.Refresh();
        }
    }
}
