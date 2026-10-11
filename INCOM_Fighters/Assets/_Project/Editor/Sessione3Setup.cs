using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Strumento del docente: porta la scena Arena della sessione 2 allo stato finale della sessione 3.
/// Menu: INCOM Fighters > Setup sessione 3. Si può rieseguire. Va lanciato DOPO i setup delle sessioni 1 e 2.
/// Resoconto in Logs/Sessione3Setup.txt.
/// </summary>
public static class Sessione3Setup
{
    const string ScenePath = "Assets/_Project/Scenes/Arena.unity";
    const string AttacksFolder = "Assets/_Project/Data/Attacks";
    const string DummyClipsFolder = "Assets/PartyMonsterRumblePBR/Animation";
    const string DummyControllerPath = "Assets/_Project/Animator/Dummy.controller";
    const string SpritesFolder = "Assets/_Project/Sprites";
    const string WhiteSpritePath = SpritesFolder + "/Bianco.png";
    const string ReportPath = "Logs/Sessione3Setup.txt";

    // La clip GetHit viene accelerata fino a durare quanto l'hitstun più lungo della sessione (Straight).
    // Gli attacchi con meno hitstun la interrompono con un CrossFade: la fine la decide il codice.
    const int LongestHitstun = 22;

    [MenuItem("INCOM Fighters/Setup sessione 3")]
    public static void Run()
    {
        var log = new StringBuilder();
        log.AppendLine($"Setup sessione 3 - {System.DateTime.Now:yyyy-MM-dd HH:mm}");

        log.AppendLine("\n# Reazione negli AttackData");
        SetReaction("Jab", 16, 0.15f, log);
        SetReaction("Straight", 22, 0.35f, log);

        CreateDummyController(log);
        CreateWhiteSprite(log);
        SetupScene(log);

        Directory.CreateDirectory("Logs");
        File.WriteAllText(ReportPath, log.ToString());
        Debug.Log("[Sessione3Setup]\n" + log);
    }

    // --- AttackData -----------------------------------------------------------

    static void SetReaction(string fileName, int hitstun, float knockback, StringBuilder log)
    {
        string path = $"{AttacksFolder}/{fileName}.asset";
        var attack = AssetDatabase.LoadAssetAtPath<AttackData>(path);
        if (attack == null)
        {
            log.AppendLine($"ERRORE: {path} non trovato, eseguire prima il setup della sessione 2");
            return;
        }
        attack.hitstun = hitstun;
        attack.knockback = knockback;
        EditorUtility.SetDirty(attack);
        AssetDatabase.SaveAssets();
        log.AppendLine($"{fileName}: hitstun {hitstun} frame, knockback {knockback} m");
    }

    // --- Animator del manichino ----------------------------------------------

    static void CreateDummyController(StringBuilder log)
    {
        AnimationClip idle = LoadClip("Idle01.fbx");
        AnimationClip hit = LoadClip("GetHit.fbx");
        if (idle == null || hit == null)
        {
            log.AppendLine("\nERRORE: clip Idle01 o GetHit del Party Monster non trovate");
            return;
        }

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(DummyControllerPath)
                         ?? AnimatorController.CreateAnimatorControllerAtPath(DummyControllerPath);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in machine.states.ToArray()) machine.RemoveState(child.state);

        AnimatorState idleState = machine.AddState("Idle", new Vector3(300f, 0f, 0f));
        idleState.motion = idle;
        AnimatorState hitState = machine.AddState("Hit", new Vector3(300f, 80f, 0f));
        hitState.motion = hit;

        float clipFrames = hit.length * 60f;
        float speed = Mathf.Max(1f, Mathf.Round(clipFrames / LongestHitstun * 20f) / 20f);
        hitState.speed = speed;
        machine.defaultState = idleState;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        log.AppendLine($"\n# Animator\n{DummyControllerPath}: Idle ({idle.name}, loop {idle.isLooping}) di default, " +
                       $"Hit ({hit.name}, {hit.length:0.00} s = {clipFrames:0} frame, loop {hit.isLooping}) con Speed {speed} " +
                       $"-> {clipFrames / speed:0} frame. Nessuna transizione: li avvia TrainingDummy.");
    }

    static AnimationClip LoadClip(string file) =>
        AssetDatabase.LoadAllAssetsAtPath($"{DummyClipsFolder}/{file}").OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));

    // --- Sprite bianco per le barre -------------------------------------------

    static void CreateWhiteSprite(StringBuilder log)
    {
        if (!File.Exists(WhiteSpritePath))
        {
            EnsureFolder(SpritesFolder);
            var texture = new Texture2D(4, 4);
            texture.SetPixels(Enumerable.Repeat(Color.white, 16).ToArray());
            File.WriteAllBytes(WhiteSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(WhiteSpritePath);
        }
        var importer = (TextureImporter)AssetImporter.GetAtPath(WhiteSpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();
        log.AppendLine($"\n{WhiteSpritePath}: sprite bianco 4 x 4 (un'Image Filled senza Source Image non si riempie)");
    }

    // --- Scena ----------------------------------------------------------------

    static void SetupScene(StringBuilder log)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Aprire una scena scarica gli asset non referenziati: si ricaricano dopo.
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(DummyControllerPath);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);

        GameObject player = GameObject.Find("Player");
        GameObject dummy = GameObject.Find("Dummy");
        var arena = Object.FindAnyObjectByType<ArenaBounds>();
        if (player == null || dummy == null || !dummy.TryGetComponent(out Health health))
        {
            log.AppendLine("\nERRORE: Player o Dummy (con Health) non trovati, eseguire prima i setup delle sessioni 1 e 2");
            return;
        }

        // Manichino: il suo Animator e il TrainingDummy
        Animator dummyAnimator = dummy.GetComponentInChildren<Animator>();
        dummyAnimator.runtimeAnimatorController = controller;
        dummyAnimator.applyRootMotion = false;
        var trainingDummy = GetOrAdd<TrainingDummy>(dummy);
        SetRefs(trainingDummy, ("animator", dummyAnimator), ("arena", arena));

        // Regole dell'allenamento
        GameObject training = GameObject.Find("Training") ?? new GameObject("Training");
        var manager = GetOrAdd<TrainingManager>(training);
        SetRefs(manager, ("playerInput", player.GetComponent<FighterInput>()), ("dummy", trainingDummy));

        BuildHud(health, sprite);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.AppendLine("\n# Scena\nDummy: Animator con Dummy.controller, TrainingDummy (knockback in 6 frame, rigenerazione dopo 120).");
        log.AppendLine("Training: TrainingManager (reset con View/Select o R).");
        log.AppendLine("HUD: Canvas Screen Space Overlay 1920 x 1080, BarraManichino in alto a destra con Riempimento " +
                       "Filled orizzontale (origine a destra), etichetta MANICHINO, TrainingHUD.");
    }

    static void BuildHud(Health dummyHealth, Sprite sprite)
    {
        GameObject old = GameObject.Find("HUD");
        if (old != null) Object.DestroyImmediate(old);

        var canvasGO = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasGO.layer = LayerMask.NameToLayer("UI");
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Barra in alto a destra, dalla parte del manichino come il secondo giocatore nei picchiaduro.
        Image bar = CreateImage("BarraManichino", canvasGO.transform, sprite, new Color(0.08f, 0.08f, 0.1f, 0.85f));
        RectTransform barRect = bar.rectTransform;
        barRect.anchorMin = barRect.anchorMax = barRect.pivot = new Vector2(1f, 1f);
        barRect.anchoredPosition = new Vector2(-60f, -80f);
        barRect.sizeDelta = new Vector2(640f, 36f);

        Image fill = CreateImage("Riempimento", bar.transform, sprite, new Color(1f, 0.78f, 0.1f));
        RectTransform fillRect = fill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Right; // si svuota verso il centro dello schermo
        fill.fillAmount = 1f;

        var labelGO = new GameObject("Nome", typeof(RectTransform), typeof(Text));
        labelGO.layer = canvasGO.layer;
        labelGO.transform.SetParent(canvasGO.transform, false);
        var label = labelGO.GetComponent<Text>();
        label.text = "MANICHINO";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.LowerRight;
        label.color = Color.white;
        label.raycastTarget = false;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = labelRect.anchorMax = labelRect.pivot = new Vector2(1f, 1f);
        labelRect.anchoredPosition = new Vector2(-64f, -36f);
        labelRect.sizeDelta = new Vector2(640f, 40f);

        var hud = canvasGO.AddComponent<TrainingHUD>();
        SetRefs(hud, ("dummyHealth", dummyHealth), ("dummyHealthFill", fill));
    }

    static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    // --- Utilità --------------------------------------------------------------

    static T GetOrAdd<T>(GameObject go) where T : Component =>
        go.TryGetComponent(out T existing) ? existing : go.AddComponent<T>();

    static void SetRefs(Object target, params (string field, Object value)[] refs)
    {
        var so = new SerializedObject(target);
        foreach (var (field, value) in refs) so.FindProperty(field).objectReferenceValue = value;
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
