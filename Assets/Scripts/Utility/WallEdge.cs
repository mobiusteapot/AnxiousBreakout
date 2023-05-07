using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEdge : MonoBehaviour
{
    [HideInInspector]
    public PartySceneObject _parentSurface;
    [HideInInspector]
    public WallEdge _siblingEdge = null;
}
