﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public abstract class Building : MonoBehaviour
{

    //----------VARIABLES--------------

    public float Radius = 1f;
    public Color radiusColor = new Color(10, 10, 120);
    public float radiusLineWidth = 0.1f;

    public int price;
    public int sellPrice;

    public float startingHealth = 0f;
    public GridArea Location;

    protected float health = 0f;
    protected bool placed = false;
    protected bool selected = false;
    public bool Placed
    {
        get { return placed; }
        set { placed = value; }
    }

    /// <summary>
    /// Used to keep track of when the tower's position changes so the radius circle can be redrawn
    /// </summary>
    private Vector3 prevPos;

    /// <summary>
    /// Holds the array of nodes that make the play area
    /// </summary>
    private WorldGrid worldGrid;

    /// <summary>
    /// LineRenderer component of the gameobject
    /// </summary>
    protected LineRenderer radiusLine;

    /// <summary>
    /// The number of segments in the radius line.
    /// </summary>
    private int numSegments = 100;


    //------------PUBLIC---------------
    #region PUBLIC
    /// <summary>
    /// Add to the tower's health
    /// </summary>
    /// <param name="toAdd">The amount of health to add (or subtract if negative)</param>
    public void updateHealth(float toAdd)
    {
        health += toAdd;
    }

    /// <summary>
    /// Checks to see if the requested location is available, and if so places the tower there
    /// </summary>
    /// <param name=""></param>
    public bool placeOnMap(GridArea loc)
    {
        bool invalidLocation = false;
        bool hasNavigation = false;


        int startX = loc.bottomLeft.x;
        int endX = startX + loc.width - 1;
        int startY = loc.bottomLeft.y;
        int endY = startY + loc.height - 1;

        //All nodes within loc must be empty or navigation, and at least 1 must be navigation
        for (int i = startX; i <= endX; ++i)
        {
            for (int j = startY; j <= endY; ++j)
            {
                Node cur = worldGrid.getAt(i, j);
                if (cur == null)
                {
                    return false;
                }

                if (cur.Occupied == Node.nodeStates.navigation)
                {
                    hasNavigation = true;
                }
                else if (cur.Occupied != Node.nodeStates.empty)
                {
                    invalidLocation = true;
                }
            }
        }

        //If an invalid location, or if none of the nodes were navigation nodes, exit
        if (invalidLocation || !hasNavigation)
        {
            return false;
        }

        Location = loc;
        updatePosition();
        worldGrid.setOccupied(loc, Node.nodeStates.building);
        placed = true;
        return true;
    }

    /// <summary>
    /// Removes the tower from the map
    /// </summary>
    public void removeFromMap()
    {
        worldGrid.setUnoccupied(Location, Node.nodeStates.building);
        placed = false;
        Destroy(transform.root.gameObject);
    }

    /// <summary>
    /// Shows the tower's radius as a cirlce on the map
    /// </summary>
    public void showRadius()
    {
        if (prevPos != transform.position)
        {
            drawRadius();
        }
        prevPos = transform.position;
        radiusLine.enabled = true;
    }

    /// <summary>
    /// Hides the tower's radius
    /// </summary>
    public void hideRadius()
    {
        radiusLine.enabled = false;
    }
    #endregion

    //-----------PROTECTED-------------
    #region PROTECTED
    /// <summary>
    /// Recalculates the points in the LineRenderer's circle. Only called when the object's position changes.
    /// </summary>
    protected void drawRadius()
    {
        float deltaTheta = (float)(2.0 * Mathf.PI) / numSegments;
        float theta = 0f;
        for (int i = 0; i < numSegments; i++)
        {
            float x = Radius * Mathf.Cos(theta);
            float z = Radius * Mathf.Sin(theta);
            Vector3 pos = new Vector3(x, 0.1f, z);
            radiusLine.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

    /// <summary>
    /// Initializes settings for the LineRenderer component which is used to display the radius.
    /// </summary>
    protected void initLineRenderer()
    {
        radiusLine = gameObject.GetComponent<LineRenderer>();
        radiusLine.positionCount = numSegments;
        radiusLine.material.color = radiusColor;
        radiusLine.startWidth = radiusLineWidth;
        radiusLine.alignment = LineAlignment.View;
        radiusLine.loop = true;
        radiusLine.useWorldSpace = false;

        prevPos = transform.position;

        drawRadius();
        radiusLine.enabled = false;
    }

    /// <summary>
    /// Calculates a new Location value based on pos, snapped to the nearest integer coordinate. Then calls updatePosition.
    /// </summary>
    /// <param name="pos">The new transform. Note: the actual transform after the call might be slightly different
    /// because Location will round bottomLeft to integer values.</param>
    protected void setCenterPosition(Vector3 pos)
    {
        int bottomLeftX = (int)(pos.x - 0.5f * (Location.width - 1));
        int bottomLeftY = (int)(pos.z - 0.5f * (Location.height - 1));

        Location.bottomLeft = new Vector2Int(bottomLeftX, bottomLeftY);
        updatePosition();
    }

    /// <summary>
    /// Recalculates and moves the object's transform based on its Location attribute.
    /// </summary>
    protected void updatePosition()
    {
        int startX = Location.bottomLeft.x;
        int endX = startX + Location.width - 1;

        int startY = Location.bottomLeft.y;
        int endY = startY + Location.height - 1;

        transform.position = new Vector3((float)(startX + endX) / 2, transform.position.y, (float)(startY + endY) / 2);
    }

    /// <summary>
    /// Sets the object's transform to the mouse if the tower is not placed. Places the tower if the mouse button is released.
    /// </summary>
    protected void handleMouse()
    {
        Vector3 screenMousPos = Input.mousePosition;
        screenMousPos.z = Camera.main.transform.position.y;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(screenMousPos);

        Vector2Int gridMousePos = new Vector2Int((int)Mathf.Round(worldMousePos.x), (int)Mathf.Round(worldMousePos.z));
        bool mouseWithinBuilding = Location.Contains(gridMousePos);

        GameObject canvas = transform.Find("Canvas").gameObject;

        //Handle mouse location
        if (placed)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                radiusLine.enabled = false;
                canvas.transform.Find("Sell").gameObject.GetComponent<GameButton>().Hide();
            }
        }
        else
        {
            setCenterPosition(worldMousePos);
            radiusLine.enabled = true;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Score.score += price;
                Destroy(gameObject);
            }
        }


        //Handle left click
        if (Input.GetMouseButtonUp(0))
        {


            if (placed)
            {
                if (mouseWithinBuilding)
                {
                    ShowUI(canvas);
                }
                else
                {
                    HideUI(canvas);
                }

            }
            else
            {
                GridArea worldGridArea = new GridArea(new Vector2Int(0, 0), worldGrid.width, worldGrid.height);
                if (worldGridArea.Contains(gridMousePos))
                {
                    if (placeOnMap(Location))
                    {
                        ShowUI(canvas);
                    }
                }
            }
        }


        //Handle right click
        if (Input.GetMouseButtonUp(1))
        {

        }




    }

    /// <summary>
    /// Shows the Sell option inherent to all buildings
    /// </summary>
    /// <param name="canvas">The canvas on which it is displayed</param>
    protected void HideUI(GameObject canvas)
    {
        radiusLine.enabled = false;
        canvas.transform.Find("Sell").gameObject.GetComponent<GameButton>().Hide();
        selected = false;
        derivedHide(canvas);

    }

    /// <summary>
    /// Hides the Sell option inherent to all buildings
    /// </summary>
    /// <param name="canvas">The canvas on which it is displayed</param>
    protected void ShowUI(GameObject canvas)
    {
        radiusLine.enabled = true;
        canvas.transform.Find("Sell").gameObject.GetComponent<GameButton>().Show();
        selected = true;
        derivedShow(canvas);
    }

    #endregion

    //------------PRIVATE--------------
    #region PRIVATE
    private void Start()
    {
        worldGrid = GameObject.FindWithTag("WorldGrid").GetComponent<WorldGrid>();
        if (worldGrid == null)
        {
            Debug.LogError("Could not find WorldGrid object in the scene. Either the tag was changed or the object is missing.");
        }

        initLineRenderer();
        derivedStart();
    }

    private void Update()
    {
        handleMouse();
        updateAction();

    }
    #endregion

    //------------ABSTRACT-------------
    #region ABSTRACT

    /// <summary>
    /// Actions to be performed by the derived class in Start.
    /// </summary>
    protected abstract void derivedStart();

    /// <summary>
    /// Actions to be performed by the derived class in each call of Update.
    /// </summary>
    protected abstract void updateAction();
    #endregion

    protected abstract void derivedHide(GameObject canvas);

    protected abstract void derivedShow(GameObject canvas);
}