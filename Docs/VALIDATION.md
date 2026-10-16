# Validation — 16 October 2026

## Current playtest state

Verified editor: **Unity 6000.3.26f1**. Recorded reference patch: **6000.3.24f1**; that exact patch has not been tested here. The project is ready for a Unity Editor playtest using the verified editor.

- C# compilation and scene generation succeeded.
- **20 PlayMode tests passed, 0 failed**:
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
- All fifteen synthesized audio files were checked as nonempty 24 kHz, mono, 16-bit PCM WAVs.
- A graphical Unity Editor render was inspected at 1920 × 1080: both characters are visible; the rope-bound plank and saddle meet the circular wooden fulcrum; farming-village scenery, parchment scorecards, timer, Help and Restart signs render together after the introductory popups disappear.
- Native importer Sprite subassets replaced the earlier serialized generated character sprites after the graphical review revealed missing character rendering.
- README embeds only the full game layout, and its local links and Markdown fences were checked.
- Original CharacterController2DP2.bin remains byte-identical to the uploaded original. The active controller has authorized Unity 6 API updates plus a read-only grounding property for jump audio.

## Build policy and limits

The user explicitly put app and APK creation on hold. The in-progress macOS build was cancelled; no completed app/APK or release was produced or published. Verification continues inside the Unity Editor only.

Headless input tests temporarily adjust and then restore Input System focus settings. Physical USB/Bluetooth controller compatibility, subjective audio mixing, long play sessions and exact-reference-patch behavior still need device testing. Touch input, dedicated punch/push sprite-sheet poses, and packaged platform builds remain future work.

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
