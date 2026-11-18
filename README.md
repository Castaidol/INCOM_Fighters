# INCOM Fighters — Sessione 7

**Obiettivo:** il gioco diventa un eseguibile da far provare ai compagni. Start o Esc apre un menu di pausa che ferma tutto: con il joypad, le frecce o il mouse scegli come si comporta il manichino, cambi la sua vita, accendi le hitbox, ricominci o esci. Accanto alle opzioni c'è la lista delle combo, generata leggendo gli AttackData. Le hitbox si vedono anche nella build, come cubi trasparenti. A fine lezione fai provare il gioco a un compagno e correggi i bug che trova.

## Da dove parti

- **Hai finito la sessione 6 nel tuo progetto:** importa `INCOM_Fighters/Pacchetti/Sessione7.unitypackage` (Assets > Import Package > Custom Package…, importa tutto). Aggiunge `TrainingMenu.cs` e `ComboList.cs` (in `Scripts/UI`) e `HitboxViewer.cs` (in `Scripts/Training`), tutti con i TODO. Le modifiche agli altri script le scrivi tu, seguendo le istruzioni qui sotto.
- **Sei rimasto indietro:** usa questo branch. Contiene la soluzione della sessione 6 e trovi già i TODO in tutti gli script da modificare.

## Cosa trovi già pronto

- Nei tre script nuovi i campi e i metodi da completare. In `TrainingMenu` anche i nomi delle opzioni in italiano e `Next`, che passa al valore successivo di un enum.
- Nel progetto c'è già quello che il menu usa: `ResetTraining` nel `TrainingManager`, le opzioni in `TrainingSettings`, l'albero delle combo negli AttackData e l'azione Pause (Start o Esc) nella mappa Fighter.
- Cerca i commenti `TODO`: il numero corrisponde al passo qui sotto.

Fai i passi nell'ordine: dopo il TODO 3a `FighterInput` cerca la mappa UI del passo 1 e, se non la trova, all'avvio dà errore. All'inizio la console mostra qualche avviso giallo per campi non ancora usati, che spariscono man mano che completi i TODO.

## Teoria in breve

- **Pausa:** `Time.timeScale = 0` ferma tutto: `FixedUpdate` non gira più, animazioni e particelle si fermano. `Update` invece continua, ed è per questo che il menu funziona. È il contrario dell'hitstop, dove si fermavano solo i combattenti.
- **Action map:** una mappa per ogni situazione. Con il menu aperto la mappa Fighter si spegne e si accende la mappa UI: i pugni non arrivano al combattente.
- **EventSystem:** sa quale pulsante è selezionato e gli manda i comandi, che l'`InputSystemUIInputModule` prende dalla mappa UI del giocatore. Con il joypad serve un pulsante già selezionato quando il menu si apre, altrimenti non sa da dove partire.
- **Build:** quello che funziona nell'editor può non funzionare nell'eseguibile. I Gizmos non ci sono, `Application.Quit` funziona solo lì, uno shader cercato per nome nel codice può mancare. Per questo la build va provata davvero.

## Passi

Dopo ogni **Verifica** prova in Play mode, cliccando nella Game view prima di usare la tastiera.

1. **Mappa UI.** Apri `Fighter.inputactions` e aggiungi la mappa `UI` con cinque azioni. Usa proprio questi nomi: l'EventSystem li cerca da solo.

   | Azione | Action Type e Control Type | Comandi |
   | --- | --- | --- |
   | Navigate | Pass Through, Vector2 | Stick sinistro e croce del joypad; frecce e WASD con Add Up/Down/Left/Right Composite |
   | Submit | Button | A (Croce), Invio, Spazio |
   | Cancel | Button | B (Cerchio), Start, Esc |
   | Point | Pass Through, Vector2 | Mouse: Position |
   | Click | Pass Through, Button | Mouse: Left Button |

   - Ogni comando di tastiera e mouse va nello schema Keyboard, ogni comando del joypad nello schema Gamepad.
   - Modifica lo schema Keyboard (menu degli schemi in alto a sinistra > Edit Control Scheme) e aggiungi il Mouse come dispositivo facoltativo (Optional).
   - Salva con Save Asset.
2. **EventSystem.**
   - GameObject > UI > Event System. Nell'`InputSystemUIInputModule` metti `Fighter` in Actions Asset: le azioni con lo stesso nome si collegano da sole. Controlla che Move sia UI/Navigate e Left Click sia UI/Click; quelle che non esistono restano vuote.
   - Nel `PlayerInput` del `Player` trascina l'EventSystem in UI Input Module: così il menu usa le azioni del giocatore.
   - Aggiungi un `GraphicRaycaster` al Canvas `HUD`: serve al mouse.
   - **Verifica:** nessun errore in Console e il gioco funziona come prima.
3. **FighterInput: pausa e cambio di mappa** (TODO 3a–3d).
   - Le azioni nuove Pause e Cancel (3a). In `Start` la mappa UI si spegne, perché il modulo dell'EventSystem la accende da solo (3b).
   - Un solo segnale apre e chiude il menu: in gioco lo dà Pause, nel menu Cancel (3c).
   - `ConsumePause` e `SetMenuMode`, che cambia mappa e svuota il buffer (3d).
   - **Verifica:** nessun errore e il gioco funziona come prima: il menu ancora non c'è.
4. **Menu di pausa.**
   - In `TrainingSettings` l'opzione `showHitboxes` (TODO 4a).
   - Nel Canvas `HUD`, in fondo alla lista così sta sopra a tutto, crea un oggetto vuoto `Menu` che riempie lo schermo (Anchor Presets con Alt, l'ultimo in basso a destra) e aggiungigli `TrainingMenu`.
   - Dentro `Menu` un'Image `Pannello` che riempie lo schermo, Source Image Bianco, nera al 60%.
   - Dentro `Pannello` un'Image `Opzioni`: Pos X −330, Pos Y 0, Width 780, Height 640, Source Image Bianco, colore scuro. Dentro:
     - un Text (UI > Legacy > Text) `Titolo` in alto, «PAUSA», font 48, grassetto;
     - un oggetto vuoto `Pulsanti` (Pos Y −40, Width 700, Height 480) con un Vertical Layout Group: Spacing 12, Control Child Size su Width e Height, Child Force Expand solo su Width.
   - Dentro `Pulsanti` un Button (UI > Legacy > Button):
     - Source Image Bianco, Normal Color scuro e Selected Color blu ben visibile;
     - Navigation Vertical con Wrap Around;
     - un Layout Element con Preferred Height 66;
     - il Text dentro con font 28, grassetto, bianco, allineato a sinistra.
   - Duplicalo (Ctrl+D) fino ad avere sei pulsanti e cambia nomi e testi: Riprendi, Comportamento, Vita, Hitbox, Ricomincia, Esci.
   - OnClick (trascina `Menu` e scegli il metodo di `TrainingMenu`): Riprendi → `Close`, Comportamento → `CycleBehavior`, Vita → `CycleHealth`, Hitbox → `ToggleHitboxes`, Ricomincia → `Restart`, Esci → `Quit`.
   - Nel `TrainingMenu` collega Player Input (`Player`), Settings e Manager (`Training`), Panel (`Pannello`), First Button (Riprendi) e i Text dei pulsanti Comportamento, Vita e Hitbox. Poi spegni `Pannello` nella scena.
   - Completa i TODO 4b–4f.
   - **Verifica:**
     - Esc apre il menu con Riprendi selezionato e il gioco si ferma.
     - Frecce e Invio cambiano i valori; dopo l'ultimo valore si torna al primo. Funziona anche il mouse.
     - Un J premuto nel menu non fa partire un pugno quando il gioco riparte.
     - Esc chiude il menu, Ricomincia rimette tutto a posto, Esci ferma il Play.
5. **Lista delle combo.**
   - In `FighterCombat` le proprietà `LightAttack` e `HeavyAttack` (TODO 5a).
   - Dentro `Pannello` un'altra Image `ListaCombo`: Pos X 420, Pos Y 0, Width 560, Height 640, Source Image Bianco, colore scuro. Dentro:
     - il titolo «LISTA COMBO»;
     - una legenda in grigio, font 22: «L  pugno leggero: X / Quadrato, tasto J» e, sotto, «H  pugno pesante: Y / Triangolo, tasto K»;
     - un Text `Lista` sotto la legenda, font 30, allineato in alto a sinistra, con Rich Text attivo.
   - Aggiungi `ComboList` a `Menu`, che resta acceso anche a menu chiuso, e collega Player (`Player`) e Text (`Lista`). Completa i TODO 5b–5c.
   - **Verifica:** il menu mostra tre combo: L L L (Jab → Cross → Hook, 22 danni), L L H (Jab → Cross → Uppercut, 27 danni) e H H (Straight → Uppercut, 27 danni).
6. **Hitbox visibili in build.**
   - In `Hitbox` le proprietà `IsActive` e `Size` (TODO 6a), in `Hurtbox` la proprietà `Bounds` (TODO 6b).
   - In `Assets/_Project/Materials` crea due materiali (Create > Material), tutti e due con Shader Universal Render Pipeline/Unlit e Surface Type Transparent:
     - `Hitbox`: Base Map rossa con alpha circa 45%;
     - `Hurtbox`: Base Map azzurra con alpha circa 30%.
   - Aggiungi `HitboxViewer` a `Training` e collega:
     - Settings (`Training`);
     - Hitboxes, con un elemento: `Player`;
     - Hurtboxes, con un elemento: la `Hurtbox` dentro `Dummy`;
     - Cube: la mesh Cube, che trovi nel selettore;
     - i due materiali.
   - Completa i TODO 6c–6d.
   - **Verifica:** con Hitbox: Sì la hurtbox azzurra si vede sempre e la hitbox rossa compare sul pugno nei frame attivi. Con Hitbox: No non si vede niente.
7. **Build.**
   - File > Build Profiles, piattaforma Windows. Nella Scene List lascia solo `Arena`: togli SampleScene.
   - Player Settings: Product Name «INCOM Fighters»; in Resolution and Presentation, Fullscreen Mode Fullscreen Window e Default Screen Width e Height 1920 e 1080.
   - Build nella cartella `INCOM_Fighters/Build/Windows`, che git ignora. La prima build dura qualche minuto. Se compaiono molti avvisi gialli sugli shader (ConvGeneric), vengono da un pacchetto del progetto, non dal tuo codice.
   - **Verifica:** nell'eseguibile funzionano menu, lista delle combo e hitbox (azzurre e rosse, non rosa), ed Esci chiude il gioco.
8. **Playtest a coppie.** Uno gioca con la build dell'altro, l'altro osserva e scrive senza suggerire; poi vi scambiate. Prima i compiti, poi gioco libero.

   **Compiti**

   - [ ] Apri il menu con Start (o Esc), metti Comportamento su «Para sempre» e verifica che il manichino pari.
   - [ ] Fai L L L: il contatore deve dire 3 colpi e 22 danni.
   - [ ] Con la vita «Normale» manda KO il manichino e aspetta che si rialzi.
   - [ ] Accendi le hitbox e scopri quale pugno arriva più lontano.
   - [ ] Trova il colpo che in parata è a −8 e prova a colpire di nuovo prima che il manichino torni libero.
   - [ ] Apri il menu nel mezzo di una combo e chiudilo: il gioco riparte da dove era e nessun pugno parte da solo?
   - [ ] Ricomincia ed Esci funzionano? Il joypad funziona ovunque, menu compreso?

   **Modulo per i bug** (una riga per bug)

   | Cosa ho fatto | Cosa mi aspettavo | Cosa è successo | Succede sempre? | Gravità |
   | --- | --- | --- | --- | --- |
   | Esempio: aperto il menu subito dopo un colpo | La camera ferma dietro al menu | La camera vibra | Sì, se il colpo è appena andato a segno | Estetico |

   Il bug dell'esempio è vero, prova a farlo comparire anche tu. Il tremolio della camera calcola a ogni frame uno spostamento casuale. Con `Time.timeScale = 0` il tempo non scorre e il tremolio non finisce mai. La correzione è il TODO 8 in `FightCamera`. Poi correggi i bug che hanno trovato i tuoi compagni, dal più grave.

## Sfide

- **Pushbox visibili:** l'`HitboxViewer` disegna anche le pushbox, in giallo.
- **Altre righe nel menu:** «Input display: Sì/No» e «Buffer: 8 frame», che scorre tra 4, 8 e 12.
- **Tasti veri nella lista:** la legenda mostra i comandi letti dall'Input System con `GetBindingDisplayString`, diversi per joypad e tastiera.
- **Valori con sinistra e destra:** le righe delle opzioni cambiano valore con le frecce sinistra e destra invece che con Invio.

## Fine sessione

Fai il commit del progetto funzionante. Il progetto è finito: tieni la build per mostrarla.
