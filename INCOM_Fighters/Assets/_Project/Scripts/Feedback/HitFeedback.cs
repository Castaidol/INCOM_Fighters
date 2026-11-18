using UnityEngine;

/// <summary>
/// Il "peso" dei colpi: a ogni contatto del player (a segno o parato) hitstop, tremolio della camera,
/// effetto nel punto d'impatto e suono. Ascolta solo Hitbox.OnContact: il combattimento non sa che esiste.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class HitFeedback : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] Hitbox playerHitbox;
    [SerializeField] HitStop hitStop;
    [SerializeField] FightCamera fightCamera;

    [Header("Colpo a segno")]
    [Tooltip("Effetto creato nel punto d'impatto.")]
    [SerializeField] GameObject hitEffect;
    [Tooltip("Metri di tremolio della camera.")]
    [SerializeField] float hitShake = 0.06f;

    [Header("Colpo parato")]
    [SerializeField] GameObject blockEffect;
    [SerializeField] AudioClip blockSound;
    [SerializeField] float blockShake = 0.025f;

    [Header("Effetti")]
    [Tooltip("Secondi dopo cui l'effetto viene distrutto.")]
    [SerializeField] float effectLifetime = 1f;

    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        playerHitbox.OnContact += Play;
    }

    void OnDisable()
    {
        playerHitbox.OnContact -= Play;
    }

    void Play(AttackData attack, HitResult result)
    {
        // Un colpo ignorato (manichino KO) non ha peso.
        if (result == HitResult.Ignored) return;
        bool blocked = result == HitResult.Blocked;

        // 1. Hitstop: i due combattenti si fermano per i frame scritti nell'attacco.
        hitStop.Freeze(attack.hitstop);

        // 2. La camera trema (in tempo reale: continua durante l'hitstop).
        fightCamera.Shake(blocked ? blockShake : hitShake);

        // 3. Effetto nel punto d'impatto, il centro della hitbox.
        GameObject effect = blocked ? blockEffect : hitEffect;
        if (effect != null) Destroy(Instantiate(effect, playerHitbox.Center, Quaternion.identity), effectLifetime);

        // 4. Suono: quello dell'attacco a segno, quello della parata se parato.
        AudioClip clip = blocked ? blockSound : attack.hitSound;
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}
