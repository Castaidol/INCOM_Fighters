using UnityEngine;

/// <summary>
/// Vita del personaggio. Chi vuole sapere quando viene colpito si iscrive a OnDamaged (pattern Observer):
/// oggi DamageLogger, nelle prossime sessioni la barra vita e il manichino.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField, Min(1)] int max = 100;

    public int Max => max;
    public int Current { get; private set; }

    /// <summary>Danno ricevuto e vita rimasta.</summary>
    public event System.Action<int, int> OnDamaged;

    void Awake()
    {
        Current = max;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        Current = Mathf.Max(0, Current - amount);
        OnDamaged?.Invoke(amount, Current);
    }
}
