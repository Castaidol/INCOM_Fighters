using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Strumento del docente: porta il progetto della sessione 3 allo stato finale della sessione 4.
/// Menu: INCOM Fighters > Setup sessione 4. Si può rieseguire. Va lanciato DOPO i setup delle sessioni 1, 2 e 3.
/// Resoconto in Logs/Sessione4Setup.txt, con il calcolo delle combo vere per ogni collegamento dell'albero.
/// </summary>
public static class Sessione4Setup
{
    const string ClipsFolder = "Assets/_Project/Aminations/Player";
    const string ControllerPath = "Assets/_Project/Animator/Fighter.controller";
    const string ScenePath = "Assets/_Project/Scenes/Arena.unity";
    const string AttacksFolder = "Assets/_Project/Data/Attacks";
    const string ReportPath = "Logs/Sessione4Setup.txt";

    // Stati nuovi: le clip sono già tagliate dalla sessione 2.
    static readonly (string file, string state, float speed)[] States =
    {
        ("X Bot@Jab Cross.fbx",       "Cross",    1.3333f),
        ("X Bot@Hook.fbx",            "Hook",     2f),
        ("X Bot@BoxingUppercut.fbx",  "Uppercut", 1.9f),
    };

    [MenuItem("INCOM Fighters/Setup sessione 4")]
    public static void Run()
    {
        var log = new StringBuilder();
        log.AppendLine($"Setup sessione 4 - {System.DateTime.Now:yyyy-MM-dd HH:mm}");

        AddStates(log);

        log.AppendLine("\n# AttackData");
        AttackData jab = Load("Jab", log);
        AttackData straight = Load("Straight", log);
        if (jab == null || straight == null) { Save(log); return; }

        AttackData cross = Create("Cross", "Cross", 6, 3, 12, new Vector3(1.02f, 1.27f, 0f), new Vector3(0.3f, 0.25f, 0.5f), 7, 18, 0.2f);
        AttackData hook = Create("Hook", "Hook", 8, 3, 16, new Vector3(1.26f, 0.67f, 0f), new Vector3(0.35f, 0.35f, 0.6f), 10, 22, 0.3f);
        AttackData uppercut = Create("Uppercut", "Uppercut", 10, 3, 22, new Vector3(1.1f, 1.13f, 0f), new Vector3(0.3f, 0.4f, 0.5f), 15, 28, 0.5f);

        // L'albero delle combo: finestra e seguiti. L'Uppercut è un solo asset usato in due rami.
        Link(jab, 5, cross, null);
        Link(cross, 8, hook, uppercut);
        Link(straight, 13, null, uppercut);
        Link(hook, 0, null, null);
        Link(uppercut, 0, null, null);
        AssetDatabase.SaveAssets();

        foreach (AttackData a in new[] { jab, cross, hook, straight, uppercut })
            log.AppendLine($"{a.name}: {a.startup}/{a.active}/{a.recovery} (totale {a.TotalFrames}), finestra da {(a.comboWindowStart > 0 ? a.comboWindowStart.ToString() : "-")}, " +
                           $"danno {a.damage}, hitstun {a.hitstun}, knockback {a.knockback} m, L -> {Name(a.nextOnLight)}, H -> {Name(a.nextOnHeavy)}");

        log.AppendLine("\n# Combo vere (colpo a segno sul primo frame attivo)");
        log.AppendLine("Il colpo successivo arriva dopo (frame d'ingresso - frame del colpo) + il suo startup: è una combo se non supera l'hitstun.");
        Check(jab, cross, log);
        Check(cross, hook, log);
        Check(cross, uppercut, log);
        Check(straight, uppercut, log);

        SetupScene(log);
        Save(log);
    }

    // --- Animator -------------------------------------------------------------

    static void AddStates(StringBuilder log)
    {
        log.AppendLine("\n# Animator");
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            log.AppendLine("ERRORE: Fighter.controller non trovato, eseguire prima il setup della sessione 1");
            return;
        }
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        int row = 2; // Jab e Straight occupano le prime due righe
        foreach (var s in States)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath($"{ClipsFolder}/{s.file}").OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
            AnimatorState state = machine.states.Select(c => c.state).FirstOrDefault(st => st.name == s.state)
                                  ?? machine.AddState(s.state, new Vector3(500f, 120f + 70f * row, 0f));
            state.motion = clip;
            state.speed = s.speed;
            row++;
            float frames = clip.length / s.speed * 60f;
            log.AppendLine($"Stato {s.state}: clip '{clip.name}', Speed {s.speed} -> {frames:0} frame, nessuna transizione");
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    // --- AttackData -----------------------------------------------------------

    static AttackData Load(string fileName, StringBuilder log)
    {
        var attack = AssetDatabase.LoadAssetAtPath<AttackData>($"{AttacksFolder}/{fileName}.asset");
        if (attack == null) log.AppendLine($"ERRORE: {fileName}.asset non trovato, eseguire prima i setup delle sessioni 2 e 3");
        return attack;
    }

    static AttackData Create(string fileName, string state, int startup, int active, int recovery,
                             Vector3 offset, Vector3 size, int damage, int hitstun, float knockback)
    {
        string path = $"{AttacksFolder}/{fileName}.asset";
        var attack = AssetDatabase.LoadAssetAtPath<AttackData>(path);
        if (attack == null)
        {
            attack = ScriptableObject.CreateInstance<AttackData>();
            AssetDatabase.CreateAsset(attack, path);
        }
        attack.stateName = state;
        attack.startup = startup;
        attack.active = active;
        attack.recovery = recovery;
        attack.hitboxOffset = offset;
        attack.hitboxSize = size;
        attack.damage = damage;
        attack.hitstun = hitstun;
        attack.knockback = knockback;
        EditorUtility.SetDirty(attack);
        return attack;
    }

    static void Link(AttackData attack, int windowStart, AttackData onLight, AttackData onHeavy)
    {
        attack.comboWindowStart = windowStart;
        attack.nextOnLight = onLight;
        attack.nextOnHeavy = onHeavy;
        EditorUtility.SetDirty(attack);
    }

    static void Check(AttackData from, AttackData to, StringBuilder log)
    {
        int hitFrame = from.startup + 1;
        int firstEntry = Mathf.Max(from.comboWindowStart, hitFrame);
        int minGap = firstEntry - hitFrame + to.startup;
        int maxGap = from.TotalFrames - hitFrame + to.startup;
        int lastTrueEntry = from.hitstun + hitFrame - to.startup;
        log.AppendLine($"{from.name} -> {to.name}: colpo al frame {hitFrame}, finestra {firstEntry}-{from.TotalFrames}, " +
                       $"tra i due colpi {minGap}-{maxGap} frame, hitstun {from.hitstun}: " +
                       $"combo vera premendo entro il frame {Mathf.Min(lastTrueEntry, from.TotalFrames)} di {from.name}");
    }

    static string Name(AttackData a) => a != null ? a.name : "-";

    // --- Scena ----------------------------------------------------------------

    static void SetupScene(StringBuilder log)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject dummy = GameObject.Find("Dummy");
        if (dummy == null || !dummy.TryGetComponent(out TrainingDummy _))
        {
            log.AppendLine("\nERRORE: Dummy con TrainingDummy non trovato, eseguire prima il setup della sessione 3");
            return;
        }
        if (!dummy.TryGetComponent(out ComboTracker _)) dummy.AddComponent<ComboTracker>();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.AppendLine("\n# Scena\nDummy: ComboTracker (scrive in Console le combo da 2 colpi in su).");
    }

    static void Save(StringBuilder log)
    {
        Directory.CreateDirectory("Logs");
        File.WriteAllText(ReportPath, log.ToString());
        Debug.Log("[Sessione4Setup]\n" + log);
    }
}
