using System;
using UnityEngine;
using SpeedFunc = System.Func<float, MovementConstants, float>;

public class SpeedTransformer {
    float _currTime;
    readonly float? _minTime;
    readonly float _maxTime;
    readonly SpeedFunc _transformer;
    public float Progression {
        get => _currTime / _maxTime;
        set => _currTime = value * _maxTime;
    }
    
    public static readonly SpeedFunc JumpFunc = (float t, MovementConstants constants) => {
        var js = constants.JumpSpeed;
        if (t < 0.6) {
            return js;
        }
        if (t <= 1) {
            return -2.5f * js * t + 2.5f * js;
        }
        return 0;
    };

    public static readonly SpeedFunc FallFunc = (float t, MovementConstants constants) => {
        return -JumpFunc(-t - 1, constants);
    };

    public static SpeedTransformer GetJumpTransformer(MovementConstants constants) {
        return new SpeedTransformer(JumpFunc, constants.MaxJumpDuration, constants.MinJumpDuration);
    }
    public static SpeedTransformer GetFallTransformer(MovementConstants constants) {
        return new SpeedTransformer(FallFunc, constants.MaxJumpDuration);
    }

    public SpeedTransformer(SpeedFunc transformer, float maxTime, float minTime = 0, float initTime = 0) {
        _transformer = transformer;
        _maxTime = maxTime;
        _minTime = minTime;
        _currTime = initTime;
    }

    public float Transform(float deltaTime, MovementConstants constants) { 
        _currTime += deltaTime;
        return _transformer(Progression, constants);
    }

    public bool CanEnd() {
        if (_minTime is float minT) {
            return _currTime > minT;
        }

        return true;
    }

    public bool Ended() {
        return _currTime > _maxTime;
    }
}

public class GenericFallingStrategy : MovementStrategy {
    // readonly float _maxFallingSpeed = 24;
    // readonly float _jumpSpeed = 18;
    // readonly float _maxJumpDuration = 0.4f;
    // readonly float _minJumpDuration = 0.1f;
    
    // float _remainingJumpDuration;
    bool _isJumping;
    readonly float? _initialFallProgression;
    readonly  SpeedTransformer _jumpTransformer;
    readonly SpeedTransformer _fallTransformer;

    public GenericFallingStrategy(Context ctx, MovementStrategyT type, SpeedTransformer fallTransformer, bool isJumping, SpeedTransformer jumpTransformer, float? initialFallProgression = null) : base(ctx, type) {
        _isJumping = isJumping;
        _jumpTransformer = jumpTransformer;
        _fallTransformer = fallTransformer;
        _initialFallProgression = initialFallProgression;
    }
    
    public override Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput,
        bool jumpContInput, float deltaTime) {
        var nearWall = IsNearWall();
        if (nearWall != 0 && !_isJumping) {
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

        var constants = _ctx.movementConstants;

        float yVelocity;
        if (_isJumping) {
            if (_jumpTransformer.Ended() || _jumpTransformer.CanEnd() && !jumpContInput || IsCeiled()) {
                _isJumping = false;
                if (_initialFallProgression is float progression) {
                    _fallTransformer.Progression = progression;
                } else {
                    _fallTransformer.Progression = Mathf.Max(1 - _jumpTransformer.Progression, 0);
                }
                
                yVelocity = _fallTransformer.Transform(deltaTime, constants);
            } else {
                yVelocity = _jumpTransformer.Transform(deltaTime, constants);
            }
        } else {
            yVelocity = _fallTransformer.Transform(deltaTime, constants);
        }
        
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
