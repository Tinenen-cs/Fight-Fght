# Asset guide

All character art is original procedural pixel art. Regenerate it with `python3 Tools/generate_assets.py` (Python 3, standard library only). The generator is deterministic and overwrites generated PNG and WAV files; save hand-painted replacements elsewhere before regeneration.

## Fighter sheets

Each `Assets/Resources/Characters/<Name>/sheet.png` is **256 × 640**, arranged as **4 columns × 10 rows** of **64 × 64** cells. Transparent padding is intentional. Characters face right; the renderer mirrors UVs for left-facing poses. PNG coordinates below start at top-left.

| Row | Pose | Playback |
| --- | --- | --- |
| 0 | Idle | Loop |
| 1 | Walk | Loop |
| 2 | Punch | One attack cycle |
| 3 | Hurt | Stun reaction |
| 4 | Knockout | Fall, then hold final frame |
| 5 | Combo finisher | One attack cycle with magic sparks |
| 6 | Special cast | One attack cycle |
| 7 | Power attack | One attack cycle with aura |
| 8 | Push | One attack cycle |
| 9 | Jump | Loop while airborne |

The 64 × 64 avatars are independent files. `Roster.cs` defines display names, colors, weapons and power names. Sprite cells are selected directly in `AcademyGame.DrawSprite`; no Sprite Editor slicing or Animator controller is required.

Each `Effects/<Name>.png` is **256 × 32**, with eight **32 × 32** cells. They animate on projectiles and impact bursts; power attacks enlarge the visual. Colors and particle motifs distinguish the spells. These are intentionally basic prototype animations; frame-by-frame character polish is a future art task.

## Arenas

Three **480 × 270** backgrounds: Witchwood, ObsidianHall, AstralTower. The game scales them to 1280 × 720, draws the animated manhole over the walkway, and overlays combat/UI. Point filtering, no compression and no mipmaps preserve hard pixel edges. The 480-to-1280 scale is fractional, so some pixels can have unequal screen widths; a future 640 × 360 redraw would give exact 2× scaling.

## Audio

Four mono 22,050 Hz, 16-bit WAV loops (Menu + each arena), plus twelve SFX. The generator synthesizes original square/sine wave patterns. Replace files with the same names to preserve `Resources.Load` lookups. Check volume levels with two players before increasing the defaults.

## Reserved destinations

`Assets/Animations`, `Assets/Prefabs` and `Assets/Art/UI` are intentional destinations for later production assets. The current prototype draws via Unity IMGUI and does not depend on those folders. Unity `.meta` files must be committed when assets are added or moved.
