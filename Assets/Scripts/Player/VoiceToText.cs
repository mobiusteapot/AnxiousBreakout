using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Voice;
using Facebook.WitAi.Lib;
using Facebook.WitAi.Interfaces;
using Facebook.WitAi.Events;
using UnityEngine.Events;
using Facebook.WitAi;
using System;
using UnityEngine.XR.Interaction.Toolkit;

public class VoiceToText : MonoBehaviour
{

    [SerializeReference]
    AppVoiceExperience voiceExperience;

    public TMPro.TMP_Text debugText;

    public ActionBasedController leftHandController;

    public delegate void TranscriptionEvent(string transcription);
    public static event TranscriptionEvent FullyTranscriped;



    private void Awake()
    {
        voiceExperience.events.onFullTranscription.AddListener((transcription) =>
        {
            debugText.text = transcription;
            FullyTranscriped?.Invoke(transcription);
            Debug.Log(transcription);
        });
    }


    private void Update()
    {

        #region Voice Activation Debug
        if (leftHandController.selectAction.action.WasPerformedThisFrame())
            voiceExperience.Activate();


        if (Input.GetKeyDown(KeyCode.Space))
        {
            voiceExperience.Activate();
        }
        #endregion


    }



    //public void FullyTranscriped(string result)
    //{
    //    Debug.Log("ANS = " + result);
    //}

    //public void PrintDialogue(WitResponseNode witResponse)
    //{        
    //    debugText.text = witResponse.Value.ToString();

    //}    

}
