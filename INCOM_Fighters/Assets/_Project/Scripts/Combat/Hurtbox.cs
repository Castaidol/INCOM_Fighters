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
        // TODO 3b (sessione 5): cerca tra i genitori chi decide com'è andato il colpo e salvalo in receiver.
        //   GetComponentInParent<IHitReceiver>() funziona anche con le interfacce.
    }

    /// <summary>Chiamato dalla Hitbox. Restituisce com'è andato il colpo.</summary>
    public HitResult ReceiveHit(AttackData attack, Transform attacker)
    {
        // TODO 3c: se receiver non è null decide lui: restituisci receiver.ReceiveHit(attack, attacker).
        //   Altrimenti il colpo va sempre a segno, come prima (le due righe qui sotto).
        health.TakeDamage(attack.damage);
        return HitResult.Hit;
    }
}
