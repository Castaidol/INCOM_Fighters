# INCOM Fighters — Sessione 5

**Obiettivo:** scegli dall'Inspector come si comporta il manichino, anche durante il Play. Con «Para sempre» sta in guardia, con «Para dopo il primo colpo» lascia passare le combo vere e para quelle con un buco. Con la vita «Normale» va KO e si rialza. Un pannello in basso a sinistra mostra i frame data dell'ultimo attacco, il vantaggio a segno e in parata e com'è andato il colpo.

## Da dove parti

- **Hai finito la sessione 4 nel tuo progetto:** importa `INCOM_Fighters/Pacchetti/Sessione5.unitypackage` (Assets > Import Package > Custom Package…, importa tutto). Aggiunge `HitResult.cs` (in `Scripts/Combat`, con il TODO 3a) e `TrainingSettings.cs` (in `Scripts/Training`, già completo). Le modifiche agli altri script le scrivi tu, seguendo le istruzioni qui sotto. Il `TrainingDummy` cambia molto: se preferisci, copia `TrainingDummy.cs` da questo branch, che contiene la soluzione delle sessioni 3 e 4 con i TODO di questa sessione.
- **Sei rimasto indietro:** usa questo branch. Contiene la soluzione della sessione 4 e trovi già i TODO in tutti gli script da modificare.

## Cosa trovi già pronto

- Le clip del manichino in `Assets/PartyMonsterRumblePBR/Animation`: `Defense` (in loop), `DefenseHit`, `Die01` e `Die01Recover`.
- `TrainingSettings` completo, con le opzioni in italiano nell'Inspector.
- In questo branch: gli stati e i campi nuovi del `TrainingDummy` e il metodo `Signed` del `TrainingHUD`, che colora il vantaggio. Cerca i commenti `TODO`: il numero corrisponde al passo qui sotto.

Finché non completi il passo 3 il manichino perde vita ma non reagisce: è normale. All'inizio la console mostra alcuni avvisi gialli per campi non ancora usati, che spariscono man mano che completi i TODO.

## Teoria in breve

- **Vantaggio in frame:** hitstun (o blockstun, se il colpo è parato) meno i frame che restano all'attaccante dopo il colpo (active − 1 + recovery). Positivo: torni libero prima tu. Negativo: torna libero prima chi ha parato, e può punirti.
- **Parata e blockstun:** un colpo parato non toglie vita, ma blocca chi para per qualche frame e lo spinge un po' indietro.
- **In hitstun non si para:** per questo le combo vere passano anche contro chi para.

## Passi

Dopo ogni **Verifica** prova in Play mode, cliccando nella Game view prima di usare la tastiera. Le opzioni del manichino si cambiano dall'Inspector dell'oggetto Training mentre il gioco è in Play.

1. **Blockstun e vantaggio** in `AttackData` (TODO 1a–1b). Aggiungi il campo `blockstun` e le proprietà `FramesAfterHit`, `AdvantageOnHit` e `AdvantageOnBlock`. Poi imposta il blockstun negli asset:

   | Asset | Blockstun | A segno | In parata |
   | --- | --- | --- | --- |
   | Jab | 10 | +5 | −1 |
   | Cross | 12 | +4 | −2 |
   | Hook | 14 | +4 | −4 |
   | Straight | 14 | +5 | −3 |
   | Uppercut | 16 | +4 | −8 |

2. **TrainingSettings.** Leggi lo script: due enum, `DummyBehavior` e `DummyHealthMode`, con `[InspectorName]` per i nomi in italiano. Aggiungilo all'oggetto `Training` e collegalo al campo Settings del `TrainingDummy`.
3. **Chi decide com'è andato il colpo.**
   - In `HitResult.cs` completa l'interfaccia `IHitReceiver` (TODO 3a).
   - In `Hurtbox` (TODO 3b–3c), in `Awake`, cerca un `IHitReceiver` tra i genitori. In `ReceiveHit`, se c'è decide lui, altrimenti il colpo va a segno come prima. `ReceiveHit` ora restituisce un `HitResult`.
   - In `Hitbox` (TODO 3d), un evento `OnContact` con l'attacco e il risultato.
   - Se lavori sul tuo progetto: togli l'evento `OnHit` della Hurtbox. Il `TrainingDummy` non si iscrive più: dichiara `: IHitReceiver` e il suo `HandleHit` diventa `public HitResult ReceiveHit(AttackData attack, Transform attacker)`, che per ora restituisce `HitResult.Hit`.
   - **Verifica:** con Comportamento Fermo tutto funziona come nella sessione 4.
4. **Animator del manichino.** In `Dummy.controller` aggiungi quattro stati **senza transizioni**: `Guard` (clip Defense), `Block` (clip DefenseHit, Speed 2.5), `KO` (clip Die01) e `GetUp` (clip Die01Recover).
5. **Parata** nel `TrainingDummy` (TODO 5a–5e).
   - `IsGuarding`: sempre vero con «Para sempre». Con «Para dopo il primo colpo», vero se dall'ultimo contatto sono passati meno di `guardMemory` frame (60). Falso con «Fermo».
   - In `ReceiveHit`, un colpo che non continua una combo, quando il manichino è in guardia, viene parato: `Block` e `HitResult.Blocked`.
   - `Block`: niente danno, blockstun dall'attacco, knockback ridotto, clip Block dal tempo 0, evento `OnBlocked`. In `FixedUpdate` il blockstun si consuma come l'hitstun.
   - Da fermo la posa segue la guardia: `Guard` quando para, `Idle` quando no.
   - **Verifica:** con «Para sempre» il manichino sta in guardia e Jab → Cross viene parato due volte senza perdere vita. Con «Para dopo il primo colpo» J J J veloci vanno a segno, il Cross premuto tardi viene parato e dopo un secondo la guardia si abbassa.
6. **KO e rialzo.**
   - In `Health` (TODO 6a–6b): la proprietà `MinHealth`, l'evento `OnKO` e nessun danno quando la vita è già a 0.
   - In `DamageLogger` (TODO 6c): scrive anche il KO.
   - Nel `TrainingDummy` (TODO 6d–6g):
     - prima del danno `MinHealth` dipende dalla vita scelta (1 con «Infinita»);
     - a vita 0 il manichino va KO, resta a terra `koFrames` frame (90), si rialza in `getUpFrames` frame (52) e torna con la vita piena;
     - durante KO e rialzo i colpi sono ignorati e la rigenerazione non conta.
   - **Verifica:** con «Normale» qualche Straight → Uppercut mandano KO, i colpi durante il KO sono ignorati e dopo circa 2,4 secondi il manichino si rialza con la barra piena. Con «Infinita» la vita si ferma a 1. R durante il KO lo rimette in piedi.
7. **ComboTracker** (TODO 7). Scrive in Console i colpi parati, per esempio «Cross parato».
8. **Frame data nell'HUD.**
   - In `FighterCombat` (TODO 8a), gli eventi `OnAttackStarted` e `OnWhiff`.
   - Nel Canvas HUD crea un'Image `FrameData` in basso a sinistra (Anchor Presets in basso a sinistra con Shift, Pos X 40, Pos Y 40, Width 820, Height 100, Source Image Bianco, colore scuro semitrasparente).
   - Dentro `FrameData` metti due Text (UI > Legacy > Text): `Dati` a sinistra (font 26) ed `Esito` a destra (font 30, grassetto), tutti e due con Rich Text attivo.
   - Nel `TrainingHUD` collega Player, Frame Data Text e Result Text, poi completa i TODO 8b–8d.
   - **Verifica:** il Jab mostra +5 e −1, l'Uppercut +4 e −8, e l'esito cambia a ogni colpo (A SEGNO, PARATO, IGNORATO, A VUOTO).

## Sfide

- **Casuale:** una quarta opzione di comportamento che para un colpo su due.
- **Stordito:** dopo una combo da 5 colpi il manichino resta stordito con la clip Dizzy e intanto non para.
- **Vantaggio misurato:** conta in gioco i frame tra quando torni libero tu e quando torna libero il manichino, e confrontali con il vantaggio calcolato.

## Fine sessione

Fai il commit del progetto funzionante.
