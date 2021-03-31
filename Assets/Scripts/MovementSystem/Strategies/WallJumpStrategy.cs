using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallJumpFallingStrategy : GenericFallingStrategy {
    static SpeedTransformer GetJumpTransformer(MovementConstants constants) {
        return new SpeedTransformer((t, c) => c.WallJumpSpeed, constants.WallJumpDuration / 2,
            constants.WallJumpDuration / 2);
    }
    
    public WallJumpFallingStrategy(Context ctx) : base(ctx, MovementStrategyT.WallJumpFalling, 
        SpeedTransformer.GetFallTransformer(ctx.movementConstants), true,
        GetJumpTransformer(ctx.movementConstants)) { }
}

public class WallJumpStrategy : FixedMovementStrategy {
    // static readonly float _wallJumpSpeed = 32;
    // static readonly float _wallJumpDuration = 0.35f;
    // static readonly float _wallJumpAngle = 55;

    static Vector2 GetVelocity(int wallDirection, MovementConstants constants) {
        return new Vector2(constants.MoveSpeed * wallDirection * -1, constants.JumpFallSpeed);
    }

    static Func<Context, MovementStrategy> fallingStrategyInst = context => new WallJumpFallingStrategy(context);

    public WallJumpStrategy(Context ctx, int wallDirection) : base(ctx, MovementStrategyT.WallJump, 
        GetVelocity(wallDirection, ctx.movementConstants), ctx.movementConstants.WallJumpDuration / 2, 
        fallingStrategyInst) {}
}