using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public abstract class FixedMovementStrategy : MovementStrategy {
    readonly Vector2 _velocity;
    readonly float _duration;
    float _currentDuration;
    Func<Context, MovementStrategy> _instNextStrategy;

    public FixedMovementStrategy(Context ctx, MovementStrategyT type, Vector2 velocity, float duration, Func<Context, MovementStrategy> instNextStrategy = null) : base(ctx, type) {
        _velocity = velocity;
        _duration = duration;
        _instNextStrategy = instNextStrategy;
    }

    public override Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput,
        bool jumpContInput, float deltaTime) {
        
        if (IsGrounded()) {
            return ctx => new StandingStrategy(ctx);
        }

        var wallDirection = IsWalled();
        var facingWall = wallDirection != 0 && wallDirection == (int) Mathf.Sign(_velocity.x);
        _currentDuration += deltaTime;
        if (_currentDuration >= _duration || IsCeiled() || facingWall) {
            if (_instNextStrategy != null) {
                return _instNextStrategy;
            }

            return ctx => new FallingStrategy(ctx);
        }

        _ctx.rb.velocity = _velocity;

        return null;
    }

    public override void OnFinish() {
        _ctx.rb.velocity = Vector2.zero;
    }
}
