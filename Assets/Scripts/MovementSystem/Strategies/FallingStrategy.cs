using System;
using UnityEngine;

public class FallingStrategy : MovementStrategy {
    // readonly float _maxFallingSpeed = 24;
    // readonly float _jumpSpeed = 18;
    // readonly float _maxJumpDuration = 0.4f;
    // readonly float _minJumpDuration = 0.1f;
    
    // float _remainingJumpDuration;
    // bool _isJumping;
    // readonly float? _initialFallProgression;
    // readonly  SpeedTransformer _jumpTransformer;
    readonly SpeedTransformer _transformer;

    public FallingStrategy(Context ctx, float? initialFallProgression = null, SpeedTransformer transformer = null) : base(ctx, MovementStrategyT.Falling) {
        // _isJumping = isJumping;
        // _jumpTransformer = jumpTransformer;
        _transformer = transformer ?? SpeedTransformer.GetFallTransformer(ctx.movementConstants);
        if (initialFallProgression is float progression) {
            _transformer.Progression = progression;
        }
    }
    
    public override Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput,
        bool jumpContInput, float deltaTime) {
        var nearWall = IsNearWall();
        if (nearWall != 0) {
            if (jumpStartInput) {
                return ctx => new WallJumpStrategy(ctx, nearWall);
            }
            
            var cliff = GetCliff(nearWall);
            if (cliff && hInput == nearWall) {  // This condition means object can hang even on a cliff adjacent to a ceiling, maybe TODO
                return ctx => new CliffHangingStrategy(ctx);
            }
            
            var wallDirection = IsWalled();
            if (wallDirection != 0 && hInput == wallDirection) {
                return ctx => new WallClingingStrategy(ctx);
            }
        }

        if (IsGrounded()) {
            return ctx => new StandingStrategy(ctx);
        }

        var constants = _ctx.movementConstants;

        float yVelocity = _transformer.Transform(deltaTime, constants);
        
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
