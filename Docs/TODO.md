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
- [x] Generate forest, academy library and observatory backgrounds.
- [x] Add four original music loops and twelve sound effects, including menu selection.
- [x] Add a pixel font with its license.
- [x] Include reproducible art/audio source generator and asset instructions.

## Playable game

- [x] Intro with game title.
- [x] Two-player card selection with independent selection and lock-in.
- [x] Mirrored health/arcana HUD, central timer and round scores.
- [x] Local shared-keyboard controls for both players.
- [x] Movement, jump, guard, melee, three-hit chain, spells and power attacks.
- [x] Per-fighter spell stats and animated effects.
- [x] Telegraphing manhole, shove force and ring-out win.
- [x] Three stages with 99-second timers, result screens and overall winner/draw.
- [x] Pause, focus-loss pause, help, mute and restart through selection.

## Validation and delivery

- [x] Run 27 pure C# combat assertions.
- [x] Unity 6000.2.2f1 compilation and asset-import validation on this Mac.
- [x] Finish visual runtime smoke test and record result (Unity 6000.2.2f1; full Game view previews included).
- [x] Commit initial game locally (`f1e0543`).
- [ ] Upload to origin/main — blocked: this Mac has no GitHub HTTPS credentials. `git push -u origin main` failed with `could not read Username`; sign in to GitHub, then retry.
- [x] Keep this Mac on verified Unity 6000.2.2f1 and record it as the project version.
- [ ] Two-person keyboard test: movement, simultaneous input, combo timing, guards and jumps.
- [ ] Auditory check of music loops and every sound cue through Mac output.
- [ ] Play through all three stages using several fighter matchups; tune balance.

## Future polish

- [ ] Hand-polish sprites and add unique body poses/special behaviors for each fighter.
- [ ] More distinctive stage music and richer combat audio.
- [ ] Gamepad support if requested.
- [ ] SpriteRenderer/Animator/UI prefab migration if needed for classroom workflow.
- [ ] Standalone desktop build and platform-specific validation if requested.

## Your first playtest

Open `Assets/Scenes/Main.unity`, press Play, Enter, then F and J to lock the default fighters. Use A/D/W/S + F/G/R/T for P1 and arrows + J/K/U/I for P2. Press H for help, Escape to pause and M for mute. After each result, Enter advances. Record bugs and preferences below.

| Date | What you tried | What happened | Next action |
| --- | --- | --- | --- |
| | | | |
