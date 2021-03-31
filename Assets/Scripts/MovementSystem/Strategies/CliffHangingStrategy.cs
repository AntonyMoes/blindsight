using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CliffHangingStrategy : MovementStrategy {
    float _savedGravityScale;
    
    public CliffHangingStrategy(Context ctx) : base(ctx, MovementStrategyT.CliffHanging) { }
    
    public override Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput, bool jumpContInput, float deltaTime) {
        var wallDirection = IsNearWall();
        if (hInput == -wallDirection) {
            return ctx => new FallingStrategy(ctx);
        }

        if (vInput == Below) {
            return ctx => new WallClingingStrategy(ctx);
        }
        
        if (jumpStartInput) {
            if (vInput == Above) {
                return ctx => new JumpStrategy(ctx);
            }
            return ctx => new WallJumpStrategy(ctx, wallDirection);
        }

        return null;
    }

    public override void OnStart() {
        _ctx.rb.velocity = Vector2.zero;
        _savedGravityScale = _ctx.rb.gravityScale;
        _ctx.rb.gravityScale = 0;

        var direction = IsNearWall();
        var cliff = GetCliff(direction);
        MoveToCliff(direction, cliff);
    }

    public override void OnFinish() {
        _ctx.rb.gravityScale = _savedGravityScale;
    }
}
