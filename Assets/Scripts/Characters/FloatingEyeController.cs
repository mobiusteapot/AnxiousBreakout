using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class FloatingEyeController : MonoBehaviour
{
    [Tooltip("Should currently be looking at player. Likely want this to false on start")]
    public bool shouldLookAtPlayer;
    [Range(0, 100)] public float shouldLookAtPlayerMinWait = 2;
    [Range(0, 100)] public float shouldLookAtPlayerMaxWait = 5;
    [SerializeField]
    private MeshRenderer targetMeshRenderer;

    [HideInInspector]
    public bool isGazeEntered { get; private set; } = false;
    private void Awake()
    {
        StartCoroutine(ShouldLookAtPlayerAfterWait(UnityEngine.Random.Range(shouldLookAtPlayerMinWait,shouldLookAtPlayerMaxWait)));
    }
    public void Update()
    {
        if (shouldLookAtPlayer)
        {
            transform.LookAt(Camera.main.transform.position);
        }
    }
    public void OnGazeEnter()
    {
        if (isGazeEntered || !shouldLookAtPlayer)
            return;
        else isGazeEntered = true;
        Debug.Log("OnGazeEnter");
        targetMeshRenderer.material.SetFloat("_CloseEyes", 1);
    }
    public void OnGazeExit()
    {
        if (!isGazeEntered || !shouldLookAtPlayer)
            return;
        else isGazeEntered = false;
        Debug.Log("OnGazeExit");
        targetMeshRenderer.material.SetFloat("_CloseEyes", 0);
    }

    IEnumerator ShouldLookAtPlayerAfterWait(float waitTime)
    {
        // Wait then set bool shouldLookAtPlayer
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Main player camera: " + Camera.main.gameObject.name);
        shouldLookAtPlayer = true;
        yield return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FloatingEyeController))]
[CanEditMultipleObjects]
public class FloatingEyeControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        FloatingEyeController controller = (FloatingEyeController)target;
        // Disabled checkbox for isGazeEntered
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Toggle("isGazeEntered", controller.isGazeEntered);
        EditorGUI.EndDisabledGroup();
    }
}

#endif