using UnityEngine;

/// <summary>
/// La zona che può essere colpita. Serve un Collider (anche trigger) su un layer che la Hitbox cerca.
/// Se tra i genitori c'è un IHitReceiver (il manichino), è lui a decidere com'è andato il colpo;
/// altrimenti il colpo va sempre a segno e toglie vita alla Health.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Hurtbox : MonoBehaviour
{
    [SerializeField] Health health;

    IHitReceiver receiver;

    void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
        // GetComponentInParent funziona anche con le interfacce.
        receiver = GetComponentInParent<IHitReceiver>();
    }

    /// <summary>Chiamato dalla Hitbox. Restituisce com'è andato il colpo.</summary>
    public HitResult ReceiveHit(AttackData attack, Transform attacker)
    {
        if (receiver != null) return receiver.ReceiveHit(attack, attacker);

        health.TakeDamage(attack.damage);
        return HitResult.Hit;
    }
}
