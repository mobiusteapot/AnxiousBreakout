using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimationBindingController : MonoBehaviour
{
    [SerializeReference]
    public string dataToAddAtStart;

    [SerializeReference]
    public AnimationClip animationClip;
    

}


#if UNITY_EDITOR
[CustomEditor(typeof(AnimationBindingController))]
public class AnimationBindingsEditor : Editor
{
    AnimationClip currentAnimationClip;

    AnimationBindingController bindingController;

    private void OnEnable()
    {
        bindingController = target as AnimationBindingController;
        currentAnimationClip = bindingController.animationClip;
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Modify Bindings"))
        {
            Clear();
            ModifyBindings();
        }

    }


    void Clear()
    {
        animationClipCurveDatas = null;
    }


    AnimationClipCurveData[] animationClipCurveDatas;
    void ModifyBindings()
    {

        animationClipCurveDatas = AnimationUtility.GetAllCurves(currentAnimationClip);

        for (int i = 0; i < animationClipCurveDatas.Length; i++)
        {
            if (animationClipCurveDatas[i].path.Contains(bindingController.dataToAddAtStart))
                continue;

            animationClipCurveDatas[i].path = bindingController.dataToAddAtStart + animationClipCurveDatas[i].path;
        }

        currentAnimationClip.ClearCurves();

        foreach (var curveDataInstance in animationClipCurveDatas)
        {
            currentAnimationClip.SetCurve(curveDataInstance.path, curveDataInstance.type, curveDataInstance.propertyName, curveDataInstance.curve);
        }

        Debug.LogError("DONE");
    }
}
#endif
