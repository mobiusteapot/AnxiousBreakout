using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EyeTrackingRayCast : MonoBehaviour
{
    
    [HideInInspector]
    public string eyeTargetTag;

    [Layer]
    public int ignoreLayer;
    int layerMask;
    private GameObject lastHitTarget;
    [SerializeField]
    private Material debugMaterial;
    private bool isDebug = false;
    private LineRenderer lineRenderer;
    void Awake(){
        layerMask = 1 << ignoreLayer;
        layerMask = ~layerMask;
    }
    void LateUpdate()
    {
        // Draw debug if enabled
        if (isDebug) {
            lineRenderer.SetPosition(0, this.transform.position);
            lineRenderer.SetPosition(1, this.transform.position + this.transform.forward * 1000);
        }


        RaycastHit hit;
        // Infinite raycast
        if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, Mathf.Infinity, layerMask))
        {
            if (hit.collider.gameObject.tag == eyeTargetTag)
            {
                GameObject target = hit.collider.gameObject;
                // If the last hit target, the current target, and the hit collider are all the same, return to prevent duplicate messages
                if (target == lastHitTarget)
                    return;
                if(lastHitTarget != null)
                {
                    lastHitTarget.SendMessage("OnGazeExit");
                }
                lastHitTarget = target;
                lastHitTarget.SendMessage("OnGazeEnter");
                return;
            }
        }

        // If hit nothing or if hit object not found in list then last gaze is null
        if (lastHitTarget != null)
        {
            lastHitTarget.SendMessage("OnGazeExit");
            lastHitTarget = null;
        }
    }

    public void EnableDebugLines()
    {
        isDebug = true;
        lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = debugMaterial;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.positionCount = 2;

        
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // Draw infinite raycast
        Gizmos.DrawRay(this.transform.position, this.transform.forward * 1000);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(EyeTrackingRayCast))]
public class EyeTrackingRayCastEditor : Editor
{
    string[] tagStr;
    int tagIndex = 0;
    private void OnEnable()
    {
        tagStr = UnityEditorInternal.InternalEditorUtility.tags;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EyeTrackingRayCast myScript = (EyeTrackingRayCast)target;
        if (tagStr.Length == 0)
        {
            return;
        } 
        tagIndex = System.Array.IndexOf(tagStr, myScript.eyeTargetTag);
        // Set eyeTargetTag in editor
        EditorGUILayout.BeginHorizontal();
        // Text displaying "Tag of all possible objects to be tracked"
        GUILayout.Label("Tag of all possible objects to be tracked");
        tagIndex = EditorGUILayout.Popup(tagIndex, tagStr);
        EditorGUILayout.EndHorizontal();
        myScript.eyeTargetTag = tagStr[tagIndex];
        // Todo: conditional set dirty
        EditorUtility.SetDirty(target);
    }
}
#endif