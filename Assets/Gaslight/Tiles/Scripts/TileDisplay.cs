using UnityEngine;
using UnityEngine.UI;

public class TileDisplay : MonoBehaviour
{
    public Color RimColorDefault;
    public Color RimColorHighlighted;
    public Color RimColorSelected;
    public RawImage baseImage;
    public int index;

    void Awake()
    {
        baseImage = this.transform.Find("BaseImage").GetComponent<RawImage>();
        if (baseImage == null)
        {
            Debug.LogError("Could not find BaseImage in children");
            return;
        }

    }

    public void SetColor(Color newColor)
    {
        baseImage.color = newColor;
    }
    private void OnValidate()
    {
        RefreshDisplay(); 
    }


    public void RefreshDisplay()
    {
    }
    
        
}
