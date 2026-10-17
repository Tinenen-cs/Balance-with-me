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

## Matched platform and fulcrum

The stump material and palette now match the movable farm plank: honey/chestnut wood, golden highlights, clean outlines, matching rope wraps and a small bearing cap. Only background artwork changed; combat, controls and board physics are retained. Audio verification additionally checks active looping playback, both emote jingles, hit reactions and round-result cues.

## Current fulcrum: round wooden log

The upright trunk and roots are replaced by a circular wooden log/ball silhouette in the village background. The support retains the plank’s warm timber palette, rope detail and woodgrain. It is a stationary visual fulcrum for the existing hinged plank, not a free-rolling physics ball. Earlier trunk notes describe previous artwork revisions. The current background is farm_village_wood_ball.png.

The round wooden support is positioned using uniform background overscan (11.6 world-unit height, y=-0.4) so it retains its circular proportions and meets the existing plank without altering player physics. The scene and scene-builder settings match.

## Smaller fulcrum and cleaner HUD

The wooden ball is about 25% smaller, with the background offset adjusted to y=-0.18 so its top remains seated beneath the plank. The BOY/GIRL keyboard/gamepad device-status labels were removed from the scene, scene builder and runtime update. Keyboard and gamepad input mappings remain available; instructions stay in the themed controls panel and README.

## Compact HUD and full action audio

Persistent farm signs now contain only the title, names/health/wins, clock, Help and Restart. Controls appear for five seconds at startup or with H, the Help button or either assigned pad's left bumper. Round-start and audio notices last 2.5 seconds; the final result remains until restart. Popup text and panel art share a CanvasGroup so neither leaves an empty plaque behind. Popup groups do not intercept clicks.

PlayerActions emits throttled footsteps while moving on the plank and a landing cue after meaningful airtime. Existing jump, punch, push, hit, shove and both emote sounds remain. GameAudio sequences a distinct failure cue before win/draw and cancels old result playback on restart. Two looping music tracks and saved mute are retained. Tools/make_audio.py reproduces all 15 WAV assets. No app or APK was built.

## Victory emote and player icons

A sole winner automatically triggers a 2.4-second gold “I DID IT!” emote and a bounce/rock animation, using the existing win jingle after the failure cue. It replaces any active attack or hurt pose; draws have no winner celebration. Restart clears all celebration state. Each scorecard now contains the matching character’s existing idle sprite as an aspect-preserving UI icon, with the name/health/wins text shifted beside it. No additional raster assets or duplicate sprite imports are needed.

## Smooth character and platform movement

Both player Rigidbody2Ds and the plank render with interpolation. Movement smoothing is 0.08 seconds, uses an explicit fixed physics timestep, and resets its stored velocity on a new round. Locomotion frames follow grounding and actual horizontal speed, while action/victory lean and bob offsets ease toward their target poses. The original .bin remains unchanged.

The plank now has mass 7 and angular damping 2.5. Centering torque is 0.75 per degree; quadratic edge resistance (4) begins at 11 degrees before the existing ±16-degree hinge stops. These settings soften the swing without removing the players’ effect on balance. GameManager exposes the three torque settings for tuning. Tests exercise acceleration/reversal/stopping and the plank’s bounded, continuous, damped response.

## Grass-trimmed wooden fulcrum

Edited the existing village background with built-in imagegen to add grass and moss along the circular wooden support’s rim, sides and ground contact. The exposed woodgrain face and top plank contact remain clear; the canvas remains 1672 × 941. The existing asset path and Unity GUID are retained. The prompt is recorded in Docs/generation_prompts.json.

## Slow cloud movement

Removed painted sky clouds from the village base using built-in imagegen, retaining the grass-covered wooden support, village placement and 1672 × 941 canvas. A new transparent cloud sprite is rendered in five sizes/positions, ahead of the background but above the village and behind gameplay/HUD. CloudDrift advances horizontal position with delta time at 0.06–0.108 world units/second. Viewport width, each sprite’s full extent and padding determine an offscreen wrap; modulo wrapping preserves overshoot even for large time steps. Clouds continue between rounds. Rebuild Playable Scene wires the cloud instances and camera automatically.

## Character action effects

ActionEffects adds per-player pools of 64 tiny SpriteRenderers using the existing pixel hand stamp and URP sprite material. Effects emit at fixed world positions, independent of subsequent player movement, with short lifetimes, fading alpha and simple ballistic motion. Walking/landing/jumping, accepted punch/push starts, throttled hit/shove reactions, happy/sad emotes, victory/failure and spawn each have a distinct emission. Restart clears previous particles before emitting spawn leaves; disabling or unloading the player clears/disposes its pool. Effects do not change colliders, damage or input.

Capture Action Effects stages a real punch/push exchange after startup notices expire and saves a full-scene preview. Tests cover action/contact selection, rejected attack inputs, both players’ movement emissions, emote/result effects, expiry, restart cleanup and bounded renderer counts under repeated bursts.


### Ready countdown, timed rounds and replay - 17 October 2026

Each round now has a three-step parchment popup (1 READY, 2 SET, 3 START!) before physics and combat resume. The clock counts down from 03:00 only during active gameplay. At zero, the player with more remaining life wins; equal life awards neither player a win. The clock changes color with a labeled sound alert at 30 seconds, followed by a beep and labeled countdown every second from 10 through 1. Restart clears pending warnings. Three synthesized cue assets are reproducible with Tools/make_audio.py.

Round resolution records fall, stamina or timeout causes before freezing physics. Fallen players are presented on the grass on their exit side; stamina and timeout defeats remain on the plank. All three rounds still play, and rounds one and two advance automatically. After round three the final result stays visible with an interactive Play Again button, including match draws. Clicking it resets scores and begins the opening countdown. This replaces the previous automatic match reset.
