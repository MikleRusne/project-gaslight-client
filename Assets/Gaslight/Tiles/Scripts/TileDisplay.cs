using UnityEngine;
using UnityEngine.UI;

public class TileDisplay : MonoBehaviour
{
    public Level gameLevel;
    public Color RimColorDefault;
    public Color RimColorHighlighted;
    public Color RimColorSelected;
    public RawImage baseImage;
    public int index;
    public enum State
    {
        Highlighted,
        Selected,
        Idle
    }

    [SerializeField]public State state;
    void Awake()
    {
        baseImage = this.transform.Find("BaseImage").GetComponent<RawImage>();
        if (baseImage == null)
        {
            Debug.LogError("Could not find BaseImage in children");
            return;
        }

    }

    private void OnValidate()
    {
        RefreshDisplay(); 
    }

    public void setState(TileDisplay.State newState)
    {
        if (state != newState)
        {
            // Debug.Log("Not refreshing");
            state = newState;
            RefreshDisplay();
        }
    }
    public void RefreshDisplay()
    {
        switch (state)
        {
            case State.Idle:
                baseImage.color = RimColorDefault;
                break;
            case State.Highlighted:
                baseImage.color = RimColorHighlighted;
                break;
            case State.Selected:
                baseImage.color = RimColorSelected;
                break;
        }
    }
    
        
}
