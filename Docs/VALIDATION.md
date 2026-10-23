# Validation record

Date: **23 October 2026**, macOS. This is a student prototype with generated academy pixel art and synthesized audio.

The project is pinned to **6000.2.2f1**, as selected by the user for this Mac.

## Passed

| Check | Result | Evidence |
| --- | --- | --- |
| Pure C# combat rules | **53 assertions passed** | `Tools/test_rules.sh`, using installed Unity Mono |
| Unity compile and asset import | **Passed** on **6000.2.2f1** | Batch entry point `Hexbound.Editor.ProjectSetup.ValidateBatch` |
| Character assets | Eight sheets, eight avatars and eight effect sheets loaded | Unity batch validation |
| Audio import | All 16 WAV assets loaded | Unity batch validation |
| Runtime title / selection / fight | **Passed** on **6000.2.2f1** | `Hexbound.Editor.PreviewCapture.Start`, rendered Game view screenshots below |
| Rendered UI inspection | Complete title, both selection panels, eight cards, mirrored HUD and arena visible | Screenshots inspected after capture |

The current rule assertions verify that no health pool exists, punches and combos apply knockback, guarding reduces force, all eight fighters can ring out an opponent with both spells and power attacks, melee and shoves can ring out, all three arenas have both edge traps and a safe center, jumps clear the trap, P2 can push left into it, score is awarded once, and timeouts are draws. They also cover arcana costs, cooldown contact, jumping, round reset and the three-round limit.

The first scene-creation attempt failed because Unity does not allow adding a scene beside an unsaved untitled scene. Setup now handles a clean untitled scene and preserves dirty work by asking the user to save it. The subsequent batch validation passed.

An initial screen-desktop capture produced cropped images because of display scaling. The capture tool now reads the actual rendered Game frame; the committed previews show the whole game view. No desktop captures are included.

## Academy artwork update

Each of eight athletic academy fighters has a separate reference image. The replacement actions are generated one four-frame sequence at a time, with multiple requests running concurrently. Ten actions per fighter produce 80 source sequences and 320 runtime frames. The final assembler preserves transparency, uses nearest-neighbor sampling and aligns the lower silhouette to a consistent baseline. Detailed backgrounds remain visibly pixelated at 640 × 360. Source images and full prompts are saved in the repository. Final assembly and Unity 6000.2.2f1 import validation passed. The rendered title, selection, all three arenas with two edge rifts, and a three-ring-out match result passed the graphical smoke check. All 80 sequences (320 nonempty frames) and eight matching portraits were saved.

![Eight academy fighters](Previews/04-roster.png)

## Runtime previews

![Title screen](Previews/01-title.png)

![Character selection](Previews/02-select.png)

![Fight HUD and academy gates](Previews/03-fight.png)

![Grand library with edge rifts](Previews/05-library.png)

![Observatory terrace with edge rifts](Previews/06-observatory.png)

![Three-round ring-out match result](Previews/07-ringout-result.png)

## Still to verify

- Physical two-person simultaneous keyboard input and key rollover.
- Listening to every sound through the Mac's actual output and checking loop seams/volume.
- A complete human playthrough of all three arenas and several fighter pairings; automated rule checks verify outcomes but do not judge combat feel.
- Other operating systems, standalone builds, controllers, and mobile/touch behavior.

The graphical smoke pass opens the title, selection and all three arenas automatically, then forces one ring-out in each arena and checks the three-win match result. It checks runtime rendering, edge-trap outcomes and match flow; it is not a substitute for manually pressing every input or playing a match.

## Run checks again

From the repository root:

```bash
Tools/test_rules.sh
```

If another editor version supplies Mono, set its bin directory:

```bash
UNITY_MONO="/Applications/Unity/Hub/Editor/6000.2.2f1/Unity.app/Contents/MonoBleedingEdge/bin" Tools/test_rules.sh
```

Unity compilation/import/rule validation (use the path to your installed editor):

```bash
"/Applications/Unity/Hub/Editor/6000.2.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "$PWD" \
  -executeMethod Hexbound.Editor.ProjectSetup.ValidateBatch \
  -logFile /tmp/hexbound-validation.log
```

Success writes `HEXBOUND VALIDATION PASSED: rules and imported assets` to the log and exits successfully. This batch command modifies/imports project settings and may update `ProjectVersion.txt` to the editor actually used.

Optional graphical smoke pass, with the project closed in other Unity processes:

```bash
"/Applications/Unity/Hub/Editor/6000.2.2f1/Unity.app/Contents/MacOS/Unity" \
  -projectPath "$PWD" \
  -executeMethod Hexbound.Editor.PreviewCapture.Start \
  -logFile /tmp/hexbound-preview.log
```

This opens Unity, enters Play mode, updates `Docs/Previews`, then **closes that Unity process automatically**. It requires a graphical desktop session. Normal gameplay uses **Assets/Scenes/Main.unity → Play**, with no automated exit.

## Repository delivery

The complete prototype is committed locally. The first upload attempt to
`https://github.com/Tinenen-cs/Fight-Fght.git` failed because Git could not obtain
a GitHub username/credential in this session. No remote update was made.
After signing in to GitHub on this Mac, publish the existing commits with:

```bash
git push -u origin main
```

No force push is needed. Local work and validation remain available without
a GitHub connection.
