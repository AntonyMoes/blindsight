using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingStrategy : GenericFallingStrategy {
    public FallingStrategy(Context ctx, bool isJumping = false) : base(ctx, MovementStrategyT.Falling,
        SpeedTransformer.GetFallTransformer(ctx.movementConstants), isJumping,
        SpeedTransformer.GetJumpTransformer(ctx.movementConstants)) { }
}
