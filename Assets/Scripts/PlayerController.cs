using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    MovementSystem _movementSystem;
    bool _isGrounded;
    bool _jumpStartInput;
    
    void Start() {
        _movementSystem = GetComponent<MovementSystem>();
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
