# Current asset guide

- `Assets/Art/Characters/two_players.png`: original transparent 1254 × 1254 sheet. Four frames each for boy idle/walk/jump/hurt, then girl idle/walk/jump/hurt. Native importer Sprite subassets are alpha-trimmed by PlayableSceneBuilder.
- `Assets/Art/Backgrounds/farm_village_wood_ball.png`: farming village with a circular wooden-log fulcrum, warm woodgrain and rope detail. Replaces the former trunk and roots. Uniform overscan preserves the round shape and aligns the support with the plank.
- `Assets/Art/Props/farm_balance_plank.png`: transparent rustic plank, rope-bound ends and a central wooden saddle.
- `Assets/Art/UI/pixel_hand.png`: native pixel hand effect used for punch/push feedback.
- `Assets/Art/Fonts/Tiny5.ttf`: pixel font under SIL OFL 1.1; license alongside the font.
- `Assets/Audio/Music`: two original background loops.
- `Assets/Audio/SFX`: ten action, hit, emote and round cues.
- `Docs/gameplay_layout.png`: current full scene captured inside Unity; the README displays this image only.

Art was generated or edited with built-in imagegen. Prompts are saved in `Docs/generation_prompts.json`. Source textures retain Point filtering and uncompressed sprite settings. Dedicated punch/push animation-sheet poses remain future polish; current combat uses existing frames and hand effects.

The current wooden ball is reduced about 25% for a more balanced composition. Its grass base blends into the foreground. See the smaller_wooden_ball entry in generation_prompts.json.
