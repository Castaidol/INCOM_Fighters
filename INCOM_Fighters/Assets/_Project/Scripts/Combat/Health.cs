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
        // TODO 5a: ignora i valori minori o uguali a 0.
        //   Scala Current senza scendere sotto 0 (Mathf.Max) e avvisa chi ascolta:
        //   OnDamaged?.Invoke(amount, Current);
    }
}
