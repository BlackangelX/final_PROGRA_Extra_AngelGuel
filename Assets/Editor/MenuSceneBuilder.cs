using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public static class MenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string BackgroundPath = "Assets/UI/Art/ArenaMenuBackground.png";
    private const string FontPath = "Assets/UI/Fonts/Bahnschrift.ttf";
    private const string FontAssetPath = "Assets/UI/Fonts/Bahnschrift SDF.asset";
    private const string ButtonAssetPath = "Assets/UI/Art/SciFiButton.asset";

    private static TMP_FontAsset fontAsset;
    private static Sprite backgroundSprite;
    private static Sprite buttonSprite;
    private static readonly Color Cyan = new Color(0.12f, 0.82f, 1f, 1f);
    private static readonly Color Orange = new Color(1f, 0.48f, 0.12f, 1f);

    [MenuItem("Tools/Proyecto Final/Construir menu completo")]
    public static void Build()
    {
        AssetDatabase.Refresh();
        ConfigureBackground();
        CreateFontAsset();
        CreateButtonSprite();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null)
        {
            Object.DestroyImmediate(oldCanvas);
        }

        PhotonManager photon = Object.FindFirstObjectByType<PhotonManager>();
        if (photon == null)
        {
            throw new System.InvalidOperationException("No se encontró PhotonManager en SampleScene.");
        }

        Canvas canvas = CreateCanvas();
        MenuUIController controller = canvas.gameObject.AddComponent<MenuUIController>();

        GameObject startPanel = CreatePanel(canvas.transform, "StartPanel");
        BuildStart(startPanel.transform);

        GameObject mainPanel = CreatePanel(canvas.transform, "MainMenuPanel");
        BuildMainMenu(mainPanel.transform, controller, photon);

        GameObject serverPanel = CreatePanel(canvas.transform, "ServerListPanel");
        ServerReferences serverRefs = BuildServerList(serverPanel.transform, controller);

        GameObject createPanel = CreatePanel(canvas.transform, "CreateMatchPanel");
        CreateReferences createRefs = BuildCreate(createPanel.transform, controller);

        GameObject joinPanel = CreatePanel(canvas.transform, "JoinMatchPanel");
        JoinReferences joinRefs = BuildJoin(joinPanel.transform, controller);

        SerializedObject serializedController = new SerializedObject(controller);
        SetObject(serializedController, "startPanel", startPanel);
        SetObject(serializedController, "mainMenuPanel", mainPanel);
        SetObject(serializedController, "serverListPanel", serverPanel);
        SetObject(serializedController, "createMatchPanel", createPanel);
        SetObject(serializedController, "joinMatchPanel", joinPanel);
        SetObject(serializedController, "photonManager", photon);
        SetObject(serializedController, "serverContent", serverRefs.content);
        SetObject(serializedController, "serverButtonTemplate", serverRefs.template);
        SetObject(serializedController, "noServersText", serverRefs.emptyText);
        SetObject(serializedController, "serverStatusText", serverRefs.statusText);
        SetObject(serializedController, "createNameInput", createRefs.nameInput);
        SetObject(serializedController, "playerCountDropdown", createRefs.playerCount);
        SetObject(serializedController, "privateToggle", createRefs.privateToggle);
        SetObject(serializedController, "createStatusText", createRefs.statusText);
        SetObject(serializedController, "joinNameInput", joinRefs.nameInput);
        SetObject(serializedController, "joinStatusText", joinRefs.statusText);
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        startPanel.SetActive(true);
        mainPanel.SetActive(false);
        serverPanel.SetActive(false);
        createPanel.SetActive(false);
        joinPanel.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("MENU_BUILD_SUCCESS: Canvas y navegación construidos correctamente.");
    }

    private static void ConfigureBackground()
    {
        TextureImporter importer = AssetImporter.GetAtPath(BackgroundPath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
    }

    private static void CreateFontAsset()
    {
        fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset != null && fontAsset.atlasTexture != null) return;

        if (fontAsset != null)
        {
            AssetDatabase.DeleteAsset(FontAssetPath);
        }

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,
            9,
            GlyphRenderMode.SDFAA,
            1024,
            1024,
            AtlasPopulationMode.Dynamic,
            true);
        fontAsset.name = "Bahnschrift SDF";
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        fontAsset.atlasTexture.name = "Bahnschrift Atlas";
        fontAsset.material.name = "Bahnschrift Material";
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
    }

    private static void CreateButtonSprite()
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ButtonAssetPath);
        if (texture == null)
        {
            texture = new Texture2D(64, 64, TextureFormat.RGBA32, false)
            {
                name = "SciFiButtonTexture",
                filterMode = FilterMode.Bilinear
            };
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                bool edge = x < 3 || x > 60 || y < 3 || y > 60;
                float glow = Mathf.Clamp01(1f - Mathf.Abs(y - 32f) / 32f);
                texture.SetPixel(x, y, edge
                    ? new Color(0.08f, 0.75f, 0.95f, 1f)
                    : new Color(0.025f, 0.11f + glow * 0.05f, 0.18f + glow * 0.07f, 0.96f));
            }
            texture.Apply();
            AssetDatabase.CreateAsset(texture, ButtonAssetPath);
            buttonSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f, 100f, 0,
                SpriteMeshType.FullRect, new Vector4(10, 10, 10, 10));
            buttonSprite.name = "SciFiButtonSprite";
            AssetDatabase.AddObjectToAsset(buttonSprite, texture);
            AssetDatabase.SaveAssets();
        }
        buttonSprite = AssetDatabase.LoadAllAssetsAtPath(ButtonAssetPath)
            .FirstOrDefault(asset => asset is Sprite) as Sprite;
    }

    private static Canvas CreateCanvas()
    {
        GameObject go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = UIObject(name, parent);
        Stretch(panel.GetComponent<RectTransform>());
        Image background = panel.AddComponent<Image>();
        background.sprite = backgroundSprite;
        background.preserveAspect = false;

        GameObject shade = UIObject("DarkOverlay", panel.transform);
        Stretch(shade.GetComponent<RectTransform>());
        shade.AddComponent<Image>().color = new Color(0f, 0.025f, 0.06f, 0.58f);
        return panel;
    }

    private static void BuildStart(Transform panel)
    {
        CreateText(panel, "GameTitle", "NEON ARENA", 82, new Vector2(0, 115), new Vector2(1200, 120), Cyan, FontStyles.Bold);
        TMP_Text prompt = CreateText(panel, "PressKeyText", "PRESIONA CUALQUIER TECLA PARA INICIAR", 31,
            new Vector2(0, -90), new Vector2(1100, 70), Color.white, FontStyles.Normal);
        prompt.characterSpacing = 4f;
        CreateText(panel, "Subtitle", "COMBATE MULTIJUGADOR", 22, new Vector2(0, 35), new Vector2(900, 45), Orange, FontStyles.Bold);
    }

    private static void BuildMainMenu(Transform panel, MenuUIController controller, PhotonManager photon)
    {
        CreateCard(panel, new Vector2(-500, 0), new Vector2(660, 720));
        CreateText(panel, "Title", "MENÚ PRINCIPAL", 52, new Vector2(-500, 255), new Vector2(600, 85), Cyan, FontStyles.Bold);
        CreateText(panel, "Subtitle", "ELIGE TU FORMA DE JUGAR", 19, new Vector2(-500, 205), new Vector2(600, 40), Orange, FontStyles.Bold);

        Button quick = CreateButton(panel, "QuickMatchButton", "INICIAR PARTIDA RÁPIDA", new Vector2(-500, 90), new Vector2(500, 78));
        Button random = CreateButton(panel, "RandomMatchButton", "UNIRSE A PARTIDA RANDOM", new Vector2(-500, -20), new Vector2(500, 78));
        Button servers = CreateButton(panel, "ServersButton", "SERVIDORES", new Vector2(-500, -130), new Vector2(500, 78));
        UnityEventTools.AddPersistentListener(quick.onClick, photon.StartGameAsHost);
        UnityEventTools.AddPersistentListener(random.onClick, photon.StartGameAsClient);
        UnityEventTools.AddPersistentListener(servers.onClick, controller.ShowServerList);
    }

    private static ServerReferences BuildServerList(Transform panel, MenuUIController controller)
    {
        CreateText(panel, "Title", "SERVIDORES DISPONIBLES", 49, new Vector2(0, 430), new Vector2(1100, 75), Cyan, FontStyles.Bold);
        GameObject listFrame = CreateCard(panel, new Vector2(0, 55), new Vector2(1100, 610));

        GameObject viewport = UIObject("Viewport", listFrame.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0, 0);
        viewportRect.anchorMax = new Vector2(1, 1);
        viewportRect.offsetMin = new Vector2(35, 55);
        viewportRect.offsetMax = new Vector2(-35, -65);
        viewport.AddComponent<Image>().color = new Color(0f, 0.03f, 0.06f, 0.25f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject content = UIObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = listFrame.AddComponent<ScrollRect>();
        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        Button template = CreateButton(content.transform, "ServerButtonTemplate", "SERVIDOR     0/0     UNIRSE", Vector2.zero, new Vector2(0, 72));
        LayoutElement templateLayout = template.gameObject.AddComponent<LayoutElement>();
        templateLayout.preferredHeight = 72;
        template.gameObject.SetActive(false);

        TMP_Text emptyText = CreateText(listFrame.transform, "NoServersText", "NO HAY SERVIDORES DISPONIBLES", 28,
            new Vector2(0, 15), new Vector2(850, 60), Color.white, FontStyles.Bold);
        TMP_Text status = CreateText(panel, "ServerStatusText", "", 20, new Vector2(0, -292), new Vector2(900, 42), Orange, FontStyles.Normal);

        Button create = CreateButton(panel, "CreateMatchButton", "CREAR PARTIDA", new Vector2(-355, -390), new Vector2(300, 68));
        Button join = CreateButton(panel, "JoinMatchButton", "UNIRSE POR NOMBRE", new Vector2(0, -390), new Vector2(300, 68));
        Button back = CreateButton(panel, "BackButton", "REGRESAR", new Vector2(355, -390), new Vector2(300, 68));
        UnityEventTools.AddPersistentListener(create.onClick, controller.ShowCreateMatch);
        UnityEventTools.AddPersistentListener(join.onClick, controller.ShowJoinMatch);
        UnityEventTools.AddPersistentListener(back.onClick, controller.ShowMainMenu);

        return new ServerReferences { content = content.transform, template = template, emptyText = emptyText, statusText = status };
    }

    private static CreateReferences BuildCreate(Transform panel, MenuUIController controller)
    {
        CreateCard(panel, Vector2.zero, new Vector2(820, 760));
        CreateText(panel, "Title", "CREAR PARTIDA", 52, new Vector2(0, 290), new Vector2(720, 80), Cyan, FontStyles.Bold);
        CreateText(panel, "NameLabel", "NOMBRE DE LA PARTIDA", 20, new Vector2(0, 190), new Vector2(620, 40), Color.white, FontStyles.Bold);
        TMP_InputField input = CreateInput(panel, "ServerNameInput", "Ej. Arena de Ángel", new Vector2(0, 130), new Vector2(600, 68));

        CreateText(panel, "PlayersLabel", "NÚMERO DE JUGADORES", 20, new Vector2(0, 50), new Vector2(620, 40), Color.white, FontStyles.Bold);
        TMP_Dropdown dropdown = CreateDropdown(panel, new Vector2(0, -10), new Vector2(600, 64));

        Toggle toggle = CreateToggle(panel, "PARTIDA PRIVADA", new Vector2(0, -110));
        TMP_Text status = CreateText(panel, "CreateStatusText", "", 20, new Vector2(0, -180), new Vector2(650, 45), Orange, FontStyles.Normal);

        Button create = CreateButton(panel, "CreateButton", "CREAR PARTIDA", new Vector2(-165, -270), new Vector2(290, 68));
        Button back = CreateButton(panel, "BackButton", "REGRESAR", new Vector2(165, -270), new Vector2(290, 68));
        UnityEventTools.AddPersistentListener(create.onClick, controller.CreateGame);
        UnityEventTools.AddPersistentListener(back.onClick, controller.ShowServerList);
        return new CreateReferences { nameInput = input, playerCount = dropdown, privateToggle = toggle, statusText = status };
    }

    private static JoinReferences BuildJoin(Transform panel, MenuUIController controller)
    {
        CreateCard(panel, Vector2.zero, new Vector2(820, 590));
        CreateText(panel, "Title", "UNIRSE A PARTIDA", 52, new Vector2(0, 205), new Vector2(720, 80), Cyan, FontStyles.Bold);
        CreateText(panel, "NameLabel", "NOMBRE DE LA PARTIDA", 20, new Vector2(0, 100), new Vector2(620, 40), Color.white, FontStyles.Bold);
        TMP_InputField input = CreateInput(panel, "ServerNameInput", "Escribe el nombre exacto", new Vector2(0, 35), new Vector2(600, 68));
        TMP_Text status = CreateText(panel, "JoinStatusText", "NO SE ENCUENTRA LA PARTIDA", 20,
            new Vector2(0, -45), new Vector2(650, 45), Orange, FontStyles.Bold);

        Button join = CreateButton(panel, "JoinButton", "UNIR", new Vector2(-165, -150), new Vector2(290, 68));
        Button back = CreateButton(panel, "BackButton", "REGRESAR", new Vector2(165, -150), new Vector2(290, 68));
        UnityEventTools.AddPersistentListener(join.onClick, controller.JoinGameByName);
        UnityEventTools.AddPersistentListener(back.onClick, controller.ShowServerList);
        return new JoinReferences { nameInput = input, statusText = status };
    }

    private static GameObject CreateCard(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject card = UIObject("PanelCard", parent);
        SetRect(card.GetComponent<RectTransform>(), position, size);
        Image image = card.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.015f, 0.06f, 0.105f, 0.93f);
        return card;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
    {
        GameObject go = UIObject(name, parent);
        SetRect(go.GetComponent<RectTransform>(), position, size);
        Image image = go.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        Button button = go.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.72f, 0.96f, 1f, 1f);
        colors.pressedColor = new Color(1f, 0.65f, 0.28f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        CreateText(go.transform, "Label", label, 21, Vector2.zero, size - new Vector2(20, 8), Color.white, FontStyles.Bold);
        return button;
    }

    private static TMP_InputField CreateInput(Transform parent, string name, string placeholderText, Vector2 position, Vector2 size)
    {
        GameObject go = UIObject(name, parent);
        SetRect(go.GetComponent<RectTransform>(), position, size);
        Image image = go.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.75f, 0.92f, 1f, 1f);

        GameObject area = UIObject("Text Area", go.transform);
        RectTransform areaRect = area.GetComponent<RectTransform>();
        Stretch(areaRect);
        areaRect.offsetMin = new Vector2(22, 8);
        areaRect.offsetMax = new Vector2(-22, -8);
        area.AddComponent<RectMask2D>();

        TMP_Text placeholder = CreateText(area.transform, "Placeholder", placeholderText, 21, Vector2.zero, Vector2.zero,
            new Color(0.6f, 0.7f, 0.75f, 1f), FontStyles.Italic);
        Stretch(placeholder.rectTransform);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        TMP_Text text = CreateText(area.transform, "Text", "", 21, Vector2.zero, Vector2.zero, Color.white, FontStyles.Normal);
        Stretch(text.rectTransform);
        text.alignment = TextAlignmentOptions.MidlineLeft;

        TMP_InputField input = go.AddComponent<TMP_InputField>();
        input.textViewport = areaRect;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.lineType = TMP_InputField.LineType.SingleLine;
        return input;
    }

    private static TMP_Dropdown CreateDropdown(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject go = UIObject("PlayerCountDropdown", parent);
        SetRect(go.GetComponent<RectTransform>(), position, size);
        Image image = go.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.type = Image.Type.Sliced;
        TMP_Text label = CreateText(go.transform, "Label", "2 JUGADORES", 21, new Vector2(-15, 0), size - new Vector2(70, 8), Color.white, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        CreateText(go.transform, "Arrow", "▼", 22, new Vector2(size.x / 2 - 35, 0), new Vector2(45, 45), Cyan, FontStyles.Bold);

        GameObject template = UIObject("Template", go.transform);
        RectTransform templateRect = template.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, -4);
        templateRect.sizeDelta = new Vector2(0, 190);
        template.AddComponent<Image>().color = new Color(0.02f, 0.08f, 0.13f, 1f);
        ScrollRect scroll = template.AddComponent<ScrollRect>();

        GameObject viewport = UIObject("Viewport", template.transform);
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.AddComponent<Image>().color = Color.white;
        GameObject content = UIObject("Content", viewport.transform);
        Stretch(content.GetComponent<RectTransform>());

        GameObject item = UIObject("Item", content.transform);
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 58);
        Toggle itemToggle = item.AddComponent<Toggle>();
        Image itemBackground = item.AddComponent<Image>();
        itemBackground.color = new Color(0.04f, 0.16f, 0.23f, 1f);
        itemToggle.targetGraphic = itemBackground;
        TMP_Text itemLabel = CreateText(item.transform, "Item Label", "OPTION", 20, Vector2.zero, new Vector2(-30, 55), Color.white, FontStyles.Normal);
        itemLabel.alignment = TextAlignmentOptions.MidlineLeft;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = content.GetComponent<RectTransform>();

        TMP_Dropdown dropdown = go.AddComponent<TMP_Dropdown>();
        dropdown.targetGraphic = image;
        dropdown.template = templateRect;
        dropdown.captionText = label;
        dropdown.itemText = itemLabel;
        dropdown.options = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("2 JUGADORES"),
            new TMP_Dropdown.OptionData("3 JUGADORES"),
            new TMP_Dropdown.OptionData("4 JUGADORES"),
            new TMP_Dropdown.OptionData("5 JUGADORES"),
            new TMP_Dropdown.OptionData("6 JUGADORES"),
            new TMP_Dropdown.OptionData("7 JUGADORES"),
            new TMP_Dropdown.OptionData("8 JUGADORES"),
            new TMP_Dropdown.OptionData("9 JUGADORES"),
            new TMP_Dropdown.OptionData("10 JUGADORES")
        };
        template.SetActive(false);
        return dropdown;
    }

    private static Toggle CreateToggle(Transform parent, string label, Vector2 position)
    {
        GameObject go = UIObject("PrivateToggle", parent);
        SetRect(go.GetComponent<RectTransform>(), position, new Vector2(600, 55));
        GameObject box = UIObject("Background", go.transform);
        SetRect(box.GetComponent<RectTransform>(), new Vector2(-270, 0), new Vector2(42, 42));
        Image background = box.AddComponent<Image>();
        background.sprite = buttonSprite;
        background.type = Image.Type.Sliced;
        GameObject mark = UIObject("Checkmark", box.transform);
        SetRect(mark.GetComponent<RectTransform>(), Vector2.zero, new Vector2(24, 24));
        Image check = mark.AddComponent<Image>();
        check.color = Orange;
        Toggle toggle = go.AddComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = check;
        CreateText(go.transform, "Label", label, 21, new Vector2(20, 0), new Vector2(500, 50), Color.white, FontStyles.Bold).alignment = TextAlignmentOptions.MidlineLeft;
        return toggle;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float size, Vector2 position, Vector2 boxSize, Color color, FontStyles style)
    {
        GameObject go = UIObject(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        if (boxSize != Vector2.zero) SetRect(rect, position, boxSize);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.font = fontAsset;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
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

    private static void SetObject(SerializedObject target, string property, Object value)
    {
        target.FindProperty(property).objectReferenceValue = value;
    }

    private sealed class ServerReferences
    {
        public Transform content;
        public Button template;
        public TMP_Text emptyText;
        public TMP_Text statusText;
    }

    private sealed class CreateReferences
    {
        public TMP_InputField nameInput;
        public TMP_Dropdown playerCount;
        public Toggle privateToggle;
        public TMP_Text statusText;
    }

    private sealed class JoinReferences
    {
        public TMP_InputField nameInput;
        public TMP_Text statusText;
    }

    private static T FirstOrDefault<T>(this IEnumerable<T> source, System.Func<T, bool> predicate)
    {
        foreach (T item in source)
        {
            if (predicate(item)) return item;
        }
        return default;
    }
}
