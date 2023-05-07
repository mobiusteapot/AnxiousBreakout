using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TempDialogue : MonoBehaviour
{

    public UnityEvent OnDialogueSelected;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "XR_Socket_Right" || other.gameObject.name == "XR_Socket_Left")
        {
            OnDialogueSelected?.Invoke();
        }
    }
}
