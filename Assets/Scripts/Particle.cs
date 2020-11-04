using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Particle 
{
    [Tooltip("Name to be referenced")] public string name;

    [Tooltip("Actual Particle")] public ParticleSystem particle;

    [Tooltip("Play when spawned in")] public bool play_on_awake = false;

    [Tooltip("How quickly the particles emit")] public float emission_rate = 1.0f;

    [Tooltip("Size of the particle emissions")] public float emission_size = 1.0f;

    [Tooltip("How long the particle plays for")] public float duration = 1.0f;

    [Tooltip("Does the particle loop?")] public bool loop = false;

    [Tooltip("Destroy when done?")] public ParticleSystemStopAction particle_end;
}
