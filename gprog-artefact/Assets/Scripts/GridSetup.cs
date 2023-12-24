using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.Tilemaps;

public class GridSetup : MonoBehaviour
{
    public NodeGrid Grid;
    public Tilemap Tilemap;
    public AStar Pathfinding;

    public void Awake()
    {
        Tilemap = GetComponentsInChildren<Tilemap>()[0];
        Grid = new NodeGrid(51, 51, Tilemap);
        Pathfinding = new AStar(Grid);
    }
}
