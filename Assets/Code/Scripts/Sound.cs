using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public static class Tag
    {
        public const string Music = "music";
        public const string SFX = "sfx";
    }

    public static Sound Main { get; private set; }
    private Dictionary<string, AudioSource> sources = new();
    private Dictionary<string, float> localVolumes = new();

    void Awake()
    {
        if(Main != null) return;

        // TODO player prefs
        // naahhh
        Volume = 1;

        DontDestroyOnLoad(this); 
        DontDestroyOnLoad(this.gameObject); 
        Main = this;
    }

    private float volume;
    public float Volume {
        get { return volume; }
        set { volume = Mathf.Clamp01(value); UpdateAllVolumes(); } }

    public float CalculateVolume(float local) => Mathf.Lerp(0, Volume, Mathf.Clamp01(local));

    public void LocalVolume(string tag, float volume)
    {
        EnsureSource(tag);
        volume = Mathf.Clamp01(volume);
        localVolumes[tag] = volume;
    }

    private void UpdateAllVolumes()
    {
        foreach(var key in sources.Keys) UpdateVolume(key);
    }

    private void UpdateVolume(string tag)
    {
        EnsureSource(tag);
        sources[tag].volume = CalculateVolume(localVolumes[tag]);
    }

    public void Play(string resourcePath, string tag, bool oneShot)
    {
        var clip = Resources.Load<AudioClip>(resourcePath);
        if(clip is null) return;
        Play(clip, tag, oneShot);
    }

    public void Play(AudioClip clip, string tag, bool oneShot)
    {
        EnsureSource(tag);
        var source = sources[tag];
        source.Stop();
        if(oneShot) source.PlayOneShot(clip); else { source.clip = clip; source.Play(); }
    }

    private void EnsureSource(string tag)
    {
        if(!localVolumes.ContainsKey(tag)) localVolumes[tag] = 1;
        if(sources.ContainsKey(tag)) return;
        var source = Instantiate(new GameObject(tag), this.transform);
        var asource = source.AddComponent<AudioSource>();
        sources[tag] = asource;
    }
}
