using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickRandomDance : StateMachineBehaviour
{
    [SerializeField]
    int numberOfDances = 1;
    int newDanceTarget = 0;
    int lastDance = 0;
    [SerializeField]
    float danceTransitionTime = 1f;
    float danceTransitionTimer = 0f;

    [SerializeField]
    int numberOfLoopsBeforeChangeMax = 3;
    [SerializeField]
    int numberOfLoopsBeforeChangeMin = 1;

    int numberOfLoopsBeforeChange;

    bool lerpToNewDance = false;
    float normalizedTimeOffset = 0;

    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        numberOfLoopsBeforeChange = Random.Range(numberOfLoopsBeforeChangeMin, numberOfLoopsBeforeChangeMax);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (lerpToNewDance)
        {
            
            // Lerp to new dance blend

            if (danceTransitionTimer < danceTransitionTime)
            {
                danceTransitionTimer += Time.deltaTime;
                animator.SetFloat("DanceBlend", Mathf.Lerp(lastDance, newDanceTarget, danceTransitionTimer / danceTransitionTime));
            }
            else
            {
                lerpToNewDance = false;
            }

        }
        else if (Mathf.FloorToInt(stateInfo.normalizedTime - normalizedTimeOffset) >= numberOfLoopsBeforeChange)
        {
            lerpToNewDance = true;
            numberOfLoopsBeforeChange = Random.Range(numberOfLoopsBeforeChangeMin, numberOfLoopsBeforeChangeMax);
            danceTransitionTimer = 0f;
            normalizedTimeOffset += stateInfo.normalizedTime;
            lastDance = newDanceTarget;
            newDanceTarget = GetNewDanceBlend();
        }
        
    }

    int GetNewDanceBlend()
    {
        int targetDance = Random.Range(0, numberOfDances);
        if (targetDance == lastDance)
        {
            return GetNewDanceBlend();
        }
        else return targetDance;
    }
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}
}
