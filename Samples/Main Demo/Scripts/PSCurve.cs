using UnityEngine;
using UnityEngine.ProSprite;

public class PSCurve : MonoBehaviour {
    [SerializeField] private ProSprite proSprite;

    float timeInSeconds = 0;

    private void OnWillRenderObject() {
        timeInSeconds += Time.deltaTime;
        proSprite.DrawLine(Mathf.Sin(timeInSeconds), 0, proSprite.height / 2, 3, 0, proSprite.width, 0, proSprite.height);
        proSprite.Stroke(4);
    }
}
