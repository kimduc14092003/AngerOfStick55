using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    public Vector2 minPosLeftBottomCamera, maxPosLeftBottomCamera;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(
            Mathf.Clamp(target.position.x, minPosLeftBottomCamera.x, maxPosLeftBottomCamera.x),
            Mathf.Clamp(target.position.y, minPosLeftBottomCamera.y, maxPosLeftBottomCamera.y),
            transform.position.z);

    }
}
