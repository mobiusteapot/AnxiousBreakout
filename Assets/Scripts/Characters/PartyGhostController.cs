using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Experimental.XR.Interaction;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PartyGhostController : MonoBehaviour
{
    // For the unique prefabs so they don't get overwritten on spawn
    [HideInInspector]
    public bool useCustomVarient;
    // For the prefab with a second ghost if a reference to it is needed
    [HideInInspector]
    public bool useTalkingToOtherGhost;
    [SerializeField, HideInInspector]
    public PartyGhostController otherTalkingGhost;
    [SerializeField, HideInInspector]
    public PartyGhostVariant ghostVariantData;

    // For other scripts to use but not to be edited in the inspector!
    [HideInInspector]
    public bool makeTalkable = false;
    //[HideInInspector]
    //public GhostAnimationStateReciever ghostAnimationReciever;


    GhostState currentState;
    public GhostState idleState = new GhostIdle();
    public GhostState talkState = new GhostTalking();
    public GhostState waitForWalletState = new GhostWaitingForWallet();
    public GhostState waitToTalkState = new GhostWaitingForPlayer();
    public GhostState talkToPlayerState = new GhostTalkingToPlayer();
    public GhostState moveState = new GhostMoving();

    [HideInInspector]
    const float ghostDefaultTurnSpeed = 0.5f;
    [HideInInspector]
    public float ghostTurnSpeedMultiplier = 1f;



    float initialRotation = 0;

    [SerializeField]
    Transform ghostHead;

    [SerializeField]
    Transform ghostBody;

    [SerializeReference, Tooltip("Angle in Degrees")]
    float angleDeltaThresholdToRotateBody = 35f;

    [HideInInspector]
    GameObject rotationHeadTarget, rotationBodyTarget;

    [SerializeField]
    public Animator bodyAnimator;

    // Head animator for male or female needs to be gotten in Start after varient is assigned
    [HideInInspector]
    public Animator headAnimator;

    [SerializeField]
    public GameObject talkIcon;

    [SerializeField]
    public Transform dialogueCanvasTransform;

    bool isGazeStay = false;
    bool startedGaze = false;
    bool endedGaze = false;
    bool rotateBody = false;

    bool isInitialized;

    public AudioSource audioSource;

    public GhostBodyComponents ghostBodyComponents;
    [Serializable]

    public class GhostBodyComponents
    {
        [SerializeField]
        public SkinnedMeshRenderer bodyRenderer;
        [SerializeField]
        public SkinnedMeshRenderer handRenderer;
        [SerializeField]
        public SkinnedMeshRenderer femaleHeadRenderer;
        [SerializeField]
        public SkinnedMeshRenderer maleHeadRenderer;
        [SerializeField]
        public MeshRenderer femaleHairRenderer;
        [SerializeField]
        public MeshRenderer maleHairRenderer;
        [SerializeField]
        public MeshRenderer leftFemaleEyeRenderer;
        [SerializeField]
        public MeshRenderer rightFemaleEyeRenderer;
        [SerializeField]
        public MeshRenderer leftMaleEyeRenderer;
        [SerializeField]
        public MeshRenderer rightMaleEyeRenderer;
    }



    void Update()
    {
        if (!isInitialized) return;
        currentState.BehaviorTick();
        float t = ghostDefaultTurnSpeed * ghostTurnSpeedMultiplier * Time.deltaTime;

        ghostBody.transform.rotation = Quaternion.Slerp(ghostBody.transform.rotation, rotationBodyTarget.transform.rotation, t);
        ghostHead.transform.rotation = Quaternion.Slerp(ghostHead.transform.rotation, rotationHeadTarget.transform.rotation, t);

        if (!rotateBody && Vector3.Angle(rotationHeadTarget.transform.forward, rotationBodyTarget.transform.forward) >= angleDeltaThresholdToRotateBody)
        {
            rotateBody = true;
            rotationBodyTarget.transform.rotation = rotationHeadTarget.transform.rotation;
            RotateTargetToDefault();
        }
        if (rotateBody && Vector3.Angle(rotationHeadTarget.transform.forward, rotationBodyTarget.transform.forward) <= angleDeltaThresholdToRotateBody)
        {
            rotateBody = false;
        }

        if (startedGaze)
        {
            currentState.OnGazeEnter();
        }
        else if (endedGaze)
        {
            currentState.OnGazeExit();
        }
        else if (isGazeStay)
        {
            currentState.OnGazeStay();
        }

        // Reset flags for one-off behavior
        // Todo: move this back to Enter and Exit gaze methods
        startedGaze = false;
        endedGaze = false;

        if (makeTalkable)
        {
            ChangeState(waitToTalkState);
            makeTalkable = false;
        }
    }
    private void Awake()
    {
        rotationHeadTarget = new GameObject();
        rotationHeadTarget.transform.SetParent(ghostHead.transform.parent, false);
        rotationHeadTarget.name = "GhostHeadRotationTarget";

        rotationBodyTarget = new GameObject();
        rotationBodyTarget.transform.SetParent(ghostBody.transform.parent, false);
        rotationBodyTarget.name = "GhostBodyRotationTarget";



        initialRotation = rotationHeadTarget.transform.localEulerAngles.y;

    }

    private IEnumerator Start()
    {

        // Small delay to make sure things are loaded (this is a hack lol)
        yield return new WaitForSeconds(0.3f);

        if (!useCustomVarient)
            ghostVariantData = PartyCharacterManager.Instance.GetPartyGhostVariant();
        else if(ghostVariantData == null){
            Debug.LogError("Ghost variant is custom but not found!");
            ghostVariantData = PartyCharacterManager.Instance.GetPartyGhostVariant();
        }
        InitializeHead(ghostVariantData.isMalePresenting);
        InitializeInstanceMaterials(ghostVariantData.isMalePresenting);


        isInitialized = true;

        if (useTalkingToOtherGhost)
        {
            // Assume both are talking
            ChangeState(talkState);
            otherTalkingGhost.ChangeState(talkState);
        }
        else
        {
            if (!(currentState is GhostTalking))
            {
                ChangeState(idleState);
            }
        }
    }


    public void ChangeState(GhostState state)
    {
        if(state == currentState)
        {
            Debug.LogWarning("Trying to change state but already in state: " + state);
            return;
        }
        if (currentState != null)
        {
            currentState.OnChangeState();
        }
        currentState = state;
        currentState.OnInitialize(this);
    }

    public void EnterGazeFromHitbox()
    {
        Debug.Log("EnterGazeFromHitbox");
        // Check in while loop currently to keep lock checks simple
        startedGaze = true;
        isGazeStay = true;
    }
    public void ExitGazeFromHitbox()
    {
        Debug.Log("ExitGazeFromHitbox");
        endedGaze = true;
        isGazeStay = false;
    }

    public void RotateTarget(float rotation, bool isRelative = false)
    {
        float targetRotation = isRelative ? rotationHeadTarget.transform.localEulerAngles.y + rotation : rotation;
        rotationHeadTarget.transform.localRotation = Quaternion.Euler(rotationHeadTarget.transform.localEulerAngles.x,
                    targetRotation,
                    rotationHeadTarget.transform.localEulerAngles.z);
    }
    public void RotateTargetToDefault()
    {
        RotateTarget(initialRotation);
    }
    public void RotateTargetLookAt(Transform target)
    {
        GameObject tmpRot = new GameObject();
        tmpRot.transform.SetParent(ghostHead.transform.parent, false);
        tmpRot.transform.LookAt(target);
        // Rotate to the Y of the tmpRot
        RotateTarget(tmpRot.transform.localEulerAngles.y);
        Destroy(tmpRot);
    }

    public void InitializeInstanceMaterials(bool isMalePresenting, bool targetSharedMaterials = false)
    {
        try
        {
            if (targetSharedMaterials)
            {
                ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.bodyRenderer.sharedMaterials[1], PartyGhostVariant.ghostMaterials.shirt);
                ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.bodyRenderer.sharedMaterials[2], PartyGhostVariant.ghostMaterials.undershirt);
                ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.bodyRenderer.sharedMaterials[0], PartyGhostVariant.ghostMaterials.skin);
                ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.handRenderer.sharedMaterial, PartyGhostVariant.ghostMaterials.skin);

                if (isMalePresenting)
                {
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.maleHairRenderer.sharedMaterial, PartyGhostVariant.ghostMaterials.hair);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.maleHeadRenderer.sharedMaterials[1], PartyGhostVariant.ghostMaterials.hair);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.maleHairRenderer.sharedMaterials[0], PartyGhostVariant.ghostMaterials.skin);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.leftMaleEyeRenderer.sharedMaterial, PartyGhostVariant.ghostMaterials.eyes);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.rightMaleEyeRenderer.sharedMaterial, PartyGhostVariant.ghostMaterials.eyes);

                }
                else
                {
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.femaleHairRenderer.sharedMaterial, PartyGhostVariant.ghostMaterials.hair);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.femaleHeadRenderer.sharedMaterials[1], PartyGhostVariant.ghostMaterials.hair);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.leftFemaleEyeRenderer.sharedMaterial, PartyGhostVariant.ghostMaterials.eyes);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.rightFemaleEyeRenderer.sharedMaterial, PartyGhostVariant.ghostMaterials.eyes);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.femaleHeadRenderer.sharedMaterials[0], PartyGhostVariant.ghostMaterials.skin);

                }

            }
            else
            {
                ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.bodyRenderer.materials[1], PartyGhostVariant.ghostMaterials.shirt);
                ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.bodyRenderer.materials[2], PartyGhostVariant.ghostMaterials.undershirt);
                ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.handRenderer.material, PartyGhostVariant.ghostMaterials.skin);
                ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.bodyRenderer.materials[0], PartyGhostVariant.ghostMaterials.skin);
                if (isMalePresenting)
                {
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.maleHairRenderer.material, PartyGhostVariant.ghostMaterials.hair);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.maleHeadRenderer.materials[1], PartyGhostVariant.ghostMaterials.hair);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.leftMaleEyeRenderer.material, PartyGhostVariant.ghostMaterials.eyes);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.rightMaleEyeRenderer.material, PartyGhostVariant.ghostMaterials.eyes);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.maleHeadRenderer.materials[0], PartyGhostVariant.ghostMaterials.skin);
                }
                else
                {
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.femaleHairRenderer.material, PartyGhostVariant.ghostMaterials.hair);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.femaleHeadRenderer.materials[1], PartyGhostVariant.ghostMaterials.hair);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.leftFemaleEyeRenderer.material, PartyGhostVariant.ghostMaterials.eyes);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.rightFemaleEyeRenderer.material, PartyGhostVariant.ghostMaterials.eyes);
                    ghostVariantData.UpdateColorsAndTextures(ghostBodyComponents.femaleHeadRenderer.materials[0], PartyGhostVariant.ghostMaterials.skin);
                }
            }
        }
        catch (NullReferenceException e)
        {
            Debug.LogWarning("Cannot initialize ghost materials: " + e);
            return;
        }
        catch (UnassignedReferenceException e)
        {
            Debug.LogWarning("Ghost materials missing reference to game object: " + e);
            return;
        }
    }
    public void InitializeHead(bool isMalePresenting)
    {
        if (isMalePresenting)
        {
            headAnimator = ghostBodyComponents.maleHeadRenderer.transform.parent.GetComponent<Animator>();
            ghostBodyComponents.maleHeadRenderer.transform.parent.gameObject.SetActive(true);
            ghostBodyComponents.femaleHeadRenderer.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            headAnimator = ghostBodyComponents.femaleHeadRenderer.transform.parent.GetComponent<Animator>();
            ghostBodyComponents.maleHeadRenderer.transform.parent.gameObject.SetActive(false);
            ghostBodyComponents.femaleHeadRenderer.transform.parent.gameObject.SetActive(true);
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw the name of the current state above the  ghost's head
        if (Application.isPlaying && isInitialized)
        {
            Vector3 position = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
            Handles.Label(position, ghostVariantData.name + ": " + currentState.GetType());
        }

    }
#endif

}

#if UNITY_EDITOR
[CustomEditor(typeof(PartyGhostController))]
public class PartyGhostControllerEditor : Editor
{
    SerializedProperty useCustomVarient;
    SerializedProperty useTalkingToOtherGhost;
    SerializedProperty ghostVariantData;
    SerializedProperty otherTalkingGhost;



    private void OnEnable()
    {
        useCustomVarient = serializedObject.FindProperty("useCustomVarient");
        useTalkingToOtherGhost = serializedObject.FindProperty("useTalkingToOtherGhost");
        ghostVariantData = serializedObject.FindProperty("ghostVariantData");
        otherTalkingGhost = serializedObject.FindProperty("otherTalkingGhost");

    }

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        EditorGUILayout.PropertyField(useCustomVarient);
        if (useCustomVarient.boolValue)
        {

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(ghostVariantData);
                }
            }
        }
        EditorGUILayout.PropertyField(useTalkingToOtherGhost);
        if (useTalkingToOtherGhost.boolValue)
        {
            EditorGUILayout.PropertyField(otherTalkingGhost);
        }
        if (serializedObject.ApplyModifiedProperties())
        {

            if (useCustomVarient.boolValue)
            {
                if (ghostVariantData.propertyType == SerializedPropertyType.ObjectReference && ghostVariantData.objectReferenceValue != null)
                {
                    ((PartyGhostController)target).InitializeInstanceMaterials(((PartyGhostController)target).ghostVariantData.isMalePresenting, true);
                }
            }

        }
    }
}
#endif
