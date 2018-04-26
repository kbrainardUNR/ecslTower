﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {
    public Material hoverMaterial;
    public Material hoverInvalidMaterial;

    private bool occupied = false;
    public bool Occupied {
        get { return occupied; }
        set { occupied = value; }
    }

    private Renderer rend;
    private Material initialMaterial;
    

    private void Start() {
        rend = GetComponent<Renderer>();
        if(rend == null) {
            Debug.Log("Cannot find Node's renderer. Did you remove the component?");
        }

        initialMaterial = rend.material;
    }

    private void Update() {
        setUnhovered();
    }

    
    /// <summary>
    /// Sets the material back to what it was originally
    /// </summary>
    public void setUnhovered() {
        if(rend.material != initialMaterial) {
            rend.material = initialMaterial;
        }
    }

    /// <summary>
    /// Sets the material to hoverMaterial (hoverInvalidMaterial if the node is occupied).
    /// </summary>
    public void setHovered() {
        if(occupied) {
            rend.material = hoverInvalidMaterial;
        } else {
            rend.material = hoverMaterial;
        }
    }

    public override string ToString() {
        return base.ToString() + " Occupied: " + occupied;
    }


}
