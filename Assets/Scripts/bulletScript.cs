using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public float bulletSpeed;
    public float lifeTime = 7;
    // Start is called before the first frame update
    void Start()
    {

        bulletSpeed = 8;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;
        transform.Translate(new Vector3(0, 0, bulletSpeed) * Time.deltaTime);
        if (lifeTime < 0)
        {
            Destroy(gameObject);
        }
    }


}
