using System.Drawing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProSprite;

public class ProSprite : MonoBehaviour {
    RenderTexture renderTexture;

    Object[] shadersAsAssets;
    [HideInInspector] public ComputeShader[] shaders;
    int[] kernels;

    ComputeBuffer palleteBuffer;

    Vector2Int chunkCount;
    const int pixelsPerUnit = 16;

    public enum S { Circles, Stroke, Curve, Affine, Texture, Clear }

    public void Setup(int width, int height) {
        SetChunkCount(width, height);
        SpriteRenderer renderer = GenerateSpriteRenderer(width, height);
        renderTexture = GenerateRenderTexture(width, height, renderer);
        CreatePalleteBuffer();
        InstantiateAndOrganizeComputeShaders();
    }

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
        shaders[(int)S.Circles].Dispatch(kernels[(int)S.Circles], chunkCount.x, chunkCount.y, 1);
    }

    protected void Stroke(int color) {
        shaders[(int)S.Stroke].SetInt("color", color);
        shaders[(int)S.Stroke].Dispatch(kernels[(int)S.Stroke], chunkCount.x, chunkCount.y, 1);
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
        shaders[(int)S.Curve].Dispatch(kernels[(int)S.Curve], chunkCount.x, chunkCount.y, 1);
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
        shaders[(int)S.Affine].Dispatch(kernels[(int)S.Affine], chunkCount.x, chunkCount.y, 1);
    }

    protected void DrawTexture(Texture2D texture) {
        shaders[(int)S.Texture].SetTexture(kernels[(int)S.Texture], "Tex", texture);
        shaders[(int)S.Texture].Dispatch(kernels[(int)S.Texture], chunkCount.x, chunkCount.y, 1);
    }

    protected void Clear() {
        shaders[(int)S.Clear].Dispatch(kernels[(int)S.Clear], chunkCount.x, chunkCount.y, 1);
    }

    protected void Rotate(float angleInRadians, int anchorPointX, int anchorPointY) {
        float generalScale = Mathf.Cos(angleInRadians);
        float generalSheer = Mathf.Sin(angleInRadians);

        Transform(0, 0, anchorPointX, anchorPointY, generalScale, generalSheer, - generalSheer, generalScale);
    }

    private void OnDisable() {
        palleteBuffer.Dispose();
    }

    protected struct Circle {
        public int x;
        public int y;
        public int radius;
        public int color;

        public Circle(int _x, int _y, int _radius, int _color) {
            x = _x;
            y = _y;
            radius = _radius;
            color = _color;
        }

        public static implicit operator Vector4(Circle circle) => 
            new Vector4 (circle.x, circle.y, circle.radius, circle.color);
    }

    private static int[] circleArrayToInt4Array(Circle[] circleArray) {
        int[] intArray = new int[circleArray.Length * 4];

        for (int i = 0; i < circleArray.Length; i++) {
            int intArrayLocation = i * 4;

            intArray[intArrayLocation]     = circleArray[i].x;
            intArray[intArrayLocation + 1] = circleArray[i].y;
            intArray[intArrayLocation + 2] = circleArray[i].radius;
            intArray[intArrayLocation + 3] = circleArray[i].color;
        }

        return intArray;
    }
}