using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicNPCHeightCalibrator : MonoBehaviour
{
    
    [SerializeField]
    Transform dialogueCanvasTransform;

    [SerializeField]
    Transform dialogueBubbleTransform;

    [SerializeField]
    Transform npcTransform;

    [SerializeField]
    Transform eyeGazeHitboxTransform;

    [SerializeField]
    float heightOffsetMax = 0.01f;


    static float verticalDeltaBetweenPlayerAndNPCEyesHitbox;

    private void OnEnable()
    {
        FakeVoiceCalibration.fakeCalibrationDone += InitHeightCalibration;
    }


    private void OnDisable()
    {
        FakeVoiceCalibration.fakeCalibrationDone -= InitHeightCalibration;
    }


    void InitHeightCalibration()
    {

        //Get Offset between player eyes and NPC eyes
        verticalDeltaBetweenPlayerAndNPCEyesHitbox = PlayerManager.Instance.playerCenterEyeTransform.position.y - eyeGazeHitboxTransform.position.y;
        AdjustNPCHeight();

        AdjustEyeGazeHitboxHeight();


        if (dialogueCanvasTransform != null)
            AdjustDialogueCanvasHeight();

        if (dialogueBubbleTransform != null)
            AdjustDialogueBubbleHeight();
    }


    Vector3 newNPCPosition;
    void AdjustNPCHeight()
    {        
        newNPCPosition = new Vector3(npcTransform.position.x, verticalDeltaBetweenPlayerAndNPCEyesHitbox +  npcTransform.position.y + Random.Range(0f, heightOffsetMax), npcTransform.position.z);
        npcTransform.position = newNPCPosition;
    }

    Vector3 newEyeHitboxPosition;
   void AdjustEyeGazeHitboxHeight()
    {
        newEyeHitboxPosition = new Vector3(eyeGazeHitboxTransform.position.x, eyeGazeHitboxTransform.position.y + verticalDeltaBetweenPlayerAndNPCEyesHitbox, eyeGazeHitboxTransform.position.z);
        eyeGazeHitboxTransform.position = newEyeHitboxPosition;
    }

    Vector3 newDialogueCanvasPosition;
    void AdjustDialogueCanvasHeight()
    {
        newDialogueCanvasPosition = new Vector3(dialogueCanvasTransform.position.x, verticalDeltaBetweenPlayerAndNPCEyesHitbox + dialogueCanvasTransform.position.y, dialogueCanvasTransform.position.z);
        dialogueCanvasTransform.position = newDialogueCanvasPosition;
    }


    Vector3 newDialogueBubbleHeight;
    void AdjustDialogueBubbleHeight()
    {
        newDialogueBubbleHeight = new Vector3(dialogueBubbleTransform.transform.position.x, dialogueBubbleTransform.transform.position.y + verticalDeltaBetweenPlayerAndNPCEyesHitbox, dialogueBubbleTransform.transform.position.z);
        dialogueBubbleTransform.position = newDialogueBubbleHeight;
    }
}
