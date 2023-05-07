using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct GuestDialogue
{
    public string dialogueLine;
    public NPCDialogue[] npcResponseToThisDialogue;
}


[System.Serializable]
public struct NPCDialogue
{
    public AudioClip dialogueAudio;
    [Tooltip("Padding in seconds")]
    public int audioPaddingOffset;
    public GuestDialogue[] guestDialogueOptions;
}



