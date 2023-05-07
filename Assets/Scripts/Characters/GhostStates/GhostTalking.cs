using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GhostTalking : GhostState
{
    Transform otherGhostTransform;
    // Just for playing a talking animation
    protected override void OnStateEnter()
    {
        if (controller.bodyAnimator != null)
            controller.bodyAnimator.SetTrigger("StartTalking");
        if (controller.useTalkingToOtherGhost)
        {
            otherGhostTransform = controller.otherTalkingGhost.transform;
            controller.RotateTargetLookAt(otherGhostTransform);
        }
    }

    protected override void OnStateExit()
    {
        if (controller.bodyAnimator != null)
            controller.bodyAnimator.SetTrigger("StopTalking");
    }
}
