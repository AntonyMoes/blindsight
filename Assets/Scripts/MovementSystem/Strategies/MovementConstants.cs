﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementConstants : MonoBehaviour {
    [SerializeField] float moveSpeed = 12;
    public float MoveSpeed => moveSpeed;
    
    
    [SerializeField] float maxFallingSpeed = 24;
    public float MaxFallingSpeed => maxFallingSpeed;
    
    
    [SerializeField] float jumpSpeed = 18;
    public float JumpSpeed => jumpSpeed;
    [SerializeField] float maxJumpDuration = 0.4f;
    public float MaxJumpDuration => maxJumpDuration;
    [SerializeField] float minJumpDuration = 0.1f;
    public float MinJumpDuration => minJumpDuration;
    
    
    [SerializeField] float wallJumpSpeed = 32;
    public float WallJumpSpeed => wallJumpSpeed;
    [SerializeField] float wallJumpDuration = 0.35f;
    public float WallJumpDuration => wallJumpDuration;
    [SerializeField] float wallJumpAngle = 55;
    public float WallJumpAngle => wallJumpAngle;
    
    
    [SerializeField] float maxWallSlidingSpeed = 6;
    public float MaxWallSlidingSpeed => maxWallSlidingSpeed;
}