using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookGrapple : MonoBehaviour
{
    LineRenderer rope;

    public GameObject player;
    public GameObject hook;


    RaycastHit hit;
    [Header("Hook Settings")]
    public float hook_area = 2.0f;
    public float shoot_speed = 3.5f;
    public float return_speed = 4.5f;

    // Has the hook been launcehd
    bool launch_hook = false;
    bool hook_latched = false;

    [Header("Hook shoot time")]
    float hook_time;

    Vector3 grapple_point;
    float tether_length;

    private void Awake()
    {
        rope = GetComponent<LineRenderer>();
        rope.SetPosition(0, player.transform.position);
        rope.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        rope.SetPosition(0, player.transform.position);
        if (!launch_hook && !hook_latched)
        {
            hook.transform.position = player.transform.position;
        }
        

        if (Input.GetMouseButtonDown(0) && !launch_hook && !hook_latched)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100.0f))
            {
                grapple_point = new Vector3(hit.point.x, hit.point.y, player.transform.position.z);
                launch_hook = true;
                hook.transform.LookAt(grapple_point);
                rope.enabled = true;
            }
         
        }

        // If the hook is flying
        if (launch_hook)
        {
            hook.transform.position += (grapple_point - hook.transform.position).normalized * shoot_speed * Time.deltaTime;
            rope.SetPosition(1, hook.transform.position);
        }

        // When the hook grabs onto something
        if (!launch_hook && hook_latched)
        {
            // Pull the player in
            if ((grapple_point - player.transform.position).magnitude > tether_length)
            {
                Vector3 direction = (grapple_point - player.transform.position).normalized;
                player.GetComponent<Rigidbody>().AddForce(direction * 4, ForceMode.Acceleration);
            }
        }

        if (launch_hook && !hook_latched)
        {
            hookHits();
        }
    }

    // If it hit's an object that isn't itself
    bool hookHits()
    {
        if (Physics.SphereCast(hook.transform.position, hook_area, hook.transform.forward, out hit, hook_area))
        {
            if (hit.collider.gameObject.layer != 8)
            {
                launch_hook = false;
                hook_latched = true;
                tether_length = (grapple_point - player.transform.position).magnitude;
            }
        }

        return false;
    }
}
