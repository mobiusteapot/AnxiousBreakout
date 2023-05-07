using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Control player state, not world or progression (those should be controlled in party scene singleton)
[RequireComponent(typeof(OVRManager))]
public class PlayerManager : MonoBehaviour
{
    static public PlayerManager Instance = null;
    
    [SerializeField]
    public Transform playerCenterEyeTransform;
    
    [SerializeField]
    private SkinnedMeshRenderer[] handRenderers;
    [SerializeField]
    private Material debugHandMaterial;
    [SerializeField]
    private EyeTrackingRayCast[] eyeTrackingRayCasts;

    [SerializeField]
    private Material guardianWallMaterial;

    // Individual toggles for debug visuals
    [SerializeField]
    bool _drawDebugPlayerVisuals = false;
    [SerializeField]
    bool _drawEyeTrackingRayCasts = false;
    [SerializeField]
    bool _drawGuardianWalls = false;

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }

        if (_drawDebugPlayerVisuals)
        {
            DrawDebugPlayerVisuals();
        }
        if (_drawEyeTrackingRayCasts)
        {
            DrawEyeTrackingRayCasts();
        }
    }

    public void DrawDebugPlayerVisuals()
    {
        foreach (SkinnedMeshRenderer handRenderer in handRenderers)
        {
            handRenderer.material = debugHandMaterial;
        }
    }
    public void DrawGuardianWalls()
    {
        if (!_drawGuardianWalls)
        {
            return;
        }
        var AllPartySceneObjects = GameObject.FindObjectsOfType<PartySceneObject>();
        foreach (PartySceneObject partySceneObject in AllPartySceneObjects)
        {
            if (partySceneObject._passthroughMesh != null)
            {
                partySceneObject._passthroughMesh.material = guardianWallMaterial;
            }
        }
    }
    public void DrawEyeTrackingRayCasts()
    {
        foreach (EyeTrackingRayCast eyeTrackingRayCast in eyeTrackingRayCasts)
        {
            eyeTrackingRayCast.EnableDebugLines();
        }
    }



    public void TogglePlayerPassthroughVisibility(System.Action<bool> isBlind)
    {
        StartCoroutine(TogglePassthrough((_userBlind) =>
        {
            if (_userBlind)
                isBlind(true);
        }));
    }

    [Space(10)]
    public float passthroughFadeSpeed = 1.2f;
    static OVRPassthroughLayer passthroughLayer;
    IEnumerator TogglePassthrough(System.Action<bool> userBlind)
    {
        if (passthroughLayer == null)
            passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();


        while(passthroughLayer.textureOpacity >= 0.02f)
        {
            passthroughLayer.textureOpacity -= Time.deltaTime * passthroughFadeSpeed;
            yield return null;
        }

        passthroughLayer.textureOpacity = 0f;
        
        userBlind(true);

        //Wait for all ghosts to be destroyed
        yield return new WaitForSeconds(2.0f);

        while (passthroughLayer.textureOpacity <= 0.98f)
        {
            passthroughLayer.textureOpacity += Time.deltaTime * passthroughFadeSpeed;
            yield return null;
        }

        passthroughLayer.textureOpacity = 1f;

        yield break;
    }
}
