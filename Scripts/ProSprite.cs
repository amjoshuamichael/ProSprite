using System;
using UnityEditor;

namespace UnityEngine.ProSprite {
    [DisallowMultipleComponent]
    [AddComponentMenu("2D Animation/ProSprite")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProSprite : MonoBehaviour {
        private RenderTexture renderTexture;
        private SpriteRenderer spriteRenderer;
        private ComputeBuffer palleteBuffer;

        private ComputeShader[] shaders;
        private int[] kernels;

        const string packagePath = "Packages/com.amjoshuamichael.prosprite/";

        private Vector2Int chunkCount;
        private Vector2Int _center;
        public Vector2Int center { get { return _center; } }
        const int pixelsPerUnit = 16;

        [SerializeField] public int width = 32;
        [SerializeField] public int height = 32;

        private enum S { Clear, Circles, Stroke, Curve, Affine, Texture }

        private void Reset() => GenerateSpriteRenderer(width, height);
        private void OnValidate() => GenerateSpriteRenderer(width, height);

        private void Start() {
            SetupSizeValues(width, height);

            spriteRenderer = GetOrAddRendererComponent();
            GenerateRenderTexture(width, height, spriteRenderer);

            CreatePalleteBuffer();
            InstantiateComputeShaders();
        }

        private void OnRenderObject() {
            Clear();
        }

        private void CreatePalleteBuffer() {
            palleteBuffer = new ComputeBuffer(Pallete.pallete.Length, sizeof(float) * 4);
            palleteBuffer.SetData(Pallete.pallete);
        }

        private void InstantiateComputeShaders() {
            Object[] shadersAsAssets = GetComputeShadersAsAssets();
            shaders = new ComputeShader[shadersAsAssets.Length];
            kernels = new int[shadersAsAssets.Length];

            for (int i = 0; i < shaders.Length; i++)
                InstantiateComputeShader(shadersAsAssets, i);
        }

        private static Object[] GetComputeShadersAsAssets() {
            string[] ShaderNames = Enum.GetNames(typeof(S));

            Object[] output = new Object[ShaderNames.Length];

            for (int i = 0; i < ShaderNames.Length; i++) {
                output[i] = AssetDatabase.LoadAssetAtPath($"{packagePath}Shaders/{i} {ShaderNames[i]}.compute", typeof(Object));
            }

            return output;
        }

        private void InstantiateComputeShader(Object[] shadersAsAssets, int index) {
            shaders[index] = (ComputeShader)shadersAsAssets[index];
            shaders[index] = Instantiate(shaders[index]);

            string kernelName = ((S)index).ToString();
            kernels[index] = shaders[index].FindKernel(kernelName);

            shaders[index].SetTexture(kernels[index], "Input", renderTexture);
            shaders[index].SetBuffer(kernels[index], "Pallete", palleteBuffer);
        }

        private void GenerateRenderTexture(int width, int height, SpriteRenderer spriteRenderer) {
            renderTexture = new RenderTexture(width, height, 1);
            renderTexture.enableRandomWrite = true;
            renderTexture.filterMode = FilterMode.Point;
            spriteRenderer.material.SetTexture("_Texture", renderTexture);
            RenderTexture.active = renderTexture;
        }

        private void GenerateSpriteRenderer(int width, int height) {
            spriteRenderer = GetOrAddRendererComponent();
            spriteRenderer.sprite = generateBlankSpriteAtSize(width, height, TextureFormat.Alpha8);
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(width / pixelsPerUnit, height / pixelsPerUnit);
            spriteRenderer.material = (Material)AssetDatabase.LoadAssetAtPath($"{packagePath}Materials/ProSprite.mat", typeof(Material));
        }

        private SpriteRenderer GetOrAddRendererComponent() {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<SpriteRenderer>();
            return renderer;
        }

        private Sprite generateBlankSpriteAtSize(int width, int height, TextureFormat textureFormat) {
            Texture2D texture = new Texture2D(width, height, textureFormat, true);
            Rect rect = new Rect(0, 0, width, height);
            Vector2 anchorPoint = new Vector2(0.5f, 0.5f);

            return Sprite.Create(texture, rect, anchorPoint, pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private void SetupSizeValues(int width, int height) {
            chunkCount = new Vector2Int(width / 32, height / 32);
            _center = new Vector2Int(width / 2, height / 2);
        }

        public void DrawCircles(Circle[] circles) {
            shaders[(int)S.Circles].SetInt("count", 8);
            shaders[(int)S.Circles].SetInts("circles", Circle.circleArrayToIntArray(circles));

            DispatchShader((int)S.Circles);
        }

        public void Stroke(int color) {
            shaders[(int)S.Stroke].SetInt("color", color);
            DispatchShader((int)S.Stroke);
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
            DispatchShader((int)S.Curve);
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
            DispatchShader((int)S.Affine);
        }

        public void Translate(Vector2 position) {
            Transform(position.x, position.y, 0, 0, 1, 0, 0, 1);
        }

        public void Rotate(Vector2 anchorPoint, float angleInRadians) {
            float generalScale = Mathf.Cos(angleInRadians);
            float generalSheer = Mathf.Sin(angleInRadians);

            Transform(0, 0, anchorPoint.x, anchorPoint.y, generalScale, generalSheer, -generalSheer, generalScale);
        }

        public void Scale(Vector2 anchorPoint, Vector2 scaleAmount) {
            Transform(0, 0, anchorPoint.x, anchorPoint.y, 1 / scaleAmount.x, 0, 0, 1 / scaleAmount.y);
        }

        public void Scale(Vector2 anchorPoint, float scaleAmount) {
            Transform(0, 0, anchorPoint.x, anchorPoint.y, scaleAmount, 0, 0, scaleAmount);
        }

        public void DrawTexture(Texture2D texture) {
            shaders[(int)S.Texture].SetTexture(kernels[(int)S.Texture], "Tex", texture);
            DispatchShader((int)S.Texture);
        }

        private void Clear() {
            DispatchShader((int)S.Clear);
        }

        private void DispatchShader(int index) {
            if (shaders[index] != null) shaders[index].Dispatch(kernels[index], chunkCount.x, chunkCount.y, 1);
        }

        private void OnDisable() {
            palleteBuffer.Dispose();
        }
    }
}