using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    public GameObject player;
    [Header("CAMERA POSITION")]
    [Tooltip("distance on the X axis")]
    public float leftRight;
    [Tooltip("distance on the Y axis")]
    public float upDown;
    [Tooltip("distance on the Z axis *NOTE* must restart game to see results")]
    public float distanceFromPlayer = 5;
    private Camera theCamera;
    // Start is called before the first frame update
    void Start()
    {
        theCamera = gameObject.GetComponent<Camera>();
        theCamera.orthographicSize = distanceFromPlayer;
    }

    private void LateUpdate()
    {
        //camera follows player
        transform.position = new Vector3(player.transform.position.x - leftRight, player.transform.position.y + upDown, player.transform.position.z + distanceFromPlayer);
        transform.LookAt(new Vector3(player.transform.position.x - leftRight, player.transform.position.y + upDown, player.transform.position.z + distanceFromPlayer));
    }
}
