﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
    public int rows = 1;
    public int cols = 1;
    public Node nodePrefab;

    private Node[,] m_grid;

    private void Awake() {
        m_grid = new Node[rows, cols];

        for(int i = 0; i < rows; ++i) {
            for(int j = 0; j < cols; ++j) {
                m_grid[i, j] = Instantiate(nodePrefab, transform);
                m_grid[i, j].transform.position = new Vector3(i + 0.5f, 0, j + 0.5f);
                m_grid[i, j].name = "Node (" + i + ", " + j + ")";
            }
        }
    }

    



}
