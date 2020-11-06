using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class playerStats
{
    private static float playerHealth;
    private static float playerMaxHealth;
    private static int noOfClicks;

    public static float health
    {
        get
        {
            return playerHealth;
        }
        set
        {
            playerHealth = value;
        }
    }
    public static float maxHealth
    {
        get
        {
            return playerMaxHealth;
        }
        set
        {
            playerMaxHealth = value;
        }
    }
    public static int clicks
    {
        get
        {
            return noOfClicks;
        }
        set
        {
            noOfClicks = value;
        }
    }
}
