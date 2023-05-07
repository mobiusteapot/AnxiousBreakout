using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How to use:
/// Call the "ToastController.ShowToast(AllToasts.</Your choice of toast>)"
/// Use callback if required
/// </summary>

public class ToastController : MonoBehaviour
{

    public static ToastController toastControllerInstance;


    public delegate void ToastEvent();
    public static event ToastEvent ShowToastEvent;
            

    static Toast currentToast;


    private void Awake()
    {
        if (toastControllerInstance == null)
            toastControllerInstance = this;
        else
            Destroy(toastControllerInstance);
    }

                
    public Toast GetCurrentToast()
    {
        return currentToast;
    }
    
    public async void ShowToast(Toast toastToShow, Action toastShown)
    {
        currentToast = toastToShow;
        ShowToastEvent?.Invoke();

        while(ToastUI.isShowingToast)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        toastShown();        
    }
   
}

public class Toast
{
    public KeyValueList<string, float> _content_OnscreenTimeList;

    public Toast(KeyValueList<string, float> content_OnscreenTime)
    {
        _content_OnscreenTimeList = content_OnscreenTime;
    }    
}

public class KeyValueList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
{
    public void Add(TKey key, TValue value)
    {
        Add(new KeyValuePair<TKey, TValue>(key, value));
    }
}

public static class AllToasts
{
    public static readonly Toast PartyInvitation = new Toast(new KeyValueList<string, float>()
    {
        {"You have been invited to this party, maybe your friends have already arrived, you should go inside.", 5f }
    });


    public static readonly Toast FriendsNotArrived = new Toast(new KeyValueList<string, float>()
    {
        {"It looks like your friends haven't arrived yet.", 5f }
    });


    public static readonly Toast SomethingAtYourFeet = new Toast(new KeyValueList<string, float>()
    {
        {"There's something at your feet?", 5f }
    });


    public static readonly Toast SomeonesWallet = new Toast(new KeyValueList<string, float>()
    {
        {"This is someone's wallet, you should return it.", 5f }
    });


    public static readonly Toast SaySomething = new Toast(new KeyValueList<string, float>()
    {
        {"You should say something.", 5f }
    });


    public static readonly Toast DidntHearYouTryAgain = new Toast(new KeyValueList<string, float>()
    {
        {"They didn't hear you, try again.", 5f }
    });


    public static readonly Toast FriendsArrived = new Toast(new KeyValueList<string, float>()
    {
        {"Your friends should have arrived by now, you can go check outside for them or join in the dance party!", 5f }
    });
}


