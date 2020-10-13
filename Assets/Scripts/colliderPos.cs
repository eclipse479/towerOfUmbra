using UnityEngine;

public class colliderPos : MonoBehaviour
{
    private GameObject thePlayer;
    public float distance;
    public float height;
    // Start is called before the first frame update
    void Start()
    {
        thePlayer = GameObject.FindGameObjectWithTag("player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = thePlayer.transform.position + (thePlayer.transform.forward * distance) + (thePlayer.transform.up * height);
    }
}
