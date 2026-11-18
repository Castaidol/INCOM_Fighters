using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Strumento del docente: porta il progetto della sessione 6 allo stato finale della sessione 7.
/// Menu: INCOM Fighters > Setup sessione 7. Si può rieseguire. Va lanciato DOPO i setup delle sessioni 1-6.
/// Aggiunge la mappa UI all'asset di input, crea i materiali delle hitbox, l'EventSystem, il menu di pausa
/// con la lista delle combo e l'HitboxViewer, e prepara le impostazioni della build.
/// Resoconto in Logs/Sessione7Setup.txt.
/// </summary>
public static class Sessione7Setup
{
    const string ScenePath = "Assets/_Project/Scenes/Arena.unity";
    const string InputPath = "Assets/_Project/Input/Fighter.inputactions";
    const string MaterialsFolder = "Assets/_Project/Materials";
    const string SpritePath = "Assets/_Project/Sprites/Bianco.png";
    const string ReportPath = "Logs/Sessione7Setup.txt";

    static readonly Color HitboxColor = new(1f, 0.15f, 0.15f, 0.45f);
    static readonly Color HurtboxColor = new(0.25f, 0.6f, 1f, 0.3f);

    [MenuItem("INCOM Fighters/Setup sessione 7")]
    public static void Run()
    {
        var log = new StringBuilder();
        log.AppendLine($"Setup sessione 7 - {System.DateTime.Now:yyyy-MM-dd HH:mm}");

        AddUiMap(log);
        MakeMaterial("Hitbox", HitboxColor);
        MakeMaterial("Hurtbox", HurtboxColor);
        AssetDatabase.SaveAssets();
        log.AppendLine($"\n# Materiali\n{MaterialsFolder}/Hitbox.mat e Hurtbox.mat: URP/Unlit trasparenti, rosso e azzurro.");

        SetupScene(log);
        SetupBuild(log);

        Directory.CreateDirectory("Logs");
        File.WriteAllText(ReportPath, log.ToString());
        Debug.Log("[Sessione7Setup]\n" + log);
    }

    // --- Input: mappa UI ------------------------------------------------------

    static void AddUiMap(StringBuilder log)
    {
        log.AppendLine("\n# Input");
        // Si lavora su una copia letta dal file e poi si riscrive il file: l'asset importato non si modifica.
        InputActionAsset asset = InputActionAsset.FromJson(File.ReadAllText(InputPath));
        if (asset.FindActionMap("UI") != null)
        {
            log.AppendLine("Mappa UI già presente.");
            Object.DestroyImmediate(asset);
            return;
        }

        // La tastiera porta con sé il mouse, facoltativo, per cliccare i pulsanti del menu.
        asset.RemoveControlScheme("Keyboard");
        asset.AddControlScheme("Keyboard").WithBindingGroup("Keyboard")
             .WithRequiredDevice("<Keyboard>").WithOptionalDevice("<Mouse>");

        InputActionMap ui = asset.AddActionMap("UI");

        InputAction navigate = ui.AddAction("Navigate", InputActionType.PassThrough, expectedControlLayout: "Vector2");
        navigate.AddBinding("<Gamepad>/leftStick", groups: "Gamepad");
        navigate.AddBinding("<Gamepad>/dpad", groups: "Gamepad");
        navigate.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow", "Keyboard").With("Down", "<Keyboard>/downArrow", "Keyboard")
            .With("Left", "<Keyboard>/leftArrow", "Keyboard").With("Right", "<Keyboard>/rightArrow", "Keyboard");
        navigate.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w", "Keyboard").With("Down", "<Keyboard>/s", "Keyboard")
            .With("Left", "<Keyboard>/a", "Keyboard").With("Right", "<Keyboard>/d", "Keyboard");

        InputAction submit = ui.AddAction("Submit", InputActionType.Button);
        submit.AddBinding("<Gamepad>/buttonSouth", groups: "Gamepad");
        submit.AddBinding("<Keyboard>/enter", groups: "Keyboard");
        submit.AddBinding("<Keyboard>/space", groups: "Keyboard");

        InputAction cancel = ui.AddAction("Cancel", InputActionType.Button);
        cancel.AddBinding("<Gamepad>/buttonEast", groups: "Gamepad");
        cancel.AddBinding("<Gamepad>/start", groups: "Gamepad");
        cancel.AddBinding("<Keyboard>/escape", groups: "Keyboard");

        InputAction point = ui.AddAction("Point", InputActionType.PassThrough, expectedControlLayout: "Vector2");
        point.AddBinding("<Mouse>/position", groups: "Keyboard");

        InputAction click = ui.AddAction("Click", InputActionType.PassThrough, expectedControlLayout: "Button");
        click.AddBinding("<Mouse>/leftButton", groups: "Keyboard");

        File.WriteAllText(InputPath, asset.ToJson());
        Object.DestroyImmediate(asset);
        AssetDatabase.ImportAsset(InputPath);
        log.AppendLine("Mappa UI: Navigate (stick, croce, frecce, WASD), Submit (A, Invio, Spazio), " +
                       "Cancel (B, Start, Esc), Point e Click (mouse). Schema Keyboard con il mouse facoltativo.");
    }

    // --- Materiali delle hitbox ------------------------------------------------

    static Material MakeMaterial(string name, Color color)
    {
        EnsureFolder(MaterialsFolder);
        string path = $"{MaterialsFolder}/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(material, path);
        }
        // Le stesse impostazioni che mette l'Inspector con Surface Type = Transparent e Blending Mode = Alpha.
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)RenderQueue.Transparent;
        material.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }

    // --- Scena ----------------------------------------------------------------

    static void SetupScene(StringBuilder log)
    {
        log.AppendLine("\n# Scena");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject player = GameObject.Find("Player");
        GameObject dummy = GameObject.Find("Dummy");
        GameObject hud = GameObject.Find("HUD");
        GameObject training = GameObject.Find("Training");
        if (player == null || dummy == null || hud == null || training == null)
        {
            log.AppendLine("ERRORE: Player, Dummy, HUD o Training non trovati, eseguire prima i setup delle sessioni 1-6");
            return;
        }
        var settings = training.GetComponent<TrainingSettings>();
        var manager = training.GetComponent<TrainingManager>();
        var fighterInput = player.GetComponent<FighterInput>();
        var playerInput = player.GetComponent<PlayerInput>();

        // EventSystem guidato dalle azioni del giocatore: il PlayerInput gli passa la sua copia dell'asset.
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        GameObject esGO = eventSystem != null ? eventSystem.gameObject : new GameObject("EventSystem");
        GetOrAdd<EventSystem>(esGO);
        var module = GetOrAdd<InputSystemUIInputModule>(esGO);
        var refs = AssetDatabase.LoadAllAssetsAtPath(InputPath).OfType<InputActionReference>().ToList();
        InputActionReference Ref(string action) =>
            refs.First(r => r.action != null && r.action.actionMap.name == "UI" && r.action.name == action);
        module.actionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
        module.move = Ref("Navigate");
        module.submit = Ref("Submit");
        module.cancel = Ref("Cancel");
        module.point = Ref("Point");
        module.leftClick = Ref("Click");
        module.rightClick = null;
        module.middleClick = null;
        module.scrollWheel = null;
        module.trackedDevicePosition = null;
        module.trackedDeviceOrientation = null;
        EditorUtility.SetDirty(module);
        playerInput.uiInputModule = module;
        EditorUtility.SetDirty(playerInput);
        log.AppendLine("EventSystem con InputSystemUIInputModule (mappa UI), collegato al PlayerInput del player.");

        // Il mouse clicca i pulsanti solo se il Canvas ha un GraphicRaycaster.
        GetOrAdd<GraphicRaycaster>(hud);

        // Opzione nuova: hitbox spente all'avvio.
        SetBool(settings, "showHitboxes", false);

        // Hitbox visibili in build.
        var viewer = GetOrAdd<HitboxViewer>(training);
        SetRefs(viewer, ("settings", settings), ("cube", Resources.GetBuiltinResource<Mesh>("Cube.fbx")),
                ("hitboxMaterial", AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/Hitbox.mat")),
                ("hurtboxMaterial", AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/Hurtbox.mat")));
        SetArray(viewer, "hitboxes", player.GetComponent<Hitbox>());
        SetArray(viewer, "hurtboxes", dummy.GetComponentsInChildren<Hurtbox>());
        log.AppendLine("Training: HitboxViewer (hitbox del player, hurtbox del manichino).");

        BuildMenu(hud, fighterInput, settings, manager, player.GetComponent<FighterCombat>());
        log.AppendLine("HUD: GraphicRaycaster, Menu (TrainingMenu, ComboList) con il pannello di pausa spento.");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void BuildMenu(GameObject hud, FighterInput fighterInput, TrainingSettings settings,
                          TrainingManager manager, FighterCombat combat)
    {
        Transform old = hud.transform.Find("Menu");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);

        // Radice sempre accesa: TrainingMenu e ComboList devono girare anche a menu chiuso.
        RectTransform root = NewRect("Menu", hud.transform);
        Stretch(root);
        root.SetAsLastSibling();
        var menu = root.gameObject.AddComponent<TrainingMenu>();
        var comboList = root.gameObject.AddComponent<ComboList>();

        // Pannello: copre lo schermo e scurisce il gioco.
        RectTransform panel = NewRect("Pannello", root);
        Stretch(panel);
        AddImage(panel, sprite, new Color(0f, 0f, 0f, 0.6f));

        // Colonna delle opzioni.
        RectTransform options = Box("Opzioni", panel, sprite, new Vector2(-330f, 0f), new Vector2(780f, 640f));
        AddTitle(options, font, "PAUSA");
        RectTransform buttons = NewRect("Pulsanti", options);
        buttons.anchorMin = new Vector2(0f, 1f);
        buttons.anchorMax = new Vector2(1f, 1f);
        buttons.pivot = new Vector2(0.5f, 1f);
        buttons.anchoredPosition = new Vector2(0f, -120f);
        buttons.sizeDelta = new Vector2(-80f, 480f);
        var layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Button resume = MakeButton(buttons, "Riprendi", font, sprite, out _);
        MakeButton(buttons, "Comportamento", font, sprite, out Text behaviorLabel);
        MakeButton(buttons, "Vita", font, sprite, out Text healthLabel);
        MakeButton(buttons, "Hitbox", font, sprite, out Text hitboxLabel);
        MakeButton(buttons, "Ricomincia", font, sprite, out _);
        MakeButton(buttons, "Esci", font, sprite, out _);
        Button[] all = buttons.GetComponentsInChildren<Button>();
        UnityEventTools.AddPersistentListener(all[0].onClick, menu.Close);
        UnityEventTools.AddPersistentListener(all[1].onClick, menu.CycleBehavior);
        UnityEventTools.AddPersistentListener(all[2].onClick, menu.CycleHealth);
        UnityEventTools.AddPersistentListener(all[3].onClick, menu.ToggleHitboxes);
        UnityEventTools.AddPersistentListener(all[4].onClick, menu.Restart);
        UnityEventTools.AddPersistentListener(all[5].onClick, menu.Quit);
        behaviorLabel.text = "Comportamento: Fermo";
        healthLabel.text = "Vita: Infinita";
        hitboxLabel.text = "Hitbox: No";

        // Colonna della lista delle combo.
        RectTransform combos = Box("ListaCombo", panel, sprite, new Vector2(420f, 0f), new Vector2(560f, 640f));
        AddTitle(combos, font, "LISTA COMBO");
        Text legend = CreateText("Legenda", combos, font, 22, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.75f, 0.75f, 0.75f));
        TopBand(legend.rectTransform, 110f, 70f);
        legend.text = "L  pugno leggero: X / Quadrato, tasto J\nH  pugno pesante: Y / Triangolo, tasto K";
        Text list = CreateText("Lista", combos, font, 30, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        list.rectTransform.offsetMin = new Vector2(40f, 30f);
        list.rectTransform.offsetMax = new Vector2(-40f, -200f);
        list.text = "(generata in Play dagli AttackData)";

        SetRefs(menu, ("playerInput", fighterInput), ("settings", settings), ("manager", manager),
                ("panel", panel.gameObject), ("firstButton", resume),
                ("behaviorLabel", behaviorLabel), ("healthLabel", healthLabel), ("hitboxLabel", hitboxLabel));
        SetRefs(comboList, ("player", combat), ("text", list));

        // Nella scena il pannello resta spento: si accende solo in pausa.
        panel.gameObject.SetActive(false);
    }

    // --- Build ----------------------------------------------------------------

    static void SetupBuild(StringBuilder log)
    {
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "INCOM Fighters";
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        AssetDatabase.SaveAssets();
        log.AppendLine("\n# Build");
        log.AppendLine("Scene nella build: solo Arena. Nome del prodotto: INCOM Fighters. Schermo intero in finestra, 1920x1080 in finestra.");
    }

    // --- Utilità UI -------------------------------------------------------------

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static void TopBand(RectTransform rect, float top, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -top);
        rect.sizeDelta = new Vector2(-80f, height);
    }

    static Image AddImage(RectTransform rect, Sprite sprite, Color color)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    static RectTransform Box(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size)
    {
        RectTransform rect = NewRect(name, parent);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        AddImage(rect, sprite, new Color(0.08f, 0.09f, 0.12f, 0.95f));
        return rect;
    }

    static void AddTitle(RectTransform parent, Font font, string title)
    {
        Text text = CreateText("Titolo", parent, font, 48, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        TopBand(text.rectTransform, 36f, 60f);
        text.text = title;
    }

    static Button MakeButton(Transform parent, string name, Font font, Sprite sprite, out Text label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        go.GetComponent<LayoutElement>().preferredHeight = 66f;
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.16f, 0.18f, 0.24f);
        colors.highlightedColor = new Color(0.24f, 0.3f, 0.46f);
        colors.selectedColor = new Color(0.2f, 0.42f, 0.86f);
        colors.pressedColor = new Color(0.32f, 0.55f, 1f);
        colors.fadeDuration = 0.06f;
        button.colors = colors;
        // Su e giù tra i pulsanti; dall'ultimo si torna al primo.
        button.navigation = new Navigation { mode = Navigation.Mode.Vertical, wrapAround = true };

        label = CreateText("Testo", go.transform, font, 28, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        label.rectTransform.offsetMin = new Vector2(28f, 0f);
        label.rectTransform.offsetMax = new Vector2(-20f, 0f);
        label.text = name;
        return button;
    }

    static Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, TextAnchor anchor, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = color;
        text.supportRichText = true;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        Stretch(text.rectTransform);
        return text;
    }

    // --- Utilità ----------------------------------------------------------------

    static T GetOrAdd<T>(GameObject go) where T : Component =>
        go.TryGetComponent(out T existing) ? existing : go.AddComponent<T>();

    static void SetRefs(Object target, params (string field, Object value)[] refs)
    {
        var so = new SerializedObject(target);
        foreach (var (field, value) in refs) so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetArray(Object target, string field, params Object[] values)
    {
        var so = new SerializedObject(target);
        SerializedProperty array = so.FindProperty(field);
        array.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetBool(Object target, string field, bool value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
