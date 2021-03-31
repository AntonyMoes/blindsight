using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallClingingStrategy : MovementStrategy {
    // readonly float _maxWallSlidingSpeed = 6;
    
    public WallClingingStrategy(Context ctx) : base(ctx, MovementStrategyT.WallClinging) {}

    public override Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput,
        bool jumpContInput, float deltaTime) {
        if (IsGrounded()) {
            return ctx => new StandingStrategy(ctx);
        }
        
        var wallDirection = IsWalled();
        if (wallDirection == 0 || hInput == -wallDirection) {
            return ctx => new FallingStrategy(ctx);
        }
        
        if (jumpStartInput) {
            return ctx => new WallJumpStrategy(ctx, wallDirection);
        }

        var yVelocity = -_ctx.movementConstants.WallSlidingSpeed;
        _ctx.rb.velocity = new Vector2(0, yVelocity);
        
        return null;
    }

    public override void OnStart() {
        _ctx.rb.velocity = Vector2.zero;

        var direction = IsWalled();
        var wall = GetWall(direction);
        MoveToWall(direction, wall);
    }
}
