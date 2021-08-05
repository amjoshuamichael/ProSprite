using System.Numerics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.ProSprite {
    public class Pallete {
        static Color[] _pallete;

        public static Color[] pallete {
            get {
                if (_pallete == null) {
                    _pallete = PalleteSettingsProvider.palleteAsArray();
                }

                return _pallete;
            }
        }
    }
}


class PalleteSettings : ScriptableObject {
    public const string PalleteSettingsPath = "Assets/Resources/ProSprite/PalleteSettings.asset";
    [SerializeField] public Color[] palleteColors;

    public static SerializedObject GetSerializedSettings() {
        Object settings = PalleteSettingsAsset();
        Debug.Log(settings);
        return new SerializedObject(settings);
    }

    public static void SavePalleteSettings(Color[] newPallete) {
        PalleteSettings settings = PalleteSettingsAsset();
        settings.palleteColors = newPallete;
    }

    public static PalleteSettings PalleteSettingsAsset() {
        return AssetDatabase.LoadAssetAtPath<PalleteSettings>(PalleteSettingsPath);
    }
}

class PalleteSettingsProvider : SettingsProvider {
    private static SerializedObject PalleteSettings;
    [SerializeField] private static SerializedProperty pallete;

    [SettingsProvider]
    public static SettingsProvider CreatePalleteSettingsProvider() {
        var provider = new PalleteSettingsProvider("Project/ProSprite Pallete", SettingsScope.Project);

        provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
        return provider;
    }

    public override void OnActivate(string searchContext, VisualElement rootElement) {
        LoadPalleteSettingsAndPallete();
    }

    public override void OnDeactivate() {
        global::PalleteSettings.SavePalleteSettings(palleteAsArray());
        base.OnDeactivate();
    }

    public override void OnInspectorUpdate() {
        if (pallete.arraySize > 64) {
            pallete.arraySize = 64;
        }
    }

    public override void OnGUI(string searchContext) {
        EditorGUILayout.PropertyField(pallete, Styles.color);
    }

    class Styles {
        public static GUIContent color = new GUIContent("Pallete");
    }

    public PalleteSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
        : base(path, scope) { }

    public static Color[] palleteAsArray() {
        if (pallete == null) {
            LoadPalleteSettingsAndPallete();
        }

        Color[] palleteAsArray = new Color[pallete.arraySize];

        for (int i = 0; i < pallete.arraySize; i++) {
            palleteAsArray[i] = pallete.GetArrayElementAtIndex(i).colorValue;
        }

        return palleteAsArray;
    }

    private static void LoadPalleteSettingsAndPallete() {
        PalleteSettings = global::PalleteSettings.GetSerializedSettings();
        pallete = PalleteSettings.FindProperty("palleteColors");
    }
}