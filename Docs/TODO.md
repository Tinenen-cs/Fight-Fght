# Development checklist

Updated: 23 October 2026. Checked boxes mean implemented or specifically verified; they do not imply every target platform was tested.

## Foundation

- [x] Create clean Unity folder structure in the requested Fight-Fght repository.
- [x] Add scene, project settings, minimal built-in packages and Unity cache exclusions.
- [x] Add VS Code recommendations.
- [x] Follow Balance-with-me README organization without copying its game.

## Art and audio

- [x] Design four magical women and four magical men with distinct palettes and weapons.
- [x] Generate eight sprite sheets, each with idle/walk/punch/hurt/knockout/combo/spell/power/push/jump rows.
- [x] Generate eight avatars and eight animated effect sheets.
- [x] Generate detailed academy gates, grand library and observatory backgrounds.
- [x] Add four original music loops and twelve sound effects, including menu selection.
- [x] Add a pixel font with its license.
- [x] Include reproducible art/audio source generator and asset instructions.

## Playable game

- [x] Intro with game title.
- [x] Two-player card selection with independent selection and lock-in.
- [x] Mirrored player/arcana HUD without health bars, central timer and round scores.
- [x] Local shared-keyboard controls for both players.
- [x] Movement, jump, guard, melee, three-hit chain, spells and power attacks.
- [x] Per-fighter spell stats and animated effects.
- [x] Two forbidden rune rifts at the left/right edges of every arena; center stays clear.
- [x] Three stages with 99-second timers, result screens and overall winner/draw.
- [x] Pause, focus-loss pause, help, mute and restart through selection.

## Validation and delivery

- [x] Run 63 pure C# combat assertions for ring-out-only combat.
- [x] Unity 6000.2.2f1 compilation and asset-import validation on this Mac.
- [x] Finish visual runtime smoke test and record result (Unity 6000.2.2f1; full Game view previews included).
- [x] Commit initial game locally (`f1e0543`).
- [x] Authenticate GitHub CLI and publish the project to the exact requested origin/main.
- [x] Verify original prototype on Unity 6000.2.2f1; preserve the current local Editor upgrade to 6000.3.27f1.
- [ ] Two-person keyboard test: movement, simultaneous input, combo timing, guards and jumps.
- [ ] Auditory check of music loops and every sound cue through Mac output.
- [ ] Play through all three stages using several fighter matchups; tune balance.

## Academy art upgrade

- [x] Finish all 80 separately generated four-frame actions, assemble athletic fighter sheets, and derive matching avatars.
- [x] Add unique magical academy names for all eight fighters.
- [x] Generate actions concurrently, with one four-frame action per request.
- [x] Finish saving all action sources and prompt records.
- [x] Replace all three plain backgrounds with detailed, visibly pixelated academy scenery.
- [x] Add matching ornate wood/brass interface skins and elemental magic sheets.
- [x] Integrate the animated rune-rift artwork and verify final Unity screens.
## Future polish

- [ ] Further hand-polish generated frame alignment and special behaviors after playtesting.
- [ ] More distinctive stage music and richer combat audio.
- [ ] Gamepad support if requested.
- [ ] SpriteRenderer/Animator/UI prefab migration if needed for classroom workflow.
- [ ] Standalone desktop build and platform-specific validation if requested.

## Your first playtest

Open `Assets/Scenes/Main.unity`, press Play, any key, then F and J to lock the default fighters. Choose the opening stage and press Enter. Use A/D/W/S + F/G/R/T for P1 and arrows + J/K/U/I for P2. Press H for help, Escape to pause and M for mute. After each result, Enter advances. Record bugs and preferences below.

| Date | What you tried | What happened | Next action |
| --- | --- | --- | --- |
| | | | |

## Selection and rules update

- [x] Text-only wizard title with any-key/click start.
- [x] Reference-inspired opposing fighter previews, side biographies, compact center roster and independent locks.
- [x] Opening-stage selection with morning, afternoon and night previews.
- [x] Four-hit temporary knockdown, recovery, guard immunity to hit counting, ring-out-only elimination.
- [x] Native saved Main scene preview before Play.

- [x] Save eight independently generated head-reaction sheets (32 frames) and wire them to live HUD and roster.
