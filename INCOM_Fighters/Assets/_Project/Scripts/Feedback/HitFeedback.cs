using UnityEngine;

/// <summary>
/// Il "peso" dei colpi: a ogni contatto del player (a segno o parato) hitstop, tremolio della camera,
/// effetto nel punto d'impatto e suono. Ascolta solo Hitbox.OnContact: il combattimento non sa che esiste.
/// Si completa un pezzo alla volta, dal passo 2 al passo 5.
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
        // TODO 2d (sessione 6): iscriviti a playerHitbox.OnContact con il metodo Play.
    }

    void OnDisable()
    {
        // TODO 2d: togli l'iscrizione.
    }

    void Play(AttackData attack, HitResult result)
    {
        // Un colpo ignorato (manichino KO) non ha peso.
        if (result == HitResult.Ignored) return;
        bool blocked = result == HitResult.Blocked;

        // TODO 2e: hitstop. I due combattenti si fermano per i frame scritti nell'attacco: hitStop.Freeze(attack.hitstop).

        // TODO 3d: la camera trema di blockShake se il colpo è parato, di hitShake se è a segno.

        // TODO 4b: crea nel punto d'impatto (playerHitbox.Center) blockEffect se il colpo è parato, hitEffect se è a segno:
        //   Instantiate(effetto, posizione, Quaternion.identity). Se l'effetto è null non fare niente.
        //   Distruggi la copia creata dopo effectLifetime secondi: Destroy(copia, effectLifetime).

        // TODO 5: con audioSource.PlayOneShot suona blockSound se il colpo è parato, attack.hitSound se è a segno.
        //   Se la clip è null non fare niente.
    }
}
