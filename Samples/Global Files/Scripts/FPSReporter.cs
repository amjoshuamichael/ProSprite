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
        Camera.onPreRender += CheckToReport;
    }

    private void FixedUpdate() {
        reportTimer++;
    }

    private void CheckToReport(Camera cam) {
        if (reportTimer >= reportInterval) {
            reportTimer = 0;

            if (text != null)
                Report();
        }
    }

    private void Report() {
        reportedFrameRate = frameRate();
        text.text = reportedFrameRate.ToString() + " fps";
    }

    public static int frameRate() {
        return (int)(1.0f / Time.deltaTime);
    }

    public int reportedFrameRate;
}
