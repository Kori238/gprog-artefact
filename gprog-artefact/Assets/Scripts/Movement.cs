using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Movement : MonoBehaviour
{
    [SerializeField] private GridSetup _world;
    [SerializeField] private Vector2Int _position;
    [SerializeField] private Path _path;
    [SerializeField] private int _pathIndex;

    private void Start()
    {
        var cell = _world.Tilemap.WorldToCell(transform.position);
        transform.position = _world.Tilemap.GetCellCenterWorld(cell);
        _position = (Vector2Int)cell;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("mouseup");
            UpdateCurrentPath();
        }
    }

    private void UpdateCurrentPath()
    {
        Vector2 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var currentPos = (Vector3Int)_position;
        var currentNode = _world.Grid.GetNodeFromCell((int)currentPos.x, (int)currentPos.y);

        var cellPos = _world.Tilemap.WorldToCell(mousePointInWorld);
        var selectedNode = _world.Grid.GetNodeFromCell(cellPos.x, cellPos.y);
            if (selectedNode == currentNode) return;
            if (_world.Tilemap.HasTile(selectedNode.Position))
            {
                if (!_world.Tilemap.GetTile<CustomTile>(selectedNode.Position).walkable) return;
                Debug.Log($"x:{currentPos.x}, y:{currentPos.y},   target x:{selectedNode.Position}");
                var path = _world.Pathfinding.FindPath(currentPos.x, currentPos.y,
                    selectedNode.Position.x, selectedNode.Position.y);
                
                if (path != null)
                {
                    Debug.Log(path);
                    _path = path;
                    _pathIndex = 1;
                    Node prevNode = null;
                    foreach (var node in _path.Nodes)
                    {
                        if (prevNode != null) Debug.DrawLine(_world.Tilemap.GetCellCenterWorld(prevNode.Position) + new Vector3(0, 0.3f, 0),
                            _world.Tilemap.GetCellCenterWorld(node.Position) + new Vector3(0, 0.3f, 0), Color.white, _path.Nodes.Count);
                        prevNode = node;
                        Debug.Log(node + "" + prevNode);
                    }
                }
            }
    }
    }
