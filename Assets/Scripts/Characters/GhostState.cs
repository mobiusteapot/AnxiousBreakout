using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class GhostState 
{
    // Children can get the controller to read/write from it
    protected PartyGhostController controller { get; private set; }
    
    public virtual void BehaviorTick() { }
    public void OnInitialize(PartyGhostController newPartyGhostController) 
    {
        controller = newPartyGhostController;
        OnStateEnter();
    }
    public void OnChangeState()
    {
        OnStateExit();
    }
    protected virtual void OnStateEnter() { }
    protected virtual void OnStateExit() { }
    public virtual void OnGazeEnter() { }
    public virtual void OnGazeExit() { }
    public virtual void OnGazeStay() { }
    
}
