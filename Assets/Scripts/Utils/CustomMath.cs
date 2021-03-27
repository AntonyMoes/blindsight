using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomMath {
    /**
     * Use angle in degrees
     */
    public static Vector2 Vector2FromAngle(float angle)
    {
        var angleRad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static float RoundToAbs(float value, float max) {
        max = Mathf.Abs(max);
        return Mathf.Sign(value) * Mathf.Min(Mathf.Abs(value), max);
    }

    public static float GetNewVelocity(float velocity, float acceleration, int accelerationInput, bool decelerateWhenIdle, float? optMaxVelocity) {
        if (Mathf.Approximately(velocity, 0) && accelerationInput == 0) {
            return 0;
        }

        float newVelocity;

        if (accelerationInput == 0 && decelerateWhenIdle) {
            newVelocity =  velocity + acceleration * -Mathf.Sign(velocity);
            if (Mathf.Sign(velocity) != Mathf.Sign(newVelocity)) {
                newVelocity = 0;
            }

            return newVelocity;
        }

        newVelocity = velocity + acceleration * accelerationInput;
        if (optMaxVelocity is float maxVelocity) {
            return RoundToAbs(newVelocity, maxVelocity);
        }

        return newVelocity;
    }
}
