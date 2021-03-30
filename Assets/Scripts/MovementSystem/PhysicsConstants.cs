using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsConstants : MonoBehaviour
{
    
    
    [SerializeField] float minDistToWall = 0.022f;
    /**
        This is empirically derived minimal distance
        between two colliders at which overlap checks
        work correctly
    */
    public float MinDistToWall => minDistToWall;

    [SerializeField] float wallCheckDepth = 0.04f;
    public float WallCheckDepth => wallCheckDepth;
}
