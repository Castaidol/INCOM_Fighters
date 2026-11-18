using UnityEngine;

/// <summary>
/// Hitbox e hurtbox visibili anche nella build, quando l'opzione è accesa (TrainingSettings o menu di pausa).
/// I Gizmos esistono solo nell'editor: qui ogni box è un cubo semitrasparente disegnato con Graphics.RenderMesh.
/// Come un Gizmo vale per un solo frame, quindi si ridisegna a ogni LateUpdate: niente GameObject e niente collider.
/// </summary>
public class HitboxViewer : MonoBehaviour
{
    [SerializeField] TrainingSettings settings;
    [Tooltip("Hitbox da mostrare nei loro frame attivi (quella del player).")]
    [SerializeField] Hitbox[] hitboxes;
    [Tooltip("Hurtbox da mostrare sempre (quella del manichino).")]
    [SerializeField] Hurtbox[] hurtboxes;

    [Header("Aspetto")]
    [Tooltip("La mesh Cube di Unity.")]
    [SerializeField] Mesh cube;
    [Tooltip("Materiale trasparente per le hitbox (rosso).")]
    [SerializeField] Material hitboxMaterial;
    [Tooltip("Materiale trasparente per le hurtbox (azzurro).")]
    [SerializeField] Material hurtboxMaterial;

    void LateUpdate()
    {
        if (settings == null || !settings.ShowHitboxes) return;

        foreach (Hurtbox hurtbox in hurtboxes)
        {
            if (hurtbox == null || !hurtbox.isActiveAndEnabled) continue;
            Bounds bounds = hurtbox.Bounds;
            Draw(bounds.center, bounds.size, hurtboxMaterial);
        }
        foreach (Hitbox hitbox in hitboxes)
        {
            if (hitbox != null && hitbox.IsActive) Draw(hitbox.Center, hitbox.Size, hitboxMaterial);
        }
    }

    void Draw(Vector3 center, Vector3 size, Material material)
    {
        // Il cubo di Unity misura 1 m: lo si porta al centro e alla misura della box con una matrice.
        Graphics.RenderMesh(new RenderParams(material), cube, 0, Matrix4x4.TRS(center, Quaternion.identity, size));
    }
}
