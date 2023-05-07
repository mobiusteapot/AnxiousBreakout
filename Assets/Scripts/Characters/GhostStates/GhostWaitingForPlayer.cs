using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Can create from menu

public class GhostWaitingForPlayer : GhostState
{

    protected override void OnStateEnter()
    {
        controller.talkIcon.SetActive(true);
        if (controller.useTalkingToOtherGhost)
        {
            // If interupttable, don't stop talking
            controller.bodyAnimator.SetTrigger("StartTalking");
            controller.otherTalkingGhost.bodyAnimator.SetTrigger("StartTalking");
        }
    }

    public override void BehaviorTick()
    {
        if ((PartyCharacterManager.Instance.playerStartConversationProximity)
                    > Vector3.Distance(PlayerManager.Instance.playerCenterEyeTransform.position, controller.transform.position))
        {
            controller.ChangeState(controller.talkToPlayerState);
            if (controller.useTalkingToOtherGhost)
            {
                controller.bodyAnimator.SetTrigger("StopTalking");
            }
        }
    }
}
