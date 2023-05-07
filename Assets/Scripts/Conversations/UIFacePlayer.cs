using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFacePlayer : MonoBehaviour
{
    static float rotateTolerance = 30f;
    static float rotateSpeed = 0.5f;
    private GameObject targetRotator;
    bool isRotating = false;
    private Transform playerEyeCenter;

    private void Start()
    {
        // Create target rotator and set it as child of ghost
        targetRotator = new GameObject("TargetRotator");
        targetRotator.transform.SetParent(transform,false);
        playerEyeCenter = PlayerManager.Instance.playerCenterEyeTransform;

        // This may be redundant? Start happens after OnEnable
        // But getting references to instance isn't safe on Awake
        UpdateRotater();
        transform.rotation = targetRotator.transform.rotation;
    }
    private void OnEnable()
    {
        if (playerEyeCenter != null)
        {
            UpdateRotater();
            transform.rotation = targetRotator.transform.rotation;
        }
    }

    // If the player is more than 10 degrees off from the last rotation, rotate to face the player
    void Update()
    {
        UpdateRotater();
        if (isRotating)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotator.transform.rotation, rotateSpeed * Time.deltaTime);
            // Flip rotation by 180 degrees
            if (Vector3.Angle(transform.forward, targetRotator.transform.forward) < rotateTolerance)
            {
                isRotating = false;
            }
        }
        else if (Vector3.Angle(transform.forward, targetRotator.transform.forward) > rotateTolerance)
        {
            isRotating = true;
        }
    }

    void UpdateRotater()
    {
        targetRotator.transform.LookAt(playerEyeCenter);
        targetRotator.transform.eulerAngles = new Vector3(0, targetRotator.transform.eulerAngles.y + 180, 0);
    }
}
