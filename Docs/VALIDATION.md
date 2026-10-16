# Validation — 16 October 2026

## Current playtest state

Verified editor: **Unity 6000.3.26f1**. Recorded reference patch: **6000.3.24f1**; that exact patch has not been tested here. The project is ready for a Unity Editor playtest using the verified editor.

- C# compilation and scene generation succeeded.
- **12 PlayMode tests passed, 0 failed**:
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
  11. Two music loops and ten cues are wired; switching, mute, emote and action cue selection work.
  12. Gamepad emote and restart buttons work.
- All twelve synthesized audio files were checked as nonempty 24 kHz, mono, 16-bit PCM WAVs.
- A graphical Unity Editor render was inspected at 1920 × 1080: both characters are visible; the rope-bound plank and saddle meet the rooted tree; farming-village scenery, parchment scorecards, timer, controls and restart sign render together.
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
