using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingStrategy : MovementStrategy {
    readonly float _maxFallingSpeed = 24;
    readonly float _jumpSpeed = 18;
    readonly float _maxJumpDuration = 0.4f;
    readonly float _minJumpDuration = 0.1f;
    float _remainingJumpDuration;
    bool _isJumping;

    public FallingStrategy(Context ctx, bool isJumping = false) : base(ctx) {
        _isJumping = isJumping;
    }
    
    public override Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput,
        bool jumpContInput, float deltaTime) {
        var nearWall = IsNearWall();
        if (nearWall != 0) {
            var cliff = GetCliff(nearWall);
            if (cliff && hInput == nearWall) {  // This condition means object can hang even on a cliff adjacent to a ceiling, maybe TODO
                return ctx => new CliffHangingStrategy(ctx);
            }
            
            var wallDirection = IsWalled();
            if (wallDirection != 0 && hInput == wallDirection) {
                return ctx => new WallClingingStrategy(ctx);
            }
        }

        if (IsGrounded() && !_isJumping) {
            return ctx => new StandingStrategy(ctx);
        }

        var yVelocity = Mathf.Max(_ctx.rb.velocity.y, -_maxFallingSpeed);
        if (_isJumping) {
            var rjd = _remainingJumpDuration;
            if ((rjd <= 0 || !jumpContInput) && (rjd <= _maxJumpDuration - _minJumpDuration) || IsCeiled()) {
                _isJumping = false;
            }
            else {
                yVelocity = _jumpSpeed;
                _remainingJumpDuration -= deltaTime;
            }
        }
        
        /*
         * The reason the next line is still there despite having "PreventCollisions" in movement system is
         * we still should not allow objects to move into (though never colliding) the walls,
         * Y movement is fucked otherwise
         */
        var hVelocity = hInput == nearWall ? 0 : hInput * _ctx.baseSpeed;
        _ctx.rb.velocity = new Vector2(hVelocity, yVelocity);

        return null;
    }

    public override void OnStart() {
        if (_isJumping) {
            _remainingJumpDuration = _maxJumpDuration;
        }
    }
}
