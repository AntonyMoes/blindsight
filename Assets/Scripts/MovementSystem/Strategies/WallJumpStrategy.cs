using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallJumpStrategy : FixedMovementStrategy {
    // static readonly float _wallJumpSpeed = 32;
    // static readonly float _wallJumpDuration = 0.35f;
    // static readonly float _wallJumpAngle = 55;

    static Vector2 GetVelocity(int wallDirection, MovementConstants constants) {
        var jumpDirection = CustomMath.Vector2FromAngle(90 + wallDirection * (90 - constants.WallJumpAngle));
        return jumpDirection * constants.WallJumpSpeed;
    }

    public WallJumpStrategy(Context ctx, int wallDirection) : base(ctx, MovementStrategyT.WallJump, 
        GetVelocity(wallDirection, ctx.movementConstants), ctx.movementConstants.WallJumpDuration) {}
}