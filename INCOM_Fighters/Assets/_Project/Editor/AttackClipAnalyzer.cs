using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Strumento del docente: fa scorrere le clip d'attacco frame per frame sul personaggio Synty
/// e misura dove arrivano le mani. Da qui si ricavano frame di contatto, taglio delle clip e hitbox.
/// Menu: INCOM Fighters > Analizza clip d'attacco. Resoconto in Logs/AnalisiClip.txt.
/// </summary>
public static class AttackClipAnalyzer
{
    const string ClipsFolder = "Assets/_Project/Aminations/Player";
    const string CharacterPath = "Assets/Synty/PolygonFantasyRivals/Prefabs/Characters/SM_Chr_AncientWarrior_01.prefab";
    const string TempControllerPath = "Assets/__AnalisiClip.controller";
    const string ReportPath = "Logs/AnalisiClip.txt";

    static readonly string[] Files =
    {
        "X Bot@Lead Jab.fbx", "X Bot@Jab Cross.fbx", "X Bot@Hook.fbx", "X Bot@Punching.fbx", "X Bot@BoxingUppercut.fbx",
    };

    [MenuItem("INCOM Fighters/Analizza clip d'attacco")]
    public static void Run()
    {
        var log = new StringBuilder();
        log.AppendLine($"Analisi clip d'attacco - {System.DateTime.Now:yyyy-MM-dd HH:mm}");
        log.AppendLine("Personaggio: SM_Chr_AncientWarrior_01, rivolto verso +Z. Misure in metri rispetto ai piedi; frame a 60 fps.");

        var character = (GameObject)Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath));
        character.hideFlags = HideFlags.HideAndDontSave;
        character.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        Animator animator = character.GetComponentInChildren<Animator>();
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        Transform root = character.transform;
        Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

        try
        {
            foreach (string file in Files)
            {
                string path = $"{ClipsFolder}/{file}";
                AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                    .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
                if (clip == null) { log.AppendLine($"\nMANCA {path}"); continue; }

                AssetDatabase.DeleteAsset(TempControllerPath);
                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(TempControllerPath, clip);
                animator.runtimeAnimatorController = controller;
                animator.Rebind();
                int stateHash = controller.layers[0].stateMachine.defaultState.nameHash;

                int n = Mathf.RoundToInt(clip.length * 60f);
                var left = new Vector3[n + 1];
                var right = new Vector3[n + 1];
                for (int i = 0; i <= n; i++)
                {
                    animator.Play(stateHash, 0, n == 0 ? 0f : (float)i / n);
                    animator.Update(0f);
                    left[i] = root.InverseTransformPoint(leftHand.position);
                    right[i] = root.InverseTransformPoint(rightHand.position);
                }
                animator.runtimeAnimatorController = null;

                Describe(log, file, clip, left, right);
            }
        }
        finally
        {
            Object.DestroyImmediate(character);
            AssetDatabase.DeleteAsset(TempControllerPath);
        }

        Directory.CreateDirectory("Logs");
        File.WriteAllText(ReportPath, log.ToString());
        Debug.Log($"[AnalisiClip] resoconto scritto in {ReportPath}");
    }

    static void Describe(StringBuilder log, string file, AnimationClip clip, Vector3[] left, Vector3[] right)
    {
        int n = left.Length - 1;
        float leftPeak = left.Max(p => p.z), rightPeak = right.Max(p => p.z);
        bool useLeft = leftPeak - left[0].z >= rightPeak - right[0].z;
        Vector3[] hand = useLeft ? left : right;
        float[] f = hand.Select(p => p.z).ToArray();

        int peak = System.Array.IndexOf(f, f.Max());
        float guard = f[0];
        float lowThr = guard + 0.1f * (f[peak] - guard);
        float highThr = guard + 0.9f * (f[peak] - guard);

        int start = peak; while (start > 0 && f[start - 1] >= lowThr) start--;
        int contact = start; while (contact < peak && f[contact] < highThr) contact++;
        int extEnd = peak; while (extEnd < n && f[extEnd + 1] >= highThr) extEnd++;
        int back = extEnd; while (back < n && f[back] >= lowThr) back++;

        log.AppendLine($"\n== {file} -> clip '{clip.name}', {clip.length:0.00} s = {n} frame a 60 fps (sorgente {clip.frameRate:0} fps)");
        log.AppendLine($"Mano che colpisce: {(useLeft ? "sinistra" : "destra")}. Picco sinistra {leftPeak:0.00} m, picco destra {rightPeak:0.00} m.");
        log.AppendLine($"Guardia: avanti {guard:0.00} m. Picco: avanti {f[peak]:0.00} m, alto {hand[peak].y:0.00} m, laterale {hand[peak].x:0.00} m al frame {peak}.");
        log.AppendLine($"Inizio movimento {start} | contatto (90%) {contact} | fine estensione {extEnd} | ritorno sotto il 10% {back}");
        log.AppendLine($"Mano al contatto: avanti {hand[contact].z:0.00}, alto {hand[contact].y:0.00}, laterale {hand[contact].x:0.00}");
        log.Append("Avanzamento mano che colpisce (cm, ogni 2 frame): ");
        for (int i = 0; i <= n; i += 2) log.Append(Mathf.RoundToInt(f[i] * 100f)).Append(' ');
        log.AppendLine();
        Vector3[] other = useLeft ? right : left;
        log.Append("Avanzamento altra mano (cm, ogni 2 frame): ");
        for (int i = 0; i <= n; i += 2) log.Append(Mathf.RoundToInt(other[i].z * 100f)).Append(' ');
        log.AppendLine();
        log.Append("Altezza mano che colpisce (cm, ogni 2 frame): ");
        for (int i = 0; i <= n; i += 2) log.Append(Mathf.RoundToInt(hand[i].y * 100f)).Append(' ');
        log.AppendLine();
    }
}
