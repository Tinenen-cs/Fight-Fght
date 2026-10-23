# Asset guide

The final visual direction uses athletic fighting-game bodies with smaller proportional heads, fitted magical academy uniforms, detailed pixel environments, matching illustrated UI panels and forbidden rune-rift traps. The eight themed names and powers are defined in `Roster.cs`.

## Character workflow

Each fighter has a separate reference in `Assets/Art/CharacterReferences`. Every action is generated separately as **one four-frame request**, using that reference. Several requests may run concurrently. Full prompts and destinations are recorded in `Docs/animation_sequence_manifest.json`; reference prompts are in `Docs/character_reference_manifest.json`.

Source sequences live in `Assets/Art/CharacterSequences/<InternalID>/<Action>.png`, arranged as **2 columns × 2 rows**, with transparent backgrounds. They are stored at 512 × 512. Frame order is top-left, top-right, bottom-left, bottom-right.

`Tools/assemble_sequences.py` assembles ten source actions into `Assets/Resources/Characters/<InternalID>/sheet.png`: **1024 × 2560**, **4 columns × 10 rows**, **256 × 256** per frame. It preserves alpha and full source colors, resamples with Lanczos, aligns each lower silhouette to pixel 232, and preserves independently authored head portraits. The old short folder IDs remain stable to preserve asset GUIDs; every visible name uses the new academy name.

| Row | Action | Playback |
| --- | --- | --- |
| 0 | Idle | Breathing guard loop |
| 1 | Walk | Four stride phases |
| 2 | Punch | Windup, extension/contact, recovery |
| 3 | Hurt | Recoil and recovery |
| 4 | Knockout / fall | Upright to horizontal; used for rift fall |
| 5 | Combo | Heavy magical melee finisher |
| 6 | Special | Signature elemental cast |
| 7 | Power | Enhanced elemental release |
| 8 | Push | Open-palm shove |
| 9 | Jump | Takeoff, apex, descent, landing |

Run the assembler from the project root after installing Pillow:

```bash
python3 -m pip install Pillow
python3 Tools/assemble_sequences.py
```

The script does not paint poses or create tweened frames. Generated frame spacing and costume consistency should still be reviewed during a human playtest.

## Scenes, effects and interface

`Docs/academy_art_manifest.json` records full prompts, original source files and runtime destinations. Run `python3 Tools/import_academy_art.py` to rebuild those runtime assets from committed sources.

- Three **1280 × 720** academy backgrounds: gate courtyard, grand library and observatory terrace. The runtime displays them on a 1280 × 720 logical canvas with bilinear filtering and native-resolution scenery. No traps are painted into the backgrounds.
- Eight elemental effect sheets: **512 × 256**, four columns × two rows, eight 128 × 128 frames. They accompany projectiles and impact bursts.
- `UI/AcademyPanel.png` and `UI/AcademyButton.png`: wood, brass and dark leather skins. The renderer uses nine-slicing to preserve illustrated corners across panel sizes.
- `Stages/RuneRift.png`: four pulse frames in a 2 × 2 grid. The same opening appears at both stage edges, with its center aligned to the ring-out collision zone. It replaces the iron manhole/lid concept.

## Audio and legacy generator

`Tools/generate_assets.py` regenerates the four original music loops and twelve sound effects using the Python standard library. It preserves existing final artwork and only fills missing legacy art files. Use the image importers above to rebuild current artwork rather than deleting final assets and asking the legacy generator to replace them.

Commit Unity `.meta` files whenever assets are added or moved. `Assets/Animations`, `Assets/Prefabs` and `Assets/Art/UI` remain available for later Animator/SpriteRenderer/UI prefab migration; this prototype selects sprite cells directly in its runtime renderer.

## Authored portrait reactions and selection art

`Assets/Art/PortraitSources/<fighter>.png` is one separately generated four-frame 2×2 head sheet per fighter. Reading order is focused, hurt, dazed, victorious. `Tools/import_portraits.py` fits full heads with padding into 256px cells and creates a 512×512 runtime `portraits.png`, plus the neutral `avatar.png`. The HUD chooses the current reaction; roster cards use the neutral head. `assemble_sequences.py` preserves authored portraits instead of replacing them with body crops.

`showcase.png` is a 768px Lanczos-resampled copy of each original character reference for large opposing selection previews. These are independent of the battle animation atlases. `Docs/portrait_art_manifest.json` and `Docs/lighting_art_manifest.json` record built-in image-tool prompts and committed source destinations.

## Diamond ability icons

Each fighter has a separately generated four-icon source in `Assets/Art/AbilityIconSources/<fighter>.png`, with full prompts in `Docs/ability_icon_manifest.json`. Order is punch, shove, spell and power (2×2 reading order). `Tools/import_ability_icons.py` crops/pads each complete illustrated diamond into a 256px cell and saves a 512×512 `abilities.png` atlas. Runtime HUD renders the four icons in a mirrored diamond arrangement; no body-sprite crops are used as ability icons.

The afternoon floor edit is recorded in `Docs/flat_library_prompt.json`. `Tools/import_academy_art.py` retains full-color 1280×720 stage art. `CombatHudLayout` keeps portal and control bounds explicit and separate.

## Softer academy presentation

The active UI fonts are Cinzel (500), Cinzel Decorative and EB Garamond (500), with SIL OFL licenses under `Assets/Art/Fonts`. Tiny5 is retained as an unused legacy asset. Resources textures use bilinear filtering, no compression and a 4096px maximum so the taller fighter atlases retain their full resolution. Original source images are preserved; final atlases no longer reduce character art to 128 colors. VFX are 1024×512, the portal sheet is 512×512, and portraits/ability sheets are 512×512.

Enlarged control diamonds are 72 logical pixels. Their mirrored clusters sit inward from the stage edges, leaving the largest 200px portal art unobstructed. `CombatHudLayout` exposes both sets of rectangles for validation.
