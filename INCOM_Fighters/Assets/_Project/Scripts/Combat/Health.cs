using UnityEngine;

/// <summary>
/// Vita del personaggio. Chi vuole sapere cosa succede si iscrive agli eventi (pattern Observer):
/// DamageLogger ascolta OnDamaged e OnKO, la barra vita ascolta OnChanged.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField, Min(1)] int max = 100;

    public int Max => max;
    public int Current { get; private set; }

    /// <summary>Sotto questo valore la vita non scende: con 1 non si va mai KO (vita "Infinita").</summary>
    public int MinHealth { get; set; }

    /// <summary>Danno ricevuto e vita rimasta.</summary>
    public event System.Action<int, int> OnDamaged;

    /// <summary>Vita attuale e vita massima, a ogni cambiamento (danno, rigenerazione, reset).</summary>
    public event System.Action<int, int> OnChanged;

    /// <summary>La vita è arrivata a 0.</summary>
    public event System.Action OnKO;

    void Awake()
    {
        Current = max;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || Current == 0) return;
        Current = Mathf.Max(MinHealth, Current - amount);
        OnDamaged?.Invoke(amount, Current);
        OnChanged?.Invoke(Current, max);
        if (Current == 0) OnKO?.Invoke();
    }

    /// <summary>Riporta la vita al massimo.</summary>
    public void RestoreFull()
    {
        if (Current == max) return;
        Current = max;
        OnChanged?.Invoke(Current, max);
    }
}
