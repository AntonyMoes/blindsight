using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandingStrategy : MovementStrategy {
    // readonly float _jumpForce = 25;
    
    public StandingStrategy(Context ctx) : base(ctx, MovementStrategyT.Standing) {}

    public override Func<Context, MovementStrategy> Move(int hInput, int vInput, bool jumpStartInput,
        bool jumpContInput, float deltaTime) {
        if (!IsGrounded()) {
            return ctx => new FallingStrategy(ctx);
        }
        
        if (jumpStartInput) {
            if (_ctx.rb.velocity.y < 0) {
                _ctx.rb.velocity = new Vector2(_ctx.rb.velocity.x, 0);
            }

            return ctx => new FallingStrategy(ctx, true);
        }

        // TODO: do something about not being able to jump when running into wall
        var hVelocity = hInput * _ctx.baseSpeed;
        _ctx.rb.velocity = new Vector2(hVelocity, _ctx.rb.velocity.y);
        return null;
    }
}
