using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RandomStartOffset : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat("AnimationStartOffset", Random.Range(0, stateInfo.length));
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(RandomStartOffset))]
public class RandomStartOffsetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("Remember to set cycle offset to the variable \"AnimationStartOffset\"",MessageType.Info);
    }
}
#endif