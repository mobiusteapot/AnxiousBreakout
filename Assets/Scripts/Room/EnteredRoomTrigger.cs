using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnteredRoomTrigger : MonoBehaviour
{
    private Transform playerTransform;
    bool hasPlayerTransform = false;
    bool hasPlayerEntered = false;

    // Debug use only
    [HideInInspector]
    public bool isRoomTriggerDisabled = false;
    public void FixedUpdate()
    {
        if (isRoomTriggerDisabled)
        {
            return;
        }
        if (!hasPlayerTransform)
        {

            playerTransform = PlayerManager.Instance.playerCenterEyeTransform;
            // Re-attempt to get player transform if null
            if(playerTransform != null)
            {
                hasPlayerTransform = true;
            }
            return;
        }

        // If player transform is greater than self on the forward Z direction, then update if player is in or out of the room

        if (transform.InverseTransformPoint(playerTransform.position).z > 0)
        {
            if (!hasPlayerEntered)
            {
                hasPlayerEntered = true;
                PartySceneSingleton.Instance.isPlayerInRoom = true;
            }
            
        }
        else
        {
            if (hasPlayerEntered)
            {
                hasPlayerEntered = false;
                PartySceneSingleton.Instance.isPlayerInRoom = false;
            }
        }
    }
}
