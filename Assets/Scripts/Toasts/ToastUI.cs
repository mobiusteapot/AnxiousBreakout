using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;

/// <summary>
/// Used to manage Toast UI animations, Events for start and stop of UI animation
/// </summary>
public class ToastUI : MonoBehaviour
{

    [SerializeField]
    Animator toastUIAnimator;

    [SerializeField]
    TMP_Text toastTextArea;

    [SerializeField]
    string openToastTrigger_Animator = "ShowToast", closeToastTrigger_Animator= "CloseToast";


    #region Unity Events , C# Events
    /// <summary>
    /// Toast UI events. Just in case. Can be removed later
    /// </summary>
    public UnityEvent OnToastUIStarted_UE, OnToastUIEnded_UE;

    public delegate void ToastUIStart();
    public static event ToastUIStart ToastUIStarted;
    
    public delegate void ToastUIEnd();
    public static event ToastUIEnd ToastUIEnded;

    #endregion


    public static bool isShowingToast = false;

    private void OnEnable()
    {
        ToastController.ShowToastEvent += OpenToastUI;
    }

    private void OnDisable()
    {
        ToastController.ShowToastEvent -= OpenToastUI;
    }

    void OpenToastUI()
    {
        isShowingToast = true;
        StartCoroutine(OpenToastUI_Coroutine());
    }


    void CloseToastUI()
    {
        isShowingToast = false;
        StartCoroutine(CloseToastUI_Coroutine());
    }



    //Get Toast content, On screen duration, and play UI animation
    IEnumerator OpenToastUI_Coroutine()
    {
        toastUIAnimator.SetTrigger(openToastTrigger_Animator);
        foreach (var content_OnscreenTime_Pair in ToastController.toastControllerInstance.GetCurrentToast()._content_OnscreenTimeList)
        {

            //If any text animations are needed add them here


            //Loop through all toast keyvalue pairs
            toastTextArea.text = content_OnscreenTime_Pair.Key;
            yield return new WaitForSeconds(content_OnscreenTime_Pair.Value);
        }

        CloseToastUI();

        yield break;
    }

    //Close UI animation
    IEnumerator CloseToastUI_Coroutine()
    {
        toastUIAnimator.SetTrigger(closeToastTrigger_Animator);

        yield break;
    }
        

    ////Animation triggers for precise timings. Check Toast UI animation
    //public void ToastUIStarted_AE()
    //{
    //    OnToastUIStarted_UE?.Invoke();
    //    ToastUIStarted?.Invoke();        
    //}

    //public void ToastUIClosed_AE()
    //{
    //    OnToastUIEnded_UE?.Invoke();
    //    ToastUIEnded?.Invoke();
    //}

}
