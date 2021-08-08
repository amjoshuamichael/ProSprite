using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSCurve : ProSprite {
    float timeInSeconds = 0;
    const int width = 32;
    const int height = 32;

    void Start() {
        Setup(width, height);
    }

    override protected void Render() {
        timeInSeconds += Time.deltaTime;
        DrawLine(Mathf.Sin(timeInSeconds), 0, height / 2, 3, 0, width, 0, height);
        Stroke(4);
    }
}
