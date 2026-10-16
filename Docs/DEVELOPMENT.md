# Development workflow

Game: **Balance with me**. Repository: **Tinenen-cs/Balance-with-me**.

## Current authorization

The user has authorized new and edited scripts to add punching, pushing, and useful gameplay. The earlier single-script restriction is superseded. The original `CharacterController2DP2.bin` remains unchanged; the active controller replaces deprecated `Rigidbody2D.velocity` calls with `linearVelocity`.

## Implementation

- `PlayerActions` reads each player's keyboard bindings and calls the movement controller in FixedUpdate.
- Punch uses a short active window and a per-attack hit guard: one health point and impulse knockback once per attack. Push applies small impulses throughout its active window without damage.
- Both actions require the target ahead and within reach. Cooldowns and hit stun prevent attack spam and immediate movement cancellation of knockback.
- Existing sprites supply idle, walk, jump, and hurt reactions. Orange and cyan pixel hand effects distinguish punch and push; these are simple effects, not new authored combat sprite sheets.
- Players have Rigidbody2Ds and capsule colliders. Ground checks use the Ground layer. A HingeJoint2D limits plank rotation to ±16°; the ball remains a stationary visual support.
- `BalanceGame` manages three health points per player, falling/zero-health wins, status, and reset.
- `PlayableSceneBuilder` rebuilds the scene, creates native Sprite assets from alpha-trimmed source rectangles, and wires component references. Rebuilding resets scene-only Inspector edits.

## Display and versions

Desktop 16:9, 1920 × 1080 UI reference and resizable standalone defaults. Select a 16:9 Game view manually. The original portrait art concept remains a reference. Target editor: 6000.3.24f1. Local checks: installed 6000.3.26f1. Restore ProjectVersion to the target before committing newer-editor validation changes.

Commit and push completed changes; keep README controls, feature status, and validation accurate.
