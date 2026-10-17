# Validation — 17 October 2026

## Current HUD and three-round match update

Life labels are independent of the filled portion and centered inside 42-pixel life bars; names and round wins have separate rows. Win rate has been removed from runtime logic and UI. Eight head-only portraits cover calm, hurt, happy and crying states. Sixteen full-body result frames cover victory and sobbing for both characters. Hits select the hurt portrait, and result expressions persist until the next round/match.

Matches play exactly three counted rounds, including draws. A 2–0 score still proceeds to round three. Final victory/crying reactions use the total score, even when the match winner lost the final round. Equal scores produce a match draw. After a five-second result, scores and reactions clear and round one starts without input. Fallen players return to a level stage for the result animation, keeping the crying sprite visible. The existing original fail/win/draw cues accompany results. Manual restart after a counted result advances rather than counting the round twice.

The updated scene is validated in a temporary project copy because desktop automation could not access the already-open Unity window. No app/APK is generated.

**32/32 PlayMode tests passed on Unity 6000.3.24f1.** Evidence is copied to `Logs/HUD-PlayMode-results.xml` (ignored by Git). The suite checks all three rounds at a 2–0 score, a 2–1 total-score winner who lost the last round, automatic score/portrait reset, three-draw matches, manual restart without duplicate counting, hurt portrait switching, 0%-life crying with visible percentage text, and result audio. Existing combat, movement, gamepad, audio and cloud tests remain passing.

The 1920 × 1080 layout, attack, hurt-portrait and result previews were rendered in Unity. The result labels sit below the players to keep the central result banner clear. Updated previews are in `Docs/gameplay_layout.png`, `Docs/action_effects.png`, `Docs/hurt_portrait.png` and `Docs/round_result.png`.

## Previous update: combat and startup

17 October update: the rebuilt scene and new combat sheet pass **30/30 PlayMode tests** on Windows Unity **6000.3.24f1**, recorded in `Logs/Update-PlayMode-results.xml`. Four new tests cover automatic advancement with score/health restoration, first-to-three match completion/new-match reset, draws/win-rate calculation/countdown cancellation, and animation frames synchronized with one-hit punch damage and life-bar updates. Existing combat, movement, gamepad, audio, cloud and effects checks also pass. The legacy hand-box assertions now check full-body sprites and hidden hand renderers instead.

The 24-frame generated sheet supplies new idle, punch and push sequences. The importer slices actual alpha gutters and anchors each pose to its boots. Round results freeze player/plank physics; the 3.2-second intermission advances automatically until a match winner is declared. The health display uses 100%, 67%, 33% and 0%; win rate includes draws in completed rounds. The startup helper only replaces an untouched empty scene and skips batch mode.

Fresh-start verification: with Unity closed, the local last-opened-scene list was set to empty. Launching the project normally (without a scene path or execute method) automatically opened Main and selected the 16:9 Game view. Both new character sprites, separate spawn positions, initial percentages, scorecards and scenery were visually confirmed before pressing Play. This reproduces an empty editor startup on this Windows machine; a second physical device was not used.

Final UI check: a solid one-pixel fill replaces the old hand texture in the life bars. The full 1920 × 1080 layout was recaptured in the Unity Editor and visually inspected. All four MatchFlow tests passed again against the final rebuilt scene (`Logs/Final-MatchFlow-results.xml`). The combat preview stages players outside contact range so neither character's punch/push animation is immediately replaced by hurt. No standalone app or APK was built.

Verified editors: **Unity 6000.3.26f1** (previous validation) and **6000.3.24f1 on Windows** (17 October 2026). The exact recorded reference patch now compiles successfully and passes all 26 PlayMode tests. Local evidence is in `Logs/PlayMode-results.xml` and `Logs/PlayMode-validation.log` (ignored by Git).

The Windows checkout initially had `sceneSetups: []` in `Library/LastSceneManagerSetup.txt`. The saved `Assets/Scenes/Main.unity` contains 72 GameObjects; all 40 distinct external asset GUID references resolve against project/package metadata, and no asset files are Git LFS pointer placeholders. An empty startup scene therefore does not indicate missing game content. Open Main from the Project window; see the README troubleshooting instructions.

- C# compilation and scene generation succeeded.
- **26 PlayMode tests passed, 0 failed**:
  1. Punch applies damage only once, respects cooldown and stuns the target.
  2. Push causes forward knockback without reducing health.
  3. Punch rejects targets behind the player or beyond reach.
  4. Restart resets health, position, facing, action state and velocity.
  5. Falling ends a round and disables attacks.
  6. A win updates the scorecard; restart preserves match wins and resets the round timer.
  7. Keyboard bindings trigger independent boy/girl actions and visible-sized effects.
  8. Both keyboard jump keys lift grounded players.
  9. Two simulated gamepads independently control attacks; disconnecting one preserves the other slot.
  10. Gamepad movement and jump input reach the physics motor.
  11. Two music loops and thirteen cues are wired; switching, mute, emote and action cue selection work.
  12. Gamepad emote and restart buttons work.
  13. Background music actively plays and loops; both happy/sad emote jingles and track switching work.
  14. Punch-hit and restart cues play; falling selects fail followed by win.
  15. Help and audio notices expire while player status and timer remain active.
  16. Both players produce footsteps, jump and landing cues through keyboard input and physics.
  17. Restart cancels a pending result jingle; simultaneous losses play fail followed by draw.
  18. Either sole winner celebrates briefly with the victory cue; the popup expires, draws omit it and restart clears it.
  19. Movement accelerates gradually, reverses without speed overshoot, stops and restarts without stored drift.
  20. The interpolated plank swings continuously within its limits and settles after an impulse; uneven player weight still tilts it.
  21. Cloud displacement follows elapsed time independently of step size and preserves height.
  22. Clouds wrap completely outside the viewport, preserving overshoot even across multiple cycles.
  23. Five cloud sprites move slowly during play and continue after the round; their render layers remain behind gameplay.
  24. Accepted attacks and contact reactions emit distinct effects; cooldown-rejected attacks add no extra particles.
  25. Happy/sad, victory/fail and spawn effects are selected correctly, expire, reset cleanly and reuse a bounded pool under 100 repeated bursts; disabling effects prevents further emissions.
  26. Both characters emit footstep, jump and landing effects through actual keyboard input and physics.
- All fifteen synthesized audio files were checked as nonempty 24 kHz, mono, 16-bit PCM WAVs.
- A graphical Unity Editor render was inspected at 1920 × 1080: both characters are visible; the rope-bound plank and saddle meet the circular wooden fulcrum; farming-village scenery, parchment scorecards, timer, Help and Restart signs render together after the introductory popups disappear.
- Native importer Sprite subassets replaced the earlier serialized generated character sprites after the graphical review revealed missing character rendering.
- README embeds only the full game layout, and its local links and Markdown fences were checked.
- Original CharacterController2DP2.bin remains byte-identical to the uploaded original. The active controller has authorized Unity 6 API updates plus a read-only grounding property for jump audio.

## Build policy and limits

The user explicitly put app and APK creation on hold. The in-progress macOS build was cancelled; no completed app/APK or release was produced or published. Verification continues inside the Unity Editor only.

Headless input tests temporarily adjust and then restore Input System focus settings. Physical USB/Bluetooth controller compatibility, subjective audio mixing and long play sessions still need device testing. Touch input, dedicated punch/push sprite-sheet poses, and packaged platform builds remain future work.

## Reproduce in Unity

1. Open Assets/Scenes/Main.unity using Unity 6000.3.26f1 and select a 16:9 Game view.
2. Press Play and focus the Game view; use the keyboard or connected gamepads.
3. Run Window → General → Test Runner → PlayMode → Run All.
4. Capture Full Layout updates Docs/gameplay_layout.png inside the Editor without building an app.

## Previous matched-wood check

The trunk was refined to match the plank’s honey/chestnut wood, rope bindings and dark outlines, retaining the farming-village layout. The regenerated 1920 × 1080 full-scene image was visually inspected. All twelve WAV files have nonzero sample peaks and RMS energy; no silent or empty audio files were found. The 14-test suite passed on Unity 6000.3.26f1 after the artwork update.

## Current wooden-ball fulcrum

The upright trunk and roots have been replaced by a circular wooden-log support. The background was renamed to farm_village_wood_ball.png with its Unity asset GUID preserved. Uniform overscan aligns the round support with the existing plank and preserves its shape. The refreshed full-scene image was captured in the Unity Editor and visually inspected. Audio sources and gameplay controls are retained; all twelve WAVs were rechecked as nonempty and non-silent.

## Smaller ball and device-label removal

The ball artwork was reduced about 25%, with the background offset adjusted to retain the plank contact point. The keyboard/gamepad device-status text object, scene reference and runtime assignment were removed. Unity 6000.3.26f1 compiled and captured the updated full scene successfully; the image was visually inspected and local scene references checked. No new tests were added for this visual change; the 14-test results above are from the previous gameplay/audio verification.

## Compact HUD and action audio check

The earlier 17-test suite passed on Unity 6000.3.26f1 after the popup and audio changes. All 15 WAV files were checked for nonzero samples and RMS energy. The full-layout capture waits six seconds so the startup help and notice have expired. Player names, health, wins, timer and corner buttons remain; control instructions and audio notices share timed panel visibility with their text. End-of-round results remain visible until restart. The original user-provided .bin is unchanged. No app/APK was built.

## Victory, character icons and smooth motion

All 20 PlayMode tests passed on Unity 6000.3.26f1 after these changes. Winner celebrations were checked for both players, including expiry, draw behavior, victory audio and immediate restart cleanup. The movement test exercises acceleration, reversal, stopping and restart drift. The plank test measures per-step angle changes, hinge bounds and angular settling after a repeatable impulse, then confirms it still responds to uneven player weight. Physics interpolation is enabled on both characters and the plank.

The updated full-layout screenshot includes matching boy/girl icons in the two farm scorecards. Frame-rate perception and controller feel should still be checked on the target device; automated physics checks do not measure display performance. Original CharacterController2DP2.bin is unchanged. No app or APK was created.

## Grass and moss on the round fulcrum

The existing background was edited with built-in imagegen to add a grass/moss fringe and ground tufts around the wooden circle. The 1672 × 941 canvas, asset GUID and scene placement are retained. Unity 6000.3.26f1 imported the image and captured the refreshed 1920 × 1080 layout successfully; the image was visually inspected for round support proportions, exposed plank contact, player icons and clear HUD. This is an artwork-only update; the 20-test gameplay result above is from the preceding gameplay verification and was not rerun. No app/APK was built.

## Slow cloud animation

All 23 PlayMode tests passed on Unity 6000.3.26f1 after adding CloudDrift and the cloud scene objects. The cloudless village base retains the 1672 × 941 canvas and the grass-covered fulcrum. The 1774 × 887 cloud PNG has an alpha channel and uses point-filtered, uncompressed sprite import. Five sprites render at varied scales with speeds 0.06–0.108 world units/second. The updated full-layout image was captured in Unity for visual inspection. No packaged app or APK was created.

## Action effects

The full 26-test PlayMode suite passed on Unity 6000.3.26f1. After adding a guard against emitting from a disabled ActionEffects component, all three ActionEffects tests were rerun and passed. Each player uses a fixed pool of 64 world-space SpriteRenderers with the existing hand Sprite and unlit material. Effects fade within 0.18–0.9 seconds, throttle alongside footsteps/shove reactions, and clear on restart or disable. Repeated burst tests confirm renderer counts do not grow. Input, damage, audio and cloud checks remain passing.

Capture Action Effects saved a full 1920 × 1080 Unity scene preview at Docs/action_effects.png after staging a punch/push exchange. The capture was visually inspected: mint streaks and gold impact sparks render around the characters, the target shows hit feedback, and the HUD remains clear. The original .bin is retained, and no app/APK was created.

## Unity Hub launch log check

Reviewed the supplied Unity Hub logs directory. The recent LocalProjectService/ProjectService error is a macOS launch failure: Code=-600, procNotFound, while Hub invokes the Unity executable through `open -a` with an activation URI. Direct invocation of the installed 6000.3.26f1 executable has already completed the project’s batchmode tests and graphical captures successfully. README now includes the direct executable command for an Editor playtest; interactive launch through that workaround still needs to be tried on the user’s device.
