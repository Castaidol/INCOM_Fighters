# INCOM Fighters — Sessione 6

**Obiettivo:** ogni colpo si deve sentire. All'impatto player e manichino si fermano per qualche frame (hitstop), la camera trema, nel punto d'impatto compaiono le scintille e si sente un suono, diverso per colpo leggero, colpo pesante e parata. Sotto la barra del manichino il contatore mostra i colpi e il danno della combo e poi svanisce; quando il manichino para compare la scritta PARATO. Se avanza tempo, a sinistra l'input display mostra gli ultimi comandi con i frame passati tra l'uno e l'altro.

## Da dove parti

- **Hai finito la sessione 5 nel tuo progetto:** importa `INCOM_Fighters/Pacchetti/Sessione6.unitypackage` (Assets > Import Package > Custom Package…, importa tutto). Aggiunge `HitStop.cs` e `HitFeedback.cs` (in `Scripts/Feedback`) e `InputDisplay.cs` (in `Scripts/UI`), tutti con i TODO, i suoni in `Audio` e gli effetti in `Prefabs/FX`. Le modifiche agli altri script le scrivi tu, seguendo le istruzioni qui sotto. Se preferisci, copia `TrainingHUD.cs` da questo branch: contiene la soluzione della sessione 5 con i campi nuovi e i TODO di questa sessione.
- **Sei rimasto indietro:** usa questo branch. Contiene la soluzione della sessione 5 e trovi già i TODO in tutti gli script da modificare.

## Cosa trovi già pronto

- Tre suoni in `Assets/_Project/Audio`: `Pugno_Leggero`, `Pugno_Pesante` e `Parata`.
- Due effetti in `Assets/_Project/Prefabs/FX`: `FX_Colpo` (scintille) e `FX_Parata` (energia azzurra). Sono varianti degli effetti Synty senza loop: una sola esplosione e poi più niente.
- Nei tre script nuovi i campi e i metodi da completare. In questo branch anche il campo `shakeDuration` e il metodo `Shake` della `FightCamera` e i campi del contatore e della scritta PARATO nel `TrainingHUD`. Cerca i commenti `TODO`: il numero corrisponde al passo qui sotto.

Finché non completi il passo 2 il gioco funziona come alla fine della sessione 5. All'inizio la console mostra alcuni avvisi gialli per campi non ancora usati, che spariscono man mano che completi i TODO.

## Teoria in breve

- **Game feel:** un colpo sembra pesante per tanti piccoli dettagli messi insieme: hitstop, tremolio, effetto e suono. Il danno da solo non si vede.
- **Hitstop:** all'impatto attaccante e colpito si fermano per qualche frame, di più con i colpi pesanti. Si fermano tutti e due per lo stesso tempo, quindi il vantaggio in frame non cambia.
- **Perché non `Time.timeScale = 0`:** fermerebbe tutto, anche camera, dissolvenze, particelle e, in futuro, il menu. Qui si fermano solo i combattenti: saltano il loro `FixedUpdate` e i loro Animator vanno a velocità 0.
- **Un osservatore:** `HitFeedback` ascolta `Hitbox.OnContact` e il combattimento non sa che esiste. Se lo disattivi il gioco funziona lo stesso, solo "muto".

## Passi

Dopo ogni **Verifica** prova in Play mode, cliccando nella Game view prima di usare la tastiera. Comportamento e vita del manichino si cambiano dall'Inspector dell'oggetto `Training`, anche durante il Play.

1. **Peso negli AttackData** (TODO 1). Aggiungi i campi `hitstop` e `hitSound`, poi impostali nei cinque asset di `Data/Attacks`:

   | Asset | Hitstop | Hit Sound |
   | --- | --- | --- |
   | Jab | 5 | Pugno_Leggero |
   | Cross | 6 | Pugno_Leggero |
   | Hook | 8 | Pugno_Pesante |
   | Straight | 9 | Pugno_Pesante |
   | Uppercut | 11 | Pugno_Pesante |

2. **Hitstop.**
   - Nella scena crea un oggetto vuoto `Feedback` con i componenti `HitStop` e `HitFeedback`. Unity aggiunge da solo un `AudioSource`: togli la spunta a Play On Awake.
   - In `HitStop` completa i TODO 2a–2c. Nella lista Animators dell'Inspector metti due elementi: l'Animator del player e quello del manichino, che stanno sui modelli dentro `Player` e `Dummy`.
   - In `HitFeedback` completa i TODO 2d–2e e collega Player Hitbox (l'oggetto `Player`), Hit Stop (`Feedback`) e Fight Camera (`Main Camera`).
   - In `FighterCombat` (TODO 2f), `TrainingDummy` (TODO 2g) e `FighterInput` (TODO 2h) aggiungi il campo `hitStop` e il controllo in `FixedUpdate`. Poi trascina `Feedback` nel campo Hit Stop dei tre componenti, su `Player` e su `Dummy`.
   - **Verifica:** con un Jab player e manichino si fermano un attimo all'impatto. J J J fanno ancora Jab → Cross → Hook e in Console compare «Combo 3 colpi – 22 danni». Con «Para sempre» si ferma anche il colpo parato.
3. **Tremolio della camera.**
   - In `FightCamera` completa i TODO 3a–3c. Se lavori sul tuo progetto, aggiungi prima, sotto un `[Header("Tremolio")]`, il campo `[SerializeField] float shakeDuration = 0.2f` e il metodo vuoto `public void Shake(float strength)`.
   - In `HitFeedback` (TODO 3d) fai partire il tremolio.
   - **Verifica:** a ogni colpo la camera trema, meno quando il colpo è parato. Il tremolio continua anche mentre i combattenti sono fermi, poi la camera torna a seguirli come prima, senza scivolare.
4. **Effetti d'impatto.**
   - In `Hitbox` (TODO 4a) la proprietà `Center`, il punto d'impatto.
   - In `HitFeedback` (TODO 4b) crea l'effetto. Nell'Inspector metti `FX_Colpo` in Hit Effect e `FX_Parata` in Block Effect.
   - **Verifica:** a ogni colpo compaiono le scintille sul pugno, in parata l'effetto azzurro. Nella Hierarchy le copie degli effetti spariscono dopo un secondo. Con il manichino KO (vita «Normale») i colpi non hanno effetti.
5. **Suoni.**
   - In `HitFeedback` (TODO 5) suona la clip giusta. Nell'Inspector metti `Parata` in Block Sound; i suoni dei colpi li hai già messi negli asset al passo 1.
   - **Verifica:** Jab e Cross hanno un suono secco, Hook, Straight e Uppercut un tonfo, la parata un "toc". Se non senti niente, controlla che nella barra della Game view non sia attivo Mute Audio.
6. **Contatore combo e PARATO.**
   - In `ComboTracker` (TODO 6a) l'evento `OnComboChanged`.
   - Nel Canvas `HUD` crea due oggetti vuoti (tasto destro su `HUD` > Create Empty) con il componente `CanvasGroup`, tutti e due con Anchor Presets in alto a destra con Shift:
     - `Combo`: Pos X −60, Pos Y −130, Width 640, Height 120;
     - `Parato`: Pos X −60, Pos Y −260, Width 640, Height 70.
   - Dentro ognuno metti un Text (UI > Legacy > Text) che lo riempie tutto (Anchor Presets con Alt, l'ultimo in basso a destra), grassetto, allineato in alto a destra, con Rich Text attivo e Horizontal e Vertical Overflow su Overflow. In `Combo` font 56 e colore giallo; in `Parato` font 48, colore azzurro e testo «PARATO». Un componente `Shadow` li rende più leggibili.
   - Nel `TrainingHUD` completa i TODO 6b–6e. Se lavori sul tuo progetto, aggiungi prima i campi copiandoli da `TrainingHUD.cs` di questo branch: le intestazioni Contatore combo, Scritta PARATO e Dissolvenza e i campi privati `comboRunning`, `comboTimer` e `blockedTimer`.
   - Nell'Inspector del `TrainingHUD` collega Combo Tracker (`Dummy`), Combo Group (`Combo`), Combo Text (il Text dentro `Combo`) e Blocked Group (`Parato`).
   - **Verifica:** all'avvio contatore e PARATO non si vedono. Con J J J compaiono «2 COLPI» e poi «3 COLPI» con sotto «22 danni», che restano un secondo dopo la fine della combo e poi svaniscono in mezzo secondo. Con «Para sempre» ogni colpo parato fa comparire PARATO.
7. **Input display (facoltativo).**
   - In `FighterInput` (TODO 7a) l'evento `OnAttackPressed`.
   - Nel Canvas `HUD` crea un'Image `InputDisplay` (Anchor Presets in basso a sinistra con Shift, Pos X 40, Pos Y 160, Width 160, Height 330, Source Image Bianco, colore scuro semitrasparente) e dentro un Text che la riempie, font 26, grassetto, allineato in alto a sinistra, con Rich Text attivo.
   - Aggiungi all'Image il componente `InputDisplay`, completa i TODO 7b–7d e collega Input e Movement (`Player`), Hit Stop (`Feedback`) e Text.
   - **Verifica:** camminando e tirando pugni compaiono le righe (→ avanti, ← indietro, · fermo, L leggero, H pesante), la più recente in alto, con in grigio i frame passati dal comando precedente. Con J J veloci leggi quanti frame passano tra le due pressioni.

## Sfide

- **Peso proporzionale:** tremolio e hitstop che crescono con il danno del colpo invece di valori fissi.
- **Colpo a vuoto:** un suono quando un attacco non tocca niente, con l'evento `OnWhiff` della sessione 5 (per esempio `Pugno_Leggero` con il `pitch` più alto).
- **KO spettacolare:** il colpo che manda KO il manichino ha un hitstop lungo (per esempio 20 frame) e un tremolio forte.
- **Pool di effetti:** riusa pochi effetti già creati invece di crearne e distruggerne uno a ogni colpo.

## Fine sessione

Fai il commit del progetto funzionante.
