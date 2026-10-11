using UnityEngine;

/// <summary>
/// La zona che può essere colpita. Serve un Collider (anche trigger) su un layer che la Hitbox cerca.
/// Inoltra il danno alla Health del personaggio.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Hurtbox : MonoBehaviour
{
    [SerializeField] Health health;

    void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
    }

    public void ReceiveHit(AttackData attack, Transform attacker)
    {
        // TODO 6: passa il danno dell'attacco (attack.damage) alla Health.
    }
}
