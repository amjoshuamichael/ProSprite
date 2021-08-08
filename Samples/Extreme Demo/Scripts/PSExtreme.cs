using UnityEngine;

public class PSExtreme : ProSprite {
    const int width = 2000;
    const int height = 1000;

    const int circleRadius = 50;

    public int numberOfCircles;

    void Start() {
        Setup(width, height);
    }

    protected override void Render() {
        int numberOfBatches = numberOfCircles / 8;

        for (int i = 0; i < numberOfBatches; i++) {
            Circle[] circles = new Circle[8];

            for (int j = 0; j < 8; j++)
                circles[j] = GenerateBigCircle();

            DrawCircles(circles);
        }
    }

    private Circle GenerateBigCircle() {
        Vector2Int position = new Vector2Int();
        position.x = (int)(Random.value * 4000 - 2000);
        position.y = (int)(Random.value * 2000 - 1000);

        return new Circle(position, circleRadius, 4);
    }
}
