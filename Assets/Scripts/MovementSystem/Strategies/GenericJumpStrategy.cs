using System;
using UnityEngine;

public class GenericJumpStrategy : MovementStrategy {
    readonly SpeedTransformer _transformer;

    public GenericJumpStrategy(Context ctx, MovementStrategyT type, SpeedTransformer transformer = null) : base(ctx,
        type) {
        _transformer = transformer ?? SpeedTransformer.GetJumpTransformer(ctx.movementConstants);
    }
    
    public override Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput, bool jumpContInput, float deltaTime) {
        var constants = _ctx.movementConstants;

        float yVelocity;
        if (_transformer.Ended() || _transformer.CanEnd() && !jumpContInput || IsCeiled()) {
            var progression = Mathf.Max(1 - _transformer.Progression, 0);
            return ctx => new FallingStrategy(ctx, progression);
        }
        yVelocity = _transformer.Transform(deltaTime, constants);

        var nearWall = IsNearWall();
        /*
         * The reason the next line is still there despite having "PreventCollisions" in movement system is
         * we still should not allow objects to move into (though never colliding) the walls,
         * Y movement is fucked otherwise
         */
        var hVelocity = hInput == nearWall ? 0 : hInput * _ctx.movementConstants.MoveSpeed;
        _ctx.rb.velocity = new Vector2(hVelocity, yVelocity);

        return null;
    }
}
