/***********************************
 *    Script: ParticleManager
 *    
 *    Purpose: Containing all the particles
 *    and allowing easy-to-edit particles
 * 
 * *********************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticleManager : MonoBehaviour
{
    public Particle[] particles;

    List<ParticleSystem> particle_effect;
 
    public static ParticleManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        // Add the particle systems to the list
        foreach (Particle p in particles)
        {
            /* Local copies of the particle systems */
            var emission = p.particle.main; // Create a local cache for each particle
            var emission_rate = p.particle.emission; // Create a local emission_rate cache to edit particles

            emission.playOnAwake = p.play_on_awake; // If it plays on awake.
            emission_rate.rateOverTime = p.emission_rate; // Particles emitted per second
            emission.startSize = p.emission_size;
            emission.loop = p.loop; // Replay?
            emission.duration = p.duration; // How long it plays
            emission.stopAction = p.particle_end;
        }
    }

    public ParticleSystem addParticle(string particle_name, Vector3 position, Quaternion rotation)
    {
        Particle p = Array.Find(particles, particle => particle.name == particle_name);
        p.particle.transform.position = position;

        if (p == null)
        {
            Debug.Log("Warning: Particle" + particle_name + " not found!");
        }

        return Instantiate(p.particle, position, rotation);
    }
}
