using UnityEngine;

public class ThreeStateShaderController: MonoBehaviour
{
    public Color RimColorDefault;
    public Color RimColorHighlighted;
    public Color RimColorSelected;
    public Material mat;

    private static int mat_isSelected = Shader.PropertyToID("isSelected");
    private static int mat_isHighlighted = Shader.PropertyToID("isHighlighted");
    public enum State
    {
        Highlighted,
        Selected,
        Idle
    }

    [SerializeField]public State state;
    void Awake()
    {
        mat = this.GetComponent<Renderer>().material;
    }

    private void OnValidate()
    {
        RefreshShader(); 
    }

    public void setState(State newState)
    {
        if (state != newState)
        {
            // Debug.Log("Not refreshing");
            state = newState;
            RefreshShader();
        }
    }
    public void RefreshShader()
    {
        switch (state)
        {
            case State.Idle:
                mat.SetInt(mat_isHighlighted, 0);
                mat.SetInt(mat_isSelected, 0);
                break;
            case State.Highlighted:
                mat.SetInt(mat_isHighlighted, 1);
                mat.SetInt(mat_isSelected, 0);
                break;
            case State.Selected:
                mat.SetInt(mat_isHighlighted, 0);
                mat.SetInt(mat_isSelected, 1);
                break;
        }
    }
    
}