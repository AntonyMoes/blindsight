using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MovementSystem : MonoBehaviour {
    [SerializeField] float moveSpeed = 12;
    // TODO: are you sure this wariable should be placed here? Cause I am not
    [SerializeField] float minDistToWall = 0.022f;
    
    Rigidbody2D _rb;
    BoxCollider2D _collider;
    [SerializeField] LayerMask platformMask;
    
    MovementStrategy.Context _ctx;
    MovementStrategy _currentStrategy;
    
    [Serializable]
    struct ActionPair {
        public MovementStrategyT type;
        public UnityEvent actions;
    }
    [Serializable]
    struct ActionTriple {
        public MovementStrategyT type1;
        public MovementStrategyT type2;
        public UnityEvent actions;
    }
    
    [SerializeField] List<ActionPair> onStarts;
    [SerializeField] List<ActionPair> onEnds;
    [SerializeField] List<ActionTriple> onTransitions;
    
    Dictionary<MovementStrategyT, UnityEvent> _onStrategyStart = new Dictionary<MovementStrategyT, UnityEvent>();
    Dictionary<MovementStrategyT, UnityEvent> _onStrategyEnd = new Dictionary<MovementStrategyT, UnityEvent>();
    Dictionary<(MovementStrategyT, MovementStrategyT), UnityEvent> _onStrategyTransition = new Dictionary<(MovementStrategyT, MovementStrategyT), UnityEvent>();

    Collider2D[] _overlapBoxResults;
    const int MaxColliders = 10;

    void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _ctx = new MovementStrategy.Context(_rb, _collider, moveSpeed, platformMask, minDistToWall);
        _overlapBoxResults = new Collider2D[MaxColliders];
        InitializeCallbacks();
        SetStrategy(context => new StandingStrategy(context));
    }

    void InitializeCallbacks() {
        foreach (var actionPair in onStarts) {
            _onStrategyStart.Add(actionPair.type, actionPair.actions);
        }
        foreach (var actionPair in onEnds) {
            _onStrategyEnd.Add(actionPair.type, actionPair.actions);
        }
        foreach (var actionTriple in onTransitions) {
            _onStrategyTransition.Add((actionTriple.type1, actionTriple.type2), actionTriple.actions);
        }
    }

    void SetStrategy(Func<MovementStrategy.Context, MovementStrategy> inst) {
        if (_currentStrategy != null) {
            _currentStrategy.OnFinish();
            _onStrategyEnd.GetValueOrDefault(_currentStrategy.type)?.Invoke();
        }

        var newStrategy = inst(_ctx);
        // Debug.Log("Changed strategy " + _currentStrategy?.GetType() + " -> " + newStrategy.GetType());
        if (_currentStrategy != null) {
            _onStrategyTransition.GetValueOrDefault((_currentStrategy.type, newStrategy.type))?.Invoke();
        }
        
        _currentStrategy = newStrategy;
        _currentStrategy.OnStart();
        _onStrategyStart.GetValueOrDefault(newStrategy.type)?.Invoke();
    }

    public void Move(int hInput, int vInput, bool jumpStartInput, bool jumpContInput, float deltaTime) {
        Func<Func<MovementStrategy.Context, MovementStrategy>> move = 
            () => _currentStrategy.Move(hInput, vInput, jumpStartInput, jumpContInput, deltaTime);

        var maxStrategyChanges = 5;
        Func<MovementStrategy.Context, MovementStrategy> instNewStrategy;
        if ((instNewStrategy = move()) != null) {
            if (maxStrategyChanges-- == 0) {
                throw new ApplicationException("Too many strategy changes");
            }
            
            SetStrategy(instNewStrategy);
        }
        
        if (_ctx.rb.velocity != Vector2.zero) {
            PreventCollisions(deltaTime);
        }
    }

    void PreventCollisions(float deltaTime) {
        var movement = _ctx.rb.velocity * deltaTime;
        var bounds = _ctx.collider.bounds;
        var size = Physics2D.OverlapBoxNonAlloc((Vector2) bounds.center + movement, bounds.size, 0, _overlapBoxResults, _ctx.platformMask);

        const int X = 0;
        const int Y = 1;
        
        /*
         * This function returns a scale factor for velocity
         * which is needed to not collide with the nearest object on the specified axis.
         * E.g.: 0.4 if player will encounter returned collider after travelling movement * 0.4
         */
        Func<int, float> getMinimalScale = (axis) => {
            Func<Bounds, Vector2> maxBoundGetter = (b) => b.max;
            Func<Bounds, Vector2> minBoundGetter = (b) => b.min;
            Func<Vector2, float> xGetter = (v) => v.x;
            Func<Vector2, float> yGetter = (v) => v.y;
            var axisGetter = axis == X ? xGetter : yGetter;
            var axisMovement = axisGetter(movement);
            float selfPos = 
                Mathf.Sign(axisMovement) == 1 ? axisGetter(maxBoundGetter(bounds)) : axisGetter(minBoundGetter(bounds));
            Func<Bounds, Vector2> otherBoundGetter = 
                Mathf.Sign(axisMovement) == 1 ? minBoundGetter : maxBoundGetter;
            
            float minimalScale = 1;
            for (int i = 0; i < size; i++) {
                var scale = Mathf.Abs((selfPos - axisGetter(otherBoundGetter(_overlapBoxResults[i].bounds))) / axisMovement);
                if (scale < minimalScale) {
                    minimalScale = scale;
                }
            }

            return minimalScale;
        };

        var finalScale = Mathf.Min(getMinimalScale(X), getMinimalScale(Y));
        _ctx.rb.velocity *= finalScale;
        // TODO: Maybe ignore movement if its magnitude is less then 0.01
    }
}
