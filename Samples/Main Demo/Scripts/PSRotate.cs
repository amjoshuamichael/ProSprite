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

    override protected void Render() {
        timeInSeconds += Time.deltaTime;

        DrawCircles(new Circle[] { new Circle( center, 64, 2 ) });

        Scale(center, new Vector2(1.5f, 0.5f));
        Rotate(center, timeInSeconds);

        Stroke(4);
    }
}
