using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Tarea04LoginSceneBuilder
{
    private const string ScenePath = "Assets/Tareas/LoginCreateAccount/Scenes/Tarea04_LoginCreateAccount.unity";
    private const string FontAssetPath = "Assets/UI/Fonts/Bahnschrift SDF.asset";

    private static TMP_FontAsset font;
    private static readonly Color Background = new Color(0.018f, 0.035f, 0.075f, 1f);
    private static readonly Color Card = new Color(0.035f, 0.075f, 0.13f, 0.98f);
    private static readonly Color Cyan = new Color(0.20f, 0.88f, 1f, 1f);
    private static readonly Color Green = new Color(0.25f, 0.95f, 0.78f, 1f);
    private static readonly Color Muted = new Color(0.62f, 0.72f, 0.82f, 1f);

    [MenuItem("Tareas/Login & Create Account/Construir escena")]
    public static void BuildScene()
    {
        AssetDatabase.Refresh();
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath) ?? TMP_Settings.defaultFontAsset;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Tarea04_LoginCreateAccount";

        CreateCamera();
        Canvas canvas = CreateCanvas();
        BuildBackground(canvas.transform);
        PlayFabLoginController controller = canvas.gameObject.AddComponent<PlayFabLoginController>();

        PanelReferences login = BuildLoginPanel(canvas.transform, controller);
        PanelReferences register = BuildRegisterPanel(canvas.transform, controller);

        SerializedObject serialized = new SerializedObject(controller);
        SetObject(serialized, "loginPanel", login.panel);
        SetObject(serialized, "createAccountPanel", register.panel);
        SetObject(serialized, "loginEmailInput", login.emailInput);
        SetObject(serialized, "loginPasswordInput", login.passwordInput);
        SetObject(serialized, "loginButton", login.primaryButton);
        SetObject(serialized, "loginStatusText", login.statusText);
        SetObject(serialized, "registerEmailInput", register.emailInput);
        SetObject(serialized, "registerUsernameInput", register.usernameInput);
        SetObject(serialized, "registerPasswordInput", register.passwordInput);
        SetObject(serialized, "registerButton", register.primaryButton);
        SetObject(serialized, "registerStatusText", register.statusText);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        login.panel.SetActive(true);
        register.panel.SetActive(false);
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();
        Selection.activeGameObject = canvas.gameObject;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"TAREA04_BUILD_SUCCESS: {ScenePath}");
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Background;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static Canvas CreateCanvas()
    {
        GameObject go = new GameObject(
            "Canvas - Tarea 04",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;
        return canvas;
    }

    private static void BuildBackground(Transform canvas)
    {
        GameObject background = UIObject("Fondo adaptable 1920x1080", canvas);
        Stretch(background.GetComponent<RectTransform>());
        background.AddComponent<Image>().color = Background;

        GameObject glowLeft = UIObject("Brillo cian", background.transform);
        SetRect(glowLeft.GetComponent<RectTransform>(), new Vector2(-760f, 370f), new Vector2(760f, 560f));
        glowLeft.AddComponent<Image>().color = new Color(0.05f, 0.55f, 0.72f, 0.15f);

        GameObject glowRight = UIObject("Brillo verde", background.transform);
        SetRect(glowRight.GetComponent<RectTransform>(), new Vector2(790f, -390f), new Vector2(720f, 520f));
        glowRight.AddComponent<Image>().color = new Color(0.10f, 0.75f, 0.52f, 0.11f);

        CreateText(background.transform, "CourseLabel", "PROGRAMACIÓN ONLINE 2  •  TAREA 04", 20f,
            new Vector2(0f, 495f), new Vector2(900f, 42f), Muted, FontStyles.Bold);
        CreateText(background.transform, "Footer", "PLAYFAB  •  CALLBACKS  •  TASKCOMPLETIONSOURCE  •  ASYNC/AWAIT", 16f,
            new Vector2(0f, -500f), new Vector2(1100f, 36f), new Color(0.42f, 0.57f, 0.70f), FontStyles.Normal);
    }

    private static PanelReferences BuildLoginPanel(Transform canvas, PlayFabLoginController controller)
    {
        GameObject panel = CreateFullPanel(canvas, "Login Panel");
        Transform card = CreateCard(panel.transform, new Vector2(760f, 720f));

        CreateBadge(card, "INICIAR SESIÓN", Green);
        CreateText(card, "Title", "BIENVENIDO", 54f, new Vector2(0f, 245f), new Vector2(650f, 75f), Color.white, FontStyles.Bold);
        CreateText(card, "Subtitle", "Accede a tu cuenta de PlayFab", 20f, new Vector2(0f, 195f), new Vector2(620f, 38f), Muted, FontStyles.Normal);

        CreateFieldLabel(card, "CorreoLabel", "CORREO", 120f);
        TMP_InputField email = CreateInput(card, "Login Email Input", "jugador@correo.com", 65f,
            TMP_InputField.ContentType.EmailAddress);

        CreateFieldLabel(card, "PasswordLabel", "CONTRASEÑA", -15f);
        TMP_InputField password = CreateInput(card, "Login Password Input", "Mínimo 6 caracteres", -70f,
            TMP_InputField.ContentType.Password);

        Button loginButton = CreateButton(card, "Login Button", "INICIAR SESIÓN", new Vector2(0f, -175f),
            new Vector2(560f, 72f), Green, new Color(0.02f, 0.13f, 0.12f));
        Button goToRegister = CreateLinkButton(card, "Go To Create Account Button", "¿NO TIENES CUENTA?  CREAR CUENTA",
            new Vector2(0f, -255f));
        TMP_Text status = CreateText(card, "Login Status Text", string.Empty, 17f, new Vector2(0f, -315f),
            new Vector2(620f, 48f), Green, FontStyles.Bold);

        UnityEventTools.AddPersistentListener(loginButton.onClick, controller.Login);
        UnityEventTools.AddPersistentListener(goToRegister.onClick, controller.ShowCreateAccountPanel);

        return new PanelReferences
        {
            panel = panel,
            emailInput = email,
            passwordInput = password,
            primaryButton = loginButton,
            statusText = status
        };
    }

    private static PanelReferences BuildRegisterPanel(Transform canvas, PlayFabLoginController controller)
    {
        GameObject panel = CreateFullPanel(canvas, "Create Account Panel");
        Transform card = CreateCard(panel.transform, new Vector2(800f, 870f));

        CreateBadge(card, "CREAR CUENTA", Cyan);
        CreateText(card, "Title", "NUEVO JUGADOR", 48f, new Vector2(0f, 325f), new Vector2(680f, 70f), Color.white, FontStyles.Bold);
        CreateText(card, "Subtitle", "Registra tus datos para comenzar", 19f, new Vector2(0f, 280f), new Vector2(650f, 36f), Muted, FontStyles.Normal);

        CreateFieldLabel(card, "EmailLabel", "CORREO", 205f);
        TMP_InputField email = CreateInput(card, "Register Email Input", "jugador@correo.com", 150f,
            TMP_InputField.ContentType.EmailAddress);

        CreateFieldLabel(card, "UsernameLabel", "USUARIO", 70f);
        TMP_InputField username = CreateInput(card, "Register Username Input", "Entre 3 y 20 caracteres", 15f,
            TMP_InputField.ContentType.Standard);

        CreateFieldLabel(card, "PasswordLabel", "CONTRASEÑA", -65f);
        TMP_InputField password = CreateInput(card, "Register Password Input", "Mínimo 6 caracteres", -120f,
            TMP_InputField.ContentType.Password);

        Button registerButton = CreateButton(card, "Create Account Button", "CREAR CUENTA", new Vector2(0f, -230f),
            new Vector2(580f, 72f), Cyan, new Color(0.02f, 0.10f, 0.16f));
        Button goToLogin = CreateLinkButton(card, "Go To Login Button", "¿YA TIENES CUENTA?  INICIAR SESIÓN",
            new Vector2(0f, -310f));
        TMP_Text status = CreateText(card, "Register Status Text", string.Empty, 17f, new Vector2(0f, -372f),
            new Vector2(660f, 48f), Green, FontStyles.Bold);

        UnityEventTools.AddPersistentListener(registerButton.onClick, controller.CreateAccount);
        UnityEventTools.AddPersistentListener(goToLogin.onClick, controller.ShowLoginPanel);

        return new PanelReferences
        {
            panel = panel,
            emailInput = email,
            usernameInput = username,
            passwordInput = password,
            primaryButton = registerButton,
            statusText = status
        };
    }

    private static GameObject CreateFullPanel(Transform canvas, string name)
    {
        GameObject panel = UIObject(name, canvas);
        Stretch(panel.GetComponent<RectTransform>());
        return panel;
    }

    private static Transform CreateCard(Transform parent, Vector2 size)
    {
        GameObject shadow = UIObject("Card Shadow", parent);
        SetRect(shadow.GetComponent<RectTransform>(), new Vector2(12f, -14f), size);
        shadow.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

        GameObject card = UIObject("Form Card", parent);
        SetRect(card.GetComponent<RectTransform>(), Vector2.zero, size);
        Image image = card.AddComponent<Image>();
        image.color = Card;
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(0.22f, 0.82f, 1f, 0.38f);
        outline.effectDistance = new Vector2(2f, -2f);
        return card.transform;
    }

    private static void CreateBadge(Transform parent, string value, Color color)
    {
        GameObject badge = UIObject("Section Badge", parent);
        SetRect(badge.GetComponent<RectTransform>(), new Vector2(0f, parent.name == "Form Card" ? 0f : 0f), new Vector2(240f, 42f));
        RectTransform rect = badge.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -22f);
        badge.AddComponent<Image>().color = new Color(color.r, color.g, color.b, 0.16f);
        Outline outline = badge.AddComponent<Outline>();
        outline.effectColor = new Color(color.r, color.g, color.b, 0.65f);
        outline.effectDistance = new Vector2(1f, -1f);
        CreateText(badge.transform, "Text", value, 15f, Vector2.zero, new Vector2(220f, 36f), color, FontStyles.Bold);
    }

    private static void CreateFieldLabel(Transform parent, string name, string value, float y)
    {
        TMP_Text label = CreateText(parent, name, value, 16f, new Vector2(-280f, y), new Vector2(120f, 30f), Muted, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.characterSpacing = 2f;
    }

    private static TMP_InputField CreateInput(
        Transform parent,
        string name,
        string placeholderValue,
        float y,
        TMP_InputField.ContentType contentType)
    {
        GameObject root = UIObject(name, parent);
        SetRect(root.GetComponent<RectTransform>(), new Vector2(0f, y), new Vector2(600f, 64f));
        Image background = root.AddComponent<Image>();
        background.color = new Color(0.015f, 0.035f, 0.065f, 1f);
        Outline outline = root.AddComponent<Outline>();
        outline.effectColor = new Color(0.18f, 0.52f, 0.70f, 0.8f);
        outline.effectDistance = new Vector2(1f, -1f);

        GameObject viewport = UIObject("Text Area", root.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);
        viewportRect.offsetMin = new Vector2(22f, 8f);
        viewportRect.offsetMax = new Vector2(-22f, -8f);
        viewport.AddComponent<RectMask2D>();

        TMP_Text placeholder = CreateText(viewport.transform, "Placeholder", placeholderValue, 19f, Vector2.zero,
            Vector2.zero, new Color(0.38f, 0.50f, 0.60f), FontStyles.Italic);
        Stretch(placeholder.rectTransform);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;

        TMP_Text text = CreateText(viewport.transform, "Text", string.Empty, 20f, Vector2.zero,
            Vector2.zero, Color.white, FontStyles.Normal);
        Stretch(text.rectTransform);
        text.alignment = TextAlignmentOptions.MidlineLeft;

        TMP_InputField input = root.AddComponent<TMP_InputField>();
        input.targetGraphic = background;
        input.textViewport = viewportRect;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.contentType = contentType;
        input.caretColor = Cyan;
        input.selectionColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.35f);
        return input;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 position,
        Vector2 size,
        Color accent,
        Color baseColor)
    {
        GameObject root = UIObject(name, parent);
        SetRect(root.GetComponent<RectTransform>(), position, size);
        Image image = root.AddComponent<Image>();
        image.color = baseColor;
        Outline outline = root.AddComponent<Outline>();
        outline.effectColor = accent;
        outline.effectDistance = new Vector2(2f, -2f);

        Button button = root.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(accent.r, accent.g, accent.b, 0.95f);
        colors.pressedColor = new Color(accent.r * 0.72f, accent.g * 0.72f, accent.b * 0.72f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.35f, 0.4f, 0.45f, 0.65f);
        button.colors = colors;

        TMP_Text text = CreateText(root.transform, "Label", label, 19f, Vector2.zero, size - new Vector2(20f, 8f),
            Color.white, FontStyles.Bold);
        text.characterSpacing = 1.5f;
        return button;
    }

    private static Button CreateLinkButton(Transform parent, string name, string label, Vector2 position)
    {
        GameObject root = UIObject(name, parent);
        SetRect(root.GetComponent<RectTransform>(), position, new Vector2(560f, 48f));
        Image image = root.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        Button button = root.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Cyan;
        colors.pressedColor = Green;
        button.colors = colors;

        TMP_Text text = CreateText(root.transform, "Label", label, 16f, Vector2.zero, new Vector2(550f, 44f), Cyan, FontStyles.Bold);
        text.characterSpacing = 1f;
        return button;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string name,
        string value,
        float size,
        Vector2 position,
        Vector2 boxSize,
        Color color,
        FontStyles style)
    {
        GameObject root = UIObject(name, parent);
        RectTransform rect = root.GetComponent<RectTransform>();
        if (boxSize != Vector2.zero)
        {
            SetRect(rect, position, boxSize);
        }

        TextMeshProUGUI text = root.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<InputSystemUIInputModule>();
    }

    private static void AddSceneToBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        int index = scenes.FindIndex(scene => scene.path == ScenePath);
        if (index < 0)
        {
            scenes.Add(new EditorBuildSettingsScene(ScenePath, false));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static GameObject UIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetObject(SerializedObject serialized, string propertyName, Object value)
    {
        serialized.FindProperty(propertyName).objectReferenceValue = value;
    }

    private sealed class PanelReferences
    {
        public GameObject panel;
        public TMP_InputField emailInput;
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public Button primaryButton;
        public TMP_Text statusText;
    }
}
