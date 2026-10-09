using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unico script del gioco che conosce l'Input System.
/// Legge la action map "Fighter" dal PlayerInput e la traduce in comandi semplici:
/// gli altri componenti li usano senza sapere se arrivano da joypad, tastiera o (in futuro) da un'IA.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class FighterInput : MonoBehaviour
{
    // TODO 3a: dichiara un campo privato di tipo InputAction per l'azione Move.

    /// <summary>Direzione orizzontale nel mondo: -1 sinistra, 0 fermo, +1 destra.</summary>
    public float Horizontal { get; private set; }

    void Awake()
    {
        // TODO 3b: prendi le azioni del giocatore con GetComponent<PlayerInput>().actions
        //   e salva nel campo l'azione "Move" con FindAction("Move", throwIfNotFound: true).
        //   PlayerInput crea una copia delle azioni per ogni giocatore: vanno cercate lì, non nell'asset.
    }

    void Update()
    {
        // TODO 3c: leggi Move come float con ReadValue<float>().
        //   In un picchiaduro la camminata è digitale: si cammina a velocità piena o si sta fermi.
        //   Se il valore assoluto è sotto 0.5, Horizontal = 0; altrimenti Horizontal = Mathf.Sign(valore).
    }
}
