using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class Movement : MonoBehaviour
{
    [SerializeField] private GridSetup _world;
    [SerializeField] private Vector2Int _position;
    [SerializeField] private Path _path;
    [SerializeField] private int _pathIndex;
    [SerializeField] private int movementSpeed = 10;
    [SerializeField] private Animator _animator;
    [SerializeField] private string _animatorDirection;
    [SerializeField] private string _animatorState;

    private Task MoveNextTask;
    private Task TraversePathTask;


    private void Start()
    {
        var cell = _world.Tilemap.WorldToCell(transform.position);
        transform.position = _world.Tilemap.GetCellCenterWorld(cell);
        _position = (Vector2Int)cell;
        _animator.Play("LookS");
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
        var currentNode = _world.Grid.GetNodeFromCell(currentPos.x, currentPos.y);

        var cellPos = _world.Tilemap.WorldToCell(mousePointInWorld);
        var selectedNode = _world.Grid.GetNodeFromCell(cellPos.x, cellPos.y);
        if (selectedNode == currentNode) return;
        if (!_world.Tilemap.HasTile(selectedNode.Position) ||
            !_world.Tilemap.GetTile<CustomTile>(selectedNode.Position).walkable) return;
        var path = _world.Pathfinding.FindPath(currentPos.x, currentPos.y,
            selectedNode.Position.x, selectedNode.Position.y);

        if (path == null) return;
        _path = null;
        _path = path;
        _pathIndex = 1;
        Node prevNode = null;
        foreach (var node in _path.Nodes)
        {
            if (prevNode != null)
                Debug.DrawLine(_world.Tilemap.GetCellCenterWorld(prevNode.Position) + new Vector3(0, 0.3f, 0),
                    _world.Tilemap.GetCellCenterWorld(node.Position) + new Vector3(0, 0.3f, 0), Color.white,
                    _path.Nodes.Count);
            prevNode = node;
        }
        
        if (TraversePathTask is not { Status: TaskStatus.WaitingForActivation }) TraversePathTask = TraversePath();
        Debug.Log(TraversePathTask.Status + " " + TaskStatus.Running);
    }

    private async Task TraversePath()
    {
        transform.position = _world.Tilemap.GetCellCenterWorld((Vector3Int)_position);
        while (_path != null)
        {
            MoveNextTask = MoveNext();
            await MoveNextTask;
        }
    }

    private async Task MoveNext()
    {
        await Task.Delay(100);
        if (_path == null) return;
        var node = _path.Nodes[_pathIndex];
        await MoveToCell(node);
        if (_pathIndex == _path.Nodes.Count - 1)
        {
            _path = null;
            _pathIndex = 1;
            return;
        }
        _pathIndex++;
    }

    public async Task MoveToCell(Node node)
    {
        _position = (Vector2Int)node.Position;
        var direction = _world.Tilemap.GetCellCenterWorld(node.Position) - transform.position;
        switch (direction.x)
        {
            case > 0.01f when direction.y > 0.01f:
                _animatorDirection = "NE";
                break;
            case > 0.01f when direction.y < -0.01f:
                _animatorDirection = "SE";
                break;
            case > 0.01f:
                _animatorDirection = "E";
                break;
            case < -0.01f when direction.y > 0.01f:
                _animatorDirection = "NW";
                break;
            case < -0.01f when direction.y < -0.01f:
                _animatorDirection = "SW";
                break;
            case < -0.01f:
                _animatorDirection = "W";
                break;
            default:
            {
                _animatorDirection = direction.y switch
                {
                    > 0.01f => "N",
                    < -0.01f => "S",
                    _ => _animatorDirection
                };
                break;
            }
        }

        _animatorState = "Walk";
        UpdateAnimator();
        while (Vector2.Distance(transform.position, _world.Tilemap.GetCellCenterWorld(node.Position)) > 0.01f)
        {
            transform.position += movementSpeed * Time.deltaTime * direction.normalized;
            await Task.Yield();
        }
        _animatorState = "Look";
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        _animator.Play(_animatorState + _animatorDirection);
    }
}
