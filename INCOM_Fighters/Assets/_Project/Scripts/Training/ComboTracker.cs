using UnityEngine;

/// <summary>
/// Conta le combo vere sul manichino: un colpo continua la combo solo se arriva mentre il manichino
/// è ancora in hitstun. Una sequenza di tasti giusta ma troppo lenta non è una combo.
/// A fine combo lancia OnComboEnded (nella sessione 6 lo userà l'HUD) e la scrive in Console;
/// segnala anche i colpi parati.
/// </summary>
[RequireComponent(typeof(TrainingDummy))]
public class ComboTracker : MonoBehaviour
{
    TrainingDummy dummy;

    /// <summary>Colpi della combo in corso.</summary>
    public int Hits { get; private set; }

    /// <summary>Danno della combo in corso.</summary>
    public int Damage { get; private set; }

    // TODO 6a (sessione 6): un evento pubblico OnComboChanged di tipo System.Action<int, int>, con colpi e danno
    //   della combo in corso, lanciato a ogni colpo dal secondo in poi.

    /// <summary>Colpi e danno di una combo appena finita (da 2 colpi in su).</summary>
    public event System.Action<int, int> OnComboEnded;

    void Awake()
    {
        dummy = GetComponent<TrainingDummy>();
    }

    void OnEnable()
    {
        dummy.OnHitTaken += CountHit;
        dummy.OnRecovered += EndCombo;
        dummy.OnBlocked += LogBlock;
    }

    void OnDisable()
    {
        dummy.OnHitTaken -= CountHit;
        dummy.OnRecovered -= EndCombo;
        dummy.OnBlocked -= LogBlock;
    }

    void CountHit(AttackData attack, bool comboHit)
    {
        // Colpo arrivato fuori dall'hitstun: comincia una combo nuova.
        if (!comboHit)
        {
            Hits = 0;
            Damage = 0;
        }
        Hits++;
        Damage += attack.damage;
        // TODO 6a: se Hits è almeno 2 lancia OnComboChanged(Hits, Damage).
    }

    void LogBlock(AttackData attack)
    {
        Debug.Log($"{attack.name} parato");
    }

    void EndCombo()
    {
        // Un colpo solo non è una combo.
        if (Hits >= 2)
        {
            Debug.Log($"Combo {Hits} colpi – {Damage} danni");
            OnComboEnded?.Invoke(Hits, Damage);
        }
        Hits = 0;
        Damage = 0;
    }
}
