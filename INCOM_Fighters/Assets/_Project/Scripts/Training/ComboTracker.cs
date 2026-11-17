using UnityEngine;

/// <summary>
/// Conta le combo vere sul manichino: un colpo continua la combo solo se arriva mentre il manichino
/// è ancora in hitstun. Una sequenza di tasti giusta ma troppo lenta non è una combo.
/// A fine combo lancia OnComboEnded (nella sessione 6 lo userà l'HUD) e la scrive in Console.
/// </summary>
[RequireComponent(typeof(TrainingDummy))]
public class ComboTracker : MonoBehaviour
{
    TrainingDummy dummy;

    /// <summary>Colpi della combo in corso.</summary>
    public int Hits { get; private set; }

    /// <summary>Danno della combo in corso.</summary>
    public int Damage { get; private set; }

    /// <summary>Colpi e danno di una combo appena finita (da 2 colpi in su).</summary>
    public event System.Action<int, int> OnComboEnded;

    void Awake()
    {
        dummy = GetComponent<TrainingDummy>();
    }

    void OnEnable()
    {
        // TODO 7a: iscriviti agli eventi del manichino: OnHitTaken con CountHit e OnRecovered con EndCombo.
    }

    void OnDisable()
    {
        // TODO 7a: togli le due iscrizioni.
    }

    void CountHit(AttackData attack, bool comboHit)
    {
        // TODO 7b: se comboHit è falso il colpo comincia una combo nuova: azzera Hits e Damage.
        //   Poi aggiungi 1 a Hits e il danno dell'attacco a Damage.
    }

    void EndCombo()
    {
        // TODO 7c: un colpo solo non è una combo. Se Hits è almeno 2:
        //   Debug.Log($"Combo {Hits} colpi – {Damage} danni") e OnComboEnded?.Invoke(Hits, Damage).
        //   In ogni caso, alla fine azzera Hits e Damage.
    }
}
