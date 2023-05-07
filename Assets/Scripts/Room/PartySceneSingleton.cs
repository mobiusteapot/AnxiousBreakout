using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static ToastController;
using System;
#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif


/// <summary>
/// In situations like on-demand object creation where we create an object of the class only when it is accessed, lazy initialization works very well. It helps application load faster because it does not depend upon instance creation at the time of application startup.
/// Generally used in cases where a class initialization is expensive, so only do it on demand.
/// </summary>
public class PartySceneSingleton : MonoBehaviour
{
    

    
    [HideInInspector]
    static public PartySceneSingleton Instance = null;

    // Roomscale walls, etc
    [HideInInspector]
    public bool isGuardianSceneInitialized = false;
    [HideInInspector]
    public bool isSceneInitialized = false;
    private float _frameWait = 0;
    // Need to be specifically not public to set because the scene singleton should get them
    public VirtualRoom vrRoom { get; private set; }
    public OVRSceneAnchor[] _sceneAnchors { get; private set; }
    public Camera _mainCamera { get; private set; }

    public PlayerManager _playerManager { get; private set; }
    [Header("This is the game manager for the whole scene. \nIt manages every ordered event for the player.")]
    [Space(10)]

    [Header("Prefabs")]
    public GameObject walletPrefab;
    public GameObject dialogueCanvasPrefab;
    public GameObject SUDSManagerPrefab;
    public GameObject voiceCalibratorPrefab;

    [HideInInspector]
    public FakeVoiceCalibration voiceCalibrator;
    [HideInInspector]
    public SUDSManager sudsManager;

    [HideInInspector]
    public PartyEvent[] partyEvents;
    

    public Stack<PartyEvent> partyEventsStack = new Stack<PartyEvent>();
    [HideInInspector]
    public PartyEvent currentPartyEvent;
    private float partyEventDelay;


    // Events for on player enter and exit
    public delegate void PlayerEnter();
    public event PlayerEnter OnPlayerEnter;
    public delegate void PlayerExit();
    public event PlayerExit OnPlayerExit;

    // Conditional variables any object may need to check
    [HideInInspector]
    public bool hasWalletBeenPickedUp = false;
    [HideInInspector]
    public bool hasWalletBeenGiven = false;
    bool _isPlayerInRoom = false;
    public bool isPlayerInRoom 
    { 
        get { return _isPlayerInRoom; } 
        set { 
            if (value) 
            {
                Debug.Log("Invoking on player enter");
                OnPlayerEnter?.Invoke(); 
            } 
            else 
            {
                Debug.Log("Invoking on player exit");
                OnPlayerExit?.Invoke();
            } 
            _isPlayerInRoom = value; 
        } 
    }
    

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        // Reverse the list of party events
        partyEvents = partyEvents.Reverse().ToArray();
        List<PartyEvent> tmpPartyEvents = new List<PartyEvent>();
        // Create stack of copies of the party event
        foreach (PartyEvent partyEvent in partyEvents)
        {
            PartyEvent newPartyEvent = Instantiate(partyEvent);
            tmpPartyEvents.Add(newPartyEvent);
            partyEventsStack.Push(newPartyEvent);
        }
        // This is a war crime lmao
        partyEvents = tmpPartyEvents.ToArray().Reverse().ToArray();
        Debug.Log("Stack contains " + partyEventsStack.Count() + " events.");
    }

    private void Start()
    {
        // Only use start /awake to do preliminary setup, anything in game must wait for the scene manager to load
        // Can assume there is only 1 player and player is always in scene before singleton loads
        _mainCamera = Camera.main;
        _playerManager = PlayerManager.Instance;




    }
    // Gameplay logic is checked in Update loop. Not concerned with efficiency here since this class can only exist once.
    private void Update()
    {
        if (!isGuardianSceneInitialized)
        {
            Debug.Log("Guardian scene not initialized!");
            return;
        }
        if (!isSceneInitialized) {
            GetRoomFromScene();
            return;
        }


        // Get first event, initialize it, and update it until cleared
        ProcessPartyEvent();
    }

    void ProcessPartyEvent()
    {
        if (currentPartyEvent == null)
        {
            currentPartyEvent = partyEventsStack.Pop();
            currentPartyEvent.OnAwake?.Invoke();
            partyEventDelay = currentPartyEvent.eventStartDelay;
            Debug.Log("Got new party event off stack: " + currentPartyEvent.name);
        }

        // If toast event, don't ever execute begin/update/complete (toasts can't also have methods)
        if (currentPartyEvent.eventCategory == EventCategory.Toast)
        {
            if (currentPartyEvent.hasShownToast == false)
            {
                currentPartyEvent.DisplayToast();
                currentPartyEvent.hasShownToast = true;
            }
            // Do not execute anything else if toast isn't complete and waiting is neccesary
            // Otherwise, set to null so next event or toast starts
            if (currentPartyEvent.waitForToast && !currentPartyEvent.isComplete)
                return;
            else
            {
                currentPartyEvent = null;
                return;
            }
        }

        
        if (!currentPartyEvent.isStarted)
        {
            if (partyEventDelay > 0)
            {
                partyEventDelay -= Time.deltaTime;
            }
            else
            {
                currentPartyEvent.isStarted = true;
                currentPartyEvent.OnBegin?.Invoke();
            }
            return;
        }

        if (currentPartyEvent.isComplete)
        {
            currentPartyEvent.OnEnd?.Invoke();
            currentPartyEvent = null;
            return;
        }

        currentPartyEvent.OnUpdate?.Invoke();

    }
#if UNITY_EDITOR
    // This should only be used for debug. This should not compile in release builds
    public void SetCurrentEvent(PartyEvent overrideEvent)
    {
        // Reset event if already cleared
        overrideEvent.isStarted = false;
        overrideEvent.isComplete = false;
        if (partyEventsStack.Peek() != overrideEvent)
        {
            partyEventsStack.Push(overrideEvent);
        }
        currentPartyEvent = null;
        Debug.Log("Manually setting the current event to: " + overrideEvent.name);
    }
#endif

    void GetRoomFromScene()
    {
        if (_frameWait < 1)
        {
            _frameWait++;
            return;
        }

        // OVRSceneAnchors have already been instantiated from OVRSceneManager
        // to avoid script execution conflicts, we do this once in the Update loop instead of directly when the SceneModelLoaded event is fired
        _sceneAnchors = FindObjectsOfType<OVRSceneAnchor>();
        ArrayList vrRoomTMP = new ArrayList();
        vrRoomTMP.AddRange(FindObjectsOfType<VirtualRoom>());
        if(vrRoomTMP.Count > 2)
        {
            Debug.Log("Duplicate rooms found! Only 1 should be in scene");
        }
        else if (vrRoomTMP.Count == 0)
        {
            Debug.Log("No rooms found! Please add a room to the scene");
            return;
        }
        vrRoom = (VirtualRoom)vrRoomTMP[0];
        vrRoom.Initialize(_sceneAnchors);
        CleanupPartyRoomSpatialObjects(vrRoom.gameObject);
        vrRoom.sceneNavMesh.BuildNavMesh();

        // Todo: can find the correct nav mesh area by finding which one is larger?

        if (!vrRoom.IsPlayerInRoom())
        {
            Debug.Log("Player started inside room! Reset experience after setting up boundaries, and before putting guest in headset");
        }
        
        PartyCharacterManager.Instance.Initialize();
        isSceneInitialized = true;
        // Also initialize SUDS manager
        sudsManager = Instantiate(SUDSManagerPrefab).GetComponent<SUDSManager>();


        // If player has debug walls enabled then show them

        _playerManager.DrawGuardianWalls();
    }

    // Utility functions
    private void CleanupPartyRoomSpatialObjects(GameObject newParent)
    {
        // Get the .gameobject of all party scene objects

        PartySceneObject[] allPartyRoomObjects = FindObjectsOfType<PartySceneObject>();
        foreach(PartySceneObject partyRoomObject in allPartyRoomObjects)
        {
            partyRoomObject.gameObject.transform.parent = newParent.transform;
        }
    }

    public void SpawnWallet()
    {
        Instantiate(walletPrefab);
    }
    
}





#if UNITY_EDITOR

[CustomEditor(typeof(PartySceneSingleton))]
public class PartySceneSingletonEditor : Editor
{
    SerializedProperty partyEvents;
    ReorderableList partyEventsList;
    

    private void OnEnable()
    {
        partyEvents = serializedObject.FindProperty("partyEvents");
        // Create list with dragging and adding/removing disabled on runtime
        partyEventsList = new ReorderableList(serializedObject, partyEvents, !Application.isPlaying, true, !Application.isPlaying, !Application.isPlaying);
        partyEventsList.drawElementBackgroundCallback = DrawPartyEventBG;
        partyEventsList.drawElementCallback = DrawPartyEvent;
        // Lambda expression for drawing header
        partyEventsList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Party Events");
        
        

    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        serializedObject.Update();

        partyEventsList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawPartyEvent(Rect rect, int index, bool isActive, bool isFocused)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        partyEventsList.elementHeightCallback = _ => (EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.singleLineHeight * 0.1f);
        // Recolor background
        
        
        // Todo: If runtime, get the stack version

        // This is all just for readability/organization
        SerializedProperty partyEvent = partyEventsList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedObject partyEventObject = new SerializedObject (partyEvent.objectReferenceValue);
        
        float xOffset = 0;
        void ReoderableListNewLine()
        {
            xOffset = 0;
            rect.y += EditorGUIUtility.singleLineHeight;
        }


        // Draw the party event itself
        EditorGUI.PropertyField(
            NewRectWithOffset(rect,ref xOffset, 180),
            partyEvent,
            GUIContent.none
        );
        xOffset += 20;

        EditorGUI.PropertyField(
                NewRectWithOffset(rect, ref xOffset, 100),
                partyEventObject.FindProperty("eventCategory"),
                GUIContent.none
            );

        ReoderableListNewLine();

        if (partyEventObject.FindProperty("eventCategory").enumValueIndex == 1)
        {
            // Todo: dynamic height later if needed
            // Disable if not playing (Can't change any part of an instance on run!)
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            GUIStyle style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
            SerializedProperty toastString = partyEventObject.FindProperty("toastString");
            toastString.stringValue = EditorGUI.TextArea(
                NewRectWithOffset(rect, ref xOffset, rect.width - 20, EditorGUIUtility.singleLineHeight * 2),
                toastString.stringValue,
                style
                );
            

            ReoderableListNewLine();
            ReoderableListNewLine();


            EditorGUI.LabelField(NewRectWithOffset(rect, ref xOffset, 90), "Toast Duration");
            EditorGUI.PropertyField(
                NewRectWithOffset(rect, ref xOffset, 40),
                partyEventObject.FindProperty("toastDuration"),
                GUIContent.none
            );
            EditorGUI.LabelField(NewRectWithOffset(rect, ref xOffset, 120), "Wait Until Finished?");
            EditorGUI.PropertyField(
                NewRectWithOffset(rect, ref xOffset, 20),
                partyEventObject.FindProperty("waitForToast"),
                GUIContent.none
            );
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            
            // Event status
            // Button to make the current event this event
            if (GUI.Button(
                NewRectWithOffset(rect, ref xOffset, 150),
                "Make Current Event"
            ))
            {
                PartySceneSingleton.Instance.SetCurrentEvent(partyEventObject.targetObject as PartyEvent);
            }
            xOffset += 20;
            if (GUI.Button(
                NewRectWithOffset(rect, ref xOffset, 130),
                "Complete Event"
            ))
            {
                PartySceneSingleton.Instance.currentPartyEvent.isComplete = true;
            }

            List<SerializedProperty> Events = new List<SerializedProperty>
            {
                partyEventObject.FindProperty("OnAwake"),
                partyEventObject.FindProperty("OnBegin"),
                partyEventObject.FindProperty("OnEnd"),
                partyEventObject.FindProperty("OnUpdate")
            };
            bool hasNoEvents = true;

            // Both toast and non-toast version should be 3 lines (rn)
            ReoderableListNewLine();
            foreach (SerializedProperty Event in Events)
            {
                // Start new line for update
                if(Event.name == "OnUpdate")
                    ReoderableListNewLine();

                int eventArraySize = Event.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize;
                EditorGUI.LabelField(
                    NewRectWithOffset(rect, ref xOffset, 80),
                    Event.name + ": "
                    );
                EditorGUI.LabelField(
                    NewRectWithOffset(rect, ref xOffset, 20),
                    eventArraySize.ToString()
                    );

                if(eventArraySize > 0)
                {
                    hasNoEvents = false;
                }
            }
            EditorGUI.LabelField(NewRectWithOffset(rect, ref xOffset, 80), "Started: ");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(
                NewRectWithOffset(rect, ref xOffset, 20),
                partyEventObject.FindProperty("isStarted"),
                GUIContent.none
            );
            EditorGUI.EndDisabledGroup();

            EditorGUI.LabelField(NewRectWithOffset(rect, ref xOffset, 80), "Completed: ");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(
                NewRectWithOffset(rect, ref xOffset, 20),
                partyEventObject.FindProperty("isComplete"),
                GUIContent.none
            );
            EditorGUI.EndDisabledGroup();
            
            if (hasNoEvents)
                EditorGUI.HelpBox(
                    NewRectWithOffset(rect, ref xOffset, 200),
                    "Event has no method calls!",
                    MessageType.Warning
                    );

            



        }
        Color lineColor = new Color(0.10196f, 0.10196f, 0.10196f, 1);
        EditorGUI.DrawRect(new Rect(rect.x - 20, rect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.singleLineHeight * 0.1f, rect.width + 27, 0.5f), lineColor);
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            // Mark edited in perforce if changed!
            AssetDatabase.MakeEditable(AssetDatabase.GetAssetPath(partyEventObject.targetObject));
        }

        partyEventObject.ApplyModifiedProperties();

    }
    void DrawPartyEventBG(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty partyEvent = partyEventsList.serializedProperty.GetArrayElementAtIndex(index);
        try
            {
            SerializedObject partyEventObject = new SerializedObject(partyEvent.objectReferenceValue);
            EditorGUI.DrawRect(rect, EventCategoryColor(partyEventObject.FindProperty("eventCategory").enumValueIndex, isFocused));
        }
        catch (ArgumentException e)
        {
            Debug.LogError("Missing an event. Was this deleted outside of Unity, or are you missing it from the source control?" + e);
            return;
        }
        
    }

    private Rect NewRectWithOffset(Rect elementRect, ref float offset, float width, float height = 0)
    {
        if (height == 0)
            height = EditorGUIUtility.singleLineHeight;
        elementRect = new Rect(elementRect.x + offset, elementRect.y, width, height);
        offset += elementRect.width;
        return elementRect;
    }

    private Color EventCategoryColor(int category, bool isSelected = false)
    {
        EventCategory eventCategory = (EventCategory)category;
        Color bgColor = new Color(0.1f, 0.1f, 0.1f, 1);
        switch (eventCategory)
        {
            case EventCategory.Gameplay:
                bgColor = new Color(0.27f, 0.35f, 0.27f, 1);
                break;
            case EventCategory.Toast:
                bgColor = new Color(0.3f, 0.21f, 0.19f, 1);
                break;
            case EventCategory.UI:
                bgColor = new Color(0.24f, 0.28f, 0.30f, 1);
                break;
            case EventCategory.RequireCondition:
                bgColor = new Color(0.43f, 0.37f, 0.18f, 1);
                break;
            default:
                bgColor = new Color(0.1f, 0.1f, 0.1f, 1);
                break;
        }
        float darkOffset = 0.1f;
        if (isSelected)
            bgColor = new Color(bgColor.r - darkOffset, bgColor.g - darkOffset, bgColor.b - darkOffset);

        return bgColor;
    }

}
#endif