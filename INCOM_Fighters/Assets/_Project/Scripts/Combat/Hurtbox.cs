using UnityEngine;

/// <summary>
/// La zona che può essere colpita. Serve un Collider (anche trigger) su un layer che la Hitbox cerca.
/// Inoltra il danno alla Health del personaggio.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Hurtbox : MonoBehaviour
{
    [SerializeField] Health health;

    // TODO 3a (sessione 3): aggiungi un evento pubblico OnHit di tipo System.Action<AttackData, Transform>:
    //   l'attacco ricevuto e chi l'ha tirato. Il manichino lo ascolterà per reagire.

    void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
    }

    public void ReceiveHit(AttackData attack, Transform attacker)
    {
        health.TakeDamage(attack.damage);
        // TODO 3b: dopo il danno lancia OnHit, passando attack e attacker.
    }
}
