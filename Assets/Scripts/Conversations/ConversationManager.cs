using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Oculus.Voice;
using System.Text.RegularExpressions;
using Facebook.WitAi.Data;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance = null;
    //public GuestDialogueChoicesUI guestDialogueChoicesUI;
    [SerializeField]
    float guestSpeechPercentageMatch = 60;


    [Space(10)]

    public NPCInitiatedConversation nPC1Conversation;
    public GuestInitiatedConversation nPC2Conversation;
    public NPCInitiatedConversation nPC3Conversation;


    [SerializeReference]
    AppVoiceExperience voiceExperience;


    //[SerializeField]
    //TMP_Text debugText, timeElapsedText;


    static bool guestHasSpoken = false, conversationHasBegun;

    //DEBUG
    //public TMP_Text micActiveText, requestActiveText, goActiveText;
    private void Update()
    {
        //Debug.LogError(voiceExperience.Active + " Current status");

        //if (Input.GetKeyDown(KeyCode.Space))
        //    StartNPC1Conversation(FindObjectOfType<AudioSource>());
        //{
        //    guestHasSpoken = false;
        //    voiceExperience.Activate();
        //}

        //if (voiceExperience.MicActive)
        //    micActiveText.text = "Mic Active";
        //else if (!voiceExperience.MicActive)
        //    micActiveText.text = "Mic Not active";


        //if (voiceExperience.IsRequestActive)
        //    requestActiveText.text = "Request Active";
        //else if (!voiceExperience.IsRequestActive)
        //    requestActiveText.text = "Request Not active";


        //if (voiceExperience.isActiveAndEnabled)
        //    goActiveText.text = "GameObject Active";
        //else if (!voiceExperience.isActiveAndEnabled)
        //    goActiveText.text = "GameObject Not active";
    }

    private void OnEnable()
    {
        OVRManager.HMDMounted += ActivateVoiceAfterHeadsetWasOnStandby;
    }
    

    void ActivateVoiceAfterHeadsetWasOnStandby()
    {
        if (!conversationHasBegun)
            return;

        voiceExperience.Deactivate();
        voiceExperience.Activate();
    }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }

        voiceExperience.events.onFullTranscription.AddListener((transcription) =>
        {            
            //debugText.text = transcription;
            if (conversationHasBegun && !guestHasSpoken)
                OnGuestFinishedResponse(transcription);
        });

        voiceExperience.VoiceEvents.OnMicStoppedListening.AddListener(() =>
        {
            Debug.LogError("MIC stopped listening");
            voiceExperience.Deactivate();
            voiceExperience.Activate();
        });
    }

    //DEBUG
    //async void Start()
    //{
    //    await System.Threading.Tasks.Task.Delay(100);
    //    guestHasSpoken = false;
    //    voiceExperience.Activate();
    //}

    static List<string> currentDialogueLines = new List<string>();

    string[] ExtrackDialogueLines(GuestDialogue[] dialogueOptions)
    {
        currentDialogueLines.Clear();


        Debug.Log(dialogueOptions.Length);

        foreach (var item in dialogueOptions)
        {
            currentDialogueLines.Add(item.dialogueLine);
        }

        return currentDialogueLines.ToArray();
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

            if (percentageMatch >= guestSpeechPercentageMatch && percentageMatch > highestPercentage)
            {
                highestPercentage = percentageMatch;
                dialogueID = i;
            }
        }
        
        return dialogueID == -1 ? "F" : dialogueOptions[dialogueID];
    }



    //Only used to start the conversation with NPC 1
    public void StartNPC1Conversation(AudioSource npcAudioSource)
    {

        conversationHasBegun = true;

        Conversation.currentNPCInitiatedConversation = nPC1Conversation;
        Conversation.currentNPCInitiatedConversation.npcAudioSource = npcAudioSource;
        Conversation.currentGuestDialogueOptions = Conversation.currentNPCInitiatedConversation.npcDialogue.guestDialogueOptions;

        //Play NPC audio
        nPC1Conversation.InitConversation(nPC1Conversation.npcDialogue.dialogueAudio, hasFinishedPlayingAudio =>
        {
            if(hasFinishedPlayingAudio)
            {
                Debug.Log("Finished NPC response");


                //Activate voice service
                voiceExperience.Activate();

                //Show toast, wait for 5 seconds. Move the following 2 lines inside the Toast call back

                ShowGuestDialogueOptions(ExtrackDialogueLines(nPC1Conversation.npcDialogue.guestDialogueOptions));

            }
        });
    }

    
    //Only used to start the conversation with NPC 2
    public void StartNPC2Conversation(AudioSource npcAudioSource)
    {
        conversationHasBegun = true;
        Conversation.currentGuestInitiatedConversation = nPC2Conversation;
        Conversation.currentGuestInitiatedConversation.npcAudioSource = npcAudioSource;
        Conversation.currentNPCDialogueOptions = Conversation.currentGuestInitiatedConversation.guestDialogue.npcResponseToThisDialogue;
        Conversation.currentGuestDialogueOptions = new GuestDialogue[] { Conversation.currentGuestInitiatedConversation.guestDialogue };

        nPC2Conversation.InitConversation();

        //Start Audio Service
        voiceExperience.Activate();

        //Show Dialogue Options
        ShowGuestDialogueOptions(new string[1] { Conversation.currentGuestInitiatedConversation.guestDialogue.dialogueLine });
    }


    //Only used to start the conversation with NPC 3
    public void StartNPC3Conversation(AudioSource npcAudioSource)
    {
        conversationHasBegun = true;
        Conversation.currentNPCInitiatedConversation = nPC3Conversation;
        Conversation.currentNPCInitiatedConversation.npcAudioSource = npcAudioSource;
        Conversation.currentGuestDialogueOptions = Conversation.currentNPCInitiatedConversation.npcDialogue.guestDialogueOptions;
        //Play NPC audio
        nPC3Conversation.InitConversation(nPC3Conversation.npcDialogue.dialogueAudio, hasFinishedPlayingAudio =>
        {
            if (hasFinishedPlayingAudio)
            {

                //Activate voice service
                voiceExperience.Activate();

                ShowGuestDialogueOptions(ExtrackDialogueLines(nPC3Conversation.npcDialogue.guestDialogueOptions));

            }
        });


    }



    public void ShowGuestDialogueOptions(string[] dialogueLines)
    {
        // Probably temporary
        GuestDialogueChoicesUI guestDialogueChoicesUI = FindObjectOfType<GuestDialogueChoicesUI>();
        guestDialogueChoicesUI.SetDialogueChoices(dialogueLines);
    }


    public void StartNPCAudioResponse()
    {

        guestHasSpoken = false;

        SetNPCResponse();

        if(Conversation.currentConversation is NPCInitiatedConversation)
        {
            Conversation.currentNPCInitiatedConversation.PlayNPCAudioResponse(Conversation.currentNPCDialogue.dialogueAudio, isResponseFinished =>
            {

                if (isResponseFinished)
                {
                    OnNPCFinishedResponse();
                }
            });
        }
        else if (Conversation.currentConversation is GuestInitiatedConversation)
        {
            Conversation.currentGuestInitiatedConversation.PlayNPCAudioResponse(Conversation.currentNPCDialogue.dialogueAudio, isResponseFinished =>
            {
                if (isResponseFinished)
                {
                    OnNPCFinishedResponse();
                }
            });
        }
        Debug.Log("Started NPC response");
        
    }

    public static Action npc3ConversationEnded;
    public void EndConversation()
    {
        //debugText.text = "Conversation End";
        guestHasSpoken = false;
        conversationHasBegun = false;
        PartySceneSingleton.Instance.currentPartyEvent.isComplete = true;
        PartyCharacterManager.Instance.SetDialogueCanvasVisibility(false);


        if (Conversation.currentConversation == nPC3Conversation)
        {
            npc3ConversationEnded?.Invoke();
        }

    }


    //Audio response by NPC -> Set text options for player
    async void OnNPCFinishedResponse()
    {

        //EDGE CASE 

        //If Serialization depth has been reached
        if (Conversation.currentGuestDialogue.npcResponseToThisDialogue == null)
        {
            EndConversation();
            return;
        }


        //Show dialogue options OR end conversation
        if (ExtrackDialogueLines(Conversation.currentGuestDialogueOptions).Length <= 0)
        {
            EndConversation();
            return;
        }

        ShowGuestDialogueOptions(ExtrackDialogueLines(Conversation.currentGuestDialogueOptions));

        //Activate voice service
        voiceExperience.Activate();

        await System.Threading.Tasks.Task.Yield();
    }


    //Audio response by player -> Set audio options for NPC
    async void OnGuestFinishedResponse(string guestResponse)
    {
        if (!GuestResponseEnd(guestResponse))
            return;

        //Add a delay between Guest response end and NPC response started
        await System.Threading.Tasks.Task.Delay(1 * 1000);


        //If Serialization depth has been reached
        if (Conversation.currentGuestDialogue.npcResponseToThisDialogue == null)
        {
            EndConversation();
            return;
        }

        //If no responses available then end the conversation
        if (Conversation.currentGuestDialogue.npcResponseToThisDialogue.Length <= 0)
        {
            EndConversation();
            return;
        }

        StartNPCAudioResponse();

    }


    public static Action<string> OnGuestResponseValidated;
    bool GuestResponseEnd(string guestDialogue)
    {
        //Check if the response is valid. Proceed only if valid
        string correctDialogue = GuestDialogueApproximation(ExtrackDialogueLines(Conversation.currentGuestDialogueOptions), guestDialogue);

        if (correctDialogue == "F")
        {
            Debug.LogError("Garbage sentence found");
            return false;
        }

        //String matching
        for (int i = 0; i < Conversation.currentGuestDialogueOptions.Length; i++)
        {
            if(Conversation.currentGuestDialogueOptions[i].dialogueLine == correctDialogue)
            {
                guestHasSpoken = true;

                Debug.Log("Found sentence: " + Conversation.currentGuestDialogueOptions[i].dialogueLine);

                //Set the current guest chosen dialogue
                Conversation.currentGuestDialogue = Conversation.currentGuestDialogueOptions[i];


                Debug.Log("Reached beyond if check");

                //Set the npc response options based on guest chosen dialogue
                Conversation.currentNPCDialogueOptions = Conversation.currentGuestDialogue.npcResponseToThisDialogue;

                //Validation event
                OnGuestResponseValidated?.Invoke(Conversation.currentGuestDialogue.dialogueLine);

                //Debug.Log(Conversation.currentNPCDialogueOptions.Length + " set");
                return true;
            }            
        }

        return false;
    }


    int randomID;
    void SetNPCResponse()
    {
        randomID = UnityEngine.Random.Range(0, Conversation.currentNPCDialogueOptions.Length - 1);

        //Select a random response
        Conversation.currentNPCDialogue = Conversation.currentNPCDialogueOptions[randomID];

        //Set Guest Responses options based on the npc chosen diaogue
        Conversation.currentGuestDialogueOptions = Conversation.currentNPCDialogue.guestDialogueOptions;        
    }    
}


public class Conversation
{

    public static Conversation currentConversation;

    public AudioSource npcAudioSource;



    //Just being stored. No references yet
    public static NPCInitiatedConversation currentNPCInitiatedConversation;
    public static GuestInitiatedConversation currentGuestInitiatedConversation;

    //Potential options
    public static GuestDialogue[] currentGuestDialogueOptions;
    public static NPCDialogue[] currentNPCDialogueOptions;

    //Selected options
    public static GuestDialogue currentGuestDialogue;
    public static NPCDialogue currentNPCDialogue;
}





[System.Serializable]
public class NPCInitiatedConversation : Conversation
{
    [Space(10)]

    public NPCDialogue npcDialogue;


    public void InitConversation(AudioClip npcResponse, Action<bool> hasFinishedPlayingAudio)
    {
        currentConversation = this;
        currentNPCInitiatedConversation = this;
        currentGuestDialogueOptions = npcDialogue.guestDialogueOptions;
                

        PlayNPCAudioResponse(npcResponse, status =>
        {
            if (status)
                hasFinishedPlayingAudio(true);
        });
    }


    public async void PlayNPCAudioResponse(AudioClip npcResponse, Action<bool> isResponseFinished)
    {
        
        npcAudioSource.clip = npcResponse;


        await System.Threading.Tasks.Task.Delay(10);

        npcAudioSource.PlayOneShot(npcResponse);


        Debug.LogError(npcResponse.length);

        await System.Threading.Tasks.Task.Delay((int)npcResponse.length * 1000);
        

        //Add Wait for audio offset time
        Debug.LogError("Will return");
        isResponseFinished(true);
    }
}




[System.Serializable]
public class GuestInitiatedConversation : Conversation
{
    public GuestDialogue guestDialogue;
    

    public void InitConversation()
    {
        currentConversation = this;
    }

    public async void PlayNPCAudioResponse(AudioClip npcResponse, Action<bool> isResponseFinished)
    {


        if(npcResponse == null)
        {
            isResponseFinished(true);
            return;
        }

        npcAudioSource.clip = npcResponse;


        await System.Threading.Tasks.Task.Delay(10);

        npcAudioSource.PlayOneShot(npcResponse);


        Debug.LogError(npcResponse.length);

        await System.Threading.Tasks.Task.Delay((int)npcResponse.length* 1000);


        //Add Wait for audio offset time
        Debug.LogError("Will return");
        isResponseFinished(true);
    }
}


