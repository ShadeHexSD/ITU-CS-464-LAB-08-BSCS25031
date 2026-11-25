using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Shared helpers for the ten Lab 08 builder scripts. Everything is idempotent: a task can be
/// re-run from Tools &gt; Lab 08 without stacking duplicate panels, listeners or audio clips.
/// </summary>
public static class Lab08EditorUtil
{
    public const string Root = "Assets";
    public const string AudioRoot = "Assets/Audio";
    public const string ClipRoot = "Assets/Audio/Clips";
    public const string MixerPath = "Assets/Audio/Lab08Mixer.mixer";
    public const string PrefabPath = "Assets/Prefabs/AudioSystem.prefab";
    public const string MenusPath = "Assets/Scenes/Menus.unity";
    public const string GameplayPath = "Assets/Scenes/Gameplay.unity";

    // ---- logging ---------------------------------------------------------------------------

    public static void Note(string message) => Debug.Log("[Lab08] " + message);

    // ---- folders and assets ----------------------------------------------------------------

    public static void EnsureFolder(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath) || AssetDatabase.IsValidFolder(assetPath)) return;
        string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
        string leaf = Path.GetFileName(assetPath);
        if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }

    public static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

    public static void SaveAssets()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void SaveOpenScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.path.StartsWith("Assets/")) EditorSceneManager.SaveScene(scene);
        }
        SaveAssets();
    }

    // ---- object lookup (includes inactive scene objects) -----------------------------------

    public static Scene Scene(string path) => EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

    public static T[] InScene<T>(Scene scene) where T : Component
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .ToArray();
    }

    public static T FindInScene<T>(Scene scene, string name) where T : Component =>
        InScene<T>(scene).FirstOrDefault(c => c.name == name);

    public static T[] All<T>() where T : Object
    {
        return Resources.FindObjectsOfTypeAll<T>()
            .Where(o =>
            {
                GameObject go = o is Component c ? c.gameObject : o as GameObject;
                return go == null || go.scene.IsValid();
            })
            .ToArray();
    }

    public static T Find<T>(string name) where T : Object =>
        All<T>().FirstOrDefault(o => o.name == name);

    public static GameObject FindGo(string name) => Find<GameObject>(name);

    public static T Add<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    public static Transform Child(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform found = Child(child, name);
            if (found != null) return found;
        }
        return null;
    }

    public static GameObject FindGoInScene(Scene scene, string name) =>
        InScene<Transform>(scene).FirstOrDefault(t => t.name == name)?.gameObject;

    public static void Remove<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component != null) Object.DestroyImmediate(component);
    }

    // ---- SerializedObject field helpers ----------------------------------------------------

    public static void SetObject(Object target, string field, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) throw new InvalidOperationException(target.GetType().Name + " has no field '" + field + "'");
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetInt(Object target, string field, int value)
    {
        SerializedObject so = new SerializedObject(target);
        so.FindProperty(field).intValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetFloat(Object target, string field, float value)
    {
        SerializedObject so = new SerializedObject(target);
        so.FindProperty(field).floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetBool(Object target, string field, bool value)
    {
        SerializedObject so = new SerializedObject(target);
        so.FindProperty(field).boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetString(Object target, string field, string value)
    {
        SerializedObject so = new SerializedObject(target);
        so.FindProperty(field).stringValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetEnum(Object target, string field, int index)
    {
        SerializedObject so = new SerializedObject(target);
        so.FindProperty(field).enumValueIndex = index;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetObjectArray(Object target, string field, Object[] values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty list = so.FindProperty(field);
        list.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetComponentEnabled(Component component, bool enabled)
    {
        SerializedObject so = new SerializedObject(component);
        so.FindProperty("m_Enabled").boolValue = enabled;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetButtonCall(Button button, Object target, string method)
    {
        SerializedObject so = new SerializedObject(button);
        SerializedProperty calls = so.FindProperty("m_PersistentCalls.m_Calls");
        calls.arraySize = 1;
        SerializedProperty call = calls.GetArrayElementAtIndex(0);
        call.FindPropertyRelative("m_Target").objectReferenceValue = target;
        call.FindPropertyRelative("m_MethodName").stringValue = method;
        call.FindPropertyRelative("m_Mode").intValue = 1;       // PersistentListenerMode.Void
        call.FindPropertyRelative("m_CallState").intValue = 2;  // RuntimeOnly
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(button);
    }

    public struct Credit
    {
        public string title, author, license, source;
        public Credit(string title, string author, string license, string source)
        {
            this.title = title; this.author = author; this.license = license; this.source = source;
        }
    }

    public static void SetCredits(Object target, IEnumerable<Credit> credits)
    {
        Credit[] list = credits.ToArray();
        SerializedObject so = new SerializedObject(target);
        SerializedProperty entries = so.FindProperty("entries");
        entries.arraySize = list.Length;
        for (int i = 0; i < list.Length; i++)
        {
            SerializedProperty e = entries.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("title").stringValue = list[i].title;
            e.FindPropertyRelative("author").stringValue = list[i].author;
            e.FindPropertyRelative("license").stringValue = list[i].license;
            e.FindPropertyRelative("source").stringValue = list[i].source;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---- UI construction -------------------------------------------------------------------

    static TMP_FontAsset Font()
    {
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        if (font != null) return font;
        font = Load<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        return font;
    }

    public static RectTransform Rect(GameObject go, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        RectTransform r = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max; r.anchoredPosition = pos; r.sizeDelta = size;
        return r;
    }

    public static Image MakeImage(string name, Transform parent, Color color, bool raycast = false)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        Rect(go, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = raycast;
        return image;
    }

    public static TextMeshProUGUI MakeText(string name, Transform parent, string content, int size = 32,
        Color? color = null, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        Rect(go, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.font = Font();
        text.text = content;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color ?? Color.white;
        text.raycastTarget = false;
        text.enableWordWrapping = true;
        return text;
    }

    public static Button MakeButton(string name, Transform parent, string label, Object target, string method)
    {
        Image image = MakeImage(name, parent, new Color(0.22f, 0.18f, 0.42f, 1f), true);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        Add<ButtonFeedback>(button.gameObject);
        TextMeshProUGUI text = MakeText("Label", button.transform, label, 30);
        Rect(text.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        if (target != null && !string.IsNullOrEmpty(method)) SetButtonCall(button, target, method);
        return button;
    }

    public static Slider MakeSlider(string name, Transform parent, Object target, string method, VolumeSlider.AudioBus bus)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        RectTransform rect = Rect(go, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, -8f), new Vector2(-220f, 22f));

        Image background = MakeImage("Background", go.transform, new Color(0.13f, 0.10f, 0.26f, 1f));
        Rect(background.gameObject, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -8f));

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.layer = 5; fillArea.transform.SetParent(go.transform, false);
        Rect(fillArea, new Vector2(0.02f, 0f), new Vector2(0.98f, 1f), Vector2.zero, new Vector2(-8f, -6f));
        Image fill = MakeImage("Fill", fillArea.transform, new Color(0.24f, 0.86f, 0.52f, 1f));
        Rect(fill.gameObject, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, 0f));

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.layer = 5; handleArea.transform.SetParent(go.transform, false);
        Rect(handleArea, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f, 0f));
        Image handle = MakeImage("Handle", handleArea.transform, Color.white);
        Rect(handle.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(24f, 30f));

        Slider slider = go.GetComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.minValue = AudioPrefs.MinLinear;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = true;

        VolumeSlider volume = Add<VolumeSlider>(go);
        SetEnum(volume, "bus", (int)bus);
        return slider;
    }

    public static UIScreen MakeScreen(string name, Transform parent, bool fullScreen = true)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        Rect(go, fullScreen ? Vector2.zero : new Vector2(0.2f, 0.25f),
             fullScreen ? Vector2.one : new Vector2(0.8f, 0.75f), Vector2.zero, Vector2.zero);
        go.GetComponent<Image>().color = fullScreen
            ? new Color(0.05f, 0.03f, 0.13f, 0.97f)
            : new Color(0.10f, 0.07f, 0.22f, 0.96f);
        go.GetComponent<Image>().raycastTarget = true;
        return Add<UIScreen>(go);
    }

    // ---- scene scaffolding -----------------------------------------------------------------

    public static Canvas EnsureCanvas(Scene scene, string name = "GameplayCanvas")
    {
        GameObject go = FindGoInScene(scene, name);
        if (go == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = 5;
            SceneManager.MoveGameObjectToScene(go, scene);
        }
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    public static void EnsureEventSystem(Scene scene)
    {
        if (InScene<EventSystem>(scene).Length > 0) return;
        GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(go, scene);
    }

    public static GameObject Primitive(Scene scene, PrimitiveType type, string name, Vector3 position, Vector3 scale)
    {
        GameObject go = FindGoInScene(scene, name);
        if (go == null)
        {
            go = GameObject.CreatePrimitive(type);
            go.name = name;
            SceneManager.MoveGameObjectToScene(go, scene);
        }
        go.transform.position = position;
        go.transform.localScale = scale;
        return go;
    }

    public static Material EnsureMaterial(string name, Color color)
    {
        EnsureFolder("Assets/Materials");
        string path = "Assets/Materials/" + name + ".mat";
        Material material = Load<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    public static Image MakeFilledBar(string name, Transform parent, Color color)
    {
        Image image = MakeImage(name, parent, color);
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = 0;
        image.fillAmount = 1f;
        return image;
    }

    public static RectTransform Place(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        return Rect(go, anchor, anchor, pos, size);
    }

    public static void WireButton(Button button, Object target, string method) => SetButtonCall(button, target, method);

    // ---- procedural audio ------------------------------------------------------------------

    static void WriteWav(string path, float[] samples, int rate)
    {
        EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
        byte[] bytes = new byte[44 + samples.Length * 2];
        using (MemoryStream stream = new MemoryStream(bytes))
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + samples.Length * 2);
            writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(samples.Length * 2);
            for (int i = 0; i < samples.Length; i++)
                writer.Write((short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767f));
        }
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>Creates a WAV once and imports it as an AudioClip. Self-made audio: no licence issue.</summary>
    public static AudioClip Clip(string name, Func<int, float[]> generator, int rate = 22050)
    {
        string path = ClipRoot + "/" + name + ".wav";
        if (!File.Exists(path))
        {
            WriteWav(path, generator(rate), rate);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        AudioClip clip = Load<AudioClip>(path);
        if (clip == null) Note("Could not import clip " + path);
        return clip;
    }

    public static float[] Tone(int rate, float seconds, float frequency, float harmonics = 0.2f, float attack = 0.01f)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(rate * seconds));
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)rate;
            float envelope = Mathf.Min(1f, t / Mathf.Max(0.001f, attack)) *
                             Mathf.Pow(1f - i / (float)count, 2f);
            data[i] = (Mathf.Sin(2f * Mathf.PI * frequency * t) +
                       harmonics * Mathf.Sin(4f * Mathf.PI * frequency * t)) * envelope * 0.45f;
        }
        return data;
    }

    public static float[] Chirp(int rate, float seconds, float from, float to, float noise = 0f)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(rate * seconds));
        float[] data = new float[count];
        System.Random random = new System.Random(1234 + (int)from);
        float phase = 0f;
        for (int i = 0; i < count; i++)
        {
            float u = i / (float)count;
            float frequency = Mathf.Lerp(from, to, u);
            phase += 2f * Mathf.PI * frequency / rate;
            float envelope = Mathf.Sin(Mathf.PI * u);
            float n = noise > 0f ? (float)(random.NextDouble() * 2.0 - 1.0) * noise : 0f;
            data[i] = (Mathf.Sin(phase) + n) * envelope * 0.4f;
        }
        return data;
    }

    public static float[] Sequence(int rate, float seconds, float[] notes, float noteSeconds)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(rate * seconds));
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)rate;
            int index = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(t / noteSeconds));
            float local = (t - index * noteSeconds) / noteSeconds;
            float envelope = Mathf.Min(1f, local * 12f) * Mathf.Pow(1f - local, 1.6f);
            float fade = Mathf.Min(1f, (seconds - t) * 3f, t * 3f);
            data[i] = Mathf.Sin(2f * Mathf.PI * notes[index] * t) * envelope * fade * 0.28f;
        }
        return data;
    }

    public static void WriteReport(string relativePath, string content)
    {
        string full = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? "", relativePath.Replace('/', Path.DirectorySeparatorChar));
        File.WriteAllText(full, content, Encoding.UTF8);
        AssetDatabase.Refresh();
        Note("Report written: " + relativePath);
    }
}
