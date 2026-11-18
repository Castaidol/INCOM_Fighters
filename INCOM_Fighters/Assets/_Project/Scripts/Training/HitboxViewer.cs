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
        // TODO 6c (sessione 7): se settings è null o l'opzione settings.ShowHitboxes è spenta non disegnare niente.
        //   Per ogni hurtbox non null e accesa (isActiveAndEnabled) disegna un cubo con il centro e la misura di
        //   hurtbox.Bounds e con hurtboxMaterial. Per ogni hitbox non null nei frame attivi (IsActive) disegna un cubo
        //   in Center, grande Size, con hitboxMaterial.
    }

    void Draw(Vector3 center, Vector3 size, Material material)
    {
        // TODO 6d: Graphics.RenderMesh(new RenderParams(material), cube, 0, matrice) disegna la mesh per un solo frame.
        //   Il cubo di Unity misura 1 m: la matrice lo porta al centro e alla misura della box,
        //   Matrix4x4.TRS(center, Quaternion.identity, size).
    }
}
