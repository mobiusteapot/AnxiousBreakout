using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnToastTest : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Toast newToast = new Toast(new KeyValueList<string, float>()
        {
            {"Hello first toast", 2f },
            {"Hello Second Toast", 4f }
        });


        ToastController.toastControllerInstance.ShowToast(newToast, () =>
        {
            Debug.Log("Done showing toasts");
        });
    }

}
