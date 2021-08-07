using System.Numerics;
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
            _pallete = PalleteSettingsProvider.palleteAsArray();
        }
    }
}


class PalleteSettings : ScriptableObject {
    public const string PalleteSettingsPath = "ProSprite/PalleteSettings";
    [SerializeField] public Color[] palleteColors;

    public static SerializedObject GetSerializedSettings() {
        Object settings = PalleteSettingsAsset();
        return new SerializedObject(settings);
    }

    public static void SavePalleteSettings(Color[] newPallete) {
        PalleteSettings settings = PalleteSettingsAsset();
        settings.palleteColors = newPallete;
    }

    public static PalleteSettings PalleteSettingsAsset() {
        return Resources.Load<PalleteSettings>(PalleteSettingsPath);
    }
}

class PalleteSettingsProvider : SettingsProvider {
    private static SerializedObject PalleteSettings;
    [SerializeField] private static SerializedProperty pallete;
    private Color[] oldPalleteSettings;

    public PalleteSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }

    [SettingsProvider]
    public static SettingsProvider CreatePalleteSettingsProvider() {
        var provider = new PalleteSettingsProvider("Project/ProSprite Pallete", SettingsScope.Project);
        provider.keywords = new string[] {"Color", "ProSprite", "Pallete"};
        return provider;
    }

    public override void OnActivate(string searchContext, VisualElement rootElement) {
        LoadPalleteSettingsAndPallete();
    }

    public override void OnDeactivate() {
        Color[] newPalleteSettings = palleteAsArray();
        CheckIfPalleteHasMoreThanOneTransparency(newPalleteSettings);
        global::PalleteSettings.SavePalleteSettings(newPalleteSettings);
        base.OnDeactivate();
    }

    private static void CheckIfPalleteHasMoreThanOneTransparency(Color[] pallete) {
        int numberOfTransparencies = 0;

        for (int i = 0; i < pallete.Length; i++)
            if (pallete[i].a == 0)
                numberOfTransparencies++;   

        if (numberOfTransparencies >= 2)
            Debug.LogWarning("In the ProSprite Pallete, there is more than one swatch that is completely clear. This is redundant. Did you mean to do this?");
    }

    public override void OnInspectorUpdate() {
        if (pallete.arraySize > 64) {
            pallete.arraySize = 64;
        }
    }

    public override void OnGUI(string searchContext) {
        EditorGUILayout.PropertyField(pallete, palleteGUIStyle);
    }

    private static GUIContent palleteGUIStyle { 
        get {
            return new GUIContent("Pallete");
        }
    }

    public static Color[] palleteAsArray() {
        if (pallete == null) LoadPalleteSettingsAndPallete();

        Color[] palleteAsArray = new Color[pallete.arraySize];

        for (int i = 0; i < pallete.arraySize; i++)
            palleteAsArray[i] = pallete.GetArrayElementAtIndex(i).colorValue;

        return palleteAsArray;
    }

    private static void LoadPalleteSettingsAndPallete() {
        PalleteSettings = global::PalleteSettings.GetSerializedSettings();
        pallete = PalleteSettings.FindProperty("palleteColors");
    }
}