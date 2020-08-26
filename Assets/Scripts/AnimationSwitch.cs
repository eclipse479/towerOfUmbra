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

    private void Awake()
    {
        state = GetComponent<EnemyBehaviour>();
        animation = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check to see which actions are being performed
        switch (state.State)
        {
            case EnemyBehaviour.STATE.SHOOT:
                
                animation.SetTrigger("Shoot");
                break;
            case EnemyBehaviour.STATE.ATTACK:
                    animation.SetTrigger("Attack");
                break;
            case EnemyBehaviour.STATE.CHASING:
                    animation.SetBool("Walk", true);
                break;
            case EnemyBehaviour.STATE.WALKING:
                    animation.SetBool("Walk", true);
                break;
            default:
                animation.SetBool("Walk", false);
                animation.SetBool("Idle", true);
                break;
        }
    }
}
