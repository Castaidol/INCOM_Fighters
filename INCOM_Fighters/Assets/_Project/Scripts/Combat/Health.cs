using UnityEngine;

/// <summary>
/// Vita del personaggio. Chi vuole sapere cosa succede si iscrive agli eventi (pattern Observer):
/// DamageLogger ascolta OnDamaged, la barra vita ascolta OnChanged.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField, Min(1)] int max = 100;

    public int Max => max;
    public int Current { get; private set; }

    /// <summary>Danno ricevuto e vita rimasta.</summary>
    public event System.Action<int, int> OnDamaged;

    /// <summary>Vita attuale e vita massima, a ogni cambiamento (danno, rigenerazione, reset).</summary>
    public event System.Action<int, int> OnChanged;

    void Awake()
    {
        Current = max;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        Current = Mathf.Max(0, Current - amount);
        OnDamaged?.Invoke(amount, Current);
        OnChanged?.Invoke(Current, max);
    }

    /// <summary>Riporta la vita al massimo.</summary>
    public void RestoreFull()
    {
        if (Current == max) return;
        Current = max;
        OnChanged?.Invoke(Current, max);
    }
}
