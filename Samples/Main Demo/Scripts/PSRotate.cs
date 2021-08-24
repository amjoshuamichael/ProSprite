using UnityEngine;
using UnityEngine.ProSprite;

public class PSRotate : MonoBehaviour {
    [SerializeField] private ProSprite proSprite;

    float timeInSeconds = 0;

    void OnWillRenderObject() {
        timeInSeconds += Time.deltaTime;

        proSprite.DrawCircles(new Circle[] { new Circle( proSprite.center, 64, 2 ) });

        proSprite.Scale(proSprite.center, new Vector2(1.5f, 0.5f));
        proSprite.Rotate(proSprite.center, timeInSeconds);

        proSprite.Stroke(4);
    }
}
