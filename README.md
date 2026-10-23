# Hexbound: Academy Duel

A local **two-player, 2D pixel-art magical fighting prototype** made with Unity and edited in VS Code. Four witches and four wizards duel across an academy gate courtyard, a grand library hall, and an observatory terrace. Attack, chain a three-strike combo, cast spells, guard, jump, and shove your opponent into a forbidden rune rift.

The supplied fighting-game screenshots inspire the **UI arrangement only**: opposing character cards, a portrait selection grid, mirrored player panels, and a central timer. Fighters use athletic, proportioned bodies inspired by the supplied fighting-game roster, fitted magical academy uniforms, and smaller heads. Each four-frame action sequence is generated separately from a consistent character reference, then assembled into a runtime sheet. The three detailed academy arenas retain visible pixel texture, and the interface uses matching walnut, brass and midnight-blue panels. This is a student prototype with synthesized audio, ready for iteration rather than a finished commercial fighting game.

![Character selection preview](Docs/Previews/02-select.png)

## Download & play

Repository: **[Tinenen-cs/Fight-Fght](https://github.com/Tinenen-cs/Fight-Fght)**. GitHub sign-in is complete and the prototype is published to this repository. Open the local project in Unity to test. No standalone application, mobile package, or online multiplayer service is included.

| Device | Output | Status |
| --- | --- | --- |
| macOS | Unity Editor playtest | Compiled, asset-validated and visually smoke-tested with installed 6000.2.2f1 |
| Project Unity version | 6000.3.27f1 | Current local Editor; original prototype also verified on 6000.2.2f1 |
| Windows / Linux | Editor / future desktop build | Not tested |
| Browser / mobile | Future builds | No touch controls or builds |

## Activity: Two-player academy duel

| Rule | Behavior | Code |
| --- | --- | --- |
| Two players | Shared keyboard, independent character choices; mirror matches allowed | `AcademyGame` |
| Intro → selection → combat | Animated title, eight portrait cards, individual lock-in buttons | `AcademyGame` |
| Basic attacks | Short attack startup, one hit per strike, reach and height checks | `DuelModel` |
| Combo | Tap punch three times about 0.35–0.65 seconds apart; the third strike hits harder | `DuelModel` |
| Spells | Eight profiles with different projectile speeds, magic and knockback | `DuelModel` |
| Power attack | Enhanced spell costs 60 arcana; regular spell costs 20 | `DuelModel` |
| Guard / jump / shove | Reduce knockback, clear hazards, push the opponent | `DuelModel` |
| Win a round | Force the rival into an edge rune rift; no health, damage or life depletion | `DuelModel` |
| Three stages | Exactly three rounds, one per arena, including draws; most wins takes match | `DuelModel` |
| Timer | Each round starts with a three-second preparation count, then 99 seconds | `AcademyGame`, `DuelModel` |
| Rune rift twist | Two permanently open rune rifts, one at each stage edge; the center is clear | `DuelModel` |

### Recording a short demonstration

1. Start the title screen, then choose different fighters for both players.
2. Lock in with **F** and **J**. Show the countdown, player cards, arcana meters and timer.
3. Move close, demonstrate punching, a three-hit combo, guarding, and a shove.
4. Cast a spell, then build enough arcana for a power attack.
5. Show the exposed rune rift. Jump across it or force the opponent in using attacks and powers.
6. Play all three arenas. Press Enter after each result; the final screen names the overall champion or a draw.

## Status

The live task list is [Docs/TODO.md](Docs/TODO.md). Per-action artwork progress is in [Docs/ART_PROGRESS.md](Docs/ART_PROGRESS.md). Evidence and limitations are in [Docs/VALIDATION.md](Docs/VALIDATION.md).

| Step | Feature | Status |
| --- | --- | --- |
| 1 | Organized Unity project, scene, source and VS Code recommendations | Implemented |
| 2 | Eight athletic academy fighters, eight matching avatars, 320 character frames | Included |
| 3 | Per-fighter animated magic sheets: 64 effect frames | Included |
| 4 | Title, two-player selection, combat HUD, pause and results | Implemented |
| 5 | Three arenas and 99-second rounds | Implemented |
| 6 | Combat, combo, spells, power attacks, guard, jump and rune rift | Implemented |
| 7 | Four music loops and twelve sound effects | Included and connected |
| 8 | Automated combat checks / Unity import and compile | Passed; see validation notes |
| 9 | Two-person hardware playtest | Pending |
| 10 | Keyboard, Xbox and PS4 input | Implemented; physical controller checks pending |
| 11 | Online play, mobile and distributable builds | Outside this prototype |

## Features

- Athletic academy fighters and detailed pixel environments, rendered from higher-resolution art with bilinear filtering.
- Four female and four male magical human characters, each with a signature weapon and power.
- Idle, walk, punch, hurt, knockout, combo, spell, power, shove and jump animation rows.
- Mana regeneration and mana gains on melee contact; stronger attacks require resource management.
- Direction automatically faces the other fighter. Jumping lets fighters pass one another.
- Original wizard-academy chamber-style loops for selection and each arena; cues for movement, combat, UI, timer alerts and results.
- Focus loss pauses combat. Escape pauses/resumes, H opens help, and M saves a mute preference.
- No external runtime packages; built-in Unity modules keep initial setup small.

**Ring-out-only rules:** attacks never remove life. A grounded fighter over an edge rune rift falls and loses the round. If 99 seconds expire without a fall, the round is a draw. Arcana is a spell resource, not health.

![Academy fighter roster](Docs/Previews/04-roster.png)

## Characters

| Fighter | Character | Power | Weapon | Power attack |
| --- | --- | --- | --- | --- |
| Emberlyn Ashveil | Girl / Ember scholar | Fire | Ember wand | Phoenix surge |
| Isolde Frostquill | Girl / Frost duelist | Ice | Glacier blade | Winter verdict |
| Luneth Noctivale | Girl / Moon archivist | Lunar magic | Lunar staff | Eclipse wave |
| Briar Mosswhisper | Girl / Thorn alchemist | Nature | Thorn rapier | Briar bloom |
| Caelum Stormcrest | Boy / Storm prefect | Lightning | Storm hammer | Thunder oath |
| Flint Oakwarden | Boy / Stone warden | Earth | Oak spear | Mountain breaker |
| Vesper Stargrave | Boy / Void astronomer | Gravity | Gravity orb | Event horizon |
| Corvin Hexthorn | Boy / Rune exile | Blood runes | Runic scythe | Crimson seal |

Spell names are thematic: the first prototype distinguishes speed, knockback, color and particle motifs. Ice does not yet freeze, and earth does not create terrain. All fighters share a basic melee moveset.

## Controls

| Action | Player 1 | Player 2 |
| --- | --- | --- |
| Select character | W A S D | Arrow keys |
| Lock choice | F | J |
| Move | A / D | Left / Right |
| Jump | W | Up |
| Guard | S (hold) | Down (hold) |
| Punch / combo | F (tap) | J (tap) |
| Push | G | K |
| Spell: 20 arcana | R | U |
| Power: 60 arcana | T | I |

**Enter:** start / next round / return to selection. **Escape:** unlock choices or pause combat. **H:** help. **M:** mute. Mouse buttons also support menu selection and confirmation. Click cards for P1 first, lock P1, then click cards for P2. Keyboard selections remain independent.

## Project structure

```text
Assets/
  Scenes/Main.unity             saved playable entry scene
  Scripts/Runtime/              game flow, roster, pure C# combat rules
  Scripts/Editor/               setup, pixel import, validation, preview capture
  Resources/Characters/        one folder per fighter: sheet.png + avatar.png
  Resources/Effects/           eight animated power-effect sheets
  Resources/Stages/            three pixel arena backgrounds
  Resources/Audio/Music/       title/select + three arena loops
  Resources/Audio/SFX/         twelve original sound cues
  Resources/UI/                wizard-style fonts and interface art
  Art/Fonts/                   font license
  Art/UI/                      reserved for future interface artwork
  Animations/                  reserved for future Animator controllers
  Prefabs/                     reserved for future scene-object refactoring
Docs/                          checklist, testing, art guide, design notes
Tools/generate_assets.py       reproducible art and WAV generator
Tools/test_rules.sh            independent combat-rule checks
Packages/                     built-in module manifest / lock
ProjectSettings/              Unity settings
.vscode/                      editor recommendations
```

The prototype uses a pure C# combat model and Unity's immediate-mode GUI to draw a fixed 1280 × 720 playfield. It does not yet use SpriteRenderer prefabs, Animator controllers, Rigidbody2D physics, or UI Toolkit. This keeps the first student version easy to run; the reserved folders support later refactoring. Sprites are animated by selecting cells in their sheets. The scene and import settings are committed with Unity `.meta` files.

## Audio credits

All music and sound effects are original synthesis from `Tools/generate_wizard_audio.py`; no commercial songs, third-party samples or recordings are used. Four distinct scores use harp, celesta, flute, strings and choir-style synthesis, with different melodies, tempos and harmony for the menu and each arena. `select`, `confirm`, `punch`, `hit`, `cast`, `power`, `jump`, `step`, `fall`, `win`, `bell`, and `block` cues are connected to game events.

## Art and font credits

Each fighter starts from a separately generated single-character reference. Every action is generated in its own four-frame request (a 2 × 2 source sequence), then `Tools/assemble_sequences.py` assembles the ten actions into the runtime sheet. Character references, sequence sources and full prompts are stored in `Assets/Art/CharacterReferences`, `Assets/Art/CharacterSequences` and `Docs/animation_sequence_manifest.json`.

Arenas, eight element-effect sheets and interface panels were generated as separate assets with the built-in image tool; source PNGs and prompts are preserved in `Assets/Art` and `Docs/academy_art_manifest.json`. The academy backgrounds are sampled at 640 × 360 and rendered from higher-resolution art with bilinear filtering to retain deliberate pixel texture. The supplied screenshots guide layout, athletic body proportions and environmental richness. No Tekken or Street Fighter assets are included.

The active interface uses Cinzel, Cinzel Decorative and EB Garamond from Google Fonts, licensed under the included SIL Open Font Licenses in `Assets/Art/Fonts`. Tiny5 remains an unused legacy asset. README organization follows the user's [Balance-with-me](https://github.com/Tinenen-cs/Balance-with-me) reference; its game code and artwork were not copied. No repository-wide license has been chosen.

## How to run

### A. Mac — Unity Hub and VS Code

1. Use this existing project folder, or clone:
   ```bash
   git clone https://github.com/Tinenen-cs/Fight-Fght.git
   cd Fight-Fght
   ```
2. In **Unity Hub → Installs**, select **6000.2.2f1**. The original prototype was verified with this version. Your currently open Editor has updated the project record to 6000.3.27f1; this update is also checked there.
3. In **Projects → Add project from disk**, choose the root `Fight-Fght` folder containing `Assets`, `Packages`, and `ProjectSettings`.
4. Let Unity finish importing and compiling. Open **Assets → Scenes → Main**, or use **Hexbound → Open Main Scene**.
5. Open the **Game** tab, choose **16:9** (preferably **1280 × 720**), and press **Play**. Main includes a visible arena preview before Play; the title and interactive UI run during Play.
6. Click inside the Game view. Press **any key or click**, choose both characters, and lock in with **F** and **J**. Choose the opening arena with A/D or arrows, then press Enter.
7. Follow the controls above. Press **Enter** after each round result to continue.
8. Open the same project folder in **VS Code**. Unity is the game editor; VS Code is the code editor, with Codex available to assist. Accept the recommended C# and Unity extensions as needed.
9. Keep the Unity Console open while testing. Stop Play before editing settings or saving scene changes.

### B. Other devices

Desktop source may be opened with Unity, but Windows/Linux and standalone builds have not been tested. Browser, Android, iOS and touch controls are not part of this prototype. Xbox and PS4 controller bindings are included; physical controller testing is pending. Use the Mac Unity Editor first.

## Troubleshooting

- **Version mismatch:** the current project record follows your open **6000.3.27f1** Editor. The original prototype was tested on **6000.2.2f1**; avoid switching a project while it is open in another Editor.
- **Empty scene:** use Hexbound → Open Main Scene. Main contains a saved arena preview visible before Play; the full menu appears after Play. Use Hexbound → Prepare Visible Scene Preview to rebuild the preview. Unsaved scenes are preserved.
- **Keys do nothing:** click the Game view. Escape resumes after focus loss. Under Project Settings → Player, keep Active Input Handling on **Both**; gameplay and menus use the Unity Input System, with legacy input retained for the mute shortcut. Restart Unity after changing input backends.
- **Too many simultaneous keys:** some keyboards cannot register both players' held keys. Try a second USB keyboard or release guard before attacking; physical keyboard testing is pending.
- **No sound:** press M, disable the Game view mute toggle, and check macOS output volume.
- **Spell does not start:** regular spells require 20 arcana, power attacks 60. Wait for regeneration or land melee hits.
- **Punch misses:** move closer and be at a similar height. Combo taps must be separated by the attack cooldown.
- **Drain loss:** do not stand over the open rune rift. Jumping clears it; guarding reduces shove force.
- **Scene generation warns about unsaved work:** save your untitled scene, then use Hexbound → Open Main Scene.

## Contributing

1. Check [Docs/TODO.md](Docs/TODO.md) and make one focused improvement.
2. Edit files in this repository; keep generated assets and their `.meta` files together.
3. Run combat checks and a Unity playtest. Record actual results in `Docs/VALIDATION.md`.
4. Commit and push the changes:
   ```bash
   git add Assets Docs Tools Packages ProjectSettings README.md .gitignore .vscode
   git commit -m "Describe the game change"
   git push origin main
   ```

Do not commit Unity caches, credentials, local logs, or generated application builds. To rebuild runtime art from committed sources, install Pillow and run `python3 Tools/import_academy_art.py`, then `python3 Tools/assemble_sequences.py`. `python3 Tools/generate_assets.py` only fills missing legacy assets and preserves final artwork/audio. To rebuild the wizard scores and sound effects, install NumPy and run `python3 Tools/generate_wizard_audio.py`, then let Unity reimport. See [Docs/ASSET_GUIDE.md](Docs/ASSET_GUIDE.md) before changing sheet dimensions.

## Updated duel flow

Press any key or click on the text-only wizard title → both players choose apprentices → choose the opening arena → duel. Character selection uses opposing large fighter artwork, side biographies, and a compact two-row roster inspired by the supplied layout. Each fighter has four authored head reactions for the HUD: focused, hurt, dazed and victorious.

Every round lasts 99 seconds. No ring-out before time expires means a draw, including when a fighter is knocked down. Five unguarded hits cause a 1.4-second knockdown, then the fighter recovers; knockdown never awards a win. Guarded hits do not count. Only falling into either forbidden rune rift eliminates a fighter. All three arenas rotate from your selected opening stage: morning gates, afternoon library, night observatory.

Head artwork and prompts are in `Assets/Art/PortraitSources` and `Docs/portrait_art_manifest.json`; rebuild with `python3 Tools/import_portraits.py`. Title typography uses Cinzel Decorative, licensed under the included OFL in `Assets/Art/Fonts`.

## Keyboard, Xbox and PS4 input

The central input manager is `Assets/Scripts/Runtime/AcademyInputManager.cs`. Editable bindings are in `Assets/Resources/Input/AcademyControls.inputactions`; double-click this asset in Unity to edit the two player action maps. Input System 1.14.1 is pinned in the package manifest. Active Input Handling is set to Both; allow Unity to finish importing and restart the Editor if prompted.

| Action | Player 1 keyboard | Player 2 keyboard | Xbox | PS4 |
| --- | --- | --- | --- | --- |
| Move / menu navigation | WASD | Arrows | Left stick / D-pad | Left stick / D-pad |
| Jump / menu confirm | W / F or Enter | Up / J or Enter | A | Cross |
| Guard (hold) | S | Down | LB | L1 |
| Punch / combo | F | J | X | Square |
| Shove / menu back | G / Esc | K / Esc | B | Circle |
| Spell (20 arcana) | R | U | Y | Triangle |
| Power (60 arcana) | T | I | RB | R1 |
| Pause / resume | Esc | Esc | Menu | Options |
| Help | H | H | View | Share |

The first connected controller owns Player 1, the second Player 2. Both keyboard sets remain available, so one controller plus keyboard or two controllers can play. A remaining controller keeps its player slot after another disconnects; a replacement fills the vacant slot. Disconnecting an assigned controller pauses an active fight. Use Menu/Options or Esc to resume. Button prompts follow the last input used by that player. Pair controllers in macOS Bluetooth settings or connect by USB before Play; actual device compatibility depends on macOS and the controller model ([Unity gamepad support](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Gamepad.html)).

The five large top segments show remaining poise, not lives. Each clean hit removes one; the fifth knocks the player down for 1.4 seconds, then all five refill. Guarded contacts reduce knockback but do not remove poise. Only a rift fall eliminates. The thin blue meter is spell arcana. Diamond clusters below the portals show dedicated punch, shove, spell and power icons, with only compact key/button badges. Movement, jump, guard, spell costs and full rules are in the help screen (H, View or Share).

## Wizarding audio and clean fight view

| Screen / arena | Original score | Style |
| --- | --- | --- |
| Title / selection | The Forbidden Admission | Mysterious celesta, choir and harp |
| Morning gates | Dawn Among the Wards | Brighter flute, strings and harp |
| Afternoon library | Sunlit Grimoire | Warm plucked harp and strings |
| Night observatory | Constellations After Curfew | Sparse celesta, choir and moonlit chimes |

Music switches automatically when the selected arena starts and when moving to the next arena. Twelve replacement effects cover crystalline menus, ward blocks, spell casts, power surges and portal falls. Audio metrics, titles and reproduction details are recorded in `Docs/audio_manifest.json`. Physical listening/volume tuning remains part of your Mac playtest.

The active fight view keeps plain head portraits, names, five poise segments, a thin arcana meter, round markers, timer and four diamond ability icons per player. Only small key/controller badges appear on the icons. Device labels, large control legends, cost descriptions, stage banners and rule footers are removed from combat; press H or View/Share for details.

## Start-screen fix and growing portals

The title accepts individual keyboard press edges (including a new key while another is held), mouse clicks, controller buttons and direct Game-view key/mouse events. Legacy keyboard handling remains available alongside the Input System. Click the Game view after pressing Play.

The portal grows at **49.5 seconds remaining** and again at **15 seconds remaining**. Art grows from 144 → 172 → 200 logical pixels, while the grounded fall radius grows from 36 → 48 → 60 pixels. Jumping still clears it; timeout remains a draw. Both reset at the next round. Larger 72px attack diamonds are positioned inward, clear of portals even at maximum growth.

Higher-resolution fighter frames and scenery, full color, smooth texture filtering and wizard-style fonts reduce the chunky pixel appearance without replacing character identities or animation poses.
