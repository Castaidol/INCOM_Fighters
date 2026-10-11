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

    // TODO 2a (sessione 3): aggiungi un secondo evento pubblico, OnChanged, dello stesso tipo
    //   System.Action<int, int>. Passa la vita attuale e la vita massima a ogni cambiamento
    //   (danno, rigenerazione, reset): è quello che ascolterà la barra vita.

    void Awake()
    {
        Current = max;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        Current = Mathf.Max(0, Current - amount);
        OnDamaged?.Invoke(amount, Current);
        // TODO 2b: avvisa anche chi ascolta OnChanged, passando Current e max.
    }

    // TODO 2c: aggiungi un metodo pubblico RestoreFull() che riporta Current a max e lancia OnChanged.
    //   Se la vita è già piena non fa nulla.
}
