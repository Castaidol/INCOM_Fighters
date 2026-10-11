using UnityEngine;

/// <summary>
/// La zona che può essere colpita. Serve un Collider (anche trigger) su un layer che la Hitbox cerca.
/// Inoltra il danno alla Health e avvisa chi è iscritto a OnHit (il manichino, per reagire).
/// </summary>
[RequireComponent(typeof(Collider))]
public class Hurtbox : MonoBehaviour
{
    [SerializeField] Health health;

    /// <summary>Attacco ricevuto e chi l'ha tirato.</summary>
    public event System.Action<AttackData, Transform> OnHit;

    void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
    }

    public void ReceiveHit(AttackData attack, Transform attacker)
    {
        health.TakeDamage(attack.damage);
        OnHit?.Invoke(attack, attacker);
    }
}
