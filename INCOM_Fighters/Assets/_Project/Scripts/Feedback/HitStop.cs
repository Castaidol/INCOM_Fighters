using UnityEngine;

/// <summary>
/// Hitstop: all'impatto i due combattenti si fermano per qualche frame. Non si usa Time.timeScale,
/// che fermerebbe tutto (camera, HUD, menu): si fermano solo i contatori in frame dei combattenti,
/// che guardano IsFrozen in cima al loro FixedUpdate, e i loro Animator.
/// </summary>
// Decide se il frame è "fermo" prima che qualunque combattente lo esegua.
[DefaultExecutionOrder(-30)]
public class HitStop : MonoBehaviour
{
    [Tooltip("Animator da fermare durante l'hitstop: quelli del player e del manichino.")]
    [SerializeField] Animator[] animators;

    int framesLeft;

    /// <summary>True se questo frame è un frame di fermo: i combattenti non devono contarlo.</summary>
    public bool IsFrozen { get; private set; }

    /// <summary>Ferma i combattenti per i prossimi "frames" frame.</summary>
    public void Freeze(int frames)
    {
        // TODO 2a (sessione 6): se frames è 0 o meno non fare niente. Altrimenti framesLeft diventa il più lungo
        //   tra il fermo in corso e quello nuovo (Mathf.Max) e gli Animator si fermano: SetAnimatorSpeed(0f).
    }

    void FixedUpdate()
    {
        // TODO 2b: qui si decide, a inizio frame, se questo frame è fermo.
        //   Salva in una variabile wasFrozen il valore di IsFrozen del frame prima, poi IsFrozen = framesLeft > 0.
        //   Se il frame è fermo scala framesLeft di 1. Se non lo è ma il frame prima lo era (wasFrozen),
        //   il fermo è finito: rimetti gli Animator a velocità 1.
    }

    void SetAnimatorSpeed(float speed)
    {
        // TODO 2c: imposta animator.speed = speed su tutti gli Animator della lista, saltando quelli null.
    }
}
