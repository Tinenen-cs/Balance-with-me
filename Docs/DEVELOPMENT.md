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


### Centered round results and controller replay - 17 October 2026

The result card is shared by round and match results and sits at the canvas center. Round results use the round winner's head/name plus a next-round countdown; match results use aggregate scores and offer Play Again with keyboard/PS4 hints. Draws show a neutral text result without a winner face. The former top banner is hidden throughout results. Result poses leave room on both sides of the card. The Ready/Set/Start popup now contains only cream text with a dark outline and shadow, with no panel Image descendants.

PlayAgain guards against replay input during active rounds. Enter and numpad Enter invoke it after match completion, as does Cross on either assigned gamepad; Options retains its restart behavior. Existing joystick/D-pad, jump, punch, push, emote and auxiliary mappings apply to DualShock pads through Unity's Gamepad aliases. Help displays PlayStation names.


### One-minute pressure rounds and downhill footing - 18 October 2026

RoundDuration is now 60 seconds and the saved/built clock starts at 01:00. The existing 30-second warning and 10-to-1 second beeps remain. Timeout still compares remaining life; three-round scoring and Play Again are unchanged.

Per-round results disable only the card's three backing Images. Heading, name and next-round text use cream lettering with dark outlines/shadows; the head has a cream outline and dark shadow. The final match card retains its parchment and Play Again button.

PlayerActions ramps a grounded lower-side drift toward a tilt-dependent cap (1.65 world units/second at 14 degrees), using 0.9 world units/second squared acceleration and a 2-degree neutral zone. The motor adds this drift separately from input, so an idle slide does not flip the character and uphill movement can overcome it. Drift requires actual plank contact, clears in the air or when play stops, and resets with the player. The upper side eases toward zero drift as the tilt reverses. Physics handles vertical contact and eventual falling; this mechanic does not teleport active players. maxSlopeDrift and slopeDriftAcceleration are adjustable per player in the Inspector.


### Above-plank announcements and win stars - 18 October 2026

Raised the background-free Ready/Set/Start group to canvas y=150 so both number and caption clear the plank. Per-round announcements now sit at y=200, putting the head, name and next-round indicator above the platform. The final match card remains centered with Play Again. Positions use the existing 1920 x 1080 CanvasScaler and work in overlay or camera canvas mode.

Replaced the numeric Round Wins HUD labels with three 44-pixel star slots per player. The new transparent round_win_star.png was created with built-in imagegen to match the game's gold/chestnut pixel palette. Full-color gold marks earned wins; muted stars mark unearned slots. RefreshHud derives stars from existing win counts, so one win earns one star, draws earn none, scores persist between rounds, and replay clears them without a second score source. The saved Main scene and scene builder both wire the six images.


### Portable Windows submission - 27 October 2026

Added GameMenu, DesktopDisplay and an opt-in RuntimeDiagnostics component. The standalone player opens the menu paused, uses landscape fullscreen, and offers persistent controls and joystick setup. Editor playtests retain immediate gameplay so scene preview and prior test workflows remain intact. Menus block gameplay input and unscaled round transitions while paused. Normal launches do not activate diagnostics; -qa-report and optional -qa-autoplay/-qa-exit flags support independent packaged-player tests using virtual gamepad events. Gameplay face buttons no longer submit old selected HUD buttons.

Raw USB Joystick slots coexist with Gamepad slots; disconnecting one device preserves the other's assignment. Five physical buttons can be mapped through the menu, saved by device layout. Ground detection now reuses a collider array and the HUD refreshes text only when health/score/time changes.

BuildSubmission.Build creates a Windows x64 Mono release with Main, Direct3D 11 and fullscreen player defaults. Tools/Package-Submission.ps1 creates the full-build ZIP and compiles Tools/PortableLauncher.cs with an embedded ZIP resource into one portable EXE. The launcher checks extraction paths, uses an owned temporary directory, prevents duplicate portable launches, forwards diagnostic arguments, waits for the game, returns its exit code, and cleans up. No installer or administrator access is requested. The standard Unity ZIP remains available if .NET or Canvas EXE attachments are unsuitable. Rebuilds and artifacts stay out of Git; scripts, source, scene and validation notes are versioned.

## Readable controls menu - 27 October 2026

Applied ui-ux-pro-max guidance to the existing Unity UI: clearer type hierarchy, high-contrast ink on parchment, short instructions, aligned controls and separated actions. Bundled Atkinson Hyperlegible Regular/Bold under SIL OFL for instructions, buttons and gameplay details. Tiny5 remains only on selected titles, countdown and winner headings. The menu now separates keyboard controls into action/Boy/Girl columns and controller controls into action/button rows. USB setup and connected-player status have their own strip; Play / Resume is the primary action. Removed dense all-capital instruction blocks and shortened status/mapping prompts.

The Canvas retains resolution scaling and landscape framing. Preview capture explicitly regenerates text at its render target scale. Added typography regression checks for font roles, clipping/wrapping and button dimensions. See Docs/submission_controls.png for the updated menu.

## Replay feedback, mouse close and HUD fit - 28 October 2026

Applied ui-ux-pro-max interaction guidance to the existing Unity UI. Farm-sign buttons now tint the visible face, with distinct hover/selection and pressed colors using a 120 ms fade. Play Again retains mouse click and keyboard/controller replay. Added a 60 x 60 X button in the menu's upper-right corner that resumes the current match without resetting it. Character name headings use the chunky Tiny5 theme font; instructions and buttons retain Atkinson Hyperlegible. The round clock uses smaller text, tighter line spacing, best-fit limits and internal padding in a taller frame. Added pointer-event checks for replay feedback/click and menu close, plus timer-fit checks across all three rounds and warning times. The preview set now includes replay_hover.png.

## Red close hover and quit confirmation - 28 October 2026

Changed the menu X's hover/selection to red with a darker red pressed state. Quit opens a centered Yes, quit / No, stay dialog; No receives initial keyboard/controller focus. The game stays paused, and the controls behind the dialog cannot be clicked or selected. No and Esc cancel back to the menu. Application.wantsToQuit also routes window-close requests through the dialog until Yes is chosen. The opt-in diagnostic -qa-exit path explicitly approves shutdown after writing its report. Added automated checks for red hover and confirmation/cancel behavior; previews include close_hover.png and quit_confirmation.png.

## Twenty-five-second rounds - 28 October 2026

Reduced each round to 25 seconds. The HUD starts at 00:25 and the menu states 25 seconds per round. Warning text and sound now fire once at 20 and once at 10 seconds remaining; the previous final-second tick sequence was removed. A frame crossing both thresholds emits each warning once. Ready/Set/Start, the three-round match, health-based timeout decisions, draw handling and replay behavior remain. Updated timer/reset tests, controller timing assertions, warning previews and current submission instructions.
