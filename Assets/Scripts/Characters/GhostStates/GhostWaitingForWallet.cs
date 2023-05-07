using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Can create from menu

public class GhostWaitingForWallet : GhostState
{
    bool hasWallet = false;
    protected override void OnStateEnter()
    {
        if (controller.bodyAnimator != null)
            controller.bodyAnimator.SetTrigger("StartHandoff");
        if (controller.headAnimator != null)
            controller.headAnimator.SetTrigger("StartHappy");
    }
    public override void BehaviorTick()
    {
        if (hasWallet) return;
        if (PartySceneSingleton.Instance.hasWalletBeenGiven)
        {
            controller.ChangeState(controller.idleState);
            hasWallet = true;
        }
    }

    protected override void OnStateExit()
    {
        if (controller.bodyAnimator != null)
            controller.bodyAnimator.SetTrigger("StopHandoff");
    }
}
