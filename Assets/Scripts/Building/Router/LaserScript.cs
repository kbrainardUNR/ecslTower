﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScript : MonoBehaviour {

    public float bufferEnd = 0.15f; //how far from the contact point the laser stays

    public VolumetricLines.VolumetricLineBehavior laser;
    private float bufferStart = 0.25f;

    public Agent hitAgent;
    

    private void Awake()
    {
        laser = transform.parent.Find("Laser").GetComponent<VolumetricLines.VolumetricLineBehavior>();
        if (laser == null)
        {
            Debug.LogError("Could not find laser child!");
            return;
        }
        laser.StartPos = transform.localPosition + Vector3.left * bufferStart;
    }

    private void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, transform.parent.TransformDirection(Vector3.left));
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 10 | 1 << 9))
        {
            if(hit.transform.tag == "Pole")
            {
                laser.EndPos = hit.transform.localPosition;
            }
            if(hit.transform.tag == "AgentModel" && hit.transform.parent.tag == "Agent")
            {
                laser.EndPos = transform.parent.InverseTransformPoint(hit.point + transform.parent.TransformDirection(Vector3.right) * bufferEnd);
                hitAgent = hit.transform.parent.GetComponent<Agent>();
            } else
            {
                hitAgent = null;
            }
            laser.EndPos = new Vector3(laser.EndPos.x, laser.StartPos.y, laser.StartPos.z);
        }
    }
}
