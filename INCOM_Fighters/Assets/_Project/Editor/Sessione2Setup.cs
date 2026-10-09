using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Strumento del docente: porta la scena Arena della sessione 1 allo stato finale della sessione 2.
/// Menu: INCOM Fighters > Setup sessione 2. Si può rieseguire. Va lanciato DOPO il setup della sessione 1
/// (che ricrea scena e controller da zero). Resoconto in Logs/Sessione2Setup.txt.
/// </summary>
public static class Sessione2Setup
{
    const string ClipsFolder = "Assets/_Project/Aminations/Player";
    const string ControllerPath = "Assets/_Project/Animator/Fighter.controller";
    const string ScenePath = "Assets/_Project/Scenes/Arena.unity";
    const string AttacksFolder = "Assets/_Project/Data/Attacks";
    const string ReportPath = "Logs/Sessione2Setup.txt";

    // Taglio delle clip in frame della sorgente (30 fps), ricavato con "Analizza clip d'attacco".
    // La Speed dello stato fa durare la clip tagliata quanto i frame data dell'attacco.
    static readonly (string file, float first, float last, string state, float speed, bool inSession2)[] Clips =
    {
        ("X Bot@Lead Jab.fbx",        4.5f, 18.5f,  "Jab",      1.75f,   true),
        ("X Bot@Punching.fbx",        7f,   36f,    "Straight", 2f,      true),
        ("X Bot@Jab Cross.fbx",       10f,  24f,    "Cross",    1.3333f, false),
        ("X Bot@Hook.fbx",            14f,  41f,    "Hook",     2f,      false),
        ("X Bot@BoxingUppercut.fbx",  6f,   39.25f, "Uppercut", 1.9f,    false),
    };

    [MenuItem("INCOM Fighters/Setup sessione 2")]
    public static void Run()
    {
        var log = new StringBuilder();
        log.AppendLine($"Setup sessione 2 - {System.DateTime.Now:yyyy-MM-dd HH:mm}");

        TrimClips(log);
        int fighterLayer = EnsureLayer("Fighter", log);
        int hurtboxLayer = EnsureLayer("Hurtbox", log);

        AttackData jab = SaveAttack("Jab", "Jab", 4, 2, 10, new Vector3(0.95f, 1.38f, 0f), new Vector3(0.3f, 0.25f, 0.5f), 5, log);
        AttackData straight = SaveAttack("Straight", "Straight", 11, 3, 15, new Vector3(0.8f, 1.11f, 0f), new Vector3(0.3f, 0.3f, 0.6f), 12, log);

        AddAttackStates(log);
        if (hurtboxLayer >= 0) SetupScene(jab, straight, hurtboxLayer, log);

        Directory.CreateDirectory("Logs");
        File.WriteAllText(ReportPath, log.ToString());
        Debug.Log("[Sessione2Setup]\n" + log);
    }

    // --- Clip -----------------------------------------------------------------

    static void TrimClips(StringBuilder log)
    {
        log.AppendLine("\n# Taglio delle clip (frame sorgente a 30 fps)");
        foreach (var c in Clips)
        {
            string path = $"{ClipsFolder}/{c.file}";
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
            {
                log.AppendLine($"MANCA {path}");
                continue;
            }
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips.Length == 0) clips = importer.defaultClipAnimations;
            clips[0].firstFrame = c.first;
            clips[0].lastFrame = c.last;
            importer.clipAnimations = clips;
            importer.SaveAndReimport();

            float seconds = (c.last - c.first) / 30f;
            int gameFrames = Mathf.RoundToInt(seconds / c.speed * 60f);
            log.AppendLine($"{c.file}: {c.first}-{c.last}, Speed {c.speed} -> {gameFrames} frame di gioco (stato {c.state})");
        }
    }

    // --- Layer ----------------------------------------------------------------

    static int EnsureLayer(string name, StringBuilder log)
    {
        int existing = LayerMask.NameToLayer(name);
        if (existing >= 0) return existing;

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 6; i < layers.arraySize; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(i);
            if (!string.IsNullOrEmpty(layer.stringValue)) continue;
            layer.stringValue = name;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
            log.AppendLine($"\nLayer {i} = {name}");
            return i;
        }
        log.AppendLine($"\nERRORE: nessun layer libero per {name}");
        return -1;
    }

    // --- AttackData -----------------------------------------------------------

    static AttackData SaveAttack(string fileName, string state, int startup, int active, int recovery,
                                 Vector3 offset, Vector3 size, int damage, StringBuilder log)
    {
        EnsureFolder(AttacksFolder);
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
        EditorUtility.SetDirty(attack);
        AssetDatabase.SaveAssets();
        log.AppendLine($"\n{path}: {startup}/{active}/{recovery} frame, hitbox {offset} x {size}, danno {damage}");
        return attack;
    }

    // --- Animator -------------------------------------------------------------

    static void AddAttackStates(StringBuilder log)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            log.AppendLine("\nERRORE: Fighter.controller non trovato, eseguire prima il setup della sessione 1");
            return;
        }
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        int row = 0;
        foreach (var c in Clips.Where(c => c.inSession2))
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath($"{ClipsFolder}/{c.file}").OfType<AnimationClip>()
                .FirstOrDefault(a => !a.name.StartsWith("__preview__"));
            AnimatorState state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == c.state)
                                  ?? machine.AddState(c.state, new Vector3(500f, 120f + 70f * row, 0f));
            state.motion = clip;
            state.speed = c.speed;
            row++;
            log.AppendLine($"Stato {c.state}: clip '{clip.name}', Speed {c.speed}, nessuna transizione (lo avvia il codice)");
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    // --- Scena ----------------------------------------------------------------

    static void SetupScene(AttackData light, AttackData heavy, int hurtboxLayer, StringBuilder log)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Aprire una scena scarica gli asset non referenziati: gli AttackData vanno ricaricati.
        light = AssetDatabase.LoadAssetAtPath<AttackData>($"{AttacksFolder}/Jab.asset");
        heavy = AssetDatabase.LoadAssetAtPath<AttackData>($"{AttacksFolder}/Straight.asset");

        GameObject player = GameObject.Find("Player");
        GameObject dummy = GameObject.Find("Dummy");
        if (player == null || dummy == null)
        {
            log.AppendLine("\nERRORE: Player o Dummy non trovati nella scena Arena");
            return;
        }

        // Player: Hitbox + FighterCombat
        var hitbox = GetOrAdd<Hitbox>(player);
        var hitboxSO = new SerializedObject(hitbox);
        hitboxSO.FindProperty("hurtboxLayers").intValue = 1 << hurtboxLayer;
        hitboxSO.ApplyModifiedPropertiesWithoutUndo();

        var combat = GetOrAdd<FighterCombat>(player);
        var combatSO = new SerializedObject(combat);
        combatSO.FindProperty("lightAttack").objectReferenceValue = light;
        combatSO.FindProperty("heavyAttack").objectReferenceValue = heavy;
        combatSO.FindProperty("animator").objectReferenceValue = player.GetComponentInChildren<Animator>();
        combatSO.ApplyModifiedPropertiesWithoutUndo();

        // Manichino: Health + DamageLogger e una Hurtbox figlia sul layer Hurtbox
        var health = GetOrAdd<Health>(dummy);
        GetOrAdd<DamageLogger>(dummy);

        Transform hurtboxTransform = dummy.transform.Find("Hurtbox");
        if (hurtboxTransform == null)
        {
            hurtboxTransform = new GameObject("Hurtbox").transform;
            hurtboxTransform.SetParent(dummy.transform, false);
        }
        GameObject hurtboxGO = hurtboxTransform.gameObject;
        hurtboxGO.layer = hurtboxLayer;
        var box = GetOrAdd<BoxCollider>(hurtboxGO);
        box.isTrigger = true;
        box.center = new Vector3(0f, 0.8f, 0f);
        box.size = new Vector3(0.9f, 1.6f, 0.9f);
        var hurtbox = GetOrAdd<Hurtbox>(hurtboxGO);
        var hurtboxSO = new SerializedObject(hurtbox);
        hurtboxSO.FindProperty("health").objectReferenceValue = health;
        hurtboxSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        log.AppendLine($"\n# Scena\nPlayer: Hitbox (layer Hurtbox) e FighterCombat (leggero {light.name}, pesante {heavy.name}).");
        log.AppendLine("Dummy: Health 100, DamageLogger, figlio Hurtbox con BoxCollider trigger 0,9 x 1,6 x 0,9 m.");
    }

    // --- Utilità --------------------------------------------------------------

    static T GetOrAdd<T>(GameObject go) where T : Component =>
        go.TryGetComponent(out T existing) ? existing : go.AddComponent<T>();

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
