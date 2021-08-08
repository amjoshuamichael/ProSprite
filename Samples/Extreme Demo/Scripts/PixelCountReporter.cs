using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixelCountReporter : MonoBehaviour {
    Text text;

    const int resolution = 2000 * 1000;
    public PSExtreme PSExtreme;
    public FPSReporter FPSReporter;

    private void Start() {
        text = GetComponent<Text>();
    }

    private void Update() {
        if (hasReportedFrameRateBeenInitialized())
            UpdateText();
    }

    private void UpdateText() {
        int PixelCount = PSExtreme.numberOfCircles * 2000000;

        text.text = "Looping through " + PixelCount.ToString() + " pixels per frame, checking against 8 circles each time." +
                    "that's " + (PixelCount / FPSReporter.reportedFrameRate).ToString() + " pixels per second.";
    }

    private bool hasReportedFrameRateBeenInitialized() {
        return FPSReporter.reportedFrameRate != 0;
    }
}
