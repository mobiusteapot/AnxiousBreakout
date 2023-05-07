using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider))]
public class PartyGhostGazeHitbox : MonoBehaviour
{
    [SerializeField]
    PartyGhostController partyGhostController;

    [HideInInspector]
    public bool isGazeEntered { get; private set; } = false;
    public void OnGazeEnter()
    {
        isGazeEntered = true;
        Debug.Log("OnGazeEnterGhost");
        partyGhostController.EnterGazeFromHitbox();
    }
    public void OnGazeExit()
    {
        isGazeEntered = false;
        Debug.Log("OnGazeExitGhost");
        partyGhostController.ExitGazeFromHitbox();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PartyGhostGazeHitbox))]
[CanEditMultipleObjects]
public class PartyGhostGazeHitboxEditor : Editor
{
    SerializedProperty partyGhostController;
    private void OnEnable()
    {
        partyGhostController = serializedObject.FindProperty("partyGhostController");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PartyGhostGazeHitbox gazeHitbox = (PartyGhostGazeHitbox)target;
        // If partyGhostController is null, show error
        if (partyGhostController.propertyType != SerializedPropertyType.ObjectReference || partyGhostController.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("PartyGhostController is not set!", MessageType.Error);
        }
        // Disabled checkbox for isGazeEntered
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Toggle("isGazeEntered", gazeHitbox.isGazeEntered);
        EditorGUI.EndDisabledGroup();
    }
}

#endif