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
        if (frames <= 0) return;
        framesLeft = Mathf.Max(framesLeft, frames);
        SetAnimatorSpeed(0f);
    }

    void FixedUpdate()
    {
        bool wasFrozen = IsFrozen;
        IsFrozen = framesLeft > 0;
        if (IsFrozen) framesLeft--;
        else if (wasFrozen) SetAnimatorSpeed(1f); // fine del fermo: le animazioni ripartono
    }

    void SetAnimatorSpeed(float speed)
    {
        foreach (Animator animator in animators)
            if (animator != null) animator.speed = speed;
    }
}
