using UnityEngine;
using System;
using static PartyGhostController;


#if UNITY_EDITOR
using UnityEditor;
#endif

// Hold all data relative to how ghosts act
[Serializable]
public class GhostTraits
{
    // All should be serialized and private set
    [SerializeField, Tooltip("Looks away from player on stare")]
    bool _isShy;
    public bool isShy { get { return _isShy; } private set { _isShy = value; } }

    [SerializeField, Tooltip("Looks at player when the player is not staring")]
    bool _isNosy;
    public bool isNosy { get { return _isNosy; } private set { _isNosy = value; } }

    [SerializeField, Tooltip("Instead of idling, picks a random dance!")]
    bool _danceIdle;
    public bool danceIdle { get { return _danceIdle; } private set { _danceIdle = value; } }
    [SerializeField, Tooltip("Gets angry when the player interupts them.")]
    bool _isInterruptable;
    public bool isInteruptable { get { return _isInterruptable; } private set { _isInterruptable = value; } }

    bool _hasLostWallet;
    public bool hasLostWallet { get { return _hasLostWallet; } private set { _hasLostWallet = value; } }

}

// Create from menu
[CreateAssetMenu(fileName = "PartyGhostVariant", menuName = "PartyGhostVariant", order = 0)]
public class PartyGhostVariant : ScriptableObject
{
    public GameObject prefab;
    public bool isMalePresenting;
    public Color skinColor;
    public Texture2D shirtTexture;
    public Texture2D shirtNormal;
    public Color shirtColor1;
    public Color shirtColor2;

    public Texture2D undershirtTexture;
    public Texture2D undershirtNormal;
    public Color underShirtColor1;
    public Color underShirtColor2;
    public bool useCustomLogo;
    public Texture2D customLogoTexture;
    public Color eyeColor;
    public Color hairColor;
    public Color edgeHairColor;
    public GhostTraits traits;

    [SerializeField]
    private Texture2D defaultShirtTexture;
    [SerializeField]
    private Texture2D defaultShirtNormal;
    [SerializeField]
    private Texture2D defaultUndershirtTexture;
    [SerializeField]
    private Texture2D defaultUndershirtNormal;
    [SerializeField]
    private Texture2D defaultCustomLogo;

    public enum ghostMaterials
    {
        shirt,
        undershirt,
        hair,
        eyes,
        skin
    }
#if UNITY_EDITOR
    private void Reset()
    {
        isMalePresenting = false;
        skinColor = Color.blue;

        shirtTexture = defaultShirtTexture;
        shirtNormal = defaultShirtNormal;
        shirtColor1 = Color.red;
        shirtColor2 = Color.white;

        undershirtTexture = defaultUndershirtTexture;
        undershirtNormal = defaultUndershirtNormal;
        underShirtColor1 = Color.white;
        underShirtColor2 = Color.white;
        useCustomLogo = false;

        eyeColor = Color.white;
        hairColor = ColorUtility.TryParseHtmlString("#D7BB40", out Color c) ? c : Color.white;
        edgeHairColor = Color.white;
    }
#endif

    public void UpdateColorsAndTextures(Material material, ghostMaterials targetMaterial)
    {
        // Switch case for each target material
        switch (targetMaterial)
        {
            case ghostMaterials.shirt:
                material.SetColor("_Base_Color_1", shirtColor1);
                material.SetColor("_Base_Color_2", shirtColor2);
                material.SetTexture("_MaskMap", shirtTexture);
                material.SetTexture("_NormalMap", shirtNormal);
                break;
            case ghostMaterials.undershirt:
                material.SetColor("_Base_Color_1", underShirtColor1);
                material.SetColor("_Base_Color_2", underShirtColor2);
                if (useCustomLogo)
                {
                    material.EnableKeyword("_USECUSTOMLOGO");
                    material.SetTexture("_Logo_Mask", customLogoTexture);
                }
                else
                {
                    material.DisableKeyword("_USECUSTOMLOGO");
                }
                material.SetTexture("_MaskMap", undershirtTexture);
                material.SetTexture("_NormalMap", undershirtNormal);
                break;
            case ghostMaterials.hair:
                material.SetColor("_Base_Color", hairColor);
                material.SetColor("_Edge_Color", edgeHairColor);
                break;
            case ghostMaterials.eyes:
                material.SetColor("_Eye_Color", eyeColor);
                break;
            case ghostMaterials.skin:
                material.SetColor("_Base_Color", skinColor);
                break;
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(PartyGhostVariant))]
[CanEditMultipleObjects]
public class PartyGhostVariantEditor : Editor
{
    private PreviewRenderUtility _previewRenderUtility;
    private GameObject targetGhost;
    private PartyGhostController ghostController;
    private SkinnedMeshRenderer _bodySkinnedMeshRenderer;



    SerializedProperty isMalePresenting;
    SerializedProperty skinTint;
    SerializedProperty shirtTexture;
    SerializedProperty shirtNormal;
    SerializedProperty shirtColor1;
    SerializedProperty shirtColor2;
    SerializedProperty undershirtTexture;
    SerializedProperty undershirtNormal;
    SerializedProperty underShirtColor1;
    SerializedProperty underShirtColor2;
    SerializedProperty useCustomLogo;
    SerializedProperty customLogoTexture;
    SerializedProperty hairColor;
    SerializedProperty edgeHairColor;
    SerializedProperty eyeColor;
    SerializedProperty traits;

    bool isMultiSelected;
    bool isPrefabValid = false;
    const string handMaterialPath = "Assets/3D/Characters/Materials/ghost_skin.mat";
    const string hairMaterialPath = "Assets/3D/Characters/Materials/ghost_hair.mat";
    const string eyeMaterialPath = "Assets/3D/Characters/Materials/ghost_eyes.mat";
    const string logoMaterialPath = "Assets/3D/Characters/Materials/ghost_clothes_alphas/aqua.png";


    private void OnEnable()
    {
        isMalePresenting = serializedObject.FindProperty("isMalePresenting");
        skinTint = serializedObject.FindProperty("skinColor");
        shirtTexture = serializedObject.FindProperty("shirtTexture");
        shirtNormal = serializedObject.FindProperty("shirtNormal");

        shirtColor1 = serializedObject.FindProperty("shirtColor1");
        shirtColor2 = serializedObject.FindProperty("shirtColor2");
        undershirtTexture = serializedObject.FindProperty("undershirtTexture");
        undershirtNormal = serializedObject.FindProperty("undershirtNormal");
        underShirtColor1 = serializedObject.FindProperty("underShirtColor1");
        underShirtColor2 = serializedObject.FindProperty("underShirtColor2");
        useCustomLogo = serializedObject.FindProperty("useCustomLogo");
        customLogoTexture = serializedObject.FindProperty("customLogoTexture");
        hairColor = serializedObject.FindProperty("hairColor");
        edgeHairColor = serializedObject.FindProperty("edgeHairColor");
        eyeColor = serializedObject.FindProperty("eyeColor");
        traits = serializedObject.FindProperty("traits");

        isMultiSelected = targets.Length > 1;
        if (isMultiSelected) return;

        ValidateData();
    }

    private void OnDisable()
    {
        if (_previewRenderUtility != null)
        {
            _previewRenderUtility.Cleanup();
            _previewRenderUtility = null;
        }
        if (targetGhost != null)
        {
            _bodySkinnedMeshRenderer.enabled = true;
            DestroyImmediate(targetGhost);
        }
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(isMalePresenting);
        EditorGUILayout.PropertyField(skinTint);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(shirtTexture);
        EditorGUILayout.PropertyField(shirtNormal);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shirt Colors");
        EditorGUILayout.PropertyField(shirtColor2, GUIContent.none);
        EditorGUILayout.PropertyField(shirtColor1, GUIContent.none);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();


        EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(undershirtTexture);
        EditorGUILayout.PropertyField(undershirtNormal);
        EditorGUILayout.PropertyField(useCustomLogo);
        if (useCustomLogo.boolValue)
        {
            EditorGUILayout.PropertyField(customLogoTexture);
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Undershirt Colors");
        EditorGUILayout.PropertyField(underShirtColor2, GUIContent.none);
        EditorGUILayout.PropertyField(underShirtColor1,GUIContent.none);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Hair Colors");
        EditorGUILayout.PropertyField(hairColor, GUIContent.none);
        EditorGUILayout.PropertyField(edgeHairColor, GUIContent.none);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Eye Color");
        EditorGUILayout.PropertyField(eyeColor, GUIContent.none);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(traits);



        if (serializedObject.ApplyModifiedProperties())
        {
            if (isMultiSelected) return;
            UpdateAllVariants(((PartyGhostVariant)target), ghostController);

        }
        if (isMultiSelected)
        {
            EditorGUILayout.HelpBox("Cannot display preview or utility while multiple ghosts are selected.", MessageType.Info);
        }
        else
        {
            if (!isPrefabValid)
            {
                // Draw a warning field 
                EditorGUILayout.HelpBox("Attached prefab isn't a party ghost!", MessageType.Error);
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Utility");
            EditorGUILayout.HelpBox("Changing names or paths of the materials used is not safe.", MessageType.Info);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Go To Hair Material"))
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(hairMaterialPath, typeof(UnityEngine.Object));
            }
            if (GUILayout.Button("Go To Hand Material"))
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(handMaterialPath, typeof(UnityEngine.Object));
            }
            if (GUILayout.Button("Go To Eye Material"))
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(eyeMaterialPath, typeof(UnityEngine.Object));
            }
            if (GUILayout.Button("Go To Alpha Logos"))
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(logoMaterialPath, typeof(UnityEngine.Object));
            }
        }
    }

    private void ValidateData()
    {
        isPrefabValid = false;
        if (_previewRenderUtility == null)
        {
            _previewRenderUtility = new PreviewRenderUtility();

            _previewRenderUtility.camera.transform.position = new Vector3(0, 0, -6);
            _previewRenderUtility.camera.transform.rotation = Quaternion.identity;
        }

        if (targetGhost == null)
        {
            targetGhost = Instantiate(((PartyGhostVariant)target).prefab);
            ghostController = targetGhost.GetComponent<PartyGhostController>();
            if (ghostController != null)
            {
                isPrefabValid = true;
            }
        }

        UpdateAllVariants(((PartyGhostVariant)target),ghostController);

    }

    public void UpdateAllVariants(PartyGhostVariant target, PartyGhostController targetGhostController)
    {



        MeshRenderer _hairStaticMeshRenderer;
        SkinnedMeshRenderer _headMeshRenderer;
        MeshRenderer _leftEyeObject;
        MeshRenderer _rightEyeObject;
        SkinnedMeshRenderer _handMeshRenderer;

        _bodySkinnedMeshRenderer = targetGhostController.ghostBodyComponents.bodyRenderer;
        _handMeshRenderer = targetGhostController.ghostBodyComponents.handRenderer;
        
        if (isMalePresenting.boolValue)
        {
            _hairStaticMeshRenderer = targetGhostController.ghostBodyComponents.maleHairRenderer;
            _headMeshRenderer = targetGhostController.ghostBodyComponents.maleHeadRenderer;
            _leftEyeObject = targetGhostController.ghostBodyComponents.leftMaleEyeRenderer;
            _rightEyeObject = targetGhostController.ghostBodyComponents.rightMaleEyeRenderer;
        }
        else
        {
            _hairStaticMeshRenderer = targetGhostController.ghostBodyComponents.femaleHairRenderer;
            _headMeshRenderer = targetGhostController.ghostBodyComponents.femaleHeadRenderer;
            _leftEyeObject = targetGhostController.ghostBodyComponents.leftFemaleEyeRenderer;
            _rightEyeObject = targetGhostController.ghostBodyComponents.rightFemaleEyeRenderer;
        }
        
        target.UpdateColorsAndTextures(_bodySkinnedMeshRenderer.sharedMaterials[1], PartyGhostVariant.ghostMaterials.shirt);
        target.UpdateColorsAndTextures(_bodySkinnedMeshRenderer.sharedMaterials[2], PartyGhostVariant.ghostMaterials.undershirt);
        target.UpdateColorsAndTextures(_hairStaticMeshRenderer.sharedMaterial, PartyGhostVariant.ghostMaterials.hair);
        target.UpdateColorsAndTextures(_headMeshRenderer.sharedMaterials[1], PartyGhostVariant.ghostMaterials.hair);
        target.UpdateColorsAndTextures(_leftEyeObject.sharedMaterial, PartyGhostVariant.ghostMaterials.eyes);
        target.UpdateColorsAndTextures(_rightEyeObject.sharedMaterial, PartyGhostVariant.ghostMaterials.eyes);

        target.UpdateColorsAndTextures(_headMeshRenderer.sharedMaterials[0], PartyGhostVariant.ghostMaterials.skin);
        target.UpdateColorsAndTextures(_bodySkinnedMeshRenderer.sharedMaterials[0], PartyGhostVariant.ghostMaterials.skin);
        target.UpdateColorsAndTextures(_handMeshRenderer.sharedMaterial, PartyGhostVariant.ghostMaterials.skin);


        // Hide male vs female
        if (isMalePresenting.boolValue)
        {
            targetGhostController.ghostBodyComponents.maleHeadRenderer.transform.parent.gameObject.SetActive(true);
            targetGhostController.ghostBodyComponents.femaleHeadRenderer.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            targetGhostController.ghostBodyComponents.maleHeadRenderer.transform.parent.gameObject.SetActive(false);
            targetGhostController.ghostBodyComponents.femaleHeadRenderer.transform.parent.gameObject.SetActive(true);
        }
    }
}


#endif