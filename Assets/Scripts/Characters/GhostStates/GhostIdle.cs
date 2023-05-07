using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class GhostIdle : GhostState
{
    bool hasTurned = false;
    float rotateOnStareAmount = 40f;

    protected override void OnStateEnter()
    {
        // There is no explicit idle animation! If other states are exited, then idle plays automatically
        if (controller.ghostVariantData.traits.danceIdle)
        {
            controller.bodyAnimator.SetTrigger("StartDancing");
            controller.headAnimator.SetTrigger("StartHappy");
        }
    }

    public override void OnGazeEnter()
    {        
        if (controller.ghostVariantData.traits.isShy)
        {
            if (!hasTurned)
            {
                // Rotate 30 degrees in either the positive or negative direction
                float randomRotation = UnityEngine.Random.Range(0, 2) == 0 ? rotateOnStareAmount : -rotateOnStareAmount;
                controller.ghostTurnSpeedMultiplier = 7f;
                controller.RotateTarget(randomRotation,true);
                hasTurned = true;
                TurnBack();
            }
        }

    }
    public override void OnGazeExit()
    {
        // If nosy, stare at the player when they stare away
        if (controller.ghostVariantData.traits.isNosy)
        {
            if (!hasTurned)
            {
                // Rotate 30 degrees in either the positive or negative direction
                hasTurned = true;
                TurnToPlayerAfterDelay();
            }
        }

    }
    // Turn to player after delay
    async void TurnToPlayerAfterDelay()
    {
        await Task.Delay(1000);
        controller.ghostTurnSpeedMultiplier = 2f;
        controller.RotateTargetLookAt(PlayerManager.Instance.playerCenterEyeTransform);
        TurnBack(6000);
    }

    async void TurnBack(int delay = 3000)
    {
        await Task.Delay(delay);
        Debug.Log("Turning back!");
        controller.ghostTurnSpeedMultiplier = 1f;
        controller.RotateTargetToDefault();
        await Task.Delay(1000);
        hasTurned = false;
    }
}
