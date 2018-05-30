﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Manages the waves, allows for building of the WavePaths and Waves
/// </summary>
[ExecuteInEditMode]
public class WaveManager : MonoBehaviour
{
    /*-----------public variables-----------*/
    public bool enablePathEditing = false;
    /// <summary>
    /// Mandatory prefab so a Wave can be created and used
    /// </summary>
    public Wave wavePrefab;
    /// <summary>
    /// Manatory prefab so an Arrow can be drawn and placed
    /// </summary>
    public Arrow arrowPrefab;
    /// <summary>
    /// How much the arrow will overlay on the grid
    /// </summary>
    /// <remarks>
    /// Default works fine
    /// </remarks>
    public float arrowOffset = 0.1f;
    /// <summary>
    /// Temperary prefab to be expanded in to a list of possible Agents
    /// </summary>
    public Agent agentPrefab;

    /// <summary>
    /// Number of AgentPaths make when Make is called
    /// </summary>
    public int makePerWave = 100;
    /// <summary>
    /// The Level that stores the current WavePaths in the inspector and the file
    /// </summary>
    //[HideInInspector]
    public Level thisLevel;
    /// <summary>
    /// Prefab that marks the Nodes that contain a startArea
    /// </summary>
    public GameObject startAreaMarker;
    /// <summary>
    /// Prefab that marks the Nodes that contain an endArea
    /// </summary>
    public GameObject endAreaMarker;
    /// <summary>
    /// Contains the Arrows and allows creation and deletion of the path of Arrows
    /// </summary>
    public ArrowContainer arrowContainer;

    public EndArea endAreaPrefab;


    /*-----------private variables-----------*/
    /// <summary>
    /// Reference to the WorldGrid object
    /// </summary>
    /// <remarks>
    /// Necessary for raycasting to Nodes
    /// </remarks>
    private WorldGrid worldGrid;
    /// <summary>
    /// List of Waves to be pushed to the player
    /// </summary>
    /// <remarks>
    /// Not implemented yet
    /// </remarks>
    private List<Wave> waveList = new List<Wave>();
    /// <summary>
    /// The Wave that the Manager is currently handling and editing
    /// </summary>
    private Wave currentWave;
    /// <summary>
    /// Used for Arrow drawing, where the base of the Arrow will lay
    /// </summary>
    private Node currStart = null;
    /// <summary>
    /// Used for Arrow drawing, where the tip of the Arrow will lay
    /// </summary>
    private Node currEnd = null;
    /// <summary>
    /// The Arrow that is currently being drawn
    /// </summary>
    private Arrow drawArrow = null;
    ///<summary>
    /// List of WavePaths recieved by the ArrowContainer or the SerializedWavePaths to be used by the Agents
    /// </summary>
    private List<WavePath> wavePathList = new List<WavePath>();

    /*-----------private MonoBehavior functions-----------*/
    /// <summary>
    /// Assigns the WorldGrid reference and currently creates a test Wave
    /// </summary>
    private void Start()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            Load();
            return;
        }
#endif
        worldGrid = GameObject.FindWithTag("WorldGrid").GetComponent<WorldGrid>();
        if (worldGrid == null)
        {
            Debug.LogError("Could not find WorldGrid object in the scene. Either the tag was changed or the object is missing.");
        }

        /*Loads the level based on inspector values, could use a clean-up*/

        UseLevel(thisLevel);

        /*test area*/
        Wave newWave = Instantiate(wavePrefab, this.transform) as Wave;
        waveList.Add(newWave);
        currentWave = waveList[0];
        /*end of test area*/
    }

    /// <summary>
    /// Call the neccessary functions for operation
    /// </summary>
    private void Update()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            return;
        }
#endif

        if (enablePathEditing)
        {
            DrawArrowIfValid();
            SelectNodeOnClick();
            RemoveArrowOnClick();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (PushPath())
                {
                    Debug.Log("Path pushed!");
                }
                else
                {
                    Debug.LogError("Invalid path!");
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Save();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                Load();
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Clear();
            }
        } else
        //Press M to make a Wave that contains makePerWave number of AgentPaths
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Made AgentPaths!");
            for (int i = 0; i < makePerWave; i++)
            {
                MakeAgentInWave(currentWave);
            }
        }
    }

    /// <summary>
    /// Will mark every Area (start and end) contained in the passed ArrowContianer
    /// </summary>
    /// <param name="container">The values are based on this parameter</param>
   /* private void MarkAreasInContainer(ArrowContainer container)
    {
        Transform markerHolder = transform.Find("MarkerHolder").transform;
        if (markerHolder == null)
        {
            Debug.LogError("Cannot find marker holder! Perhaps it was moved or renamed?");
            return;
        }

        foreach (GridArea start in arrowContainer.startAreas)
        {
            MarkArea(start, true, markerHolder);
        }

        foreach (GridArea end in arrowContainer.endAreas)
        {
            MarkArea(end, false, markerHolder);
        }
    }*/

    /// <summary>
    /// Marks the Area either Start or End based on parameters
    /// </summary>
    /// <param name="area">The GridArea to be marked</param>
    /// <param name="start">If start is true, marked as a StartArea, if false, marked as an EndArea</param>
    /// <param name="parent">Parent object that the Markers will be Instantiated under</param>
   /* private void MarkArea(GridArea area, bool start, Transform parent)
    {

        GameObject marker = start ? startAreaMarker : endAreaMarker;
        for (int i = area.bottomLeft.x; i < area.bottomLeft.x + area.width; i++)
        {
            for (int j = area.bottomLeft.y; j < area.bottomLeft.y + area.height; j++)
            {
                GameObject newMarker = Instantiate(marker, worldGrid.getAt(i, j).transform.position, Quaternion.identity) as GameObject;
                newMarker.transform.parent = parent;
            }
        }
    }*/

    /// <summary>
    /// Makes an AgentPath based on a random path and adds it to the Wave
    /// </summary>
    /// <param name="wave">AgentPath is added to this Wave</param>
    private void MakeAgentInWave(Wave wave)
    {
        if (wavePathList.Count > 0)
        {
            int pathIndex = Random.Range(0, wavePathList.Count);
            WavePath currentPath = wavePathList[pathIndex];
            wave.AddNewAgent(agentPrefab, currentPath);
        }
        else
        {
            Debug.LogError("Wave list is empty!");
        }
    }

    /// <summary>
    /// Clears both the wavePathList and the file of the Level
    /// </summary>
    private void Clear()
    {
        Debug.Log("Cleared!");
        wavePathList = new List<WavePath>();
        thisLevel.DeleteLevel();
    }

    /// <summary>
    /// Saves the current wavePathList to the Level which will write to file
    /// </summary>
    private void Save()
    {
        if (wavePathList != null && wavePathList.Count > 0)
        {
            thisLevel.SetLevel(wavePathList, arrowContainer.startAreas, arrowContainer.endAreas);
            thisLevel.SaveLevel();

            foreach (WavePath path in wavePathList)
            {
                Debug.Log("Saved: " + path);
            }
        }
        else
        {
            Debug.LogError("Path list has nothing in it");
        }
    }

    /// <summary>
    /// Loads the Level from file, can ultimately just read from the Level in the inspector
    /// </summary>
    public void Load()
    {
        Level tempLevel = thisLevel.LoadLevel();
        List<SerializableWavePath> tempWavePathList = tempLevel.wavePaths;
        List<SerializableEndArea> tempEndAreaList = tempLevel.endAreas;
        if (tempWavePathList != null && tempEndAreaList != null)
        {

            thisLevel.SetLevel(tempLevel);
            if (worldGrid != null)
            {
                UseLevel(thisLevel);
                foreach (WavePath path in wavePathList)
                {
                    Debug.Log("Loaded: " + path);
                }
            } else
            {
                //Debug.LogError("WorldGrid is null!");
            }
        }
        else
        {
            Debug.LogError("Loading failed!");
        }
    }


    /*-----------private functions-----------*/

    private void SetArrowContainerAreas(List<SerializableEndArea> endAreas)
    {
        Transform markerHolder = transform.Find("MarkerHolder").transform;
        if (markerHolder == null)
        {
            Debug.LogError("Cannot find marker holder! Perhaps it was moved or renamed?");
            return;
        }
        foreach (SerializableEndArea area in endAreas)
        {
            EndArea endArea = Instantiate(endAreaPrefab, markerHolder);
            endArea.endSetting = area.Sink ? endOptions.sink : endOptions.source;
            endArea.SetColor();
            GridArea tempArea = new GridArea(area);
            endArea.PlaceEndArea(worldGrid.getAt(tempArea.bottomLeft.x, tempArea.bottomLeft.y), worldGrid.getAt(tempArea.bottomLeft.x + tempArea.width - 1, tempArea.bottomLeft.y + tempArea.height - 1));
        }
    }

    /// <summary>
    /// On RightClick, the Arrow selected and every Arrow after it is removed
    /// </summary>
    private void RemoveArrowOnClick()
    {
        if (Input.GetMouseButtonDown(1))
        {

            Node NodePointedAt = worldGrid.getRaycastNode();
            if (NodePointedAt == null) //exit function if no node is selected
            {
                return;
            }
            List<Arrow> deleteArrows = arrowContainer.RemoveArrows(NodePointedAt);
            if (deleteArrows.Count == 0)
            {
                return;
            }
            foreach (Arrow arrow in deleteArrows)
            {
                SetNodeOccupation(arrow, Node.nodeStates.empty);
                arrow.KillArrrow();
            }
        }
    }

    /// <summary>
    /// Will update the Arrow about to be placed to be drawn with the mouse
    /// </summary>
    private void DrawArrowIfValid()
    {
        if (currStart != null && drawArrow != null && currEnd == null)
        {
            Node nodePointedAt = worldGrid.getRaycastNode();
            if (nodePointedAt != null)
            {
                drawArrow.DrawArrow(currStart.transform.position, GetCardinalNode(nodePointedAt, currStart).transform.position, arrowOffset);
            }
        }
    }

    /// <summary>
    /// Returns a Node that is perpendicular to the start Node
    /// </summary>
    /// <param name="target">The Node which the mouse is currently over</param>
    /// <param name="origin">The origin of the Arrow which needs to be placed</param>
    /// <returns></returns>
    private Node GetCardinalNode(Node target, Node origin)
    {
        int distanceY = Mathf.Abs(target.Coordinate.y - origin.Coordinate.y);
        int distanceX = Mathf.Abs(target.Coordinate.x - origin.Coordinate.x);

        if (distanceX >= distanceY)
        {
            return worldGrid.getAt(target.Coordinate.x, origin.Coordinate.y);
        }
        else
        {
            return worldGrid.getAt(origin.Coordinate.x, target.Coordinate.y);
        }
    }

    /// <summary>
    /// Handles the Node selection on LeftClick to provide currStart and currEnd valid Nodes to be used by other functions
    /// </summary>
    private void SelectNodeOnClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Node currentNode = worldGrid.getRaycastNode();
            if (currentNode == null) //exit function if no node is selected
            {
                return;
            }

            if (currStart == null) //if there is no selected start node
            {
                if (arrowContainer.IsVaildNode(currentNode))
                {
                    currStart = currentNode;
                    //Debug.Log("Start Point set to: " + currStart.name);
                    drawArrow = Instantiate(arrowPrefab, transform) as Arrow;
                    drawArrow.DrawArrow(currStart.transform.position, worldGrid.getRaycastNode().transform.position, arrowOffset);
                }
                else
                {
                    Debug.LogError("Node" + currentNode.Coordinate + " cannot be placed here!");
                }
            }

            else //if start node has been selected, select the end node
            {
                currEnd = GetCardinalNode(currentNode, currStart);
                if (currEnd == currStart || currEnd.Occupied == Node.nodeStates.navigation) //end and start cannot be the same
                {
                    Debug.LogError("Node" + currentNode.Coordinate + " is occupied!");
                    currEnd = null;
                }
                else
                {
                    //Debug.Log("End Point set to: " + currEnd.name);
                }
            }

            if (currStart != null && currEnd != null) //if both the start and end exist, set the path segment
            {
                SetPathSegment(currStart, currEnd);
            }

        }
    }

    /// <summary>
    /// Creates the visual Arrow path
    /// </summary>
    /// <param name="wavePaths">Will create the path based on this value</param>
    private void DisplayPath(List<WavePath> wavePaths)
    {
        foreach (WavePath path in wavePaths)
        {
            WavePath temp = new WavePath(path);
            Node current = null;
            Node previous = null;
            while ((current = temp.GetNextNode()) != null)
            {
                if (previous == null)
                {
                    previous = current;
                    continue;
                }

                drawArrow = Instantiate(arrowPrefab, transform) as Arrow;
                SetPathSegment(previous, current);
                previous = current;
            }
        }
    }


    /// <summary>
    /// When finished placing an Arrow, the Arrow will be placed and adhere to the Grid
    /// </summary>
    /// <param name="start">The Node in which the base of the Arrow is in</param>
    /// <param name="end">The Node in which the tip of the Arrow is in</param>
    private void SetPathSegment(Node start, Node end)
    {

        if (start == null || end == null)
        {
            Debug.LogError("Passed invalid nodes to create segment!");
            return;
        }
        drawArrow.PlaceArrow(start, end, arrowOffset);
        if (arrowContainer.AddArrowToContainer(drawArrow) != null)
        {
            SetNodeOccupation(drawArrow, Node.nodeStates.navigation);
        }

        currStart = null;
        currEnd = null;
    }

    /// <summary>
    /// Sets all Nodes between Arrow endpoint to a certain nodeState
    /// </summary>
    /// <param name="arrow">Arrow to be set</param>
    /// <param name="state">State that the Node will be set to (navigational or emmpty)</param>
    private void SetNodeOccupation(Arrow arrow, Node.nodeStates state)
    {
        bool vertical = arrow.Origin.Coordinate.x == arrow.Destination.Coordinate.x;
        if (vertical == (arrow.Origin.Coordinate.y == arrow.Destination.Coordinate.y))
        {
            Debug.LogError("Error marking an arrow occupied! The Arrow could be invalid!");
            return;
        }
        int start = vertical ? arrow.Origin.Coordinate.y : arrow.Origin.Coordinate.x;
        int end = vertical ? arrow.Destination.Coordinate.y : arrow.Destination.Coordinate.x;
        int constCoord = vertical ? arrow.Origin.Coordinate.x : arrow.Origin.Coordinate.y;
        if (start > end)
        {
            int temp = start;
            start = end;
            end = temp;
        }


        for (int i = start + 1; i <= end; i++)
        {
            if (vertical)
            {
                worldGrid.getAt(constCoord, i).Occupied = state;
            }
            else
            {
                worldGrid.getAt(i, constCoord).Occupied = state;
            }
        }
    }

    /// <summary>
    /// Pushes the WavePath to the currentWave's Path list to be used in that object
    /// </summary>
    /// <returns>If the Path is valid, returns true, else false</returns>
    private bool PushPath()
    {
        wavePathList = arrowContainer.ToWavePaths();
        return true;
    }

    /// <summary>
    /// Populates wavePathList based on a List of SerializableWavePaths
    /// </summary>
    /// <param name="paths">List to be converted</param>
    private void UseLevel(Level level)
    {
        SetArrowContainerAreas(level.endAreas);
        wavePathList = new List<WavePath>();
        foreach (SerializableWavePath path in level.wavePaths)
        {
            wavePathList.Add(new WavePath(path, worldGrid));
        }
        DisplayPath(wavePathList);
    }



}
