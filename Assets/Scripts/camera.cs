using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //camera follows player
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z + 10);
    }
}
