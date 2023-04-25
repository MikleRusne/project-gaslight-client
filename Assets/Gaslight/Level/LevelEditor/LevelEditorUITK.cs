using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class LevelEditorUITK : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    public UnityEvent<String> BasetileChanged;
    public VisualElement tileInfoSlideOut = default;
    public Button tileInfoSlideOutButton = default;
    
    
    
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        // SetMouseInOut(_uiDocument.rootVisualElement.Q("CurrentTileIndicator"));
        // SetMouseInOut(_uiDocument.rootVisualElement.Q("TileSettingsContainer"));
        // SetMouseInOut(_uiDocument.rootVisualElement); 
        
        if (_uiDocument == null)
        {
            Debug.LogError("UI document null");
        }
        var baseTileDropdown = _uiDocument.rootVisualElement.Q("SelectedBaseTile") as DropdownField;
        baseTileDropdown.RegisterValueChangedCallback((evt) =>
        {
            ChangeBaseTile(evt.newValue);
            BasetileChanged.Invoke(evt.newValue);
        });
        tileInfoSlideOut = _uiDocument.rootVisualElement.Q("TileSettingsContainer") as VisualElement;
        tileInfoSlideOutButton = _uiDocument.rootVisualElement.Q("TileInfoSlideOutButton") as Button;
        tileInfoSlideOutButton.RegisterCallback<ClickEvent>(shill1);
        // ToggleTileInfoSlideOut();
    }

    void ChangeBaseTile(string newValue)
    {
        Level.instance.ChangeTileBase(Level.instance.selectedTile.tileKey, newValue);
    }
    void Start()
    {
        
        RegisterTileCallbacks();
    }
    public void RegisterTileCallbacks()
    {
        if (Level.instance == null)
        {
            Debug.Log("No level");
        }
        Level.instance.TileSelected.AddListener(OnTileSelected);
    }

    private void OnTileSelected()
    {
        Debug.Log("Fixing dropdown");
        var selectedTile = Level.instance.selectedTile;
        SetBaseTile(selectedTile.baseTileDescriptor.name, Level.instance.GetValidBaseTileNames());
        SetSelectedTileLabel(Level.instance.selectedTile.tileKey.ToString()); 
        
    }

    public void shill1(ClickEvent e){
        ToggleTileInfoSlideOut();
    }
    
    public void ToggleTileInfoSlideOut()
    {
        if (tileInfoSlideOut.ClassListContains("slideout-1-inactive"))
        {
            tileInfoSlideOut.RemoveFromClassList("slideout-1-inactive");
            tileInfoSlideOut.AddToClassList("slideout-1-active");
        }
        else
        {
            tileInfoSlideOut.AddToClassList("slideout-1-inactive");
            tileInfoSlideOut.RemoveFromClassList("slideout-1-active");
        }
    }
    public void SetBaseTile(string curBasetileName, List<String> allBaseTiles)
    { 
        var dropdownField = _uiDocument.rootVisualElement.Q("SelectedBaseTile") as DropdownField;
        dropdownField.choices = allBaseTiles;
        dropdownField.index = allBaseTiles.IndexOf(curBasetileName);
        dropdownField.SetValueWithoutNotify(curBasetileName);
    }

    public void SetSelectedTileLabel(string text)
    {
        var selectedTileLabel = _uiDocument.rootVisualElement.Q("SelectedTileCoord") as Label;
        selectedTileLabel.text = text;
    }
    public void SetDecoTiles(string[] DecoTiles)
    {
        
    }

    
}
