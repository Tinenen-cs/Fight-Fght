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
- [x] Replace stage music with distinct wizarding scores and all twelve sound effects with magical cues.
- [x] Central input manager with keyboard, Xbox and PS4 bindings; independent controller slots, menu navigation, hot-plug and adaptive prompts.
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
- [x] Five-hit temporary knockdown, recovery, guard immunity to hit counting, ring-out-only elimination.
- [x] Native saved Main scene preview before Play.

- [x] Save eight independently generated head-reaction sheets (32 frames) and wire them to live HUD and roster.

## Fight HUD and controllers

- [x] Mirror portrait/name HUD with five poise segments, arcana meter, round-win markers and central timer.
- [x] Set knockdown to five unguarded hits; refill poise on recovery and keep ring-out-only elimination.
- [x] Visible circular lower-side controls, spell-cost previews and casting effects.
- [x] Add editable two-player Input System action maps.
- [x] Verify virtual controller and keyboard isolation, navigation, deadzone, press edges, disconnect/replacement, Xbox and PlayStation prompts.
- [ ] Physical Xbox and DualShock 4 tests on this Mac over USB/Bluetooth.

## Five-bar and portal clearance update

- [x] Five poise bars; five unguarded hits trigger temporary knockdown.
- [x] Flat afternoon library floor and common fighting baseline for all stages.
- [x] Dedicated illustrated diamond ability icons, one four-icon generation per character.
- [x] Diamond control clusters placed below the complete portal bounds.
- [x] Add layout checks for every ability button against both portal rectangles.

- [x] Remove circular frames from head portraits.
- [x] Simplify the fight view to icons and compact binding badges; move extra information into help.
- [x] Check audio waveform identity, stereo format, peak headroom and loop boundaries.
- [ ] Listen on the Mac and tune the new scores/SFX balance with two players.

## Start, readability and timed portal growth

- [x] Fix start detection for individual keyboard edges, mouse input and direct Game-view key/mouse events.
- [x] Add actual game-update start regression checks and shared-backend keyboard fallback.
- [x] Enlarge diamond guides to 72px and keep them clear of maximum-size portals.
- [x] Replace active pixel fonts with Cinzel / Cinzel Decorative / EB Garamond.
- [x] Rebuild 256px fighter frames, 1280×720 stages and higher-resolution VFX/portraits/icons; preserve full source color and enable bilinear filtering.
- [x] Grow portal art and collision together at half-time and final 15 seconds; verify both sides, jumping and timeout draws.
- [x] Pass 73 combat and 22 input/start assertions.

- [x] Verify the complete keyboard and two-controller selection/stage/combat flow through the actual runtime update.
- [x] Fix Escape pause/cancel overlap and verify pause/resume retains the duel.
- [x] Pass 36 input/start/flow assertions and 73 combat assertions.
