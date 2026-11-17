using UnityEngine;

/// <summary>
/// Scrive in Console ogni danno ricevuto e il KO. È il primo "osservatore" di Health:
/// la barra vita ascolta un altro evento della stessa Health.
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
        health.OnDamaged += LogDamage;
        health.OnKO += LogKO;
    }

    void OnDisable()
    {
        health.OnDamaged -= LogDamage;
        health.OnKO -= LogKO;
    }

    void LogDamage(int damage, int current)
    {
        Debug.Log($"{name}: -{damage} (vita {current}/{health.Max})");
    }

    void LogKO()
    {
        Debug.Log($"{name}: KO");
    }
}
