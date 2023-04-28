using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class SelectUnderMouse : MonoBehaviour
{
    public TileCoordinate? hitTileCoord = null;
    public int? highlightedTileIndex;

    public Color _highlightColor;
    public Color _selectedColor;
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
    // private bool firstConfirm = false;
    bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }
    [ContextMenu("Accept")]
    public void ConfirmSelection(InputAction.CallbackContext ctx)
    {
        if (isInputRequested && ctx.performed && IsMouseOverGameWindow && isMouseOverTile && highlightedTileIndex!=null)
        {
            // firstConfirm = true;
            Debug.Log("Confirming input");
            tileSelected = true;
        }
    }

    public void DehighlightTile(int? location)
    {
        if (location != null)
        {
            Level.instance.ResetTileDisplayColor(location.Value);
        }
    }

    private bool isInputRequested = false;
    //Does not modify the activation state of the tiledisplay gameobjects
    public async Task<int?> SelectTile(Predicate<Tile> predicate, CancellationToken cancellationToken)
    {
        tileSelected = false;
        isInputRequested = true;
        highlightedTileIndex = null;
        RaycastHit hit;
        Ray ray;
        Level.instance.ChangeTileDisplayStateWithPredicate((Tile tile)=>predicate(tile), true);
        Level.instance.ChangeTileSelectionColliderState((Tile tile)=>predicate(tile), true);
        while (tileSelected==false)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Level.instance.TurnOffAllDisplays();
                Debug.LogWarning("Cancelling tile select");
                isInputRequested = false;
                highlightedTileIndex = null;
                cancellationToken.ThrowIfCancellationRequested();
            }
            CheckMouseOverUI();
            if (isMouseOverUI)
            {
                DehighlightTile(highlightedTileIndex);
                highlightedTileIndex = null;
                await Task.Yield();
            }
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawLine(ray.origin, ray.origin + ray.direction*500.0f, Color.blue);
            if (!Physics.Raycast(ray, out hit, 400.0f, 1<<LayerMask.NameToLayer("Tiles")))
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
            var hitObjectName = objectHit.name;
            Debug.Log(hitObjectName);
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
                    Level.instance.ResetTileDisplayColor(highlightedTileIndex.Value);
                }
                highlightedTileIndex = newTileIndex;
                Level.instance.ChangeTileDisplayColor(highlightedTileIndex.Value, _highlightColor);
            }
            
            await Task.Yield();
        }

        isInputRequested = false;
        if (highlightedTileIndex == null)
        {
            Debug.LogWarning("highlightedTileIndex null");
        }
        else
        {
            Level.instance.ChangeTileDisplayColor(highlightedTileIndex.Value, _selectedColor);
        }
        Level.instance.TurnOffAllColliders();
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
    }
}
