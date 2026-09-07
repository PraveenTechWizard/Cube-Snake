using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraTargetSetter : MonoBehaviour
{
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        //
        if (player != null)
        {
            CinemachineCamera camera = GetComponent<CinemachineCamera>();
            //
            if (camera != null)
            {
                camera.Target.TrackingTarget =  player.transform;
            }
        }
        else
        {
            Debug.LogError("No player found");
        }
    }
}
