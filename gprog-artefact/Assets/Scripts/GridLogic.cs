using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node
{
    public int FCost, GCost, HCost;
    public readonly Vector3Int Position;
    public Node PreviousNode;

    public Node(Vector3Int position)
    {
        Position = position;
    }

    public void UpdateFCost()
    {
        FCost = GCost + HCost;
    }
}

public class NodeGrid
{
    public readonly Vector2Int Dimensions;
    public Tilemap Tilemap;
    public readonly Node[,] Grid;

    public NodeGrid(int width, int height, Tilemap tilemap)
    {
        Dimensions = new(width, height);
        Tilemap = tilemap;
        Grid = new Node[width, height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var position = new Vector3Int(x - width / 2, y - height / 2);
                Grid[x, y] = new Node(position);
            }
        }
    }

    public Node GetNodeFromCell(int x, int y)
    {
        return Grid[x + Dimensions.x / 2, y + Dimensions.y / 2];
    }
}

public class Path
{
    public int FCost, Cost;
    public List<Node> Nodes;

    public Path()
    {
        FCost = 0;
        Cost = 0;
        Nodes = new List<Node>();
    }
}


public class AStar
{
    private const int DIAGONAL_COST = 14;
    private const int STRAIGHT_COST = 10;

    private readonly Vector2Int _gridBounds;
    private readonly int _layersCount;

    private readonly NodeGrid _grid;
    private List<Node> _searchedNodes;
    private List<Node> _unsearchedNodes;

    public AStar(NodeGrid grid)
    {
        _gridBounds = new Vector2Int(grid.Dimensions.x / 2, grid.Dimensions.y / 2);
        _grid = grid;
    }

    public NodeGrid GetGrid()
    {
        return _grid; 
    }

    public Path FindPath(int x0, int y0, int x1, int y1)
    {
        var grid = GetGrid();
        var startNode = grid.GetNodeFromCell(x0, y0);
        var endNode = grid.GetNodeFromCell(x1, y1);

        _unsearchedNodes = new List<Node> { startNode };
        _searchedNodes = new List<Node>();

        for (var x = -(grid.Dimensions.x / 2); x < grid.Dimensions.x / 2 ; x++)
        {
            for (var y = -(grid.Dimensions.y / 2); y < grid.Dimensions.y / 2; y++)
            {
                var node = grid.GetNodeFromCell(x, y);
                node.GCost = int.MaxValue;
                node.UpdateFCost(); 
                node.PreviousNode = null;
            }
        }

        var i = 0;
        while (_unsearchedNodes.Count > 0 && i < 1000)
        {
            i++;
            var currentNode = FindLowestFCostNode(_unsearchedNodes);

            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            _unsearchedNodes.Remove(currentNode);
            _searchedNodes.Add(currentNode);
            var currentTile = _grid.Tilemap.GetTile<CustomTile>(currentNode.Position);
            var hasTile = _grid.Tilemap.HasTile(currentNode.Position);

            if (!hasTile) continue;

            List<Node> adjacents = FindAdjacents(currentNode.Position);

            foreach (var adjacentNode in adjacents)
            {
                if (_searchedNodes.Contains(adjacentNode) || !hasTile) continue;
                if (!currentTile.walkable) 
                { 
                    _searchedNodes.Add(adjacentNode);
                    continue;
                }
                var tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNode, adjacentNode);
                if (tentativeGCost < adjacentNode.GCost)
                {
                    adjacentNode.PreviousNode = currentNode; 
                    adjacentNode.GCost = tentativeGCost; 
                    adjacentNode.HCost = CalculateDistanceCost(adjacentNode, endNode); 
                    adjacentNode.UpdateFCost();
                    
                    if (!_unsearchedNodes.Contains(adjacentNode)) 
                    { 
                        _unsearchedNodes.Add(adjacentNode); 
                    }
                }
            }
        }
        return null;
    }

    private static Path CalculatePath(Node endNode)
    {
        var path = new Path { FCost = endNode.FCost };
        path.Nodes.Add(endNode);
        var currentNode = endNode;
        while (currentNode.PreviousNode != null)
        {
            path.Nodes.Add(currentNode.PreviousNode);
            currentNode = currentNode.PreviousNode;
        }
        path.Nodes.Reverse();
        return path;
    }

    private static Node FindLowestFCostNode(List<Node> nodeList)
    {
        var lowestFCostNode = nodeList[0];
        foreach (var node in nodeList)
        {
            if (node.FCost < lowestFCostNode.FCost)
            {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }

    private List<Node> FindAdjacents(Vector3Int position)
    {
        var directions = new List<Vector2Int> {
            new Vector2Int(position.x - 1, position.y), new Vector2Int(position.x + 1, position.y), //cardinals:
            new Vector2Int(position.x, position.y - 1), new Vector2Int(position.x, position.y + 1),

            new Vector2Int(position.x - 1, position.y - 1), new Vector2Int(position.x - 1, position.y + 1), //diagonals:
            new Vector2Int(position.x + 1, position.y - 1), new Vector2Int(position.x + 1, position.y + 1),
        };

        List<Node> adjacents = new();

        foreach (var direction in directions)
        {
            if (
                direction.x > _grid.Dimensions.x/2 || direction.x < -_grid.Dimensions.x/2 || //boundaries check
                direction.y > _grid.Dimensions.y/2 || direction.y < -_grid.Dimensions.y/2 ||
                _grid.Tilemap.HasTile((Vector3Int)direction)
                ) 
                adjacents.Add(_grid.GetNodeFromCell(direction.x, direction.y));
        }
        return adjacents;
    }

    public int CalculateDistanceCost(Node a, Node b)
    {
        var xDistance = Mathf.Abs(a.Position.x - b.Position.x);
        var yDistance = Mathf.Abs(a.Position.x - b.Position.y);
        var remaining = Mathf.Abs(xDistance - yDistance);

        return DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + STRAIGHT_COST * remaining;
    }
}