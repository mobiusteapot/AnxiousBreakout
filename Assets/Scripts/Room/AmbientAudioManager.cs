using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AmbientAudioManager : MonoBehaviour
{
    static public AmbientAudioManager Instance = null;
    [Header("This class generates it's own audio sources. Do not add an audio source.")] 
    public float fadeTime = 1f;
    [Range(0,1),Tooltip("This is on a logarithmic scale")]
    public float maxVolume = 1f;
    public AudioMixer audioMixer;
    [Space]
    public AudioMixerGroup insideMixer;
    public string insideParam = "Inside";
    public AudioMixerGroup outsideMixer;
    public string outsideParam = "Outside";

    [SerializeField] 
    AmbientAudioClip[] ambientAudioClips;

    // Find correct components and set default values
    private void Reset()
    {
        // If contains any audio source components, remove them
        AudioSource[] oldAudioSources = this.gameObject.GetComponents<AudioSource>();
        foreach(AudioSource oldAudioSource in oldAudioSources)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(GetComponent<AudioSource>());
            };
#endif
        }
    }
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        
        // Create an audio source for each clip and assign defaults
        // Todo: serialize this and generate new audio sources and use HideFlags.HideInInspector
        foreach (AmbientAudioClip ambientAudioClip in ambientAudioClips)
        {
            AudioSource inAudioSource = this.gameObject.AddComponent<AudioSource>();
            inAudioSource.playOnAwake = false;
            inAudioSource.loop = true;
            inAudioSource.clip = ambientAudioClip.insideClip;
            inAudioSource.volume = maxVolume;
            inAudioSource.outputAudioMixerGroup = insideMixer;

            ambientAudioClip.audioInsideSource = inAudioSource;
            

            AudioSource outAudioSource = this.gameObject.AddComponent<AudioSource>();
            outAudioSource.playOnAwake = false;
            outAudioSource.loop = true;
            outAudioSource.clip = ambientAudioClip.outsideClip;
            outAudioSource.volume = maxVolume;
            outAudioSource.outputAudioMixerGroup = outsideMixer;
            
            ambientAudioClip.audioOutsideSource = outAudioSource;

            
        }
    }
    private void OnEnable()
    {
        PartySceneSingleton.Instance.OnPlayerEnter += OnPlayerEnter;
        PartySceneSingleton.Instance.OnPlayerExit += OnPlayerExit;
    }
    private void OnDisable()
    {
        PartySceneSingleton.Instance.OnPlayerEnter -= OnPlayerEnter;
        PartySceneSingleton.Instance.OnPlayerExit -= OnPlayerExit;
    }
    public void StartAudio()
    {
        
        foreach (AmbientAudioClip ambientAudioClip in ambientAudioClips)
        {
            PlayAmbientAudioClip(ambientAudioClip);
        }
        StartCoroutine(FadeAudioClip(PartySceneSingleton.Instance.isPlayerInRoom,0f));
    }
    void PlayAmbientAudioClip(AmbientAudioClip audioClip)
    {
        audioClip.audioInsideSource.Play();
        audioClip.audioOutsideSource.Play();
    }
    void ChangeAudioClipState(bool isNowInside)
    {
        if (isNowInside)
        {
            
        }
        else
        {
            StartCoroutine(FadeAudioClip(false));
        }
        Debug.Log("Ambient Audio State is now: " + isNowInside);
    }
    IEnumerator FadeAudioClip(bool fadeToInside, float duration = 1f)
    {
        audioMixer.GetFloat(insideParam, out float insideStartVolume);
        audioMixer.GetFloat(outsideParam, out float outsideStartVolume);

        insideStartVolume = Mathf.Pow(10, insideStartVolume / 20);
        outsideStartVolume = Mathf.Pow(10, outsideStartVolume / 20);

        float insideNewVolume = fadeToInside ? 1 : 0.0001f;
        float outsideNewVolume = fadeToInside ? 0.0001f : 1;

        Debug.Log("audiomixer: " + audioMixer.name);
        if (fadeToInside) Debug.Log("Fading out"); else Debug.Log("Fading in");
        
        float t = 0;
        while (t <= duration)
        {

            t += Time.deltaTime;
            audioMixer.SetFloat(insideParam, Mathf.Log10(Mathf.Lerp(insideStartVolume, insideNewVolume, t / duration)) * 20);
            // Stagger fade
            
            audioMixer.SetFloat(outsideParam, Mathf.Log10(Mathf.Lerp(outsideStartVolume, outsideNewVolume, t * t * (3f - 2f * t) / duration)) * 20);
            yield return null;
        }
        yield break;
    }
    
    void OnPlayerEnter()
    {
        foreach (AmbientAudioClip ambientAudioClip in ambientAudioClips)
        {
            StartCoroutine(FadeAudioClip(true, fadeTime));
        }
        Debug.Log("Player has now entered, playing entered audio");
        
    }
    void OnPlayerExit()
    {
        foreach (AmbientAudioClip ambientAudioClip in ambientAudioClips)
        {
            StartCoroutine(FadeAudioClip(false,fadeTime));
        }
        Debug.Log("Player has now exited, playing exit audio");
    }
}

[Serializable]
public class AmbientAudioClip
{
    public string label;
    [HideInInspector]
    public AudioSource audioInsideSource;
    [HideInInspector]
    public AudioSource audioOutsideSource;
    [SerializeReference]
    AudioClip _insideClip;
    public AudioClip insideClip
    {
        get
        {
            return _insideClip;
        }
        set
        {
            _insideClip = value;
            duration = _insideClip.length;
        }
    }
    [SerializeReference]
    AudioClip _outsideClip;
    public AudioClip outsideClip
    {
        get
        {
            return _outsideClip;
        }
        set
        {
            _outsideClip = value;
            duration = _outsideClip.length;
        }
    }
    [HideInInspector]
    public float duration;
}

// Fix this AFTER the playtest
#if UNITY_EDITOR
[CustomEditor(typeof(AmbientAudioManager))]
public class PartSceneAmbientAudioManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (Application.isPlaying)
        {
            // Button to swap if inside or not
            if (GUILayout.Button("Player Enter"))
            {
                // Disable in game check if manually defining
                FindObjectOfType<EnteredRoomTrigger>().isRoomTriggerDisabled = true;
                PartySceneSingleton.Instance.isPlayerInRoom = true;
            }
            if (GUILayout.Button("Player Exit"))
            {
                FindObjectOfType<EnteredRoomTrigger>().isRoomTriggerDisabled = true;
                PartySceneSingleton.Instance.isPlayerInRoom = false;
            }
            EditorGUILayout.HelpBox("Selecting either of these choices will disable the player room trigger from updating.", MessageType.Warning);
        }
    }
}


#endif