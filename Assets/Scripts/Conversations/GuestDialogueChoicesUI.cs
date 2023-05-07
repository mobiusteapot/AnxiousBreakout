using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GuestDialogueChoicesUI : MonoBehaviour
{
    public Material defaultDialogueBoxMat, selectedDialogueBoxMat;
    public GameObject[] dialogueBoxes;


    private void OnEnable()
    {
        ConversationManager.OnGuestResponseValidated += ChooseDialogueBox;
    }

    private void OnDisable()
    {
        ConversationManager.OnGuestResponseValidated += ChooseDialogueBox;
    }


    public void SetDialogueChoices(string[] dialogueChoices)
    {
        //Disable all boxes
        for (int i = 0; i < dialogueBoxes.Length; i++)
        {
            dialogueBoxes[i].SetActive(false);
        }

        //Set the default material
        //Set the right dialogue lines
        //Activate the dialogue boxes that have the dialogue lines
        for (int i = 0; i < dialogueChoices.Length; i++)
        {
            dialogueBoxes[i].GetComponent<MeshRenderer>().sharedMaterial = defaultDialogueBoxMat;
            dialogueBoxes[i].GetComponentInChildren<TMP_Text>().text = dialogueChoices[i];
            dialogueBoxes[i].SetActive(true);
        }
    }

    //Find correct dialogue box based on current guest response
    void ChooseDialogueBox(string guestResponse)
    {
        for (int i = 0; i < dialogueBoxes.Length; i++)
        {  
            if(dialogueBoxes[i].GetComponentInChildren<TMP_Text>().text == guestResponse)
            {
                dialogueBoxes[i].GetComponent<MeshRenderer>().sharedMaterial = selectedDialogueBoxMat;
            }            
        }
    }

}
