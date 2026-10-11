using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// La zona che colpisce. FighterCombat la aggiorna a ogni frame dell'attacco:
/// nei frame attivi cerca le hurtbox con Physics.OverlapBox e colpisce ognuna una sola volta per attacco.
/// </summary>
public class Hitbox : MonoBehaviour
{
    [Tooltip("Layer su cui stanno le hurtbox da colpire.")]
    [SerializeField] LayerMask hurtboxLayers;

    readonly HashSet<Hurtbox> alreadyHit = new();

    AttackData attack;
    Vector3 center;
    bool isActive;

    /// <summary>Inizio di un nuovo attacco: dimentica chi è stato colpito da quello precedente.</summary>
    public void Begin(AttackData data)
    {
        attack = data;
        alreadyHit.Clear();
        isActive = false;
    }

    /// <summary>Chiamato a ogni frame dell'attacco. facing: +1 guarda a destra, -1 a sinistra.</summary>
    public void Tick(bool active, float facing)
    {
        if (attack == null) return;

        // L'offset è scritto "verso l'avversario": lo giriamo dal lato in cui guarda il combattente.
        Vector3 offset = attack.hitboxOffset;
        offset.x *= facing;
        center = transform.position + offset;
        isActive = active;
        if (!active) return;

        Collider[] hits = Physics.OverlapBox(center, attack.hitboxSize * 0.5f, Quaternion.identity,
                                             hurtboxLayers, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out Hurtbox hurtbox)) continue;
            if (hurtbox.transform.root == transform.root) continue; // non ci si colpisce da soli
            if (!alreadyHit.Add(hurtbox)) continue;                 // già colpita da questo attacco
            hurtbox.ReceiveHit(attack, transform);
        }
    }

    /// <summary>Fine dell'attacco.</summary>
    public void End()
    {
        attack = null;
        isActive = false;
    }

    void OnDrawGizmos()
    {
        if (attack == null) return;
        // Rossa piena nei frame attivi, grigia nel resto dell'attacco: aiuta ad allinearla al pugno.
        Gizmos.color = isActive ? new Color(1f, 0.1f, 0.1f, 0.6f) : new Color(0.7f, 0.7f, 0.7f, 0.8f);
        if (isActive) Gizmos.DrawCube(center, attack.hitboxSize);
        else Gizmos.DrawWireCube(center, attack.hitboxSize);
    }
}
