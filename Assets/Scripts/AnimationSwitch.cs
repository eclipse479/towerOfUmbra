/*
 * Script: AnimationSwitch
 * Author: Nixon Sok
 * 
 * Purpose: Work with the A.I script to determine the animation to be played.
 * 
 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSwitch : MonoBehaviour
{
    EnemyBehaviour state;
    Animator animation;

    // Start is called before the first frame update
    void Start()
    {
        state = GetComponent<EnemyBehaviour>();
        animation = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
