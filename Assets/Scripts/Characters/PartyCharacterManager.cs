using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Meant to control spawning the characters, as well as updating their behavior when the whole group needs to recieve a message
public class PartyCharacterManager : MonoBehaviour
{
    static public PartyCharacterManager Instance = null;
    public GameObject[] customGhostPrefabs;
    public GameObject regularGhostPrefab;
    [SerializeField,Tooltip("Each attribute is applied once then looped again")]
    private List<PartyGhostVariant> ghostVarients = new List<PartyGhostVariant>();
    private Stack<PartyGhostVariant> ghostVarientsStack = new Stack<PartyGhostVariant>();
    private List<GameObject> regularGhosts = new List<GameObject>();
    private List<GameObject> keyGhosts = new List<GameObject>();
    private PartyGhostController currentTalkableGhost;

    [Space]
    [Header("Ghosts are spawned per total number of wall corners.")]
    [Range(1,10)]
    public int spawnQuantity = 1;


    public float maxSampleDistanceFromWall = 10f;
    public float minDistanceApart = 0.2f;
    public int maxSampleAttempts = 3;

    // Should never need to be bigger than twice the max sample distance (twice bc positive or negative direction)
    private float maxNavmeshDistance
    {
        get
        {
            return maxSampleDistanceFromWall * 2;
        }
    }

    


    public float playerStartConversationProximity = 2f;

    private int currentConversationIndex = 0;
    private AudioSource currentAudioSource;
    private GameObject dialogueCanvas;

    private GameObject roomCenter;
    private bool isInitialized = false;
    

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

    public void EnableConversationByIndex(int index)
    {
        if(index < keyGhosts.Count)
        {
            currentTalkableGhost = keyGhosts[index].GetComponent<PartyGhostController>();
            currentConversationIndex = index;
            currentAudioSource = currentTalkableGhost.audioSource;
            currentTalkableGhost.makeTalkable = true;
            UpdateDialogueCanvasTransform(currentTalkableGhost.dialogueCanvasTransform);
        }
        else
        {
            Debug.LogError("Cannot find custom character at index " + index);
        }
        
    }
    
    public void OverrideGhostStateByIndex(int index, GhostState newState)
    {
        if (index < keyGhosts.Count)
        {
            keyGhosts[index].GetComponent<PartyGhostController>().ChangeState(newState);
        }
        else
        {
            Debug.LogError("Cannot find custom character at index " + index);
        }
    }
    public void StartNextConversation()
    {
        StartConversationByIndex(currentConversationIndex);
        currentConversationIndex++;
    }

    public void StartConversationByIndex(int index)
    {
        // Switch case per index
        switch (index)
        {
            case 0:
                SetDialogueCanvasVisibility(true);
                ConversationManager.Instance.StartNPC1Conversation(currentAudioSource);
                break;
            case 1:
                SetDialogueCanvasVisibility(true);
                ConversationManager.Instance.StartNPC2Conversation(currentAudioSource);
                break;
            case 2:
                SetDialogueCanvasVisibility(true);
                ConversationManager.Instance.StartNPC3Conversation(currentAudioSource);
                break;
            default:
                Debug.Log("Cannot match conversation index with id: " + index);
                break;
        }
    }
    public void SetDialogueCanvasVisibility(bool visible)
    {
        dialogueCanvas.SetActive(visible);
    }
    public void UpdateDialogueCanvasTransform(Transform newTransform)
    {
        // Set parent to new transform
        dialogueCanvas.transform.SetParent(newTransform, false);
    }
    

    public void Initialize()
    {
        if (!isInitialized)
        {
            SpawnCharactersByWalls();
            // Instantiate dialogue canvas at first character's dialogue transform
            Transform spawnTransform = keyGhosts[0].GetComponent<PartyGhostController>().dialogueCanvasTransform;
            dialogueCanvas = Instantiate(PartySceneSingleton.Instance.dialogueCanvasPrefab, spawnTransform);
            SetDialogueCanvasVisibility(false);
            isInitialized = true;
        }
        else
        {
            // Destroy all regular ghosts
            foreach (GameObject ghost in regularGhosts)
            {
                Destroy(ghost);
            }
            foreach (GameObject ghost in keyGhosts)
            {
                Destroy(ghost);
            }
            Destroy(roomCenter);
            regularGhosts = new List<GameObject>();
            keyGhosts = new List<GameObject>();
            SpawnCharactersByWalls();
        }
    }
    

    #region Spawning and Setup
    // Spawns each custom character once, and then spawns generic characters based on density
    void SpawnCharactersByWalls()
    {
        int customCharacterIndex = 0;
        int customCharacterCount = customGhostPrefabs.Length;
        Vector3[] clockwiseRoomCornerPoints = PartySceneSingleton.Instance.vrRoom._cornerPoints.ToArray();
        List<Vector3> previousSpawnPositions = new List<Vector3>();
        
        

        roomCenter = new GameObject("RoomCenter");
        roomCenter.transform.position = FindCenterOfRoom();

        // Spawn routine per spawn quantity (special ghost count isn't reset)
        for(int i =0; i < spawnQuantity; i++)
        {
            foreach (Vector3 cornerPoint in clockwiseRoomCornerPoints)
            {
                bool hasSpawnedCustomGhost = false;
                // Temp random rotation
                //Quaternion ghostRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                // Currently rotate to center of room

                // Just for debug
                //new GameObject("Corner Point").transform.position = cornerPoint;

                // For i less than sample attempt
                for (int j = 0; j < maxSampleAttempts; j++)
                {


                    // Random point on X and Z within min and max sample distance
                    Vector3 samplePoint = new Vector3(cornerPoint.x + RandomWithinRange(), 0, cornerPoint.z + RandomWithinRange());
                    // Find closest navmesh point to spawn location
                    if (NavMesh.SamplePosition(samplePoint, out NavMeshHit hit, maxNavmeshDistance, 1))
                    {
                        bool shouldSpawn = true;
                        if (previousSpawnPositions != null && previousSpawnPositions.Count != 0)
                        {
                            // Check if point is too close to where a ghost already is
                            // If too close to any point then don't spawn
                            foreach (Vector3 previousSpawnPos in previousSpawnPositions)
                            {
                                if (Vector3.Distance(hit.position, previousSpawnPos) < minDistanceApart)
                                {
                                    shouldSpawn = false;
                                }
                            }
                        }
                        if (shouldSpawn)
                        {
                            GameObject ghost;
                            // Spawn custom character if all haven't been spawned yet
                            if (customCharacterIndex < customCharacterCount && hasSpawnedCustomGhost == false)
                            {
                                ghost = Instantiate(customGhostPrefabs[customCharacterIndex], hit.position, Quaternion.identity);
                                keyGhosts.Add(ghost);
                                ghost.name = ghost.name + " c" + customCharacterIndex;
                                customCharacterIndex++;
                                hasSpawnedCustomGhost = true;
                            }
                            else
                            {
                                ghost = Instantiate(regularGhostPrefab, hit.position, Quaternion.identity);
                                regularGhosts.Add(ghost);
                            }
                            previousSpawnPositions.Add(hit.position);

                            if(hasSpawnedCustomGhost && customCharacterIndex == 1)
                            {
                                Debug.LogError("Reached " + customGhostPrefabs[customCharacterIndex].name);
                                break;
                            }

                            GameObject temporaryRot = new GameObject();
                            temporaryRot.transform.position = ghost.transform.position;
                            temporaryRot.transform.LookAt(roomCenter.transform);
                            ghost.transform.rotation = Quaternion.Euler(ghost.transform.rotation.eulerAngles.x, temporaryRot.transform.rotation.eulerAngles.y, ghost.transform.rotation.eulerAngles.z);
                            Destroy(temporaryRot);

                            break;
                        }
                    }
                }
                Debug.LogWarning("Tried to spawn ghost after " + maxSampleAttempts + " attempts, but always further than " + maxNavmeshDistance + " from the navmesh!");
            }
        }
        
        if (customCharacterIndex != customCharacterCount)
            Debug.LogError("Not all key characters have been initialized");
    }

    Vector3 FindCenterOfRoom()
    {
        float totalX = 0f, totalZ = 0f;
        int count = 0;
        foreach(OVRSceneAnchor sceneAnchor in PartySceneSingleton.Instance._sceneAnchors)
        {
            totalX += sceneAnchor.transform.position.x;
            totalZ += sceneAnchor.transform.position.z;
            count++;
        }
        return new Vector3(totalX / count, 0, totalZ / count);
    }
    // Random positive or negative value within range
    private float RandomWithinRange()
    {
        return Random.Range(-maxSampleDistanceFromWall, maxSampleDistanceFromWall);
    }
    // Holds a stack of all ghost traits 
    public PartyGhostVariant GetPartyGhostVariant()
    {
        // If stack empty refill
        if (ghostVarientsStack.Count == 0)
        {
            // Randomize order of ghost traits
            Shuffle.ShuffleList(ghostVarients);
            ghostVarientsStack = new Stack<PartyGhostVariant>(ghostVarients);
        }
        PartyGhostVariant outputVariant = null;
        PartyGhostVariant toFind = ghostVarientsStack.Pop();
        // Find variant in list and make that the output variant
        foreach (PartyGhostVariant ghostVariant in ghostVarients)
        {
            if(ghostVariant == toFind)
            {
                outputVariant = ghostVariant;
            }
        }
        return outputVariant;
    }


    #endregion
}

#if UNITY_EDITOR

[CustomEditor(typeof(PartyCharacterManager))]
public class PartyCharacterManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        if (Application.isPlaying)
        {
            if (GUILayout.Button("Re-Initialize"))
            {
                PartyCharacterManager.Instance.Initialize();
            }
            if (GUILayout.Button("Get Ghost Variant"))
            {
                PartyGhostVariant variant = PartyCharacterManager.Instance.GetPartyGhostVariant();
                Debug.Log("Popped variant: " + variant.name);
            }
        }
    }
}
#endif