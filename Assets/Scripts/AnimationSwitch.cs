﻿/*
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
    bool knockback;
    bool stun;

    private void Awake()
    {
        state = GetComponent<EnemyBehaviour>();
        animation = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check to see which actions are being performed
        if (!state.IsDead)
        {
             if (!state.isStunned && !state.IsDizzy)
             {
                knockback = false;
                stun = false;
                animation.SetBool("stun", stun);
                animation.SetFloat("Stun Time", 0.0f);
                switch (state.State)
                 {
                     case EnemyBehaviour.STATE.SHOOT:
                         // Check the shooting trigger
                         if (state.canShoot)
                         {
                             state.canShoot = false;
                             state.isAttacking = false;
                             animation.SetTrigger("Shoot");
                         }
                         break;
                     case EnemyBehaviour.STATE.ATTACK:
                             state.IsShooting = false;
                        if (state.stun_duration < 0)
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
             else if (state.isStunned)
             {
                if (!knockback)
                {
                    animation.SetTrigger("Knockback");
                    knockback = true;
                }
             }
             else
             {
                knockback = false;
                if (state.StunTime > 0.0)
                {
                    stun = true;
                    animation.SetBool("stun", stun);
                }
                
                if (stun)
                    animation.SetFloat("Stun Time", state.StunTime);
             }
        }

    }


}
