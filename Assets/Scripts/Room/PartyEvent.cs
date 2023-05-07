using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Can add new but do not reorder
public enum EventCategory
{
    Gameplay,
    Toast,
    UI,
    RequireCondition
}

// For all "re-orderable" events. Designed to contain all events, so game order can be re-ordered
[System.Serializable]
[CreateAssetMenu(fileName = "PartyEvent", menuName = "PartyEvent", order = 1)]
public class PartyEvent : ScriptableObject
{
    [SerializeField,Tooltip("This is just for organization. Does not affect gameplay")]
    public EventCategory eventCategory = EventCategory.Gameplay;

    [SerializeField]
    public string toastString = "Toast String";
    [SerializeField,Tooltip("How long toasts will be displayed for before disappearing")]
    public float toastDuration = 5f;
    [SerializeField, Tooltip("If true, will wait for toast to finish displaying before continuing")]
    public bool waitForToast = false;
    public bool hasShownToast = false;

    [SerializeField, Tooltip("Executes before start delay")]
    public UnityEvent OnAwake;
    [SerializeField,Tooltip("Delay before the start method will begin")]
    public float eventStartDelay = 0;
    [SerializeField, Tooltip("Executes after start delay")]
    public UnityEvent OnBegin;
    [SerializeField, Tooltip("Executes every update frame")]
    public UnityEvent OnUpdate;
    [SerializeField, Tooltip("Executes after event has been completed")]
    public UnityEvent OnEnd;



    [HideInInspector]
    public bool isStarted = false;
    // Each event must be marked as complete somewhere in it's chain of events.
    // Any script can mark it as complete, but game will softlock if nothing ever does.
    [HideInInspector]
    public bool isComplete = false;

    


    // All methods, or all start methods for each event must be stored locally
    public void StartSUDs()
    {
        PartySceneSingleton.Instance.sudsManager.StartSUDS();
    }

    public void StartVoiceCalibration()
    {
        Instantiate(PartySceneSingleton.Instance.voiceCalibratorPrefab).GetComponent<FakeVoiceCalibration>().StartFakeVoiceCalibration();
    }

    public void StartMusic()
    {
        AmbientAudioManager.Instance.StartAudio();
        isComplete = true;
    }
    // won't mark as complete unless player is in room. Needs to be in an update method
    // Can be used wherever as a sanity check
    public void WaitForPlayerToBeInRoom()
    {
        if (PartySceneSingleton.Instance.isPlayerInRoom)
            isComplete = true;
    }
    public void WaitForPlayerToGiveWallet()
    {
        if (PartySceneSingleton.Instance.hasWalletBeenGiven)
            isComplete = true;
    }
    public void SpawnWallet()
    {
        PartySceneSingleton.Instance.SpawnWallet();
        isComplete = true;
    }

    public void WaitForWalletToBePickedUp()
    {

        if (PartySceneSingleton.Instance.hasWalletBeenPickedUp)
            isComplete = true;
    }

    // Returns the toast it is displaying. This is primarily to make sure it can't be selected by someone as a unity event (hack)
    // But also might be actually useful
    public Toast DisplayToast()
    {
        Toast toast = new Toast(new KeyValueList<string, float>()
        {
            { toastString, toastDuration },
        });
        Debug.Log("Displaying toast: " + toastString);
       



        ToastController.toastControllerInstance.ShowToast(toast, () =>
        {
            isComplete = true;
        });
        return toast;
    }
    /// <summary>
    /// Makes NPC talkable by a player. Will wait for player to get close enough to start dialogue
    /// </summary>
    /// <param name="npcIndex"></param>
    public void MakeNPCTalkable(int npcIndex)
    {
        PartyCharacterManager.Instance.EnableConversationByIndex(npcIndex);
    }
    public void MakeNPCIdle(int npcIndex)
    {
        PartyCharacterManager.Instance.OverrideGhostStateByIndex(npcIndex, new GhostIdle());
    }
    /// <summary>
    /// Visually makes NPC appear to be talking to other NPC. Contains no logic for talking to a player or looking to talk to a player
    /// </summary>
    /// <param name="npcIndex"></param>
    public void MakeNPCTalkToNPC(int npcIndex)
    {
        PartyCharacterManager.Instance.OverrideGhostStateByIndex(npcIndex, new GhostTalking());
    }

    public void MakeNPCWaitForWallet(int npcIndex)
    {
        PartyCharacterManager.Instance.OverrideGhostStateByIndex(npcIndex, new GhostWaitingForWallet());
    }


    public void EndExperience()
    {
        //ToDo:
        //OVR Passthrough toggle
        FindObjectOfType<PlayerManager>().TogglePlayerPassthroughVisibility((isUserBlind) =>
        { 
            if(isUserBlind)
            {
                //Destroy Wallet
                Destroy(FindObjectOfType<WalletInteractable>().gameObject);

                //Destroy all ghosts
                foreach (var item in FindObjectsOfType<PartyGhostController>())
                {
                    Debug.LogError("Will destroy now");
                    Destroy(item.gameObject);
                }

                PartySceneSingleton.Instance.currentPartyEvent.isComplete = true;
            }
        });        
        
    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(PartyEvent))]
public class PartyEventEditor : Editor
{
    SerializedProperty eventCategory;

    SerializedProperty toastString;
    SerializedProperty toastDuration;
    SerializedProperty waitForToast;
    List<SerializedProperty> toastInfo = new List<SerializedProperty>();

    SerializedProperty OnAwake;
    SerializedProperty eventStartDelay;
    SerializedProperty OnBegin;
    SerializedProperty OnUpdate;
    SerializedProperty OnEnd;
    List <SerializedProperty> Events = new List<SerializedProperty>();

    SerializedProperty isComplete;
    SerializedProperty isStarted;


    private void OnEnable()
    {
        eventCategory = serializedObject.FindProperty("eventCategory");

        toastString = serializedObject.FindProperty("toastString");
        toastDuration = serializedObject.FindProperty("toastDuration");
        waitForToast = serializedObject.FindProperty("waitForToast");
        // Do not include isToastEvent or any other "is" check in any list
        // Manually handling toast string for style override
        //toastInfo.Add(toastString);
        toastInfo.Add(toastDuration);
        toastInfo.Add(waitForToast);


        OnAwake = serializedObject.FindProperty("OnAwake");
        OnBegin = serializedObject.FindProperty("OnBegin");
        OnUpdate = serializedObject.FindProperty("OnUpdate");
        OnEnd = serializedObject.FindProperty("OnEnd");

        Events.Add(OnAwake);
        Events.Add(OnBegin);
        Events.Add(OnUpdate);
        Events.Add(OnEnd);

        eventStartDelay = serializedObject.FindProperty("eventStartDelay");

        isComplete = serializedObject.FindProperty("isComplete");
        isStarted = serializedObject.FindProperty("isStarted");

    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        
        var bgColor = GUI.backgroundColor;
        GUI.backgroundColor = EventCategoryColor(eventCategory.enumValueIndex);

        
        Texture2D icon = SetIconForEvent(eventCategory.enumValueIndex);
        if (target != null && icon != null)
        {
            
            EditorGUIUtility.SetIconForObject(target, icon);
        }
        EditorGUILayout.PropertyField(eventCategory);
        GUI.backgroundColor = bgColor;
        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        if (eventCategory.enumValueIndex == 1)
        {
            eventCategory.enumValueIndex = 1;
            GUIStyle style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
            toastString.stringValue = EditorGUILayout.TextArea(
                toastString.stringValue,
                style
                );
            
            foreach (SerializedProperty toastInfoPiece in toastInfo)
            {
                EditorGUILayout.PropertyField(toastInfoPiece);
            }
        }
        else
        {
            EditorGUILayout.PropertyField(eventStartDelay);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            // Display events, not toast info
            bool hasNoMethodCalls = true;
            foreach (SerializedProperty Event in Events)
            {
                EditorGUILayout.PropertyField(Event);
                SerializedProperty persistentCalls = Event.FindPropertyRelative("m_PersistentCalls.m_Calls");
                if (persistentCalls.arraySize > 0)
                {
                    hasNoMethodCalls = false;
                    for (int i = 0; i < persistentCalls.arraySize; ++i)
                        persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_CallState").intValue = (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
                }
            }
            if (hasNoMethodCalls)
            {
                EditorGUILayout.HelpBox("Event has no methods!\nSet a method in any event or else the party event will cause a soft lock!", MessageType.Warning);
            }
        }

        if(EditorGUI.EndChangeCheck()){
            // Mark edited in perforce if changed!
            AssetDatabase.MakeEditable(AssetDatabase.GetAssetPath(target));
        }

        serializedObject.ApplyModifiedProperties();
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {

        Texture2D newPreview = SetIconForEvent(eventCategory.enumValueIndex);
        Texture2D icon = new Texture2D(width, height);
        
        if (newPreview != null)
        {
            EditorUtility.CopySerialized(newPreview, icon);
            //Debug.Log("new icon: " + icon.name);
            return icon;
        }
        else
        {
            //Debug.Log("Didn't load icon!");
            return null;
        }
    }


    private Texture2D SetIconForEvent(int category)
    {
        EventCategory eventCategory = (EventCategory)(category);
        switch (eventCategory)
        {
            case EventCategory.Gameplay:
                return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/2D/internalUI/gameplay_logo.png");
            case EventCategory.Toast:
                return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/2D/internalUI/toast_logo.png");
            case EventCategory.UI:
                return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/2D/internalUI/suds_logo.png");
            case EventCategory.RequireCondition:
                return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/2D/internalUI/conditional_logo.png");
            default:
                return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/2D/internalUI/toast_logo.png");


        }

    }
    private Color EventCategoryColor(int category)
    {
        EventCategory eventCategory = (EventCategory)category;
        switch (eventCategory)
        {
            case EventCategory.Gameplay:
                return new Color(0.27f, 0.35f, 0.27f, 1);
            case EventCategory.Toast:
                return new Color(0.3f, 0.21f, 0.19f, 1);
            case EventCategory.UI:
                return new Color(0.24f, 0.28f, 0.30f, 1);
            case EventCategory.RequireCondition:
                return new Color(0.43f, 0.37f, 0.18f, 1);
            default:
                return new Color(0.1f, 0.1f, 0.1f, 1);
        }
    }
}
#endif



