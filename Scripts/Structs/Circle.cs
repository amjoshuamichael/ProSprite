namespace UnityEngine.ProSprite {
    public class Circle {
        public Vector2Int position;
        public int radius;
        public int color;

        public Circle(Vector2Int _position, int _radius, int _color) {
            position = _position;
            radius = _radius;
            color = _color;
        }

        public static implicit operator Vector4(Circle circle) =>
            new Vector4(circle.position.x, circle.position.y, circle.radius, circle.color);

        // Because of the way commuication between hlsl and c# works, in order to modify an array of int4s
        // in the Circles shader, we don't pass in an array of Vector4s to the ComputeShader.SetInts function.
        // Intead, we input an array four times the size of the corresponding hlsl array, where each int4
        // corresponds to a set of four consecutive integers.
        public static int[] circleArrayToIntArray(Circle[] circleArray) {
            int[] intArray = new int[circleArray.Length * 4];

            for (int i = 0; i < circleArray.Length; i++) {
                int intArrayLocation = i * 4;

                intArray[intArrayLocation] = circleArray[i].position.x;
                intArray[intArrayLocation + 1] = circleArray[i].position.y;
                intArray[intArrayLocation + 2] = circleArray[i].radius;
                intArray[intArrayLocation + 3] = circleArray[i].color;
            }

            return intArray;
        }
    }
}