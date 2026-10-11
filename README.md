# INCOM Fighters — Sessione 3

**Obiettivo:** il manichino reagisce ai tuoi pugni. Arretra, resta nella reazione per qualche frame e torna in guardia. Una barra in alto a destra mostra la sua vita, che torna piena dopo 2 secondi senza colpi. View/Select (o R) rimette tutto al punto di partenza.

## Da dove parti

- **Hai finito la sessione 2 nel tuo progetto:** importa `INCOM_Fighters/Pacchetti/Sessione3.unitypackage` (Assets > Import Package > Custom Package…, importa tutto). Aggiunge gli scheletri di `TrainingDummy` e `TrainingManager` in `Assets/_Project/Scripts/Training`, quello di `TrainingHUD` in `Assets/_Project/Scripts/UI` e lo sprite `Assets/_Project/Sprites/Bianco.png`. Le modifiche ad `AttackData`, `Health`, `Hurtbox` e `FighterInput` (passi 1, 2, 3 e 9) le scrivi tu, seguendo le istruzioni qui sotto.
- **Sei rimasto indietro:** usa questo branch. Contiene la soluzione della sessione 2, e in quei quattro script trovi già i TODO.

## Cosa trovi già pronto

- Le clip del manichino in `Assets/PartyMonsterRumblePBR/Animation`: `Idle01` e `GetHit` (con Loop Time già spento).
- Gli scheletri dei nuovi script: `TrainingDummy`, `TrainingManager` e `TrainingHUD`. Cerca i commenti `TODO`: il numero corrisponde al passo qui sotto.
- Lo sprite bianco `Assets/_Project/Sprites/Bianco.png` per la barra vita: un'Image di tipo Filled senza sprite non si riempie.

All'inizio la console mostra avvisi gialli per campi non ancora usati: spariscono man mano che completi i TODO. Se in Console non compare nessun messaggio, controlla che l'icona dei messaggi in alto a destra della Console sia attiva.

## Passi

Dopo ogni **Verifica** prova in Play mode, cliccando nella Game view prima di usare la tastiera. Per colpire il manichino avvicinati: il Jab arriva a circa 1,1 m.

1. **Reazione negli AttackData.** In `AttackData` aggiungi i campi `hitstun` (int, frame) e `knockback` (float, metri) (TODO 1). Poi imposta gli asset:

   | Asset | Hitstun | Knockback |
   | --- | --- | --- |
   | Jab | 16 | 0.15 |
   | Straight | 22 | 0.35 |

2. **Health** (TODO 2a–2c). Aggiungi un secondo evento `OnChanged` dello stesso tipo di `OnDamaged`, che passa vita attuale e vita massima. Lancialo in `TakeDamage` dopo `OnDamaged` e in un nuovo metodo pubblico `RestoreFull()` che riporta la vita al massimo.
3. **Hurtbox** (TODO 3a–3b). Aggiungi un evento pubblico `OnHit` di tipo `System.Action<AttackData, Transform>` e lancialo in `ReceiveHit` dopo il danno, passando l'attacco e chi l'ha tirato.
4. **Animator del manichino.** Crea `Assets/_Project/Animator/Dummy.controller` con due stati **senza transizioni**: `Idle` (Motion: clip Idle01, stato di default) e `Hit` (Motion: clip GetHit, Speed 1.45). Mettilo nel campo Controller dell'Animator del manichino, al posto di `Default`.
   - **Verifica:** in Play il manichino respira in Idle come prima.
5. **TrainingDummy.** Aggiungilo al manichino e collega Animator (quello del manichino) e Arena. Completa i TODO 5a–5c.
   - **Verifica:** il Jab fa reagire il manichino per un attimo, lo Straight più a lungo; due Jab di fila mostrano due reazioni.
6. **Knockback** (TODO 6a–6b).
   - **Verifica:** il Jab sposta il manichino di 15 cm e lo Straight di 35 cm; spinto all'angolo, il manichino si ferma al bordo dell'arena.
7. **Rigenerazione** (TODO 7).
   - **Verifica:** colpisci, aspetta 2 secondi e colpisci di nuovo: in Console la vita riparte da 100 (`Dummy: -5 (vita 95/100)`).
8. **Barra vita.**
   - GameObject > UI > Canvas. Nel Canvas Scaler: UI Scale Mode = Scale With Screen Size, Reference Resolution 1920 × 1080, Match 0.5.
   - Nel Canvas, UI > Image e chiamala `BarraManichino`. In Anchor Presets scegli in alto a destra tenendo premuto Shift (sposta anche il pivot), poi Pos X -60, Pos Y -80, Width 640, Height 36. Source Image: Bianco, colore scuro.
   - Dentro `BarraManichino`, un'altra Image chiamata `Riempimento`. Anchor Presets: stretch su tutti e due gli assi, con Left, Right, Top e Bottom a 4. Source Image: Bianco, colore giallo, Image Type = Filled, Fill Method = Horizontal, Fill Origin = Right (si svuota verso il centro dello schermo).
   - Aggiungi `TrainingHUD` al Canvas, collega Dummy Health (il manichino) e Dummy Health Fill (`Riempimento`) e completa i TODO 8a–8b.
   - **Verifica:** la barra cala a ogni colpo e torna piena con la rigenerazione.
9. **Reset.** In `FighterInput` (TODO 9a–9d) leggi l'azione `Reset`, già presente nella action map, come fai con i pugni. Se lavori sul tuo progetto, aggiungi:
   - un campo `InputAction` per `Reset` e un `bool resetQueued`;
   - in `Awake`, la ricerca dell'azione con `FindAction`;
   - in `Update`, `resetQueued = true` quando `WasPressedThisFrame()` è vero;
   - un metodo pubblico `bool ConsumeReset()` che restituisce il valore e lo rimette a `false`.

   Poi crea un oggetto vuoto `Training` con `TrainingManager`, collega Player Input (il player) e Dummy (il manichino) e completa i TODO 9e–9f.
   - **Verifica:** View/Select o R riporta player e manichino al punto di partenza, con la barra piena.

## Sfide

- **Colpi pesanti più forti:** ritara hitstun e knockback negli asset (per esempio Straight a 26 frame e 0.5 m) e verifica che il manichino arretri visibilmente di più e resti più a lungo nella reazione.
- **All'angolo arretra l'attaccante:** quando il bordo dell'arena ferma il manichino, la spinta che avanza deve respingere chi ha colpito. In `ApplyKnockback` calcola quanto ha tolto il Clamp e passalo a un nuovo metodo `Push(float dx)` di `FighterMovement`, che sposta il player restando nell'arena. Senza questa regola, all'angolo si può colpire all'infinito restando attaccati.
- **Rigenerazione graduale:** invece di tornare piena di colpo, dopo i 120 frame la vita risale di 1 punto per frame, con un metodo `Heal` in `Health`.

## Fine sessione

Fai il commit del progetto funzionante.
