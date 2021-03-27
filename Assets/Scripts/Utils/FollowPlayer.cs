using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {
    public Transform target;
    public float zOffset;

    void LateUpdate() {
        if (!target) {
            return;
        }
        
        var targetPos = target.position;
        transform.position = new Vector3(targetPos.x, targetPos.y, zOffset);
    }
}
