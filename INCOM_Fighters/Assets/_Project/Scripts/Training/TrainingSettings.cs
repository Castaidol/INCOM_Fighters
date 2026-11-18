using UnityEngine;

/// <summary>Come si difende il manichino.</summary>
public enum DummyBehavior
{
    [InspectorName("Fermo")] Stand,
    [InspectorName("Para sempre")] AlwaysBlock,
    [InspectorName("Para dopo il primo colpo")] BlockAfterFirstHit,
}

/// <summary>Cosa succede quando la vita del manichino arriva a 0.</summary>
public enum DummyHealthMode
{
    [InspectorName("Infinita")] Infinite,
    [InspectorName("Normale")] Normal,
}

/// <summary>
/// Le opzioni dell'allenamento. Si cambiano dall'Inspector anche durante il Play:
/// il manichino le rilegge a ogni colpo. Nella sessione 7 le scriverà il menu.
/// </summary>
public class TrainingSettings : MonoBehaviour
{
    [Tooltip("Fermo: non para mai. Para sempre: para tutto quello che non arriva durante l'hitstun. " +
             "Para dopo il primo colpo: lascia passare il primo, poi para finché non passano 60 frame senza contatti.")]
    [SerializeField] DummyBehavior behavior = DummyBehavior.Stand;

    [Tooltip("Infinita: la vita non scende sotto 1. Normale: a 0 il manichino va KO e si rialza con la vita piena.")]
    [SerializeField] DummyHealthMode healthMode = DummyHealthMode.Infinite;

    // TODO 4a (sessione 7): un'opzione nuova [SerializeField] bool showHitboxes, con il Tooltip
    //   "Mostra hitbox e hurtbox come cubi colorati, anche nella build.", e la proprietà pubblica ShowHitboxes
    //   con get e set, come le altre due qui sotto.

    public DummyBehavior Behavior { get => behavior; set => behavior = value; }
    public DummyHealthMode HealthMode { get => healthMode; set => healthMode = value; }
}
