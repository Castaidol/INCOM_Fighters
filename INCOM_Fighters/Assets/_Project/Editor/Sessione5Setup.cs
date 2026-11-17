using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Strumento del docente: porta il progetto della sessione 4 allo stato finale della sessione 5.
/// Menu: INCOM Fighters > Setup sessione 5. Si può rieseguire. Va lanciato DOPO i setup delle sessioni 1-4.
/// Resoconto in Logs/Sessione5Setup.txt, con il vantaggio in frame di ogni attacco.
/// </summary>
public static class Sessione5Setup
{
    const string ScenePath = "Assets/_Project/Scenes/Arena.unity";
    const string AttacksFolder = "Assets/_Project/Data/Attacks";
    const string DummyClipsFolder = "Assets/PartyMonsterRumblePBR/Animation";
    const string DummyControllerPath = "Assets/_Project/Animator/Dummy.controller";
    const string ReportPath = "Logs/Sessione5Setup.txt";

    // La clip DefenseHit viene accelerata fino a durare quanto il blockstun più lungo (Uppercut):
    // i blockstun più corti la interrompono con un CrossFade, come GetHit con l'hitstun.
    const int LongestBlockstun = 16;

    static readonly (string attack, int blockstun)[] Blockstun =
    {
        ("Jab", 10), ("Cross", 12), ("Hook", 14), ("Straight", 14), ("Uppercut", 16),
    };

    [MenuItem("INCOM Fighters/Setup sessione 5")]
    public static void Run()
    {
        var log = new StringBuilder();
        log.AppendLine($"Setup sessione 5 - {System.DateTime.Now:yyyy-MM-dd HH:mm}");

        SetBlockstun(log);
        int getUpFrames = AddDummyStates(log);
        SetupScene(getUpFrames, log);

        Directory.CreateDirectory("Logs");
        File.WriteAllText(ReportPath, log.ToString());
        Debug.Log("[Sessione5Setup]\n" + log);
    }

    // --- AttackData -----------------------------------------------------------

    static void SetBlockstun(StringBuilder log)
    {
        log.AppendLine("\n# Blockstun e vantaggio (colpo sul primo frame attivo)");
        log.AppendLine("Vantaggio = hitstun (o blockstun) - (active - 1 + recovery)");
        foreach (var (name, blockstun) in Blockstun)
        {
            var attack = AssetDatabase.LoadAssetAtPath<AttackData>($"{AttacksFolder}/{name}.asset");
            if (attack == null)
            {
                log.AppendLine($"ERRORE: {name}.asset non trovato, eseguire prima il setup della sessione 4");
                continue;
            }
            attack.blockstun = blockstun;
            EditorUtility.SetDirty(attack);
            log.AppendLine($"{name}: hitstun {attack.hitstun}, blockstun {blockstun}, frame dopo il colpo {attack.FramesAfterHit} " +
                           $"-> a segno {attack.AdvantageOnHit:+0;-0;0}, in parata {attack.AdvantageOnBlock:+0;-0;0}");
        }
        AssetDatabase.SaveAssets();
    }

    // --- Animator del manichino ----------------------------------------------

    static int AddDummyStates(StringBuilder log)
    {
        log.AppendLine("\n# Animator del manichino");
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(DummyControllerPath);
        if (controller == null)
        {
            log.AppendLine("ERRORE: Dummy.controller non trovato, eseguire prima il setup della sessione 3");
            return 52;
        }
        AnimatorStateMachine machine = controller.layers[0].stateMachine;

        AnimationClip defenseHit = LoadClip("DefenseHit.fbx");
        float blockSpeed = Mathf.Max(1f, Mathf.Round(defenseHit.length * 60f / LongestBlockstun * 20f) / 20f);
        AnimationClip getUp = LoadClip("Die01Recover.fbx");

        AddState(machine, "Guard", LoadClip("Defense.fbx"), 1f, 0, log);
        AddState(machine, "Block", defenseHit, blockSpeed, 1, log);
        AddState(machine, "KO", LoadClip("Die01.fbx"), 1f, 2, log);
        AddState(machine, "GetUp", getUp, 1f, 3, log);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return Mathf.RoundToInt(getUp.length * 60f);
    }

    static void AddState(AnimatorStateMachine machine, string name, AnimationClip clip, float speed, int row, StringBuilder log)
    {
        AnimatorState state = machine.states.Select(c => c.state).FirstOrDefault(s => s.name == name)
                              ?? machine.AddState(name, new Vector3(600f, 80f * row, 0f));
        state.motion = clip;
        state.speed = speed;
        float frames = clip.length * 60f;
        log.AppendLine($"Stato {name}: clip {clip.name} ({frames:0} frame, loop {clip.isLooping}), Speed {speed} -> {frames / speed:0} frame");
    }

    static AnimationClip LoadClip(string file) =>
        AssetDatabase.LoadAllAssetsAtPath($"{DummyClipsFolder}/{file}").OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));

    // --- Scena ----------------------------------------------------------------

    static void SetupScene(int getUpFrames, StringBuilder log)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject player = GameObject.Find("Player");
        GameObject dummy = GameObject.Find("Dummy");
        GameObject training = GameObject.Find("Training");
        GameObject hud = GameObject.Find("HUD");
        if (player == null || dummy == null || training == null || hud == null)
        {
            log.AppendLine("\nERRORE: Player, Dummy, Training o HUD non trovati, eseguire prima i setup delle sessioni 1-4");
            return;
        }

        var settings = training.TryGetComponent(out TrainingSettings existing) ? existing : training.AddComponent<TrainingSettings>();
        var dummySO = new SerializedObject(dummy.GetComponent<TrainingDummy>());
        dummySO.FindProperty("settings").objectReferenceValue = settings;
        dummySO.FindProperty("getUpFrames").intValue = getUpFrames;
        dummySO.ApplyModifiedPropertiesWithoutUndo();

        BuildFrameDataPanel(hud, player.GetComponent<FighterCombat>());

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.AppendLine("\n# Scena");
        log.AppendLine("Training: TrainingSettings (Comportamento Fermo, Vita Infinita).");
        log.AppendLine($"Dummy: TrainingDummy con TrainingSettings, KO 90 frame, rialzo {getUpFrames} frame, knockback in parata 50%, guardia 60 frame.");
        log.AppendLine("HUD: pannello FrameData in basso a sinistra (nome, startup, active, recovery, vantaggio, esito).");
    }

    static void BuildFrameDataPanel(GameObject hud, FighterCombat player)
    {
        Transform old = hud.transform.Find("FrameData");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Bianco.png");
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var panelGO = new GameObject("FrameData", typeof(RectTransform), typeof(Image));
        panelGO.layer = hud.layer;
        panelGO.transform.SetParent(hud.transform, false);
        var panel = panelGO.GetComponent<Image>();
        panel.sprite = sprite;
        panel.color = new Color(0.05f, 0.05f, 0.08f, 0.75f);
        panel.raycastTarget = false;
        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = Vector2.zero;
        panelRect.anchoredPosition = new Vector2(40f, 40f);
        panelRect.sizeDelta = new Vector2(820f, 100f);

        Text data = CreateText("Dati", panelGO.transform, font, 26, FontStyle.Normal, TextAnchor.MiddleLeft);
        data.rectTransform.offsetMin = new Vector2(20f, 8f);
        data.rectTransform.offsetMax = new Vector2(-200f, -8f);

        Text result = CreateText("Esito", panelGO.transform, font, 30, FontStyle.Bold, TextAnchor.MiddleRight);
        result.rectTransform.offsetMin = new Vector2(600f, 8f);
        result.rectTransform.offsetMax = new Vector2(-20f, -8f);

        var hudSO = new SerializedObject(hud.GetComponent<TrainingHUD>());
        hudSO.FindProperty("player").objectReferenceValue = player;
        hudSO.FindProperty("frameDataText").objectReferenceValue = data;
        hudSO.FindProperty("resultText").objectReferenceValue = result;
        hudSO.ApplyModifiedPropertiesWithoutUndo();
    }

    static Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = Color.white;
        text.supportRichText = true;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        return text;
    }
}
