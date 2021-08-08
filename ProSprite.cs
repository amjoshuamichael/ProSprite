using UnityEngine;
using UnityEngine.ProSprite;

public abstract class ProSprite : MonoBehaviour {
    RenderTexture renderTexture;

    Object[] shadersAsAssets;
    [HideInInspector] public ComputeShader[] shaders;
    int[] kernels;

    ComputeBuffer palleteBuffer;

    Vector2Int chunkCount;
    const int pixelsPerUnit = 16;
    protected Vector2Int center;

    public enum S { Clear, Circles, Stroke, Curve, Affine, Texture }

    public void Setup(int width, int height) {
        SetChunkCount(width, height);
        center = new Vector2Int(width / 2, height / 2);

        SpriteRenderer renderer = GenerateSpriteRenderer(width, height);
        renderTexture = GenerateRenderTexture(width, height, renderer);

        CreatePalleteBuffer();
        InstantiateAndOrganizeComputeShaders();

        Camera.onPreRender += CallRender;
    }

    private void CallRender(Camera camera) {
        if(camera == Camera.main) {
            Clear();
            Render();
        }
    }

    protected abstract void Render();

    private void CreatePalleteBuffer() {
        palleteBuffer = new ComputeBuffer(Pallete.pallete.Length, sizeof(float) * 4);
        palleteBuffer.SetData(Pallete.pallete);
    }

    private void InstantiateAndOrganizeComputeShaders() {
        shadersAsAssets = Resources.LoadAll("ProSprite/Shaders", typeof(ComputeShader));
        shaders = new ComputeShader[shadersAsAssets.Length];
        kernels = new int[shadersAsAssets.Length];

        for (int i = 0; i < shaders.Length; i++) {
            shaders[i] = (ComputeShader)shadersAsAssets[i];
            shaders[i] = Instantiate(shaders[i]);
            string kernelName = ((S)i).ToString();
            kernels[i] = shaders[i].FindKernel(kernelName);
            shaders[i].SetTexture(kernels[i], "Input", renderTexture);
            shaders[i].SetBuffer(kernels[i], "Pallete", palleteBuffer);
        }
    }

    private RenderTexture GenerateRenderTexture(int width, int height, SpriteRenderer renderer) {
        RenderTexture renderTexture = new RenderTexture(width, height, 1);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderer.material.SetTexture("_Texture", renderTexture);
        RenderTexture.active = renderTexture;

        return renderTexture;
    }

    private SpriteRenderer GenerateSpriteRenderer(int width, int height) {
        SpriteRenderer renderer = gameObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        renderer.sprite = generateBlankSpriteAtSize(width, height);
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(width / pixelsPerUnit, height / pixelsPerUnit);
        renderer.material = Resources.Load<Material>("ProSprite/Texture");
        return renderer;
    }

    private Sprite generateBlankSpriteAtSize(int width, int height) {
        Texture2D texture = new Texture2D(width, height, TextureFormat.Alpha8, true);
        Rect rect = new Rect(0, 0, width, height);
        Vector2 anchorPoint = new Vector2(0.5f, 0.5f);
        return Sprite.Create(texture, rect, anchorPoint, pixelsPerUnit, 0, SpriteMeshType.FullRect);
    }

    private void SetChunkCount(int width, int height) {
        chunkCount = new Vector2Int(width / 32, height / 32);
    }

    protected void DrawCircles(Circle[] circles) {
        shaders[(int)S.Circles].SetInt("count", 8);
        shaders[(int)S.Circles].SetInts("circles", circleArrayToInt4Array(circles));
        DispatchShader((int)S.Circles);
    }

    protected void Stroke(int color) {
        shaders[(int)S.Stroke].SetInt("color", color);
        DispatchShader((int)S.Stroke);
    }

    protected void DrawLine(float a, float b, float c, int color, int xMin, int xMax, int yMin, int yMax) {
        shaders[(int)S.Curve].SetFloat("a", a);
        shaders[(int)S.Curve].SetFloat("b", b);
        shaders[(int)S.Curve].SetFloat("c", c);
        shaders[(int)S.Curve].SetVector("color", Pallete.pallete[color]);
        shaders[(int)S.Curve].SetInt("xMin", xMin);
        shaders[(int)S.Curve].SetInt("xMax", xMax);
        shaders[(int)S.Curve].SetInt("yMin", yMin);
        shaders[(int)S.Curve].SetInt("yMax", yMax);
        DispatchShader((int)S.Curve);
    }

    protected void Transform(float H, float V, float X, float Y, float A, float B, float C, float D) {
        shaders[(int)S.Affine].SetFloat("H", H);
        shaders[(int)S.Affine].SetFloat("V", V);
        shaders[(int)S.Affine].SetFloat("X", X);
        shaders[(int)S.Affine].SetFloat("Y", Y);
        shaders[(int)S.Affine].SetFloat("A", A);
        shaders[(int)S.Affine].SetFloat("B", B);
        shaders[(int)S.Affine].SetFloat("C", C);
        shaders[(int)S.Affine].SetFloat("D", D);
        DispatchShader((int)S.Affine);
    }

    protected void DrawTexture(Texture2D texture) {
        shaders[(int)S.Texture].SetTexture(kernels[(int)S.Texture], "Tex", texture);
        DispatchShader((int)S.Texture);
    }

    private void Clear() {
        DispatchShader((int)S.Clear);
    }

    protected void Translate(Vector2 position) {
        Transform(position.x, position.y, 0, 0, 1, 0, 0, 1);
    }

    protected void Rotate(Vector2 anchorPoint, float angleInRadians) {
        float generalScale = Mathf.Cos(angleInRadians);
        float generalSheer = Mathf.Sin(angleInRadians);

        Transform(0, 0, anchorPoint.x, anchorPoint.y, generalScale, generalSheer, -generalSheer, generalScale);
    }

    protected void Scale(Vector2 anchorPoint, Vector2 scaleAmount) {
        Transform(0, 0, anchorPoint.x, anchorPoint.y, 1 / scaleAmount.x, 0, 0, 1 / scaleAmount.y);
    }

    protected void Scale(Vector2 anchorPoint, float scaleAmount) {
        Transform(0, 0, anchorPoint.x, anchorPoint.y, scaleAmount, 0, 0, scaleAmount);
    }

    private void DispatchShader(int index) {
        shaders[index].Dispatch(kernels[index], chunkCount.x, chunkCount.y, 1);
    }

    private void OnDisable() {
        palleteBuffer.Dispose();
    }

    protected struct Circle {
        public Vector2Int position;
        public int radius;
        public int color;

        public Circle(Vector2Int _position, int _radius, int _color) {
            position = _position;
            radius = _radius;
            color = _color;
        }

        public static implicit operator Vector4(Circle circle) => 
            new Vector4 (circle.position.x, circle.position.y, circle.radius, circle.color);
    }

    // Because of the way commuication between hlsl and c# works, in order to modify an array of int4s
    // in the Circles shader, we don't pass in an array of Vector4s to the ComputeShader.SetInts function.
    // Intead, we input an array four times the size of the corresponding hlsl array, where each int4
    // corresponds to a set of four consecutive integers.
    private static int[] circleArrayToInt4Array(Circle[] circleArray) {
        int[] intArray = new int[circleArray.Length * 4];

        for (int i = 0; i < circleArray.Length; i++) {
            int intArrayLocation = i * 4;

            intArray[intArrayLocation]     = circleArray[i].position.x;
            intArray[intArrayLocation + 1] = circleArray[i].position.y;
            intArray[intArrayLocation + 2] = circleArray[i].radius;
            intArray[intArrayLocation + 3] = circleArray[i].color;
        }

        return intArray;
    }
}