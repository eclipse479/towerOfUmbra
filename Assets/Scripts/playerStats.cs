using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class playerStats
{
    private static float playerHealth;

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
}
