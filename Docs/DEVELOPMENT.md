# Development workflow

Game: **Balance with me**. Repository: **Tinenen-cs/Balance-with-me**.

## Current authorization

The user has authorized new and edited scripts to add punching, pushing, and useful gameplay. The earlier single-script restriction is superseded. The original `CharacterController2DP2.bin` remains unchanged; the active controller replaces deprecated `Rigidbody2D.velocity` calls with `linearVelocity`.

## Implementation

- `PlayerActions` reads each player's keyboard bindings and calls the movement controller in FixedUpdate.
- Punch uses a short active window and a per-attack hit guard: one health point and impulse knockback once per attack. Push applies small impulses throughout its active window without damage.
- Both actions require the target ahead and within reach. Cooldowns and hit stun prevent attack spam and immediate movement cancellation of knockback.
- Existing sprites supply idle, walk, jump, and hurt reactions. Orange and cyan pixel hand effects distinguish punch and push; these are simple effects, not new authored combat sprite sheets.
- Players have Rigidbody2Ds and capsule colliders. Ground checks use the Ground layer. A HingeJoint2D limits plank rotation to ±16°; a rooted tree in the farming-village background is the stationary visual support.
- `BalanceGame` manages three health points per player, falling/zero-health wins, status, and reset.
- `PlayableSceneBuilder` rebuilds the scene, creates native Sprite assets from alpha-trimmed source rectangles, and wires component references. Rebuilding resets scene-only Inspector edits.

## Display and versions

Desktop 16:9, 1920 × 1080 UI reference and resizable standalone defaults. Select a 16:9 Game view manually. The original portrait art concept remains a reference. Target editor: 6000.3.24f1. Local checks: installed 6000.3.26f1. Restore ProjectVersion to the target before committing newer-editor validation changes.

Commit and push completed changes; keep README controls, feature status, and validation accurate.

## Keyboard, controller and audio update

LocalInputRouter assigns the first two gamepads to separate player slots and preserves the remaining assignment on disconnect. PlayerActions combines keyboard and assigned-pad input with a stick deadzone. Global gamepad buttons handle restart, mute and music switching. Simulated-device tests cover independent action routing, jump/movement, disconnect stability and emotes.

GameAudio provides two original loops and ten named cues. Python synthesis is reproducible from Tools/make_audio.py. Music and effects use separate 2D AudioSources; mute persists in PlayerPrefs. Happy/sad emotes show labels and play jingles, while hits and shoves trigger short reaction cues. Jump audio is triggered only when the controller was grounded.

App/APK creation is on hold at the user’s explicit request. The in-progress macOS build was cancelled and its build/smoke tooling was removed from the project. LayoutScreenshot captures the scene inside the Unity Editor without building an app.

## Farming-village artwork

A new original 16:9 village background contains farmhouses, crop fields, fences, a windmill and a central tree stump with roots extending through the foreground land. The round rock renderer was removed. The plank sits at y=-0.2 to meet the tree’s cut top; spawn positions derive from the board height. The main README displays the captured full layout only, not the character sprite sheet. Image-generation prompt is saved in generation_prompts.json.

## Farm interface and native sprite import

Tiny5 (SIL OFL 1.1) provides pixel lettering. Wood-rimmed parchment panels hold farmer names, health, match wins, the round clock, instructions and restart. Match wins persist across restarts within the same scene session; reloading the scene resets them. The timer freezes when the round ends.

Character frames and the new farm plank are now imported as native Unity Sprite subassets using alpha-trimmed rectangles, rather than runtime-created serialized Sprite assets. This fixes missing character rendering found during the graphical layout review. The new plank has rope bindings and a central saddle aligned with the tree trunk. LayoutScreenshot warms font glyphs and renders the complete desktop view at 1920 × 1080.
