using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SUDSCorrectSpawnTest : MonoBehaviour
{

    [SerializeField]
    Camera playerCamera;

    // Start is called before the first frame update
    void Start()
    {

    }

    Vector3 projectedVector;
    Vector3 playerForwardAtHeight;
    float zDistanceFromPlayer;
    // Update is called once per frame
    void Update()
    {
        projectedVector = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up);
        
        projectedVector.y = playerCamera.transform.position.y;

        //if (Vector3.Dot(playerCamera.transform.forward, projectedVector) < 0)
        //{
        //    playerForwardAtHeight = new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y, projectedVector.z);
        //}
        //else
        //{
        //    playerForwardAtHeight = new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y, 1.2f);
        //}

        transform.position = projectedVector;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawRay(playerCamera.transform.position, projectedVector);
    //}


}
