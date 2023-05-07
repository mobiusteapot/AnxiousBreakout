using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Can create from menu

public class GhostTalkingToPlayer : GhostState
{
    
    protected override void OnStateEnter()
    {

        PartyCharacterManager.Instance.StartNextConversation();
        controller.talkIcon.SetActive(false);
        if (controller.bodyAnimator != null)
        {
            controller.bodyAnimator.SetTrigger("StopDancing");
            
            if (controller.useTalkingToOtherGhost)
            {
                controller.otherTalkingGhost.bodyAnimator.SetTrigger("StopTalking");
                controller.otherTalkingGhost.headAnimator.SetTrigger("StartAngry");
                controller.bodyAnimator.SetTrigger("StopTalking");
                controller.headAnimator.SetTrigger("StartAngry");

            }
            else
            {
                controller.bodyAnimator.SetTrigger("StartTalking");
            }
        }
    }
    public override void BehaviorTick()
    {
        // Look at player
        controller.RotateTargetLookAt(PlayerManager.Instance.playerCenterEyeTransform);
    }

    protected override void OnStateExit()
    {
        controller.bodyAnimator.SetTrigger("StopTalking");
        Debug.Log("Stop talking animation here!");
        if (controller.ghostVariantData.traits.isInteruptable)
        {
            controller.headAnimator.SetTrigger("StopAngry");
            controller.otherTalkingGhost.headAnimator.SetTrigger("StopAngry");
            controller.otherTalkingGhost.bodyAnimator.SetTrigger("StartTalking");
            // Make next state talking again
        }
    }
}
