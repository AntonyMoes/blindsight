using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MovementSystem : MonoBehaviour {
    MovementStrategy.Context _ctx;
    MovementStrategy _currentStrategy;
    Collider2D[] _overlapBoxResults;
    const int MaxColliders = 10;

    void Start() {
        _overlapBoxResults = new Collider2D[MaxColliders];
    }

    public void SetStrategy(Func<MovementStrategy.Context, MovementStrategy> inst) {
        _currentStrategy?.OnFinish();
        var newStrategy = inst(_ctx);
        Debug.Log("Changed strategy " + _currentStrategy?.GetType() + " -> " + newStrategy.GetType());
        _currentStrategy = newStrategy;
        _currentStrategy.OnStart();
    }

    public void SetContext(MovementStrategy.Context ctx) {
        _ctx = ctx;
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
        
        // if (!Mathf.Approximately(finalScale, 1)) {
        //     Debug.Log("Using scale to prevent collision: " + finalScale + "\nMovement magnitude: " + (_ctx.rb.velocity * deltaTime).magnitude);
        // }
    }
}
