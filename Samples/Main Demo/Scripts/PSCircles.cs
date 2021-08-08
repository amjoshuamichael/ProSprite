using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSCircles : ProSprite
{
    float timeInSeconds = 0;
    const int width = 32;
    const int height = 32;
    const int radiusOfCircleMovement = 8;
    const int majorCircleRadius = 42;
    const int minorCircleRadius = 25;
    const float minorCircleDelay = 0.5f;

    void Start() {
        Setup(width, height);
    }

    override protected void Render() {
        timeInSeconds += Time.deltaTime;

        DrawCircles(new Circle[] { majorCircle(), minorCircle() });

        Stroke(4);
    }

    private Circle majorCircle() {
        Vector2Int position = new Vector2Int();
        position.x = (int)(Mathf.Sin(timeInSeconds) * radiusOfCircleMovement) + center.x;
        position.y = (int)(Mathf.Cos(timeInSeconds) * radiusOfCircleMovement) + center.y;

        return new Circle(position, majorCircleRadius, 0);
    }

    private Circle minorCircle() {
        Vector2Int position = new Vector2Int();
        position.x = (int)(Mathf.Sin(timeInSeconds - minorCircleDelay) * radiusOfCircleMovement) + center.x;
        position.y = (int)(Mathf.Cos(timeInSeconds - minorCircleDelay) * radiusOfCircleMovement) + center.y;

        return new Circle(position, minorCircleRadius, 1);
    }
}
