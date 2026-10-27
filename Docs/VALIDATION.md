# Validation — 17 October 2026

## Current reaction, village-motion and winner-announcement update

The ui-ux-pro-max layout guidance was applied to avoid overlapping fixed elements and keep labels legible. Character reaction labels now follow the top of the actual sprite bounds in both overlay and camera canvases. The round notice moves above the reaction area during results, temporarily hiding the clock. HUD portraits have no colored backing image. A named happy-head winner panel eases in after the third round and clears automatically on reset; draws do not announce a winner.

The village base has baked smoke and windmill sails removed. A separate rotor turns at 12 degrees per second and each chimney uses a fixed six-puff pool that rises, drifts and fades. These components continue independently of round state. Original scenery is retained as an art reference. No app or APK is built.

**35/35 PlayMode tests passed on Unity 6000.3.24f1**, recorded in `Logs/Final-Village-results.xml` (ignored by Git). New presentation tests check head-only portrait containers, reaction-label placement above the entire sprite in overlay and camera canvases, clearance below the result notice, UI sorting above world sprites, fixed smoke pool size and motion during results, and correct named-head announcements for either winner followed by automatic clearing/reset. The existing 32 gameplay checks remain passing.

The first pass caught a taller-aspect label/notice overlap; the notice now uses the clock area during results with enough clearance. The Canvas sorts at 100 so the plank cannot draw over the winner name. Five 1920 × 1080 previews were captured and inspected, including `Docs/winner_announcement.png`. The same cached temporary Unity project was used for validation, then the rebuilt scene, new asset metadata and preview images were copied back into the repository.

## Previous HUD and three-round match update

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


## Ready countdown, timed rounds and Play Again - 17 October 2026

Verified in Unity 6000.3.24f1 using an isolated copy of the project so the open source Editor session was not interrupted. The final PlayMode run passed **41/41 tests** (0 failures). The local report is Logs/round-clock-final-results.xml.

Coverage includes the 1 READY / 2 SET / 3 START countdown, frozen physics and rejected attacks during preparation, a full 180 seconds remaining after preparation, higher-life timeout wins for either player, equal-life draws, prevention of duplicate round scoring, the 30-second cue and ten final-second ticks, warning reset, and automatic advancement to the next round's preparation. Fall results stay on grass until the next round; zero-life and timeout results stay on the plank. Three-round match scoring, both named winner announcements, and match draws remain covered. The replay test waits beyond the former five-second automatic reset, confirms the actual button receives UI raycasts, sends a pointer click, and verifies cleared scores and a fresh Ready countdown.

Rebuilt Main with all new UI and audio references, then captured seven 1920 x 1080 previews. Inspected the ready popup, grass-level fall result, warning notice and winner panel; corrected final character spacing so the panel does not cover the winner. Updated images are in Docs, including round_start.png and time_warning.png. All three new WAV assets have nonzero sample peaks; tests verify imported clips and cue emission. Speaker output was not manually auditioned. Unity's background SearchDatabase indexing emitted an Editor startup exception during capture; scene rebuilding, all seven captures and the complete gameplay suite still completed successfully. No app/APK was created.


## Centered result popup, Enter replay and PlayStation mappings - 17 October 2026

Unity 6000.3.24f1 passed **45/45 PlayMode tests**, with zero failures. The local report is Logs/centered-results-tests.xml. New checks cover both round winners' centered head/name/next-round popup, hiding the old top banner, automatic handoff into Ready/Set/Start, absence of countdown background Images, outlined/shadowed text, keyboard Enter and numpad Enter replay, and two simulated DualShockGamepad devices. PlayStation checks exercise stick movement, Cross jump, Square punch, Circle push, Triangle emote and Cross replay for both assigned players; existing generic controller tests retain restart, audio, routing and disconnect coverage. Physical PS4 hardware was not available for manual input validation.

Rebuilt the saved Main scene and regenerated all seven 1920 x 1080 preview images. Visually inspected round_result.png, winner_announcement.png and round_start.png: round results show a centered winner portrait and next-round countdown, match results show the Play Again button and input hints, and the background-free countdown uses cream text with a dark outline/shadow. Character result poses remain visible beside the central card. The known Editor SearchDatabase startup indexing exception also occurred during capture; all captures and gameplay tests completed. No app/APK was created.


## One-minute rounds, transparent round results and downhill slide - 18 October 2026

Unity 6000.3.24f1 passed **48/48 PlayMode tests** (zero failures). New physics checks verify that lower-side slide speed builds gradually, the upper side is unaffected by the added drift, tilt reversal transfers the pressure to the new lower side, uphill keyboard input overcomes the slide, jumping clears it, and an unattended player near the lower edge eventually falls and awards the opponent a round. Restart clears drift. Clock tests now verify 60 seconds after preparation/reset and retain the 30-second warning plus all ten final-second cues. The original CharacterController2DP2.bin was retained; the active .cs motor accepts a separate grounded drift velocity.

After reducing the small-text outline and correctly toggling both Outline and Shadow components for round/match results, all **5 presentation tests passed** again. Rebuilt Main and regenerated all seven 1920 x 1080 previews. Visually inspected round_result.png and winner_announcement.png after the refinement: round results have no backing box, readable outlined text and a visible head, while the final match retains its working parchment/Play Again card. Gameplay preview shows the one-minute clock counting down. Local reports: Logs/one-minute-slide-tests.xml and Logs/one-minute-slide-ui-tests.xml. The known Unity SearchDatabase startup indexing exception appeared during capture; all captures completed. No app/APK was created.


## Above-plank countdown/results and earned star HUD - 18 October 2026

Unity 6000.3.24f1 passed **50/50 PlayMode tests** with zero failures. The local report is Logs/stars-raised-hud-tests.xml. Added checks verify that countdown number/caption and the per-round winner head/name/next-round indicator clear the plank in both overlay and camera canvas modes. Star checks verify three initially muted slots per player, one gold star per win, no star for a draw, score retention across rounds, three gold stars after a sweep for either player, and clearing on Play Again. Existing timer, controller, combat, slide and match-flow tests continue to pass.

Created round_win_star.png with built-in imagegen and copied it into Assets/Art/UI with its Unity .meta. Confirmed 1254 x 1254 RGBA data with transparent corners and alpha ranging from 0 to 255. Scene import uses point filtering and uncompressed Sprite mode. Rebuilt Main and regenerated all seven 1920 x 1080 previews. Visually inspected round_start.png, round_result.png and winner_announcement.png: number/caption and all per-round result text now sit above the plank; scorecards show muted slots, a single earned star after one round, and three earned stars after a sweep. The known Unity Editor SearchDatabase indexing exception appeared during capture; all captures completed successfully. No app/APK was created.


## Windows portable submission audit - 27 October 2026

The final regression suite passed **56/56 PlayMode tests**, with zero failures (Logs/submission-56-tests.xml). This includes the earlier 50 gameplay/presentation checks plus menu pause/resume, prepared-countdown pause, new-match score clearing, scene font/audio/animation integrity, controller hotplug, raw USB joystick mapping, and a regression preventing a gamepad jump from submitting a previously selected Restart HUD button. The first new-controller test attempt exposed test-fixture focus handling and a bitfield event-construction error; those fixtures were corrected before the final all-pass run.

File-integrity audit: **15 PNG images and 18 WAV files verified**, with no corrupt files. Reduced per-physics-frame ground-query allocations by reusing an eight-collider buffer; cached the plank collider and changed HUD/result text/style refresh to update on value changes. Added a startup/pause menu with controls, connected-player assignments, Resume, New Match, sound, display and Quit. Keyboard, standard Gamepad/Xbox-compatible and PlayStation controls remain; raw Joysticks have five-button mapping (Jump/Punch/Push/Emote/Menu) saved by layout. Added fullscreen startup and a landscape 16:9 viewport. Losing focus pauses normal gameplay.

The final **Windows x64 Mono release build succeeded with 0 errors and 0 warnings**, Unity 6000.3.24f1, Direct3D 11, default 1920 x 1080 native fullscreen. Build summary: Logs/submission-build-summary.txt. A complete-build ZIP and an installer-free, single-file portable launcher were produced. ZIP integrity/CRC checks passed, all required Unity/Mono/data files were present, and no PDB or DoNotShip files were shipped. Portable extraction cleanup was verified after exit. The launcher uses the Windows .NET Framework and temporary storage; the standard ZIP is available as a native Unity fallback.

Actual packaged-player automated combat sample: **4394 frames over 75 seconds after a five-second warmup**, **58.6 average FPS**, p95 16.70 ms, p99 33.37 ms, max 33.49 ms, **0 frames above 50 ms**, and approximately 107.1 MB Unity-allocated memory. The sample included 1997 active-gameplay frames, **8 resolved rounds and 2 completed matches**, with virtual gamepads driving real combat/physics. **0 runtime errors and 0 missing sprite/material references** were reported. Screen state was **1920 x 1080, fullscreen=true, landscape=true**. The portable process exited normally with code 0. Hardware: AMD Ryzen 7 5700G with Radeon Graphics, AMD Radeon(TM) Graphics, Windows 11. Report: Logs/submission-autoplay-performance.json. An earlier 90-second menu/background rendering run averaged 58.5 FPS with 0 errors. These are measurements on this PC, not guarantees for another computer.

Limitations: no physical controllers were connected during the recorded checks; controller compatibility and speaker output still need the user's final playtest. Computer Use ended during read-only Canvas assignment inspection because the current browser URL could not be verified; no further Windows/browser UI automation or Canvas submission was attempted. The packaged gameplay run uses the game's own opt-in diagnostic code and Unity input events, independent of desktop/browser automation. The assignment's extension allowlist remains unverified. The supplied tutorial was identified by YouTube metadata as ?How to Build a Standalone Executable with Unity?; video playback/transcript contents could not be retrieved. Packaging follows the written requirements and Unity's complete Windows build structure.

All eight Unity-rendered previews were regenerated, and the controls menu was visually inspected in Docs/submission_controls.png (its windowed label reflects Editor capture; release fullscreen state is verified above). The player logs contain one URP ComputeBuffer disposal warning during shutdown, with no gameplay exception; the Editor retains its previously known SearchDatabase indexing startup exception during capture. Neither blocked the build, tests or player run. Original user changes in ProjectSettings/ProjectSettings.asset and URPProjectSettings.asset were preserved, and build-only player settings were configured in the isolated build project.

Final local artifacts:
- Submission/BalanceWithMe-Portable-Windows-x64.exe ? 49,680,384 bytes; SHA256 b615e27590a615cc492f63b75a4dcd3dcc4210a55db656a6491ad0e560b47d7e.
- Submission/BalanceWithMe-Windows-x64.zip ? 49,673,095 bytes; SHA256 05df15189a0eba589084a57c7511be01fc3c0deaf45465f04cdde376643ff087.
- Submission/READ-ME-FIRST.txt, PLAYTEST-CHECKLIST.txt, PROFESSOR-COMMENT.txt and SHA256SUMS.txt.

**No Canvas upload or submission was performed.** The user chose to test the final file afterward.

## Readable typography and menu - 28 October 2026

Applied the requested ui-ux-pro-max guidance to the existing Unity menu: Atkinson Hyperlegible Regular/Bold for instructions, buttons, controls and gameplay details; Tiny5 only for selected game/menu titles, countdown and winner headings. Fonts and SIL OFL license are bundled locally. Keyboard controls have aligned action/Boy/Girl columns; controller actions have separate rows. Short instructions, separate connection/USB setup strip, larger action buttons and sentence-case captions replace the crowded all-pixel block.

**58/58 PlayMode tests passed**, zero failures (Logs/readable-menu-tests.xml). New checks verify font roles, menu text fit without clipping/wrapping and button dimensions. All eight 1920 x 1080 previews were regenerated. Visually inspected the controls menu, gameplay and round-result text. Capture now regenerates text at the render target's scale instead of retaining the low-resolution batch Game View font atlas. The existing Editor SearchDatabase indexing exception occurred during capture; previews completed.

The updated Windows x64 release succeeded with **0 errors and 0 warnings**, 108,517,316 build bytes. The newly packaged portable EXE launched, completed its automated combat run and exited with code 0; its temporary game folder was cleaned up. ZIP CRC passed across 162 files; Unity engine/data/Mono payload present and debug files excluded. New actual portable-player sample: 1769 frames over 30 seconds after warmup, **58.9 average FPS**, p95 16.68 ms, p99 33.37 ms, max 50.05 ms (one frame above 50 ms), 107.5 MB allocated memory. Three rounds and one match completed. **0 runtime errors and 0 missing sprite/material references**; fullscreen landscape 1920 x 1080. Logs/readable-menu-player-performance.json records the measurement. Physical controller and speaker-output limits from the earlier audit still apply; user confirmed physical Esc closes the menu. The known URP buffer-disposal warning remains at shutdown.

Current local submission files (replace the earlier artifacts listed above):
- Submission/BalanceWithMe-Portable-Windows-x64.exe: 49,752,064 bytes; SHA256 CE097B4A082B4E60C9AED85B02F4D31B9F49AE4FFDEA245458E16EC94D77507C.
- Submission/BalanceWithMe-Windows-x64.zip: 49,744,681 bytes; SHA256 E5369179F1F4E332BB2C7ECCD0D3795ED1A17CC43142B6EF042F7F39BFB8252D.

Previous working binaries are retained in Builds/Previous-Submission-20261027. User ProjectSettings changes remain untouched. No Canvas upload was performed; final user playtest remains pending.

## Replay interaction, mouse close and timer fit - 28 October 2026

**61/61 PlayMode tests passed** with zero failures (Logs/ui-polish-tests.xml). Added real Unity pointer-event coverage for the visible replay face changing on hover/press and clicking it to start a new match; menu X resuming without resetting the round; and round-clock text fitting with padding at all three round numbers and 01:00/00:30/00:10/00:00. Theme-font role checks now include character name headings. These checks supplement the existing keyboard/controller and gameplay regression suite.

Applied ui-ux-pro-max active/hover feedback guidance to the existing Unity buttons. Farm-sign buttons tint the visible face with separate hover/selection and pressed colors and a 120 ms fade; their layout bounds remain stable. Added a 60 x 60 X at the menu's upper-right. Character names use Tiny5's chunky display lettering; instructions/buttons remain Atkinson Hyperlegible. The 250 x 96 clock frame uses 26px maximum text, 1.1 line spacing and a padded 222 x 76 text region with best-fit limits and truncation safeguards. Nine Unity-rendered previews were regenerated; menu, timer and replay-hover previews were visually inspected.

Windows x64 release build succeeded with **0 errors and 0 warnings**, 108,517,597 build bytes. Complete-build ZIP CRC passed across 162 files, required Unity/data/Mono payload present and no debug symbols. Native release-player automated combat sample: **1763 frames over 30 seconds after warmup**, 58.7 average FPS, p95 16.70 ms, p99 33.37 ms, max 33.40 ms, 0 sampled frames over 50 ms, 107.5 MB allocated memory. Three rounds and one match resolved; **0 errors and 0 missing sprite/material references**; fullscreen landscape 1920 x 1080; exit code 0 (Logs/ui-polish-player-performance.json). The existing URP disposal warning remains at shutdown.

Computer Use initially cancelled clicks because user input was detected. The user closed the older build, allowing replacement. Launched the updated portable EXE through Computer Use and visually confirmed its fullscreen controls screen, clicked the new X to resume, then clicked Menu to reopen it. Left that updated game paused at its menu for the user's playtest. Replay hover/press/click is covered by Unity pointer-event tests and rendered preview; it was not manually played through a full match. Physical controller and speaker-output checks still require the user.

Current local upload artifacts (supersede earlier versions):
- Submission/BalanceWithMe-Portable-Windows-x64.exe: 49,752,064 bytes; SHA256 146DB4177158AC30B2B8F90B1BF67C7C4F8AC68D99037294B73C6050E6B83B8B.
- Submission/BalanceWithMe-Windows-x64.zip: 49,744,868 bytes; SHA256 936259017742AF704688EEDAEFCCAA1844A4612B47C7734CFF3E551D57AF75C5.

Previous binaries retained in Builds/Previous-Submission-UI-Polish-20261028. ProjectSettings changes preserved. No Canvas upload was performed. Because the game includes an in-game controls menu, the written assignment does not require a controls comment; Submission/PROFESSOR-COMMENT.txt remains an optional ready-to-paste note.
