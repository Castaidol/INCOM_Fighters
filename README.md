# INCOM Fighters — Sessione 2

**Obiettivo:** il tuo personaggio tira il Jab (J / tasto ovest) e lo Straight (K / tasto nord), resta fermo durante il pugno e, quando colpisce il manichino, gli toglie vita una sola volta. Il danno compare in Console.

## Da dove parti

- **Hai finito la sessione 1 nel tuo progetto:** importa `INCOM_Fighters/Pacchetti/Sessione2.unitypackage` (Assets > Import Package > Custom Package…, importa tutto). Aggiunge gli scheletri dei nuovi script in `Assets/_Project/Scripts/Combat`, la cartella `Assets/_Project/Data/Attacks` e aggiorna il taglio delle clip d'attacco. Le modifiche a `FighterInput` e `FighterMovement` (passi 2 e 4) le scrivi tu, seguendo le istruzioni qui sotto.
- **Sei rimasto indietro:** usa questo branch. Contiene la soluzione della sessione 1 e in `FighterInput` e `FighterMovement` trovi già i TODO dei passi 2 e 4.

## Cosa trovi già pronto

- Le clip d'attacco del player tagliate: Lead Jab (per il Jab) e Punching (per lo Straight), più Jab Cross, Hook e BoxingUppercut per la sessione 4.
- Gli scheletri in `Assets/_Project/Scripts/Combat`: `AttackData`, `FighterCombat`, `Hitbox`, `Hurtbox`, `Health`, `DamageLogger`. Cerca i commenti `TODO`: il numero corrisponde al passo qui sotto.

All'inizio la console mostra avvisi gialli per campi ed eventi non ancora usati: spariscono man mano che completi i TODO. Se in Console non compare nessun messaggio, controlla che l'icona dei messaggi in alto a destra della Console sia attiva.

## Passi

Dopo ogni **Verifica** prova in Play mode, cliccando nella Game view prima di usare la tastiera.

1. **AttackData.** Completa i TODO 1a e 1b. Poi crea due asset in `Assets/_Project/Data/Attacks` con Create > INCOM Fighters > Attack Data:

   | Asset | State Name | Startup | Active | Recovery | Hitbox Offset | Hitbox Size | Damage |
   | --- | --- | --- | --- | --- | --- | --- | --- |
   | Jab | Jab | 4 | 2 | 10 | 0.95, 1.38, 0 | 0.3, 0.25, 0.5 | 5 |
   | Straight | Straight | 11 | 3 | 15 | 0.8, 1.11, 0 | 0.3, 0.3, 0.6 | 12 |

   - **Verifica:** gli asset si aprono e si modificano dall'Inspector.
2. **Input dei pugni** in `FighterInput` (TODO 2a–2d se usi questo branch). Se lavori sul tuo progetto, aggiungi:
   - due campi `InputAction` per `LightPunch` e `HeavyPunch` e due `bool` `lightQueued` e `heavyQueued`;
   - in `Awake`, la ricerca delle due azioni con `FindAction`;
   - in `Update`, `lightQueued = true` quando `WasPressedThisFrame()` è vero (e lo stesso per il pesante);
   - due metodi pubblici `bool ConsumeLight()` e `bool ConsumeHeavy()` che restituiscono il valore e lo rimettono a `false`.
3. **Animator.** In `Fighter.controller` aggiungi due stati **senza transizioni**: `Jab` (Motion: clip Lead Jab, Speed 1.75) e `Straight` (Motion: clip Punching, Speed 2). Li farà partire il codice.
4. **FighterCombat.** Completa i TODO 4a–4f. Aggiungi `FighterCombat` al player (aggiunge da solo `Hitbox`) e assegna Light Attack = Jab e Heavy Attack = Straight.
   Poi in `FighterMovement` (TODO 4g): durante un attacco il player non deve muoversi né girarsi. In `Awake` cerca il `FighterCombat` con `TryGetComponent`; in `Update`, se c'è e il suo `State` è `FighterState.Attack`, salta `UpdateFacing()` e usa 0 al posto di `input.Horizontal`.
   - **Verifica:** J e K fanno partire i pugni, il player non cammina finché il pugno non finisce e poi torna in guardia.
5. **Health e DamageLogger.** Completa i TODO 5a–5c e aggiungi i due componenti al manichino.
6. **Hurtbox.** Crea il layer `Hurtbox` (Project Settings > Tags and Layers). Nel manichino crea un figlio vuoto `Hurtbox` su quel layer, con un Box Collider (Is Trigger attivo, Center 0, 0.8, 0, Size 0.9, 1.6, 0.9) e il componente `Hurtbox`. Completa il TODO 6.
7. **Hitbox.** Sul player imposta Hurtbox Layers = Hurtbox e completa i TODO 7a–7c.
   - **Verifica:** a contatto il Jab toglie 5 e lo Straight 12, una sola volta per pugno. In Scene view la hitbox è grigia durante l'attacco e rossa nei frame attivi.

## Sfide

- **Allinea la hitbox al pugno:** in Play mode metti in pausa e usa il pulsante Step per avanzare un frame alla volta; confronta in Scene view la hitbox rossa con la mano e correggi `Hitbox Offset` nell'asset.
- **Misura la portata:** trova la distanza a cui il Jab colpisce e lo Straight no.
- **Un pesante diverso:** crea un asset e uno stato `Hook` (clip Hook, Speed 2) e usalo come attacco pesante.

## Fine sessione

Fai il commit del progetto funzionante.
