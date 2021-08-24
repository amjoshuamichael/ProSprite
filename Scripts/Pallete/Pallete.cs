using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.ProSprite {
    public class Pallete {
        static Color[] _pallete;
        static Vector4[] _palleteAsVectorArray;

        public static Color[] pallete {
            get {
                InstantiateIfNull();
                return _pallete;
            }
        }

        public static Vector4[] palleteAsVectorArray {
            get {
                InstantiateIfNull();
                return _palleteAsVectorArray;
            }
        }

        private static void InstantiateIfNull() {
            if (_pallete == null) Instantiate();
        }

        private static void Instantiate() {
            InstantiatePallete();
            InstantiatePalleteAsVectorArray();
        }

        private static void InstantiatePalleteAsVectorArray() {
            _palleteAsVectorArray = new Vector4[_pallete.Length];
            for (int i = 0; i < _pallete.Length; i++) {
                _palleteAsVectorArray[i] = _pallete[i];
            }
        }

        private static void InstantiatePallete() {
            _pallete = PalleteSettings.LoadPallete();
        }
    }
}

static class PalleteSettings {
    const string palletePath = "ProSprite/Scripts/Pallete/pallete";

#if UNITY_EDITOR
    public static void SavePallete(SerializedProperty newPalleteProperty) {
        Texture2D palleteAtlas = GetTextureAtlas();
        palleteAtlas.Resize(newPalleteProperty.arraySize, 1);

        Color[] palleteColors = SerializedPropertyToColorArray(newPalleteProperty);

        palleteAtlas.SetPixels(palleteColors);
        palleteAtlas.Apply();

        SaveTextureToPalletePNG(palleteAtlas);
    }

    private static Color[] SerializedPropertyToColorArray(SerializedProperty newPalleteProperty) {
        Color[] palleteColors = new Color[newPalleteProperty.arraySize];

        for (int i = 0; i < newPalleteProperty.arraySize; i++)
            palleteColors[i] = newPalleteProperty.GetArrayElementAtIndex(i).colorValue;

        return palleteColors;
    }

    private static void SaveTextureToPalletePNG(Texture2D texture) {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(GetFullPalleteAtlasPath(), bytes);
    }

    private static string GetFullPalleteAtlasPath() {
        return Application.dataPath + "/Resources/" + palletePath + ".png";
    }
#endif

    public static Color[] LoadPallete() {
        Texture2D palleteAtlas = GetTextureAtlas();
        Color[] palleteColors = palleteAtlas.GetPixels();

        return palleteColors;
    }

    private static Texture2D GetTextureAtlas() {
        Texture2D texture = new Texture2D(64, 1, TextureFormat.ARGB32, true);
        texture = (Texture2D)Resources.Load(palletePath);
        return texture;
    }
}

#if UNITY_EDITOR
class PalleteSettingsContainer : ScriptableObject {
    public Color[] pallete;
}

class PalleteSettingsProvider : SettingsProvider {
    private static PalleteSettingsContainer palleteContainer;
    private static SerializedProperty palleteProperty;

    public PalleteSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }

    [SettingsProvider]
    public static SettingsProvider CreatePalleteSettingsProvider() {
        var provider = new PalleteSettingsProvider("Project/ProSprite Pallete", SettingsScope.Project);
        provider.keywords = new string[] { "Color", "ProSprite", "Pallete" };
        return provider;
    }

    public override void OnActivate(string searchContext, VisualElement rootElement) {
        LoadPalleteContainer();
    }

    public override void OnDeactivate() {
        if(palleteContainer != null) {
            WarnIfPalleteHasMoreThanOneTransparency();
            PalleteSettings.SavePallete(palleteProperty);
            base.OnDeactivate();
        }
    }

    private static void WarnIfPalleteHasMoreThanOneTransparency() {
        int numberOfTransparencies = 0;

        for (int i = 0; i < palleteContainer.pallete.Length; i++)
            if (palleteContainer.pallete[i].a == 0)
                numberOfTransparencies++;

        if (numberOfTransparencies >= 2)
            Debug.LogWarning("In the ProSprite Pallete, there is more than one swatch that is completely clear. This is redundant. Did you mean to do this?");
    }

    public override void OnInspectorUpdate() {
        
    }

    public override void OnGUI(string searchContext) {
        EditorGUILayout.PropertyField(palleteProperty, palleteGUIStyle());
    }

    private static GUIContent palleteGUIStyle() {
        return new GUIContent("Pallete");
    }

    private static void LoadPalleteContainer() {
        palleteContainer = ScriptableObject.CreateInstance<PalleteSettingsContainer>();
        palleteContainer.pallete = PalleteSettings.LoadPallete();

        palleteProperty = new SerializedObject(palleteContainer).FindProperty("pallete");
    }
}
#endif