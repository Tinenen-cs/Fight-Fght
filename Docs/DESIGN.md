# Hexbound design and implementation

## Scope decisions

“8 player” is interpreted as **eight selectable fighters**, with **two simultaneous local players**, matching the explicit two-player requirement. “Three stages” is implemented as exactly three rounds, each in a different arena. Every round has 99 seconds, and ties do not award wins. The player with the most round wins is the champion; an overall tie is a match draw.

The setting is a forbidden academy tournament. Women: Lyra (fire), Mira (ice), Selene (moon), Iris (nature). Men: Alden (lightning), Rowan (earth), Orion (gravity), Dorian (blood rune). Each has a weapon silhouette, palette, avatar and animated sheet. The reference screenshots inform only interface positioning.

## Flow

Title → P1 and P2 selection/lock-in → three-second countdown → Witchwood fight → result → Enter → Obsidian Library → result → Enter → Astral Observatory → final champion/draw → character selection.

Escape pauses live combat. Losing application focus pauses automatically. Help overlays the screen; it does not pause on its own, so pause first if reading during combat. Menu music and stage music loop, with action cues layered on top.

## Combat rules

- Health: 100. Starting arcana: 35, cap: 100. Regeneration: 4 per second.
- Movement: 235 logical pixels/second, reduced to 75 while guarding.
- Melee: 115-pixel reach, 70-pixel vertical tolerance, 0.30-second cycle, active hit after 45% of cycle.
- Basic damage: 7. Third combo strike: 18. Combo grace period: 0.85 seconds from the last start.
- Push: zero damage, strong knockback, 0.45-second cycle.
- Spell: 20 arcana, 0.48-second cycle. Power: 60 arcana, 0.75-second cycle; twice spell damage and 1.7× knockback.
- Guard: 80% damage reduction and 65% knockback reduction. Guard does not eliminate push risk.
- Hits grant the attacker 7 arcana on melee contact, including shoves. Receiving an unguarded hit stuns for 0.22 seconds.
- Jump: upward speed 510, gravity 1100. No double jump. Fighters can pass over one another when vertically separated.
- Health depletion and manhole falls end the round. At 99 seconds, higher health wins.
- Drain opens at elapsed second 8, alternates open/closed every 7 seconds, and warns for 2 seconds before opening. Position changes each arena. Standing over the open center with height below 12 causes a fall loss.

## Architecture

`DuelModel` is pure C#, exposing inputs, player state, projectiles, impact bursts and audio event names. It does not depend on Unity. `AcademyGame` handles input, menus, sound, timing and IMGUI presentation. `Roster` holds display metadata. `ProjectSetup` handles first-run scene setup and point-filtered asset import. `RuleChecks` verifies gameplay rules without external packages.

Logical coordinates use a 1280 × 720 canvas, letterboxed to the actual view. The runtime delta is capped at 1/30 second for stability; severe frame stalls slow gameplay rather than skipping physics, so this prototype's timer is simulated gameplay time rather than wall-clock time. The local game is not designed for network lockstep.

## Next production steps

Test the feel with two people, tune reach/knockback, improve frame silhouettes, add more distinct special behaviors, then consider migrating the renderer into SpriteRenderer/UI prefabs. Keep the pure combat model while extending tests for new rules. Controller support and standalone builds need their own implementation and validation.
