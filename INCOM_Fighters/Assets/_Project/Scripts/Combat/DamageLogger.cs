using UnityEngine;

/// <summary>
/// Scrive in Console ogni danno ricevuto. È il primo "osservatore" di Health:
/// nella sessione 3 lo affianca la barra vita, che ascolta lo stesso evento.
/// </summary>
[RequireComponent(typeof(Health))]
public class DamageLogger : MonoBehaviour
{
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        // TODO 5b: iscrivi il metodo LogDamage all'evento health.OnDamaged.
    }

    void OnDisable()
    {
        // TODO 5c: togli l'iscrizione, così un oggetto disattivato non riceve più eventi.
    }

    void LogDamage(int damage, int current)
    {
        Debug.Log($"{name}: -{damage} (vita {current}/{health.Max})");
    }
}
