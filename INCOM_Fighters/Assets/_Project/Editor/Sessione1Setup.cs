using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Strumento del docente: prepara la soluzione di riferimento della sessione 1.
/// Menu: INCOM Fighters > Setup sessione 1. Si può rieseguire: ricrea controller e scena da zero.
/// Scrive un resoconto in Logs/Sessione1Setup.txt.
/// </summary>
public static class Sessione1Setup
{
    const string ClipsFolder = "Assets/_Project/Aminations/Player";
    const string ControllerPath = "Assets/_Project/Animator/Fighter.controller";
    const string ActionsPath = "Assets/_Project/Input/Fighter.inputactions";
    const string ScenePath = "Assets/_Project/Scenes/Arena.unity";
    const string PlayerPrefabPath = "Assets/Synty/PolygonFantasyRivals/Prefabs/Characters/SM_Chr_AncientWarrior_01.prefab";
    const string DummyPrefabPath = "Assets/PartyMonsterRumblePBR/Prefab/C01.prefab";
    const string ReportPath = "Logs/Sessione1Setup.txt";

    // File Mixamo e se la clip va in loop.
    static readonly (string file, bool loop)[] MixamoClips =
    {
        ("X Bot@Idle.fbx", true),
        ("X Bot@Walking.fbx", true),
        ("X Bot@Walking Backwards.fbx", true),
        ("X Bot@Lead Jab.fbx", false),
        ("X Bot@Jab Cross.fbx", false),
        ("X Bot@Hook.fbx", false),
        ("X Bot@Punching.fbx", false),
        ("X Bot@BoxingUppercut.fbx", false),
    };

    [MenuItem("INCOM Fighters/Setup sessione 1")]
    public static void Run()
    {
        var log = new StringBuilder();
        log.AppendLine($"Setup sessione 1 - {System.DateTime.Now:yyyy-MM-dd HH:mm}");

        ConfigureMixamoClips(log);
        ConfigurePartyMonsterClips(log);
        SetFixedTimestep(log);

        AnimatorController controller = CreateFighterController(log);
        if (controller != null) CreateArenaScene(controller, log);

        Directory.CreateDirectory("Logs");
        File.WriteAllText(ReportPath, log.ToString());
        Debug.Log("[Sessione1Setup]\n" + log);
    }

    // --- Clip -----------------------------------------------------------------

    static void ConfigureMixamoClips(StringBuilder log)
    {
        log.AppendLine("\n# Clip Mixamo (Humanoid, Bake Into Pose su rotazione, Y e XZ)");
        foreach (var (file, loop) in MixamoClips)
        {
            string path = $"{ClipsFolder}/{file}";
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
            {
                log.AppendLine($"MANCA {path}");
                continue;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;

            string niceName = Path.GetFileNameWithoutExtension(file).Split('@').Last();
            foreach (ModelImporterClipAnimation c in clips)
            {
                if (string.IsNullOrEmpty(c.name) || c.name == "mixamo.com") c.name = niceName;
                c.loopTime = loop;
                c.loopPose = loop;
                c.lockRootRotation = true;    // Root Transform Rotation: Bake Into Pose
                c.lockRootHeightY = true;     // Root Transform Position (Y): Bake Into Pose
                c.lockRootPositionXZ = true;  // Root Transform Position (XZ): Bake Into Pose
                c.keepOriginalOrientation = true;
                c.keepOriginalPositionY = true;
                c.keepOriginalPositionXZ = true;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();

            AnimationClip clip = LoadClip(path);
            if (clip != null)
                log.AppendLine($"{file}: '{clip.name}', {clip.length:0.00} s = {Mathf.RoundToInt(clip.length * 60f)} frame a 60 fps, loop {(loop ? "sì" : "no")}");
        }
    }

    static void ConfigurePartyMonsterClips(StringBuilder log)
    {
        log.AppendLine("\n# Clip Party Monster");
        foreach (string name in new[] { "GetHit", "DefenseHit" })
        {
            string path = $"Assets/PartyMonsterRumblePBR/Animation/{name}.fbx";
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
            {
                log.AppendLine($"MANCA {path}");
                continue;
            }
            ModelImporterClipAnimation[] clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            foreach (ModelImporterClipAnimation c in clips) c.loopTime = false;
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            log.AppendLine($"{name}: Loop Time disattivato");
        }
    }

    static void SetFixedTimestep(StringBuilder log)
    {
        Object timeManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset").FirstOrDefault();
        if (timeManager == null) return;

        var so = new SerializedObject(timeManager);
        SerializedProperty step = so.FindProperty("Fixed Timestep");
        SerializedProperty count = step?.FindPropertyRelative("m_Count");
        SerializedProperty numerator = step?.FindPropertyRelative("m_Rate.m_Numerator");
        SerializedProperty denominator = step?.FindPropertyRelative("m_Rate.m_Denominator");

        if (count != null && numerator != null && denominator != null)
        {
            // Unity 6 salva il Fixed Timestep come frazione: secondi = m_Count / (m_Numerator / m_Denominator).
            count.longValue = numerator.longValue / (denominator.longValue * 60);
        }
        else if (step != null && step.propertyType == SerializedPropertyType.Float)
        {
            step.floatValue = 1f / 60f;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        log.AppendLine($"\nFixed Timestep = {Time.fixedDeltaTime:0.0000000} s (1/60: un FixedUpdate = un frame)");
    }

    // --- Animator Controller --------------------------------------------------

    static AnimatorController CreateFighterController(StringBuilder log)
    {
        AnimationClip idle = LoadClip($"{ClipsFolder}/X Bot@Idle.fbx");
        AnimationClip walkFwd = LoadClip($"{ClipsFolder}/X Bot@Walking.fbx");
        AnimationClip walkBack = LoadClip($"{ClipsFolder}/X Bot@Walking Backwards.fbx");
        if (idle == null || walkFwd == null || walkBack == null)
        {
            log.AppendLine("ERRORE: mancano le clip di locomozione, controller non creato");
            return null;
        }

        EnsureFolder(Path.GetDirectoryName(ControllerPath));
        AssetDatabase.DeleteAsset(ControllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);

        AnimatorState locomotion = controller.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
        tree.blendType = BlendTreeType.Simple1D;
        tree.blendParameter = "MoveX";
        tree.useAutomaticThresholds = false;
        tree.AddChild(walkBack, -1f);
        tree.AddChild(idle, 0f);
        tree.AddChild(walkFwd, 1f);
        controller.layers[0].stateMachine.defaultState = locomotion;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        log.AppendLine($"\n# Controller\n{ControllerPath}: stato Locomotion, blend tree 1D su MoveX (WalkBack -1, Idle 0, WalkFwd +1)");
        return controller;
    }

    // --- Scena ----------------------------------------------------------------

    static void CreateArenaScene(AnimatorController controller, StringBuilder log)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            log.AppendLine("Scena non creata: operazione annullata");
            return;
        }
        // Scena nuova con camera e luce di default (URP aggiunge da solo i suoi componenti).
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EnsureFolder(Path.GetDirectoryName(ScenePath));
        AssetDatabase.DeleteAsset(ScenePath);

        // Pavimento: un Plane di 20 x 10 m.
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(2f, 1f, 1f);

        // Arena
        var arena = new GameObject("Arena").AddComponent<ArenaBounds>();

        // Player
        var player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath));
        player.name = "Player";
        player.transform.SetPositionAndRotation(new Vector3(-1.5f, 0f, 0f), Quaternion.LookRotation(Vector3.right));
        Animator playerAnimator = player.GetComponentInChildren<Animator>();
        playerAnimator.runtimeAnimatorController = controller;
        playerAnimator.applyRootMotion = false;

        var playerInput = player.AddComponent<PlayerInput>();
        playerInput.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
        playerInput.defaultActionMap = "Fighter";
        playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

        var playerPushbox = player.AddComponent<Pushbox>();
        SetFields(playerPushbox, ("halfWidth", 0.35f), ("height", 1.9f));
        player.AddComponent<FighterInput>();
        var movement = player.AddComponent<FighterMovement>();

        // Manichino
        var dummy = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(DummyPrefabPath));
        dummy.name = "Dummy";
        dummy.transform.SetPositionAndRotation(new Vector3(1.5f, 0f, 0f), Quaternion.LookRotation(Vector3.left));
        Animator dummyAnimator = dummy.GetComponentInChildren<Animator>();
        if (dummyAnimator != null) dummyAnimator.applyRootMotion = false;
        var dummyPushbox = dummy.AddComponent<Pushbox>();
        SetFields(dummyPushbox, ("halfWidth", 0.55f), ("height", 1.6f));

        var movementSO = new SerializedObject(movement);
        movementSO.FindProperty("opponent").objectReferenceValue = dummy.transform;
        movementSO.FindProperty("arena").objectReferenceValue = arena;
        movementSO.FindProperty("animator").objectReferenceValue = playerAnimator;
        movementSO.ApplyModifiedPropertiesWithoutUndo();

        // Camera laterale
        Camera cam = Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
        cam.transform.SetPositionAndRotation(new Vector3(0f, 1.2f, -7f), Quaternion.identity);
        cam.fieldOfView = 30f;
        var fightCamera = cam.gameObject.AddComponent<FightCamera>();
        var camSO = new SerializedObject(fightCamera);
        camSO.FindProperty("fighterA").objectReferenceValue = player.transform;
        camSO.FindProperty("fighterB").objectReferenceValue = dummy.transform;
        camSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenePath);

        var buildScenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
        buildScenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();

        log.AppendLine($"\n# Scena\n{ScenePath}: Player ({Path.GetFileNameWithoutExtension(PlayerPrefabPath)}) a x = -1,5, Dummy (C01) a x = +1,5, arena da -6 a +6, camera FOV 30. Prima scena in Build Settings.");
    }

    // --- Utilità --------------------------------------------------------------

    static AnimationClip LoadClip(string fbxPath) =>
        AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));

    static void SetFields(Object target, params (string name, float value)[] fields)
    {
        var so = new SerializedObject(target);
        foreach (var (name, value) in fields) so.FindProperty(name).floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void EnsureFolder(string path)
    {
        path = path.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
