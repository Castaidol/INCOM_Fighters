# INCOM Fighters — Sessione 1

**Obiettivo:** alla fine della sessione il tuo personaggio cammina avanti e indietro col joypad, si gira sempre verso il manichino, non lo attraversa e la camera inquadra entrambi.

## Cosa trovi già pronto

- Progetto Unity 6000.5.10f1 con URP e Input System. Per clonare il repository serve **Git LFS**.
- Personaggi Synty POLYGON Fantasy Rivals in `Assets/Synty/PolygonFantasyRivals/Prefabs/Characters` e manichini Party Monster in `Assets/PartyMonsterRumblePBR/Prefab`.
- Le clip del player in `Assets/_Project/Aminations/Player`, già configurate: rig Humanoid, *Bake Into Pose* attivo, loop solo su Idle e camminate.
- Script completi: `Pushbox` e `ArenaBounds`.
- Script da completare: `FighterInput`, `FighterMovement` e `FightCamera`. Cerca i commenti `TODO`: il numero corrisponde al passo qui sotto.

All'inizio la console mostra alcuni avvisi gialli (*CS0414: assigned but its value is never used*): sono campi che userai nei TODO e spariscono man mano che li completi.

## Passi

Dopo ogni **Verifica** prova in Play mode, cliccando nella Game view prima di usare la tastiera.

1. **Scena.** Crea `Assets/_Project/Scenes/Arena.unity`.
   - Pavimento: un Plane con scala (2, 1, 1).
   - Main Camera in (0; 1,2; -7), rotazione (0, 0, 0), Field of View 30.
   - Un oggetto vuoto "Arena" con il componente `ArenaBounds`.
   - Player: un personaggio Rivals **senza** `BR_` nel nome, in x = -1,5, ruotato di 90° su Y.
   - Manichino: `C01`, in x = +1,5, ruotato di -90° su Y.
   - *Apply Root Motion* spento sull'Animator di entrambi.
   - **Verifica:** con la Game view a 16:9 si vedono i due personaggi di profilo.
2. **Action map.** Crea `Assets/_Project/Input/Fighter.inputactions` (Create > Input Actions).
   - Map `Fighter`.
   - `Move`: Action Type *Value*, Control Type *Axis*, con tre binding: `Left Stick/X`, `D-Pad/X` e un *1D Axis* con A (negative) e D (positive).
   - `LightPunch` (Button West / J), `HeavyPunch` (Button North / K), `Reset` (Select / R), `Pause` (Start / Esc).
   - Control scheme `Gamepad` e `Keyboard`.
   - Sul player aggiungi un `PlayerInput`: Actions = Fighter, Default Map = Fighter, Behavior = *Invoke C Sharp Events*.
   - **Verifica:** in Play mode la sezione Debug del PlayerInput mostra lo schema e il dispositivo attivi.
3. **FighterInput.** Aggiungilo al player e completa i TODO 3a, 3b e 3c.
4. **FighterMovement.** Aggiungilo al player (aggiunge da solo anche `Pushbox`), collega *Opponent* al manichino e *Arena* all'oggetto Arena. Completa i TODO 4a e 4b.
   - **Verifica:** A/D e lo stick muovono il player, ancora senza animazione, fino ai bordi dell'arena.
5. **Animator.** Crea `Assets/_Project/Animator/Fighter.controller`.
   - Parametro float `MoveX`.
   - Stato `Locomotion` con un Blend Tree 1D su `MoveX`: Walking Backwards a -1, Idle a 0, Walking a +1, con *Automate Thresholds* disattivato.
   - Assegnalo all'Animator del player.
6. **Orientamento.** Completa i TODO 6a, 6b e 6c.
   - **Verifica:** verso il manichino parte la camminata in avanti, allontanandoti quella all'indietro. Se durante il Play sposti il manichino dall'altra parte in Scene view, il player si gira.
7. **Pushbox.** Aggiungi `Pushbox` al manichino (Half Width 0,55, Height 1,6) e completa il TODO 7.
   - **Verifica:** il player si ferma a contatto e i due Gizmo gialli si toccano.
8. **Camera.** Aggiungi `FightCamera` alla Main Camera, collega i due combattenti e completa il TODO 8.
   - **Verifica:** la camera resta centrata tra i due.

## Sfide

- **Dead zone regolabile:** oggi la soglia è fissa a 0,5; rendila un campo modificabile dall'Inspector e provala con lo stick.
- **Velocità diverse:** all'indietro si cammina più piano che in avanti.
- **Zoom:** la camera si allontana quando i due personaggi si allontanano, così restano sempre in inquadratura.
- **Extra:** arreda l'arena con i pezzi Synty e scegli il tuo personaggio.

## Fine sessione

Fai il commit della scena funzionante.
