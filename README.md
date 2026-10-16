# Balance with me

A 2D pixel-art project in Unity featuring a chibi boy and girl on opposite ends of a plank balanced on a large ball, against clouds and green land.

<p align="center"><img src="Docs/layout_preview.png" alt="Art layout preview of Balance with me" width="320"></p>

> Generated art layout preview, not a gameplay screenshot. The current Main scene is a visual setup preview. Only the exact supplied CharacterController2DP2 script is included as custom C# code.

## Download & play

Source: [Tinenen-cs/Balance-with-me](https://github.com/Tinenen-cs/Balance-with-me). No downloadable game builds or hosted browser version are available yet.

## Activity: Two-character balance game

The intended design has two players walking and jumping on a board above a ball, with hurt animations. Input bindings, animation wiring, balancing behavior, and round rules remain pending under the restriction to use only the supplied script.

The unchanged controller provides ground checks and a public `Move(float move, bool crouch, bool jump)` method, horizontal movement, jumping, crouching, and flipping. It does **not** read player input, select animation clips, or implement balancing, damage, or game over. Including the file does not make those features playable.

## Status

| Step | Feature | Status |
| --- | --- | --- |
| 1 | Unity project folders, URP 2D settings, VS Code recommendations | Prepared |
| 2 | Original boy/girl sprites, background, plank/ball artwork | Created |
| 3 | Main scene with visual layout | Setup preview |
| 4 | Exact supplied CharacterController2DP2 | Preserved byte-for-byte |
| 5 | Input, animation, controller/physics scene wiring | Pending |
| 6 | Audio, balancing rules, game over and polish | Pending |
| 7 | Platform builds and release packages | Pending |

## Features

- Original boy and girl chibi characters in a cozy farming RPG pixel aesthetic.
- Transparent sprite sheet containing 32 frames: idle, walking, jumping, and hurt for each character.
- Cloud-and-green-land portrait background.
- Transparent plank-and-ball prop and generated composition preview.
- Original controller file retained as `CharacterController2DP2.bin`, plus its identical `.cs` copy in Assets.
- No additional custom runtime, editor, or test C# scripts.

## Controls

No keyboard or touch bindings are configured. The supplied controller needs an external call to its `Move` method; no input-calling script has been added.

## Tuning the controller

Once the controller is attached to a player with a Rigidbody2D, assign Ground Check and Ceiling Check transforms and the ground layer mask in the Inspector. Jump force, movement smoothing, crouch speed, and air control are exposed by the supplied script. Do not modify the source without explicit permission.

The current visual scene uses Unity UI images; it is not a wired Rigidbody2D gameplay scene.

## Screen layout

Portrait 9:16 with a 1080 × 1920 Canvas reference. The boy stands to the left and the girl to the right of the centered plank/ball artwork. Select 9:16 in the Game view. The background fills the Canvas.

## Unity version and packages

- Target: **Unity 6.3 LTS — 6000.3.24f1**, matching the reference repository.
- Local compile validation uses installed **6000.3.26f1**; exact-target-version validation is pending.
- URP 2D, 2D Sprite, 2D Animation, Input System, uGUI, and Unity Test Framework package configuration is in `Packages/manifest.json`.
- VS Code extension recommendations are in `.vscode/extensions.json`.

## Project structure

```text
Assets/
  Art/
    Characters/two_players.png
    Backgrounds/clouds_green_land.png
    Props/platform_ball.png
    UI/                         reserved UI art
    Fonts/                      font notes
    Icon/                       reserved app icon
  Audio/                        reserved audio and credit notes
  Editor/                       reserved; no custom editor scripts
  Scenes/Main.unity             visual setup preview
  Scripts/
    Players/CharacterController2DP2.cs  only custom C# script
    UI/                         reserved
    World/                      reserved
    Audio/                      reserved
  Data/                         reserved
  Settings/                     official Unity template URP settings
  Tests/PlayMode/                reserved; no custom test scripts
Docs/
  layout_preview.png            generated artwork composition
  generation_prompts.json       original image prompts
  ASSET_GUIDE.md                image sizes and animation row ordering
  DEVELOPMENT.md               current constraints and workflow
  VALIDATION.md                checks and their limitations
Packages/                       package manifest and lock
ProjectSettings/                Unity settings
Tools/                          reserved; no custom build scripts
.vscode/                        editor recommendations
CharacterController2DP2.bin      exact user-supplied original
```

## Audio credits

No music or sound effects are included yet. Store future licensed audio in `Assets/Audio` and document its source and license here.

## Art and font credits

| Asset | Source |
| --- | --- |
| Characters, background, plank/ball, layout preview | Original images generated for this project using built-in imagegen |
| UI text | Unity built-in font |
| Project and URP configuration | Official Unity 2D template from the installed editor |
| CharacterController2DP2 | User-supplied file, preserved unchanged; redistribution license not provided |

README section organization follows the supplied [CSEL Tamagotchi reference](https://github.com/Tinenen-cs/Tinenen-cs-csel-tamagotchi). No Stardew Valley game assets are included. No repository-wide license has been selected.

## How to run

### A. Mac (Terminal)

```bash
git clone https://github.com/Tinenen-cs/Balance-with-me.git
cd Balance-with-me
```

Install Unity 6000.3.24f1 in Unity Hub, add the root folder as a project, and open `Assets/Scenes/Main.unity`. Select 9:16 and press Play to inspect the visual setup. This is not currently a playable two-player game. Open the root folder in VS Code to view the unchanged script.

### Test-run in the Unity Editor (Windows, step by step)

1. Clone the repository and add its root in Unity Hub.
2. Open it with 6000.3.24f1 and let packages import.
3. Open Main and choose a portrait Game view.
4. Inspect the layout and Console. Keyboard controls are not wired.

### B. Other devices

No Android, WebGL, or desktop builds have been validated. Use Unity Build Profiles after gameplay wiring is completed and the matching build support modules are installed. No custom build script is included.

## Validation

See [validation notes](Docs/VALIDATION.md). File identity checks include the original `.bin`, the `.cs` copy, and the staged Git contents, including line endings. Unity compilation alone does not establish gameplay or input behavior.

## Troubleshooting

- **No movement:** expected; the exact controller does not read keyboard input and is not wired into the visual preview.
- **No animation changes:** animation-selection code has not been added under the single-script constraint.
- **Sprites need adjustment:** use Sprite Editor to inspect slices and pivots; generated frames can need alignment work.
- **Version upgrade prompt:** use target 6000.3.24f1. Validation on 6000.3.26f1 is reported separately.
- **Missing scene:** open `Assets/Scenes/Main.unity`. No custom scene-generation script is included.

## Contributing

Keep the supplied script unchanged. Commit each completed project change with a descriptive message, including Unity `.meta` files. Push commits to GitHub when authenticated. Use feature branches for further development. Do not commit Library, Temp, Logs, UserSettings, build outputs, or credentials.
