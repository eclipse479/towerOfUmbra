using UnityEngine.Audio;
using UnityEngine;
using System;

[Serializable]
public class Sound
{
    [Tooltip("What the sound is called")]
    public string name;
    [Tooltip("the actual audio clip")]
    public AudioClip clip;
    [Range(0.1f,3)]
    [Tooltip("base pitch for the sound")]
    public float pitch;
    [Tooltip("small value either added or subtracted from the pitch so it is different each time")]
    public float pitchRandomizer;
    [Range(0.1f,1)]
    [Tooltip("how loud the sound will be")]
    public float volume;
    [Tooltip("will the sound loop")]
    public bool loop;
    [HideInInspector]
    public AudioSource source;
}
