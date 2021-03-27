using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MovementStrategyT {
    Standing,
    Falling,
    WallClinging,
    CliffHanging,
    WallJump,
}
public abstract class MovementStrategy {
    public class Context {
        public Context(Rigidbody2D rb, Collider2D collider, float baseSpeed, LayerMask platformMask, float minDistToWall) {
            this.rb = rb;
            this.collider = collider;
            this.baseSpeed = baseSpeed;
            this.platformMask = platformMask;
            this.minDistToWall = minDistToWall;
        }

        public readonly Rigidbody2D rb;
        public readonly Collider2D collider;
        public readonly float baseSpeed;
        public readonly LayerMask platformMask;
        public readonly float minDistToWall;
    }

    protected Context _ctx;

    protected const int Left = -1;
    protected const int Right = 1;
    protected const int Below = -1;
    protected const int Above = 1;

    public readonly MovementStrategyT type;
    
    // TODO: maybe place all these Physics-Hacks into one place
    protected readonly float _wallEpsX = 0.04f;
    
    public MovementStrategy(Context ctx, MovementStrategyT t) {
        _ctx = ctx;
        type = t;
    }

    /**
     * 1 - above, -1 - below
     */
    protected Collider2D WallAboveOrBelow(int direction) {
        var epsX = _ctx.minDistToWall;
        // TODO: this too
        var epsY = 0.06f;

        var bounds = _ctx.collider.bounds;
        var boxPosY = direction == Below ? bounds.min.y - epsY / 2 : bounds.max.y + epsY / 2;
        var boxPos = new Vector2(bounds.center.x, boxPosY);
        var boxSize = new Vector2(bounds.size.x - epsX * 2, epsY);
        var hit = Physics2D.OverlapBox(boxPos, boxSize, 0, _ctx.platformMask);
        
        var color = hit != null ? Color.green : Color.red;
        CustomDebug.DebugDrawBox(boxPos, boxSize, color);

        return hit;
    }

    protected bool IsGrounded() {
        return WallAboveOrBelow(Below);
    }
    protected bool IsCeiled() {
        return WallAboveOrBelow(Above);
    }
    
    protected int IsWalled() {
        var bounds = _ctx.collider.bounds;
        var boxSize = new Vector2(_wallEpsX, bounds.size.y * 0.05f);
        Func<int, Vector2[]> getBoxPositions = (direction) => {
            var colliderBoundX = direction == Left ? bounds.min.x : bounds.max.x;
            var boxPosX = colliderBoundX + direction * _wallEpsX / 2;
            var boxPosY1 = bounds.min.y + bounds.size.y * 0.225f;
            var boxPosY2 = bounds.min.y + bounds.size.y * 0.55f;
            var boxPosY3 = bounds.min.y + bounds.size.y * 0.875f;
            
            return new[] {
                new Vector2(boxPosX, boxPosY1),
                new Vector2(boxPosX, boxPosY2),
                new Vector2(boxPosX, boxPosY3)
            };
        };

        Func<int, bool> getHit = (direction) => {
            var resultHit = getBoxPositions(direction)
                .Select(pos => {
                    var hit = Physics2D.OverlapBox(pos, boxSize, 0, _ctx.platformMask);
                    var color = hit != null ? Color.green : Color.red;
                    CustomDebug.DebugDrawBox(pos, boxSize, color);
                    return hit;
                })
                .All(hit => hit != null);

            return resultHit;
        };

        if (getHit(Left)) {
            return Left;
        }
        if (getHit(Right)) {
            return Right;
        }
        
        return 0;
    }
    
    protected Collider2D GetWall(int direction) {
        var bounds = _ctx.collider.bounds;
        var boxPosY = bounds.min.y + bounds.size.y * 0.5f;
        var boxSize = new Vector2(_wallEpsX, bounds.size.y - 2 * _ctx.minDistToWall);
        
        Func<int, Vector2> getBoxPosition = (dir) => {
            var colliderBoundX = dir == Left ? bounds.min.x : bounds.max.x;
            var boxPosX = colliderBoundX + dir * _wallEpsX / 2;

            return new Vector2(boxPosX, boxPosY);
        };
        
        var hit = Physics2D.OverlapBox(getBoxPosition(direction), boxSize, 0, _ctx.platformMask);
        var color = hit ? Color.green : Color.red;
        CustomDebug.DebugDrawBox(getBoxPosition(direction), boxSize, color);
        return hit;
    }
    protected int IsNearWall() {
        if (GetWall(Left)) {
            return Left;
        }
        if (GetWall(Right)) {
            return Right;
        }
        
        return 0;
    }

    protected Collider2D GetCliff(int direction) {
        var potentialCliff = GetWall(direction);
        if (!potentialCliff) {
            return null;
        }

        // TODO: maybe tweak this (and walled check accordingly)
        var yEps = _ctx.collider.bounds.size.y * 0.2;

        var yDiff = potentialCliff.bounds.max.y - _ctx.collider.bounds.max.y;
        if (Mathf.Abs(yDiff) > yEps) {
            return null;
        }

        return potentialCliff;
    }

    protected void MoveToWall(int direction, Collider2D wall) {
        var wallX = direction == -1 ? wall.bounds.max.x : wall.bounds.min.x;
        var bodyX = direction == -1 ? _ctx.collider.bounds.min.x : _ctx.collider.bounds.max.x;

        var delta = wallX - bodyX - (direction * _ctx.minDistToWall);
        
        _ctx.rb.MovePosition(_ctx.rb.position + new Vector2(delta, 0));
    }

    protected void MoveToCliff(int direction, Collider2D cliff) {
        MoveToWall(direction, cliff);
        
        var delta = cliff.bounds.max.y - _ctx.collider.bounds.max.y;
        _ctx.rb.MovePosition(_ctx.rb.position + new Vector2(0, delta));
    }
    
    abstract public Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput, bool jumpContInput, float deltaTime);
    virtual public void OnStart() {}
    virtual public void OnFinish() {}
}
