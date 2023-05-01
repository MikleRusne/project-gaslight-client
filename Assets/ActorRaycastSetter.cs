using Gaslight.Characters;
using UnityEngine;

public class ActorRaycastSetter : MonoBehaviour
{
    public Actor myActor;

    public void Awake()
    {
        if (myActor != null)
        {
            myActor._detectionRaycastOffset = this.transform.localPosition;
            GameObject.DestroyImmediate(this.gameObject);
        }
    }
}
