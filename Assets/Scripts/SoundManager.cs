using UnityEngine.Audio;
using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public Sound[] sounds;
    public static SoundManager instance;
    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();  //creates the audio source
            s.source.clip = s.clip;                             //gives the audio source a sound
            s.source.volume = s.volume;                         //changes the initial volume of the sound
            s.source.pitch = s.pitch;                           //changes the initial pitch of the sound
            s.source.loop = s.loop;                             //will the sound loop
        }
        playSound("theme");
    }

 public void playSound(string soundName)
    {
        
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if(s == null)
        {
            Debug.LogWarning("sound " + soundName + " not found");
            return;
        }
        s.source.pitch = UnityEngine.Random.Range(s.pitch - s.pitchRandomizer, s.pitch + s.pitchRandomizer);
        s.source.Play();
    }
}
