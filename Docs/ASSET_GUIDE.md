# Asset guide

- `Assets/Art/Characters/two_players.png`: transparent PNG, 1254 × 1254; four columns and eight rows. Rows top-to-bottom: boy idle, walk, jump, hurt; girl idle, walk, jump, hurt. Four frames per action.
- `Assets/Art/Backgrounds/clouds_green_land.png`: opaque PNG, 941 × 1672.
- `Assets/Art/Props/platform_ball.png`: transparent PNG, 1774 × 887.
- `Docs/layout_preview.png`: generated composition reference, not a gameplay capture.

Sprite import metadata is included. Use Point filtering, no mipmaps, and uncompressed textures. Inspect generated slices and pivots in Sprite Editor; grid dimensions are uneven. PlayerActions selects the existing animation frames. PlayableSceneBuilder creates trimmed native Sprite assets plus simple pixel hand effects in Characters/Generated; the source PNGs are unchanged. Dedicated punch/push sprite-sheet frames are not included. Artwork was generated with built-in imagegen; prompts are in `generation_prompts.json`.
