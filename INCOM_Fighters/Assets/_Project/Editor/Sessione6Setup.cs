using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Strumento del docente: porta il progetto della sessione 5 allo stato finale della sessione 6.
/// Menu: INCOM Fighters > Setup sessione 6. Si può rieseguire. Va lanciato DOPO i setup delle sessioni 1-5.
/// I suoni (Assets/_Project/Audio) sono già nel progetto: sono generati, non registrati.
/// Resoconto in Logs/Sessione6Setup.txt.
/// </summary>
public static class Sessione6Setup
{
    const string ScenePath = "Assets/_Project/Scenes/Arena.unity";
    const string AttacksFolder = "Assets/_Project/Data/Attacks";
    const string AudioFolder = "Assets/_Project/Audio";
    const string FxFolder = "Assets/_Project/Prefabs/FX";
    const string SyntyFx = "Assets/Synty/PolygonFantasyRivals/Prefabs/FX";
    const string ReportPath = "Logs/Sessione6Setup.txt";

    static readonly (string attack, int hitstop, bool heavy)[] Weights =
    {
        ("Jab", 5, false), ("Cross", 6, false), ("Hook", 8, true), ("Straight", 9, true), ("Uppercut", 11, true),
    };

    [MenuItem("INCOM Fighters/Setup sessione 6")]
    public static void Run()
    {
        var log = new StringBuilder();
        log.AppendLine($"Setup sessione 6 - {System.DateTime.Now:yyyy-MM-dd HH:mm}");

        var light = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/Pugno_Leggero.wav");
        var heavy = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/Pugno_Pesante.wav");
        var block = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/Parata.wav");
        if (light == null || heavy == null || block == null)
            log.AppendLine($"ATTENZIONE: suoni mancanti in {AudioFolder}");

        log.AppendLine("\n# Peso dei colpi");
        foreach (var (name, hitstop, isHeavy) in Weights)
        {
            var attack = AssetDatabase.LoadAssetAtPath<AttackData>($"{AttacksFolder}/{name}.asset");
            if (attack == null) { log.AppendLine($"ERRORE: {name}.asset non trovato"); continue; }
            attack.hitstop = hitstop;
            attack.hitSound = isHeavy ? heavy : light;
            EditorUtility.SetDirty(attack);
            log.AppendLine($"{name}: hitstop {hitstop} frame, suono {(isHeavy ? "Pugno_Pesante" : "Pugno_Leggero")}");
        }
        AssetDatabase.SaveAssets();

        MakeEffect("FX_Colpo", "FX_Sparks_01", new Color(1f, 0.85f, 0.45f), 25, 2f, 5f, 0.05f, 0.18f, 0.1f, 0.3f, 0.5f);
        MakeEffect("FX_Parata", "FX_EnergyPush_01", new Color(0.55f, 0.8f, 1f), 40, 1.5f, 3.5f, 0.02f, 0.08f, 0.15f, 0.35f, 0f);
        log.AppendLine($"\n# Effetti\n{FxFolder}/FX_Colpo.prefab: variante di FX_Sparks_01, una sola esplosione di 25 scintille.");
        log.AppendLine($"{FxFolder}/FX_Parata.prefab: variante di FX_EnergyPush_01, 40 particelle azzurre, più piccola e corta.");

        SetupScene(block, log);

        Directory.CreateDirectory("Logs");
        File.WriteAllText(ReportPath, log.ToString());
        Debug.Log("[Sessione6Setup]\n" + log);
    }

    // --- Effetti d'impatto -----------------------------------------------------

    static GameObject MakeEffect(string name, string syntyName, Color color, int burst, float minSpeed, float maxSpeed,
                                 float minSize, float maxSize, float minLife, float maxLife, float gravity)
    {
        EnsureFolder(FxFolder);
        string path = $"{FxFolder}/{name}.prefab";
        var source = AssetDatabase.LoadAssetAtPath<GameObject>($"{SyntyFx}/{syntyName}.prefab");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = name;

        // Un effetto d'impatto è una sola esplosione: niente loop, niente emissione continua.
        var ps = instance.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.duration = 0.3f;
        main.playOnAwake = true;
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startLifetime = new ParticleSystem.MinMaxCurve(minLife, maxLife);
        main.startColor = color;
        main.gravityModifier = gravity;
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burst) });

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    // --- Scena ----------------------------------------------------------------

    static void SetupScene(AudioClip blockSound, StringBuilder log)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Gli asset vanno ricaricati dopo OpenScene.
        blockSound = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioFolder}/Parata.wav");
        var hitFx = AssetDatabase.LoadAssetAtPath<GameObject>($"{FxFolder}/FX_Colpo.prefab");
        var blockFx = AssetDatabase.LoadAssetAtPath<GameObject>($"{FxFolder}/FX_Parata.prefab");

        GameObject player = GameObject.Find("Player");
        GameObject dummy = GameObject.Find("Dummy");
        GameObject hud = GameObject.Find("HUD");
        var fightCamera = Object.FindAnyObjectByType<FightCamera>();
        if (player == null || dummy == null || hud == null || fightCamera == null)
        {
            log.AppendLine("\nERRORE: Player, Dummy, HUD o FightCamera non trovati, eseguire prima i setup delle sessioni 1-5");
            return;
        }

        // Oggetto Feedback: HitStop, HitFeedback e la sorgente audio.
        GameObject feedback = GameObject.Find("Feedback") ?? new GameObject("Feedback");
        var hitStop = GetOrAdd<HitStop>(feedback);
        var audio = GetOrAdd<AudioSource>(feedback);
        audio.playOnAwake = false;
        audio.spatialBlend = 0f;
        var hitFeedback = GetOrAdd<HitFeedback>(feedback);

        Animator playerAnimator = player.GetComponentInChildren<Animator>();
        Animator dummyAnimator = dummy.GetComponentInChildren<Animator>();
        var hitStopSO = new SerializedObject(hitStop);
        SerializedProperty animators = hitStopSO.FindProperty("animators");
        animators.arraySize = 2;
        animators.GetArrayElementAtIndex(0).objectReferenceValue = playerAnimator;
        animators.GetArrayElementAtIndex(1).objectReferenceValue = dummyAnimator;
        hitStopSO.ApplyModifiedPropertiesWithoutUndo();

        SetRefs(player.GetComponent<FighterCombat>(), ("hitStop", hitStop));
        SetRefs(player.GetComponent<FighterInput>(), ("hitStop", hitStop));
        SetRefs(dummy.GetComponent<TrainingDummy>(), ("hitStop", hitStop));
        SetRefs(hitFeedback, ("playerHitbox", player.GetComponent<Hitbox>()), ("hitStop", hitStop), ("fightCamera", fightCamera),
                ("hitEffect", hitFx), ("blockEffect", blockFx), ("blockSound", blockSound));

        BuildHud(hud, player, dummy.GetComponent<ComboTracker>(), hitStop);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.AppendLine("\n# Scena");
        log.AppendLine("Feedback: HitStop (Animator di player e manichino), HitFeedback (FX_Colpo, FX_Parata, suono Parata, shake 0,06 / 0,025 m), AudioSource 2D.");
        log.AppendLine("HitStop collegato a FighterCombat, FighterInput e TrainingDummy.");
        log.AppendLine("HUD: Combo (sotto la barra del manichino), Parato, InputDisplay (a sinistra, sopra i frame data).");
    }

    static void BuildHud(GameObject hud, GameObject player, ComboTracker comboTracker, HitStop hitStop)
    {
        foreach (string old in new[] { "Combo", "Parato", "InputDisplay" })
        {
            Transform t = hud.transform.Find(old);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Bianco.png");

        // Contatore combo, sotto la barra del manichino.
        var combo = CreateGroup("Combo", hud.transform, new Vector2(1f, 1f), new Vector2(-60f, -130f), new Vector2(640f, 120f));
        Text comboText = CreateText("Testo", combo.transform, font, 56, FontStyle.Bold, TextAnchor.UpperRight, new Color(1f, 0.82f, 0.3f));
        comboText.text = "3 COLPI\n<size=30>22 danni</size>";
        var comboShadow = comboText.gameObject.AddComponent<Shadow>();
        comboShadow.effectDistance = new Vector2(3f, -3f);

        // Scritta PARATO, sotto il contatore.
        var parato = CreateGroup("Parato", hud.transform, new Vector2(1f, 1f), new Vector2(-60f, -260f), new Vector2(640f, 70f));
        Text paratoText = CreateText("Testo", parato.transform, font, 48, FontStyle.Bold, TextAnchor.UpperRight, new Color(0.45f, 0.75f, 1f));
        paratoText.text = "PARATO";
        paratoText.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3f, -3f);

        // Input display, a sinistra sopra il pannello dei frame data.
        var displayGO = new GameObject("InputDisplay", typeof(RectTransform), typeof(Image));
        displayGO.layer = hud.layer;
        displayGO.transform.SetParent(hud.transform, false);
        var bg = displayGO.GetComponent<Image>();
        bg.sprite = sprite;
        bg.color = new Color(0.05f, 0.05f, 0.08f, 0.5f);
        bg.raycastTarget = false;
        RectTransform rect = bg.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(40f, 160f);
        rect.sizeDelta = new Vector2(160f, 330f);
        Text displayText = CreateText("Testo", displayGO.transform, font, 26, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        displayText.rectTransform.offsetMin = new Vector2(16f, 10f);
        displayText.rectTransform.offsetMax = new Vector2(-10f, -10f);
        var inputDisplay = displayGO.AddComponent<InputDisplay>();
        SetRefs(inputDisplay, ("input", player.GetComponent<FighterInput>()), ("movement", player.GetComponent<FighterMovement>()),
                ("hitStop", hitStop), ("text", displayText));

        SetRefs(hud.GetComponent<TrainingHUD>(), ("comboTracker", comboTracker), ("comboGroup", combo),
                ("comboText", comboText), ("blockedGroup", parato));
    }

    static CanvasGroup CreateGroup(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var group = go.GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        return group;
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
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return text;
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
