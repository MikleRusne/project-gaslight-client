using System;
using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


public class SelectUnderMouse : MonoBehaviour
{
    [SerializeField] private bool isMouseOverUI = false;    
    void Update()
    {
        //Check if mouse is on UI
        //If not, then raytrace to choose highlighted tile
        #region Absolute banger comments
        //If no tile is highlighted, then make it so that no tile is highlighted
        #endregion
        //If no tile is in raycast result, then nullify highlighted tile
        //We'll do multiple tile selections sometime later :)
        CheckMouseOverUI();
        if(!isMouseOverUI) RaycastAndHighlight();
    }
    
    private void CheckMouseOverUI()
    {
        isMouseOverUI = EventSystem.current.IsPointerOverGameObject();
        if (isMouseOverUI)
        {
            Level.instance.DehighlightTile();
        }
    }

    public void SelectHighlightedTile()
    {
        //As in if they're over UI
        if (isMouseOverUI)
        {
            return;
        }

        // Debug.Log("Selecting "+ hitTileCoord.Value.index());
        if (hitTileCoord.HasValue)
        {
            Level.instance.SelectTile(hitTileCoord.Value.index());
        }
    }

    void Start()
    {
    }
    public TileCoordinate? hitTileCoord = null;
    public Transform camTarget = default;
    private void RaycastAndHighlight()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (!Physics.Raycast(ray, out hit))
        {
            return;
        }

        if (Level.instance == null)
        {
            Debug.LogWarning("Level null");
        }
        
        Transform objectHit = hit.transform;
        if (objectHit == null)
        {
            Debug.LogWarning("hitObject null null");
        }
        
        var newTileCoord = TileCoordinate.PositionToCoord(hit.point);
        // if (hitTileCoord==null || 
        //     (
        //         (newTileCoord.x!=hitTileCoord.Value.x)&& 
        //         (newTileCoord.y != hitTileCoord.Value.y))
        //     )
        // {
            hitTileCoord = newTileCoord;
        // }
        int index = hitTileCoord.Value.y * Level.LWidth + hitTileCoord.Value.y;
        
        if (!Level.instance.isLocationValid(index))
        {
            Debug.Log("Tile at "+ hitTileCoord.ToString() + " is invalid to be highlighted");
            return;
        }

        HighlightTile();

    }

    void HighlightTile()
    {
        Level.instance.HighlightTile(hitTileCoord.Value.index());
    }
}
