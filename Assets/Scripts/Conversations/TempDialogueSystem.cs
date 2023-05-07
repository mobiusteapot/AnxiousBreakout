using System.Collections;
using System.Collections.Generic;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.Events;

public class TempDialogueSystem : MonoBehaviour
{


    public Dialogues dialogueObject;

    public GameObject tempDialogueBox;


    public float distanceFromPlayerToActivatedialogueBox;
    
    Vector2 dialoguePosition, mainCamPosition = Vector2.zero;


    public UnityEvent OnGuestFinishedSpeaking;


    private void Awake()
    {
        dialogueObject.voiceExperience.events.onFullTranscription.AddListener((transcription) =>
        {
            OnGuestFinishedSpeaking?.Invoke();

            Toast transcriptionToast = new Toast(new KeyValueList<string, float>()
                {
                    { transcription, 5f }
                });

            ToastController.toastControllerInstance.ShowToast(transcriptionToast, () =>
            {

            });



            if (CompareDialogue(dialogueObject.dialogueToCompare, transcription))
            {

                //Return phone
            }

            Debug.Log(transcription);
        });
    }



    // Start is called before the first frame update
    void Start()
    {
        dialoguePosition = new Vector2(transform.position.x, transform.position.z);
        mainCamPosition = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void StartListening()
    {
        dialogueObject.SelectDialogue();
    }

    public void OpenDialogueBox()
    {
        tempDialogueBox.SetActive(true);
    }

    public void CloseDialogueBox()
    {
        tempDialogueBox.SetActive(false);
    }



    public bool CompareDialogue(string dialogue1, string dialogue2)
    {
        return string.Equals(dialogue1.ToLower(), dialogue2.ToLower());
    }

}

[System.Serializable]
public class Dialogues
{

    public string dialogueToCompare = "";


    [SerializeReference]
    public AppVoiceExperience voiceExperience;

    public delegate void TranscriptionEvent(string transcription);
    public static event TranscriptionEvent FullyTranscriped;


    public void SelectDialogue()
    {
        voiceExperience.Activate();
    }
}

