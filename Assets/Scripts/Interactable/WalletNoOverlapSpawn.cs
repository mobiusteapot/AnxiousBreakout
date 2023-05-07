using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalletNoOverlapSpawn : MonoBehaviour
{
    public GameObject walletPrefab;

    public float maximumSpawnDistanceFromPlayer = 1.2f, minimumDistanceFromNPC = 0.1f;
    public int numberOfSamplePoints = 10;
    public float spawnAngle = 50f;

    List<Transform> currentVisibleTransforms = new List<Transform>();
    Plane[] cameraPlanes;

    BoxCollider[] allHeadBoxColliders;


    static Vector2 player2DPosition;
    // Start is called before the first frame update
    void Start()
    {
        allHeadBoxColliders = FindObjectsOfType<BoxCollider>();
        player2DPosition = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            FindSpawnPoint();
    }


    Vector3 finalSpawnPoint = Vector3.zero;
    static List<Vector3> currentSamplePoints = new List<Vector3>();
    static Vector2 npc2DPosition;
    public void FindSpawnPoint()
    {
        //Clear previous values
        currentVisibleTransforms.Clear();

        //Find NPC's (Head Colliders) in view
        cameraPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        foreach (var collider in allHeadBoxColliders)
        {
            if (GeometryUtility.TestPlanesAABB(cameraPlanes, collider.bounds))
            {
                if (collider.gameObject.CompareTag("EyeGazeTarget"))
                {
                    npc2DPosition = new Vector2(collider.gameObject.transform.position.x, collider.gameObject.transform.position.z);
                    
                    if (Mathf.Abs(Vector2.Distance(player2DPosition, npc2DPosition)) <= maximumSpawnDistanceFromPlayer)
                        currentVisibleTransforms.Add(collider.gameObject.transform);
                }
                
            }
        }
        Debug.Log(currentVisibleTransforms.Count);

        if (currentVisibleTransforms.Count == 0)
        {
            walletPrefab.transform.position = Camera.main.transform.position + Camera.main.transform.forward * maximumSpawnDistanceFromPlayer;
        }
        else
        {
                        
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(finalSpawnPoint, 0.2f);
    }

}
