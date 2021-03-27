using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] float moveSpeed = 12;
    // TODO: are you sure this wariable should be placed here? Cause I am not
    [SerializeField] float minDistToWall = 0.022f;

    Rigidbody2D _rb;
    BoxCollider2D _collider;
    MovementSystem _movementSystem;
    public LayerMask platformMask;
    bool _isGrounded;
    bool _jumpStartInput;
    
    void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _movementSystem = GetComponent<MovementSystem>();
        
        var ctx = new MovementStrategy.Context(_rb, _collider, moveSpeed, platformMask, minDistToWall);
        _movementSystem.SetContext(ctx);
        _movementSystem.SetStrategy(context => new StandingStrategy(context));
        // _movementSystem.SetStrategy(new StandingStrategy(ctx));

        // Time.timeScale = 0.1f;
    }

    void Update() {
        _jumpStartInput = _jumpStartInput || Input.GetButtonDown("Jump");
    }

    void FixedUpdate() {
        var hInput = (int) Input.GetAxisRaw("Horizontal");
        var vInput = (int) Input.GetAxisRaw("Vertical");
        var jumpContInput = Input.GetButton("Jump");
        
        _movementSystem.Move(hInput, vInput, _jumpStartInput, jumpContInput, Time.fixedDeltaTime);
        _jumpStartInput = false;
    }
}
