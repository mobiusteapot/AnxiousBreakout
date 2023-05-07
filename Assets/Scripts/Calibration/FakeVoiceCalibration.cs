using System.Collections;
using UnityEngine;
using Oculus.Voice;
using System.Text.RegularExpressions;
using System;

public class FakeVoiceCalibration : MonoBehaviour
{

    [SerializeReference]
    AppVoiceExperience voiceExperience;

    public GameObject instructionCube;
    public GameObject dialogueOptions1, dialogueOptions2;

    public Material correctDialogueMat;


    public static Action fakeCalibrationDone;
    private void Awake()
    {
        if (voiceExperience == null)
            voiceExperience = FindObjectOfType<AppVoiceExperience>();

        voiceExperience.events.onFullTranscription.AddListener((transcription) =>
        {
            ValidateGuest(transcription);            
        });

        voiceExperience.VoiceEvents.OnMicStoppedListening.AddListener(() =>
        {

            Debug.LogError("MIC stopped listening");
            voiceExperience.Deactivate();
            voiceExperience.Activate();
        });

    }




    public void StartFakeVoiceCalibration()
    {
        StartCoroutine(ActivateFakeVoiceCalibration());
        Vector3 positionOffset = PlayerManager.Instance.playerCenterEyeTransform.forward * 0.45f;
        transform.position = PlayerManager.Instance.playerCenterEyeTransform.position;
        transform.position = new Vector3(transform.position.x + positionOffset.x, transform.position.y, transform.position.z + positionOffset.z);
    }


    public void StopFakeVoiceCalibration()
    {
        //Stop voice service
        voiceExperience.Deactivate();

        //Show dialogue options
        dialogueOptions1.SetActive(false);
        dialogueOptions2.SetActive(false);
        fakeCalibrationDone?.Invoke();
        PartySceneSingleton.Instance.currentPartyEvent.isComplete = true;
        Destroy(this.gameObject);
    }

    IEnumerator ActivateFakeVoiceCalibration()
    {
        ////Waiting for player to put on the headset. Remove if unnecessary
        //yield return new WaitForSeconds(3f);

        //Show the instruction box
        instructionCube.SetActive(true);

        yield return new WaitForSeconds(2f);

        //Start voice service
        voiceExperience.Activate();

        //Show dialogue options
        dialogueOptions1.SetActive(true);
        dialogueOptions2.SetActive(true);

        yield break;
    }


    IEnumerator DialogueSelected()
    {
        correctDialogueOption.transform.GetComponent<MeshRenderer>().sharedMaterial = correctDialogueMat;
        
        yield return new WaitForSeconds(2.5f);

        StopFakeVoiceCalibration();

        yield break;
    }


    GameObject correctDialogueOption;
    void ValidateGuest(string guestSpeech)
    {
        string correctDialogueLine = GuestDialogueApproximation(new string[] { dialogueOptions1.GetComponentInChildren<TMPro.TMP_Text>().text,
        dialogueOptions2.GetComponentInChildren<TMPro.TMP_Text>().text}, guestSpeech);

        if(correctDialogueLine == "F")
        {
            return;
        }

        if(correctDialogueLine == dialogueOptions1.GetComponentInChildren<TMPro.TMP_Text>().text)
        {
            correctDialogueOption = dialogueOptions1;
            StartCoroutine(DialogueSelected());
            return;
        }

        if (correctDialogueLine == dialogueOptions2.GetComponentInChildren<TMPro.TMP_Text>().text)
        {
            correctDialogueOption = dialogueOptions2;
            StartCoroutine(DialogueSelected());
            return;
        }
    }
    
    static float percentageMatch;
    static int currentLevenshteinScore, dialogueID;
    string GuestDialogueApproximation(string[] dialogueOptions, string guestDialogue)
    {

        //Remove all punctuations
        guestDialogue = Regex.Replace(guestDialogue, "[!\"#$%&'()*+,-./:;<=>?@\\[\\]^_`{|}~]", string.Empty);
        guestDialogue = guestDialogue.ToLower();

        float highestPercentage = 0;
        dialogueID = -1;

        for (int i = 0; i < dialogueOptions.Length; i++)
        {
            currentLevenshteinScore = StringMatcher.LevenshteinDistance(guestDialogue, dialogueOptions[i]);

            percentageMatch = 100f * (1 - ((float)currentLevenshteinScore) / (float)Math.Max(guestDialogue.Length, dialogueOptions[i].Length));

            Debug.Log(percentageMatch);

            if (percentageMatch >= 50 && percentageMatch > highestPercentage)
            {
                highestPercentage = percentageMatch;
                dialogueID = i;
            }
        }

        return dialogueID == -1 ? "F" : dialogueOptions[dialogueID];
    }


}
