using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grapple : MonoBehaviour
{
    private LineRenderer lRend;
    private Vector3 grapplePoint;
    private SpringJoint spring;

    public Transform mainCam, player, gunTip;
    public LayerMask grappleable;
    public float extendRate;

    //improved extending and retracting
    private Vector3 grappleStartingPos;
    private float lerpPercent = 0;
    private Vector3 maxExtendedPoint;

    public float maxLength;
    private bool extending;
    private bool retracting;

    private List<MeshRenderer> rends;

    // Start is called before the first frame update
    void Awake()
    {
        lRend = gameObject.GetComponent<LineRenderer>();
        //renderers of the grappling hook;
        rends = new List<MeshRenderer>();
        for (int i = 1; i <= 3; i++)
        {
            rends.Add(gameObject.transform.GetChild(i).GetComponent<MeshRenderer>());
        }
        for (int i = 0; i < 3; i++)
        {
            rends.Add(gameObject.transform.GetChild(0).gameObject.transform.GetChild(i).GetComponent<MeshRenderer>());
        }
        disappear();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            startGrapple();
        }
        if (Input.GetMouseButtonUp(1))
        {
            stopGrapple();
        }
    }

    private void LateUpdate()
    {
        drawRope();
    }

    private void startGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100.0f))
        {
            grapplePoint = hit.point;
            spring = player.gameObject.AddComponent<SpringJoint>();
            spring.autoConfigureConnectedAnchor = false;
            spring.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            //distance grapple will try to stay at
            spring.maxDistance = distanceFromPoint * 0.8f;
            spring.minDistance = distanceFromPoint * 0.25f;
            //change these at will:
            spring.damper = 7;
            spring.spring = 4.5f;
            spring.massScale = 4.5f;

            lRend.positionCount = 2;
        }
    }
private void stopGrapple()
    {
        lRend.positionCount = 0;
        Destroy(spring);
    }
    
    void drawRope()
    {
        if (!spring)
            return;

        lRend.SetPosition(0, grapplePoint);
        lRend.SetPosition(1, gunTip.position);
    }

    private void disappear()
    {
        foreach (MeshRenderer rend in rends)
        {
            rend.enabled = false;
        }
    }

    private void reappear()
    {
        foreach (MeshRenderer rend in rends)
        {
            rend.enabled = true;
        }
    }
}