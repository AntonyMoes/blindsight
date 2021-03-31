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
    
    public static readonly SpeedFunc JumpFunc = (t, constants) => {
        var js = constants.JumpFallSpeed;
        if (t < 0.6) {
            return js;
        }
        if (t <= 1) {
            return -2.5f * js * t + 2.5f * js;
        }
        return 0;
    };

    public static readonly SpeedFunc FallFunc = (t, constants) => {
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