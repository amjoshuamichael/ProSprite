using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProSprite;

public class ProSprite : MonoBehaviour {
    RenderTexture renderTexture;

    Object[] shadersObj;
    [HideInInspector] public ComputeShader[] shaders;
    int[] kernels;

    Vector2Int chunkCount;
    const int pixelsPerUnit = 16;

    public enum S { Circles, Stroke, Curve, Affine, Texture, Clear }

    public void Setup(int width, int height) {
        // ChunkCount & Colors Setup
        chunkCount = new Vector2Int(width / 32, height / 32);

        // SpriteRenderer Setup
        SpriteRenderer renderer = gameObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        renderer.sprite = Sprite.Create(new Texture2D(width, height), new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16.0f);
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(width / pixelsPerUnit, height / pixelsPerUnit);
        renderer.material = Resources.Load<Material>("ProSprite/Texture");

        // RenderTexture Setup
        renderTexture = new RenderTexture(width, height, 1);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderer.material.SetTexture("_OtherTex", renderTexture);
        RenderTexture.active = renderTexture;

        // Shaders Setup
        shadersObj = Resources.LoadAll("ProSprite/Shaders", typeof(ComputeShader));
        shaders = new ComputeShader[shadersObj.Length];
        kernels = new int[shadersObj.Length];

        for (int i = 0; i < shaders.Length; i++) {
            shaders[i] = (ComputeShader)shadersObj[i];
            shaders[i] = Instantiate(shaders[i]);
            kernels[i] = shaders[i].FindKernel( ( (S)i ).ToString() );
            shaders[i].SetTexture(kernels[i], "Input", renderTexture);
        }

        Vector4[] palleteAsVectors = new Vector4[Pallete.pallete.Length];
        for (int i = 0; i < Pallete.pallete.Length; i++) {
            palleteAsVectors[i] = Pallete.pallete[i];
        }

        shaders[(int)S.Circles].SetVectorArray("colors", palleteAsVectors);
    }

    public void DrawCircles(Vector4[] circles) {
        shaders[(int)S.Circles].SetInt("count", 8);
        shaders[(int)S.Circles].SetVectorArray("circles", circles);
        shaders[(int)S.Circles].Dispatch(kernels[(int)S.Circles], chunkCount.x, chunkCount.y, 1);
    }

    public void Stroke(int color) {
        shaders[(int)S.Stroke].SetVector("color", Pallete.pallete[color]);
        shaders[(int)S.Stroke].Dispatch(kernels[(int)S.Stroke], chunkCount.x, chunkCount.y, 1);
    }

    public void DrawLine(float a, float b, float c, int color, int xMin, int xMax, int yMin, int yMax) {
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

    public void Transform(float H, float V, float X, float Y, float A, float B, float C, float D) {
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

    public void DrawTexture(Texture2D texture) {
        shaders[(int)S.Texture].SetTexture(kernels[(int)S.Texture], "Tex", texture);
        shaders[(int)S.Texture].Dispatch(kernels[(int)S.Texture], chunkCount.x, chunkCount.y, 1);
    }

    public void Clear() {
        shaders[(int)S.Clear].Dispatch(kernels[(int)S.Clear], chunkCount.x, chunkCount.y, 1);
    }

    public void Rotate(float angleInRadians, int anchorPointX, int anchorPointY) {
        float generalScale = Mathf.Cos(angleInRadians);
        float generalSheer = Mathf.Sin(angleInRadians);

        Transform(0, 0, anchorPointX, anchorPointY, generalScale, generalSheer, - generalSheer, generalScale);
    }
}
