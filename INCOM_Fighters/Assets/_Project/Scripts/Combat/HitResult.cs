using UnityEngine;

/// <summary>Com'è andato un colpo che ha toccato una hurtbox.</summary>
public enum HitResult
{
    /// <summary>A segno: danno, hitstun e knockback.</summary>
    Hit,
    /// <summary>Parato: niente danno, blockstun e knockback ridotto.</summary>
    Blocked,
    /// <summary>Ignorato: chi lo riceve non può essere colpito (per esempio il manichino KO).</summary>
    Ignored,
}

/// <summary>
/// Chi possiede una hurtbox e vuole decidere cosa succede quando viene toccata (il manichino).
/// Un evento avvisa ma non può rispondere: qui la Hitbox ha bisogno di sapere com'è andata,
/// quindi è una chiamata che restituisce un valore.
/// </summary>
public interface IHitReceiver
{
    // TODO 3a (sessione 5): dichiara il metodo che la Hurtbox chiamerà:
    //   HitResult ReceiveHit(AttackData attack, Transform attacker);
    //   In un'interfaccia si scrive solo la firma, senza corpo: chi la implementa deve avere questo metodo pubblico.
}
