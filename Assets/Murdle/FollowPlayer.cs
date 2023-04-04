using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class FollowPlayer : MonoBehaviour
{
    public Transform Player;

    public GameObject cam;

    private bool isPlayerSet = false;
    // Start is called before the first frame update
    void SetPlayer()
    {
        var PlayerObject = GameObject.Find("Player");
        if (PlayerObject != null)
        {
            isPlayerSet = true;
            Player = PlayerObject.transform;
        }
    }
    void Start()
    {
        SetPlayer();
        cam = transform.Find("Main Camera").gameObject;
    }

    public Vector3 PosOffset;
    // Update is called once per frame
    private Vector3 RotOffset;
    void Update()
    {
        if(Player==null){SetPlayer();
            // return;
        }
        else
        {
            this.transform.position = Player.transform.position + PosOffset;
            // Camera.transform.rotation = Quaternion.LookRotation(Player.transform.position, Vector3.up);
            cam.transform.LookAt(Player.position);
        }
    }
}
