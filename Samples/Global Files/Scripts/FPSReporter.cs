using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSReporter : MonoBehaviour
{
    Text text;

    public const int reportInterval = 100;
    int reportTimer;

    private void Start() {
        text = GetComponent<Text>();
        Camera.onPreRender += IncrementReportInterval;
    }

    private void IncrementReportInterval(Camera cam) {
        reportTimer++;

        if (reportTimer >= reportInterval) {
            reportTimer = 0;
            Report();
        }
    }

    private void Report() {
        if (text != null)
            text.text = frameRate().ToString() + " fps";
    }

    int frameRate() {
        return (int)(1.0f / Time.deltaTime);
    }
}
