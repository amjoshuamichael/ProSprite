using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSRotate : ProSprite
{
    float timeInSeconds = 0;
    const int width = 32;
    const int height = 32;

    void Start() {
        Setup(width, height);
    }

    void Update() {
        Clear();

        timeInSeconds += Time.deltaTime;

        DrawCircles(new Circle[] {
            new Circle( width / 2, height / 2, 10, 2 )
        });

        Transform(0, 0, width / 2, 0, 0.5f, 0, 0, 1f);
        Rotate(timeInSeconds, width / 2, height / 2);

        Stroke(4);
    }
}
