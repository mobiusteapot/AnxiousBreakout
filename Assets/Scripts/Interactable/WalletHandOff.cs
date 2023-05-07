using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WalletHandOff : MonoBehaviour
{
    [SerializeField]
    PartyGhostController partyGhostController;

    private void OnEnable()
    {
        ConversationManager.npc3ConversationEnded += CanGiveWallet;

    }


    private void OnDisable()
    {
        ConversationManager.npc3ConversationEnded -= CanGiveWallet;
    }

    bool canGiveWallet = false;

    void CanGiveWallet()
    {
        canGiveWallet = true;
        transform.GetComponent<XRSocketInteractor>().socketActive = true;
        partyGhostController.ChangeState(partyGhostController.waitForWalletState);
    }


    public void GiveWalletToNPC()
    {
        if (!canGiveWallet)
            return;
        
        foreach (var item in FindObjectsOfType<XRSocketInteractor>())
        {
            if (item == transform.GetComponent<XRSocketInteractor>())
                continue;

            item.socketActive = false;
        }
        PartySceneSingleton.Instance.hasWalletBeenGiven = true;
    }
}
