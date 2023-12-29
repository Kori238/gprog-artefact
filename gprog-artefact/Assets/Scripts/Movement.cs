using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class Movement : MonoBehaviour
{
    [SerializeField] private GridSetup _world;
    [SerializeField] private Vector3Int _position;
    [SerializeField] private Path _path;
    [SerializeField] private int _pathIndex;
    [SerializeField] private int movementSpeed = 10;
    [SerializeField] private Animator _animator;
    [SerializeField] private string _animatorDirection;
    [SerializeField] private string _animatorState;
    [SerializeField] private int _startingLayer = 0;
    [SerializeField] private SortingGroup visualLayer;
    [SerializeField] public Orb _heldItem = null;
    [SerializeField] private bool _interactWithPodiumWhenPathFinished = false;
    [SerializeField] private Podium _podiumToInteract;

    private Task MoveNextTask;
    private Task TraversePathTask;


    private void Start()
    {
        var cell = _world.Tilemaps[_startingLayer].WorldToCell(transform.position);
        transform.position = _world.Tilemaps[_startingLayer].GetCellCenterWorld(cell);
        _position = new Vector3Int(cell.x, cell.y, _startingLayer);
        visualLayer.sortingOrder = _startingLayer + 1;
        _animator.Play("LookS");
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            UpdateCurrentPath();
        }
    }

    private void UpdateCurrentPath()
    {
        Vector2 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var currentPos = _position;
        var currentNode = _world.Grid.GetNodeFromCell(currentPos.x, currentPos.y, currentPos.z);
        Node destinationNode = null;
        Path path = null;

        for (var layer = _world.Grid.Dimensions.z - 1; layer >= 0; layer--) //iterate backwards through each layer to find the highest z point with a valid tile
        {
            var cellPos = _world.Tilemaps[layer].WorldToCell(mousePointInWorld);
            cellPos = new Vector3Int(cellPos.x - layer, cellPos.y - layer, cellPos.z);
            var selectedNode = _world.Grid.GetNodeFromCell(cellPos.x, cellPos.y, layer);
            if (selectedNode == currentNode) continue;
            if (!_world.Tilemaps[layer].HasTile(cellPos) ||
                !_world.Tilemaps[layer].GetTile<CustomTile>(cellPos).walkable) continue;
            Debug.Log(currentPos + " " + selectedNode.Position);
            path = _world.Pathfinding.FindPath(currentPos.x, currentPos.y, currentPos.z,
                selectedNode.Position.x, selectedNode.Position.y, selectedNode.Position.z);
            destinationNode = selectedNode;
            break;
        }
        if (path == null) return;

        _path = path;
        _pathIndex = 1;
        Node prevNode = null;
        _interactWithPodiumWhenPathFinished = (destinationNode.OccupiedBy == NodeOccupiers.Podium); 
        _podiumToInteract = destinationNode.Occupant;
        foreach (var node in _path.Nodes)
        {
            if (prevNode != null)
                Debug.DrawLine(_world.Grid.GetCenter(prevNode.Position) + new Vector3(0, 0.3f, 0),
                    _world.Grid.GetCenter(node.Position) + new Vector3(0, 0.3f, 0), Color.white,
                    _path.Nodes.Count);
            prevNode = node;
        }

        if (TraversePathTask is not { Status: TaskStatus.WaitingForActivation }) TraversePathTask = TraversePath();
    }

    private async Task TraversePath() 
    {
        transform.position = _world.Grid.GetCenter(_position);
        while (_path != null)
        {
            MoveNextTask = MoveNext();
            await MoveNextTask;
        }
    }

    private async Task MoveNext()
    {
        await Task.Delay(100);
        if (!(_pathIndex >= _path.Nodes.Count))
        {
            var node = _path.Nodes[_pathIndex];
            await MoveToCell(node);
        }
        if (_pathIndex >= _path.Nodes.Count - 1)
        {
            _path = null;
            _pathIndex = 1;
            if (_interactWithPodiumWhenPathFinished) _podiumToInteract.Interact(this);
            return;
        }
        _pathIndex++;
    }

    public async Task MoveToCell(Node node)
    {
        _position = node.Position;
        if(_position.z+1 > visualLayer.sortingOrder) visualLayer.sortingOrder = _position.z + 1;
        var target = _world.Grid.GetCenter(node.Position);
        Vector2 direction = target - transform.position;
        _animatorState = "Walk";
        _animatorDirection = direction.x switch
        {
            > 0.01f when direction.y > 0.01f => "NE",
            > 0.01f when direction.y < -0.01f => "SE",
            > 0.01f => "E",
            < -0.01f when direction.y > 0.01f => "NW",
            < -0.01f when direction.y < -0.01f => "SW",
            < -0.01f => "W",
            _ => direction.y switch
            {
                > 0.01f => "N",
                < -0.01f => "S",
                _ => _animatorDirection
            }
        };
        UpdateAnimator();
        while (Vector2.Distance(transform.position, target) > 0.05f) 
        {
            direction = target - transform.position;
            transform.position += (Vector3)(movementSpeed * Time.deltaTime * direction.normalized);
            await Task.Yield();
        }
        if (_position.z + 1 < visualLayer.sortingOrder) visualLayer.sortingOrder = _position.z + 1;
        _animatorState = "Look";
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        _animator.Play(_animatorState + _animatorDirection);
    }
}
