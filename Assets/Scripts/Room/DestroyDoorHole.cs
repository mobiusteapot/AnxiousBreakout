using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Find nearest PartySceneObject and destroy it so the player has a door they can walk through
[RequireComponent(typeof(PartySceneObject))]
public class DestroyDoorHole : MonoBehaviour
{
    public float doorDestroyDelay = 1f;
    float t;
    PartySceneObject[] partySceneObjects;

    PartySceneObject closestPartySceneObject;
    float lastClosestDistance = float.MaxValue;
    private void Update()
    {
        if (PartySceneSingleton.Instance.isSceneInitialized)
        {
            if (t < doorDestroyDelay)
            {
                t += Time.deltaTime;
            }
            else
            {
                DestroyNearestWall();
            }
        }
    }
    public void DestroyNearestWall()
    {
        // Find all party scene objects in the scene
        partySceneObjects = FindObjectsOfType<PartySceneObject>();
        
        // Find the object
        foreach(PartySceneObject partySceneObject in partySceneObjects)
        {
            if (partySceneObject == this.GetComponent<PartySceneObject>())
                continue;
            // Compare distance between self and party scene Object and replace closest if it is closer than the last
            float currentDistance = Vector3.Distance(this.transform.position, partySceneObject.transform.position);
            if(currentDistance < lastClosestDistance)
            {
                closestPartySceneObject = partySceneObject;
                lastClosestDistance = currentDistance;
            }
        }
        Debug.Log("Destroying wall: " + closestPartySceneObject);
        closestPartySceneObject.gameObject.SetActive(false);
        Destroy(this);
    }
}
