using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Can create from menu

public class GhostMoving : GhostState
{
    
    protected override void OnStateEnter()
    {
        Debug.Log("Start talking animation here!");
    }

    protected override void OnStateExit()
    {
        Debug.Log("Stop talking animation here!");
    }
}
