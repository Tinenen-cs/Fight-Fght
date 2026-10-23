# Hexbound design and implementation

## Scope decisions

“8 player” is interpreted as **eight selectable fighters**, with **two simultaneous local players**, matching the explicit two-player requirement. “Three stages” is implemented as exactly three rounds, each in a different arena. Every round has 99 seconds, and ties do not award wins. The player with the most round wins is the champion; an overall tie is a match draw.

The setting is a forbidden academy tournament. Women: Emberlyn Ashveil (fire), Isolde Frostquill (ice), Luneth Noctivale (moon), Briar Mosswhisper (nature). Men: Caelum Stormcrest (lightning), Flint Oakwarden (earth), Vesper Stargrave (gravity), Corvin Hexthorn (blood rune). Each has a weapon silhouette, palette, avatar and animated sheet. The reference screenshots inform only interface positioning.

## Flow

Title → P1 and P2 selection/lock-in → three-second countdown → Witchwood fight → result → Enter → Obsidian Library → result → Enter → Astral Observatory → final champion/draw → character selection.

Escape pauses live combat. Losing application focus pauses automatically. Help overlays the screen; it does not pause on its own, so pause first if reading during combat. Menu music and stage music loop, with action cues layered on top.

## Combat rules

- No health or damage pool; only falling into a rune rift can lose a round. Starting arcana: 35, cap: 100. Regeneration: 4 per second.
- Movement: 235 logical pixels/second, reduced to 75 while guarding.
- Melee: 115-pixel reach, 70-pixel vertical tolerance, 0.30-second cycle, active hit after 45% of cycle.
- Basic punch knockback: 320. Third combo strike: 590. Combo grace period: 0.85 seconds from the last start.
- Push: 760 knockback, 0.45-second cycle.
- Spell: 20 arcana, 0.48-second cycle. Power: 60 arcana, 0.75-second cycle; 1.7× spell knockback.
- Guard: 65% knockback reduction. Guard does not eliminate push risk.
- Hits grant the attacker 7 arcana on melee contact, including shoves. Receiving an unguarded hit stuns for 0.22 seconds.
- Jump: upward speed 510, gravity 1100. No double jump. Fighters can pass over one another when vertically separated.
- Only rune rift falls end the round with a winner. At 99 seconds without a fall, declare a draw.
- Each arena has two forbidden rune rifts: centers x=100 and x=1180 on the 1280-wide playfield. Starting players are at x=410 and x=870. The center is safe. A grounded fighter within 36 logical pixels of either rift center falls and loses the round. The opening is a carved academic seal around a magical void, not a sewer cover. Its four-frame pulse loops while visible.

## Architecture

`DuelModel` is pure C#, exposing inputs, player state, projectiles, impact bursts and audio event names. It does not depend on Unity. `AcademyGame` handles input, menus, sound, timing and IMGUI presentation. `Roster` holds display metadata. `ProjectSetup` handles first-run scene setup and point-filtered asset import. `RuleChecks` verifies gameplay rules without external packages.

Logical coordinates use a 1280 × 720 canvas, letterboxed to the actual view. The runtime delta is capped at 1/30 second for stability; severe frame stalls slow gameplay rather than skipping physics, so this prototype's timer is simulated gameplay time rather than wall-clock time. The local game is not designed for network lockstep.

## Character artwork

Eight single-character reference sprites establish athletic fighting-game bodies, smaller heads, fitted academy jackets, articulated limbs and signature weapons. Each of the ten actions has its own four-frame image-tool request, using that character reference. The final runtime sheet still has four columns and ten rows of 128 × 128 cells. Transparent source sequences use a 2 × 2 layout. The assembler aligns their lower silhouette to a consistent pixel baseline and derives matching head portraits. Generated frame spacing and pose consistency remain a human playtest/polish task.

The three 640 × 360 backgrounds use richer brickwork, gothic academic architecture, amber window light and layered skies inspired by the supplied street scene. Point sampling retains visible pixel texture. Illustrated wood/brass UI skins use nine-slice rendering to preserve corner details across menus, player cards, timer panels and results. Arcana is the only resource meter; no health or lives are tracked.

## Next production steps

Test the feel with two people, tune reach/knockback, improve frame silhouettes, add more distinct special behaviors, then consider migrating the renderer into SpriteRenderer/UI prefabs. Keep the pure combat model while extending tests for new rules. Controller support and standalone builds need their own implementation and validation.

The renderer uses an arena-specific floor baseline for the library’s lower foreground pavement; movement, jump height and trap checks remain relative to that floor. Both rune rifts use the same horizontal collision positions across the three arenas.
