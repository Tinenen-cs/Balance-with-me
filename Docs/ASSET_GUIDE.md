# Asset guide

- `Assets/Art/Characters/two_players.png`: transparent PNG, 1254 × 1254; four columns and eight rows. Rows top-to-bottom: boy idle, walk, jump, hurt; girl idle, walk, jump, hurt. Four frames per action.
- `Assets/Art/Backgrounds/clouds_green_land.png`: opaque PNG, 941 × 1672.
- `Assets/Art/Props/platform_ball.png`: transparent PNG, 1774 × 887.
- `Docs/layout_preview.png`: generated composition reference, not a gameplay capture.

Sprite import metadata is included. Use Point filtering, no mipmaps, and uncompressed textures. Inspect generated slices and pivots in Sprite Editor; grid dimensions are uneven. PlayerActions selects the existing animation frames. PlayableSceneBuilder configures native importer Sprite subassets for the character frames and farm plank, and keeps simple hand effects/material in Characters/Generated. Source PNGs are unchanged. Dedicated punch/push sprite-sheet frames are not included. Artwork was generated with built-in imagegen; prompts are in `generation_prompts.json`.

- `Assets/Art/Backgrounds/farm_village_tree.png`: new original landscape village, centered tree trunk and visible roots. This replaces the old background in Main. The original ball artwork is retained as source history but no ball is rendered.
- `Docs/gameplay_layout.png`: current full desktop scene captured in Unity; this is the README image.

- `Assets/Art/Props/farm_balance_plank.png`: original transparent rustic plank with rope-bound ends and a small central saddle matching the tree fulcrum.
- Tiny5 font: SIL OFL 1.1, license alongside the font in Assets/Art/Fonts.

## Matched wood refinement

The current farm_village_tree.png uses simplified honey/chestnut wood, dark pixel outlines, rope wraps and a central bearing cap matched to farm_balance_plank.png. The village placement and exposed roots remain unchanged. Updated full-scene preview: Docs/gameplay_layout.png. Edit prompt and built-in imagegen provenance: Docs/generation_prompts.json.
