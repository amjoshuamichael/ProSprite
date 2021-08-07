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

    void Update() {
        Clear();

        timeInSeconds += Time.deltaTime;

        DrawCircles(new Circle[] {
            new Circle(
                (int)(Mathf.Sin(timeInSeconds) * radiusOfCircleMovement) + width / 2,
                (int)(Mathf.Cos(timeInSeconds) * radiusOfCircleMovement) + height / 2,
                majorCircleRadius,
                0
            ),
            new Circle(
                (int)(Mathf.Sin(timeInSeconds - minorCircleDelay) * radiusOfCircleMovement) + width / 2,
                (int)(Mathf.Cos(timeInSeconds - minorCircleDelay) * radiusOfCircleMovement) + height / 2,
                minorCircleRadius,
                1
            )
        });

        Stroke(4);
    }
}
