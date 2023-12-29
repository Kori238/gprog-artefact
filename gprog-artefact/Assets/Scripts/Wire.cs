using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Wire : MonoBehaviour
{
    [SerializeField] public int _startingLayer;
    [SerializeField] public GridSetup _world;
    [SerializeField] public Vector3Int _position;
    [SerializeField] public SortingGroup sortingGroup;
    [SerializeField] public SpriteRenderer _spriteRenderer;
    [SerializeField] public Color poweredColour;
    [SerializeField] public Color unpoweredColour;
    void Start()
    {
        var cell = _world.Tilemaps[_startingLayer].WorldToCell(transform.position - new Vector3(0, _startingLayer * 0.5f, 0));
        transform.position = _world.Tilemaps[_startingLayer].GetCellCenterWorld(cell) - new Vector3(0, 0.01f, 0);
        _position = new Vector3Int(cell.x, cell.y, _startingLayer);
        sortingGroup.sortingOrder = _startingLayer;
    }

    public virtual void PowerUp()
    {
        _spriteRenderer.color = poweredColour;
    }

    public virtual void PowerDown()
    {
        _spriteRenderer.color = unpoweredColour;
    }
}
