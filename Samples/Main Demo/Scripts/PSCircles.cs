using UnityEngine;
using UnityEngine.ProSprite;

public class PSCircles : MonoBehaviour {
    [SerializeField] private ProSprite proSprite;

    float timeInSeconds = 0;
    const int radiusOfCircleMovement = 8;
    const int majorCircleRadius = 42;
    const int minorCircleRadius = 25;
    const float minorCircleDelay = 0.5f;

    private void OnWillRenderObject() {
        timeInSeconds += Time.deltaTime;
        proSprite.DrawCircles(new Circle[] { majorCircle(), minorCircle() });
        proSprite.Stroke(4);
    }

    private Circle majorCircle() {
        Vector2Int position = new Vector2Int();
        position.x = (int)(Mathf.Sin(timeInSeconds) * radiusOfCircleMovement) + proSprite.center.x;
        position.y = (int)(Mathf.Cos(timeInSeconds) * radiusOfCircleMovement) + proSprite.center.y;

        return new Circle(position, majorCircleRadius, 0);
    }

    private Circle minorCircle() {
        Vector2Int position = new Vector2Int();
        position.x = (int)(Mathf.Sin(timeInSeconds - minorCircleDelay) * radiusOfCircleMovement) + proSprite.center.x;
        position.y = (int)(Mathf.Cos(timeInSeconds - minorCircleDelay) * radiusOfCircleMovement) + proSprite.center.y;

        return new Circle(position, minorCircleRadius, 1);
    }
}
