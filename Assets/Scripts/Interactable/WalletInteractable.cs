using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalletInteractable : MonoBehaviour
{
    [Tooltip("Maximum distance away for spawn")]
    public float maxSearchDistance = 10f;
    public float spawnKickDistance = 1f;
    public float spawnKickDuration = 0.5f;

    
    // Todo: Sound
    void Start()
    {
        StartCoroutine(SpawnKick());
    }

    IEnumerator SpawnKick()
    {
        // Spawn at player feet
        Transform playerTransform = PartySceneSingleton.Instance._playerManager.playerCenterEyeTransform;
        // Warp to player transform then put on nearest position on navmesh
        this.transform.position = playerTransform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(this.transform.position, out hit, maxSearchDistance, NavMesh.AllAreas))
        {
            this.transform.position = hit.position;
        }
        else
        {
            Debug.LogWarning("Attempted to spawn wallet but cannot find nav mesh!");
        }
        // Tmp set to intended position manually
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.5f, this.transform.position.z);
        // Get a position spawnKickDistance away from the player, facing the player's forward direction and lerp to it
        Vector3 targetPos = this.transform.position + (playerTransform.forward * spawnKickDistance);
        // Put it back on the ground
        targetPos = new Vector3(targetPos.x, this.transform.position.y + 0.5f, targetPos.z);
        Vector3 startingPos = this.transform.position;
        float time = 0;
        while (time < spawnKickDuration)
        {
            float t = (time / spawnKickDuration);
            this.transform.position = Vector3.Lerp(startingPos, targetPos, t * t * (3f - 2f * t));
            time += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    public void WalletPickedUp()
    {
        PartySceneSingleton.Instance.hasWalletBeenPickedUp = true;
    }


    public void OnWalletRevceivedByNPC()
    {
        //NPC says thank you
    }




}
