using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartySceneObject : MonoBehaviour
{
    public MeshRenderer _passthroughMesh;
    public int _surfaceID = 0;
    public Vector3 _dimensions = Vector3.one;

    [HideInInspector]
    public bool _isWall = false;
    [HideInInspector]
    public bool _isFurniture = false;

    [HideInInspector]
    public float _effectTimer = 0.0f;
    const float _effectTime = 1.0f;
    public bool _passthroughWallActive = true;
    public List<WallEdge> wallEdges = new List<WallEdge>();

}