# Validation — 16 October 2026

## Playable push/punch update

Editor used: **Unity 6000.3.26f1**. Project target remains **6000.3.24f1**; exact-target-patch verification is pending.

- Main scene generated successfully with player Rigidbody2Ds, capsule colliders, configured ground checks, a hinged plank, separate ball/plank sprites, keyboard actions, health UI, winner detection, and restart.
- Runtime and editor C# compilation passed.
- **7 PlayMode tests passed, 0 failed**:
  - Punch damages only once per attack, respects cooldown, and stuns its target.
  - Push applies forward knockback without health damage.
  - Punch cannot hit behind the attacker or beyond reach.
  - Restart restores health, action state, positions, facing, and velocity.
  - Falling ends the round and disables further attacks.
  - Keyboard bindings invoke boy push and girl punch; their hand effects are enabled and visible-sized.
  - Both jump keys lift grounded players.
- Keyboard injection tests temporarily configure Input System to process devices without Game-view focus in batch mode, then restore the original settings.
- The original `CharacterController2DP2.bin` remains unchanged. The active `.cs` now uses `Rigidbody2D.linearVelocity`, as permitted by the user's updated authorization.
- Main remains a desktop 16:9 scene; runtime sprites are native Unity assets referencing the original textures.

## Limits

Headless PlayMode tests verify state, input mapping, physics impulses, and effect dimensions. Interactive visual polish, extended matches, exact-target-patch validation, and packaged desktop/mobile/browser builds have not been verified. Existing animation frames plus simple pixel hand effects provide feedback; dedicated punch/push sprite-sheet frames, sound, touch controls, and free rolling-ball physics remain future work.

Earlier checks covered the static desktop preview. That preview is superseded by the playable Main scene.
