# INCOM Fighters — Sessione 4

**Obiettivo:** con i due pugni esegui le combo dell'albero: Jab → Cross → Hook, Jab → Cross → Uppercut e Straight → Uppercut. Un pugno premuto con qualche frame di anticipo non va più perso, e in Console compare «Combo 3 colpi – 22 danni» solo se ogni colpo è arrivato mentre il manichino era ancora nella reazione.

## Da dove parti

- **Hai finito la sessione 3 nel tuo progetto:** importa `INCOM_Fighters/Pacchetti/Sessione4.unitypackage` (Assets > Import Package > Custom Package…, importa tutto). Aggiunge lo scheletro di `ComboTracker` in `Assets/_Project/Scripts/Training`. Le modifiche ad `AttackData`, `FighterInput`, `FighterCombat`, `Hitbox` e `TrainingDummy` le scrivi tu, seguendo le istruzioni qui sotto.
- **Sei rimasto indietro:** usa questo branch. Contiene la soluzione della sessione 3, e in quei cinque script trovi già i TODO.

## Cosa trovi già pronto

- Le clip di Cross (Jab Cross), Hook e Uppercut (BoxingUppercut), già tagliate nella sessione 2.
- In questo branch: l'enum `AttackButton`, il campo `bufferFrames` e la proprietà `BufferedButton` in `FighterInput`, e il metodo `AttackFor` in `FighterCombat`. Cerca i commenti `TODO`: il numero corrisponde al passo qui sotto.

Finché non completi i passi 4 e 5 il player cammina ma non tira pugni: è normale. Se in Console non compare nessun messaggio, controlla che l'icona dei messaggi in alto a destra della Console sia attiva.

## Teoria in breve

- **Combo vera o sequenza:** una combo è vera se ogni colpo arriva mentre il manichino è ancora in hitstun. Gli stessi tasti premuti troppo tardi sono solo una sequenza: il manichino fa in tempo a tornare in guardia.
- **Input buffer:** un pugno premuto resta valido per 8 frame, così si può premere un po' in anticipo.
- **Finestra di combo:** da un certo frame dell'attacco fino alla fine, un pugno nel buffer fa partire subito il colpo successivo e salta il resto del recovery.

## Passi

Dopo ogni **Verifica** prova in Play mode, cliccando nella Game view prima di usare la tastiera. Per colpire il manichino avvicinati: le catene funzionano a contatto.

1. **Animator.** In `Fighter.controller` aggiungi tre stati **senza transizioni**: `Cross` (Motion: clip Jab Cross, Speed 1.3333), `Hook` (Motion: clip Hook, Speed 2) e `Uppercut` (Motion: clip BoxingUppercut, Speed 1.9).
2. **Combo in AttackData** (TODO 2a–2c). Aggiungi i campi `comboWindowStart`, `nextOnLight` e `nextOnHeavy` e i metodi `IsInComboWindow(int frame)` e `NextFor(AttackButton button)`. Se lavori sul tuo progetto, l'enum `AttackButton` lo crei al passo 4: scrivi prima quello.
3. **Asset.** Crea `Cross`, `Hook` e `Uppercut` in `Assets/_Project/Data/Attacks` (Create > INCOM Fighters > Attack Data), poi completa anche Jab e Straight con finestra e seguiti:

   | Asset | State Name | Startup | Active | Recovery | Hitbox Offset | Hitbox Size | Damage | Hitstun | Knockback | Combo Window Start | Next On Light | Next On Heavy |
   | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
   | Jab | (già fatto) | | | | | | | | | 5 | Cross | — |
   | Cross | Cross | 6 | 3 | 12 | 1.02, 1.27, 0 | 0.3, 0.25, 0.5 | 7 | 18 | 0.2 | 8 | Hook | Uppercut |
   | Hook | Hook | 8 | 3 | 16 | 1.26, 0.67, 0 | 0.35, 0.35, 0.6 | 10 | 22 | 0.3 | 0 | — | — |
   | Straight | (già fatto) | | | | | | | | | 13 | — | Uppercut |
   | Uppercut | Uppercut | 10 | 3 | 22 | 1.1, 1.13, 0 | 0.3, 0.4, 0.5 | 15 | 28 | 0.5 | 0 | — | — |

   - **Verifica:** nell'Inspector del Cross, i campi Next On Light e Next On Heavy portano a Hook e Uppercut.
4. **Input buffer** in `FighterInput` (TODO 4a–4d se usi questo branch). Se lavori sul tuo progetto:
   - sopra la classe dichiara `public enum AttackButton { None, Light, Heavy }` e aggiungi `[DefaultExecutionOrder(-20)]` alla classe, così il buffer invecchia prima che `FighterCombat` lo legga;
   - al posto di `lightQueued` e `heavyQueued`: un campo `[SerializeField, Range(1, 20)] int bufferFrames = 8`, un campo `AttackButton buffered` e un `int bufferAge`, più la proprietà `public AttackButton BufferedButton => buffered;`;
   - in `Update`, a ogni pressione di un pugno, `buffered` diventa quel tasto e `bufferAge` torna a 0;
   - in `FixedUpdate`, se il buffer non è vuoto `bufferAge` aumenta di 1, e oltre `bufferFrames` il buffer torna a `None`;
   - al posto di `ConsumeLight` e `ConsumeHeavy` un metodo pubblico `ConsumeBuffer()` che rimette `buffered` a `None`.
5. **Catene** in `FighterCombat` (TODO 5a–5b) e `Hitbox` (TODO 5c). Se lavori sul tuo progetto:
   - in `FixedUpdate` non si usano più `ConsumeLight` e `ConsumeHeavy`: da fermo, se nel buffer c'è un tasto, si parte con l'attacco di quel tasto (Light → `lightAttack`, Heavy → `heavyAttack`) e si svuota il buffer;
   - in `TickAttack`, subito dopo `hitbox.Tick`, se il frame è nella finestra (`IsInComboWindow`) e la hitbox ha toccato (`HasHit`), il seguito per il tasto nel buffer (`NextFor`) parte subito con `StartAttack`, dopo aver svuotato il buffer;
   - in `Hitbox`, `public bool HasHit => alreadyHit.Count > 0;`.
   - **Verifica:** vicino al manichino J J J fa Jab → Cross → Hook, J J K fa Jab → Cross → Uppercut, K K fa Straight → Uppercut. Lontano dal manichino J J fa due Jab: a vuoto la catena non prosegue.
6. **Eventi del manichino** in `TrainingDummy` (TODO 6a–6c). Due eventi pubblici: `OnHitTaken` (`System.Action<AttackData, bool>`), lanciato alla fine di `HandleHit` con `true` se lo stato era già `Hit` **prima** di aggiornarlo, e `OnRecovered` (`System.Action`), lanciato quando il manichino torna in Idle e in `ResetState` se era in Hit.
7. **ComboTracker** (TODO 7a–7c). Aggiungilo al manichino e completa i TODO.
   - **Verifica:** J J J veloci scrivono «Combo 3 colpi – 22 danni». Premendo il secondo J all'ultimo momento la catena parte lo stesso, ma in Console non compare nessuna combo: era solo una sequenza.

## Sfide

- **La tua combo:** progetta e bilancia una catena nuova usando solo asset, per esempio un secondo Jab (Jab → Jab → Straight) duplicando l'asset del Jab. Il colpo successivo arriva dopo (frame in cui premi − frame del colpo) + il suo startup: se non supera l'hitstun, la combo è vera. Verificalo in Play.
- **Bilancia le distanze:** da lontano, in fondo a Jab → Cross, l'Uppercut va a vuoto perché il knockback ha allontanato il manichino. Ritara i knockback perché arrivi da qualunque distanza da cui arriva il Jab.
- **Buffer più corto o più lungo:** prova `bufferFrames` a 4 e a 12 e osserva come cambia la difficoltà.

## Fine sessione

Fai il commit del progetto funzionante.
