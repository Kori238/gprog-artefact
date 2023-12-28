using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Podium : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GridSetup _world;
    [SerializeField] private Vector3Int _position;
    [SerializeField] private int _startingLayer;
    void Start()
    {
        var cell = _world.Tilemaps[_startingLayer].WorldToCell(transform.position);
        transform.position = _world.Tilemaps[_startingLayer].GetCellCenterWorld(cell);
        _world.Grid.GetNodeFromCell(cell.x, cell.y, cell.z).OccupiedBy = NodeOccupiers.Podium;
        _position = new Vector3Int(cell.x, cell.y, _startingLayer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
