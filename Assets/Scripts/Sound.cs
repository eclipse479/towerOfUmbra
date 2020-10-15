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
    [Tooltip("a value that is added and subtracted to get a range of pitch that is randomly selected each time it is played")]
    public float pitchRandomizer;
    [Range(0.1f,1)]
    [Tooltip("how loud the sound will be")]
    public float volume;
    [Tooltip("will the sound loop")]
    public bool loop;
    [HideInInspector]
    public AudioSource source;
}
