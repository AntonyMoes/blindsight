using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomDebug {
    public static void DebugDrawBox(Vector2 position, Vector2 size, Color color) {
        var lineStart = new Vector2(position.x - size.x / 2, position.y - size.y / 2);
        var lineEnd = new Vector2(position.x + size.x / 2, position.y - size.y / 2);
        var lineStart1 = new Vector2(position.x - size.x / 2, position.y + size.y / 2);
        var lineEnd1 = new Vector2(position.x + size.x / 2, position.y + size.y / 2);
        Debug.DrawLine(lineStart, lineEnd, color);
        Debug.DrawLine(lineStart1, lineEnd1, color);
        Debug.DrawLine(lineStart, lineStart1, color);
        Debug.DrawLine(lineEnd, lineEnd1, color);
    }
}
