using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneUI : MonoBehaviour
{
    public Color inactiveColor = Color.blue, activeColor = Color.green;

    private void Awake()
    {
        FindObjectOfType<Oculus.Voice.AppVoiceExperience>().VoiceEvents.OnMicStoppedListening.AddListener(() =>
        {
            SetMicColor(false);
        });


        FindObjectOfType<Oculus.Voice.AppVoiceExperience>().VoiceEvents.OnRequestCreated.AddListener((requestedCreated) =>
        {            
            SetMicColor(true);
        });
    }


    public void SetMicColor(bool enabledStatus)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).GetComponent<MeshRenderer>())
                continue;

            transform.GetChild(i).GetComponent<MeshRenderer>().sharedMaterial.color = enabledStatus ? activeColor : inactiveColor;
        }
    }
}
