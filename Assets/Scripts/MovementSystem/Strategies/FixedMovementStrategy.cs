﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FixedMovementStrategy : MovementStrategy {
    readonly Vector2 _velocity;
    readonly float _duration;
    float _currentDuration;

    public FixedMovementStrategy(Context ctx, MovementStrategyT type, Vector2 velocity, float duration) : base(ctx, type) {
        _velocity = velocity;
        _duration = duration;
    }

    public override Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput,
        bool jumpContInput, float deltaTime) {
        var wallDirection = IsWalled();
        if (wallDirection == (int) Mathf.Sign(_velocity.x)) {
            return ctx => new WallClingingStrategy(ctx);
        }
        
        if (IsGrounded()) {
            return ctx => new StandingStrategy(ctx);
        }

        _currentDuration += deltaTime;
        if (_currentDuration >= _duration) {
            return ctx => new FallingStrategy(ctx);
        }

        _ctx.rb.velocity = _velocity;

        return null;
    }

    public override void OnFinish() {
        _ctx.rb.velocity = Vector2.zero;
    }
}
