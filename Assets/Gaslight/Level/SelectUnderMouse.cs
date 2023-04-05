using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


public class SelectUnderMouse : MonoBehaviour
{
    public TileCoordinate? hitTileCoord = null;
    public int? highlightedTileIndex;
    
    public Transform camTarget = default;
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
        // if(!isMouseOverUI) RaycastAndHighlight();
    }

    private bool tileSelected = false;

    private bool isMouseOverTile => !(isMouseOverUI && highlightedTileIndex == null);
    public void ConfirmSelection()
    {
        if (isMouseOverTile)
        { 
            tileSelected = true;
        }
    }

    public void DehighlightTile(int? location)
    {
        if (location != null)
        {
            Level.instance.TileDisplays[location.Value].setState(TileDisplay.State.Idle);
        }
    }
    //Does not modify the activation state of the tiledisplay gameobjects
    public async Task<int?> SelectTile(Predicate<Tile> predicate)
    {
        tileSelected = false;
        highlightedTileIndex = null;
        RaycastHit hit;
        Ray ray;
        Level.instance.ChangeTileDisplayStateWithPredicate((Tile tile)=>predicate(tile), true);
        while (tileSelected==false)
        {
            CheckMouseOverUI();
            if (isMouseOverUI)
            {
                DehighlightTile(highlightedTileIndex);
                highlightedTileIndex = null;
                await Task.Yield();
            }
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit))
            {
                DehighlightTile(highlightedTileIndex);
                highlightedTileIndex = null;
                await Task.Yield();
                continue;
            }

            if (Level.instance == null)
            {
                Debug.LogError("Level null");
                continue;
            }
        
            Transform objectHit = hit.transform;
            if (objectHit == null)
            {
                DehighlightTile(highlightedTileIndex);
                highlightedTileIndex = null;
                Debug.LogWarning("hitObject null");
                continue;
            }
        
            var newTileCoord = TileCoordinate.PositionToCoord(hit.point);
            int newTileIndex = TileCoordinate.CoordToIndex(newTileCoord);
            if (highlightedTileIndex==null || highlightedTileIndex != newTileIndex)
            {
                if (!Level.instance.isLocationValid(newTileIndex))
                {
                    // Debug.Log("Tile at "+ hitTileCoord.ToString() + " is invalid to be highlighted");

                    await Task.Yield();
                    continue;
                }

                if (!predicate(Level.instance.Tiles[newTileIndex]))
                {
                    DehighlightTile(highlightedTileIndex);
                    highlightedTileIndex = null;
                    // Debug.Log("Tile fails predicate");
                    await Task.Yield();
                    continue;
                }
                if (highlightedTileIndex != null)
                {
                    Level.instance.TileDisplays[highlightedTileIndex.Value].setState(TileDisplay.State.Idle);
                }
                highlightedTileIndex = newTileIndex;
                Level.instance.TileDisplays[highlightedTileIndex.Value].setState(TileDisplay.State.Highlighted);
            }
            
            await Task.Yield();
        }
        
        Level.instance.TileDisplays[highlightedTileIndex.Value].setState(TileDisplay.State.Selected);
        return highlightedTileIndex;
    }
    
    
    
    
    private void CheckMouseOverUI()
    {
        if (EventSystem.current== null)
        {
            return;
        }
        isMouseOverUI = EventSystem.current.IsPointerOverGameObject();
        // if (isMouseOverUI)
        // {
        //     Level.instance.DehighlightTile();
        // }
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
