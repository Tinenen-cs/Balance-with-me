# Balance with me

A 2D pixel-art project in Unity featuring a chibi boy and girl on opposite ends of a plank balanced on a large ball, against clouds and green land.

<p align="center"><img src="Docs/layout_preview.png" alt="Art layout preview of Balance with me" width="320"></p>

<p align="center"><img src="Assets/Art/Characters/two_players.png" alt="Boy and girl sprite sheet: idle, walking, jumping, and hurt" width="720"></p>

> Images above: generated composition preview and the character animation sheet. Neither is a gameplay screenshot. The current Main scene is a visual setup preview. Only the exact supplied CharacterController2DP2 script is included as custom C# code.

## Download & play

Source: **[Tinenen-cs/Balance-with-me](https://github.com/Tinenen-cs/Balance-with-me)**. No downloadable builds are available yet. Publication of the local commits still requires GitHub authentication.

| Device | Planned file / output | How to run |
| --- | --- | --- |
| Windows | `Balance-with-me-Windows.zip` | Pending; inspect the source in Unity for now |
| Android | `Balance-with-me.apk` | Pending; mobile input is not configured |
| macOS | `Balance-with-me-macOS.zip` | Pending; inspect the source in Unity for now |
| Browser | WebGL build | Pending; no hosted game link exists |

## Activity: Two-character balance game

The intended design has two players walking and jumping on a board above a ball, with hurt animations. Input bindings, animation wiring, balancing behavior, and round rules remain pending under the restriction to use only the supplied script.

The unchanged controller provides ground checks and a public `Move(float move, bool crouch, bool jump)` method, horizontal movement, jumping, crouching, and flipping. It does **not** read player input, select animation clips, or implement balancing, damage, or game over. Including the file does not make those features playable.

| Requirement | Current project behavior | Code / asset | Verification |
| --- | --- | --- | --- |
| Boy and girl characters | Both appear in the visual scene | `Assets/Scenes/Main.unity` | Visual review pending |
| Walk, jump, and hurt frames | Four frames per action for each character | `Assets/Art/Characters/two_players.png` | Artwork inspected; transitions not wired |
| Horizontal movement | Supplied controller can apply smoothed movement when called | `CharacterController2DP2.Move` | Compilation passed; no input caller |
| Jumping | Supplied controller applies jump force when grounded and requested | `CharacterController2DP2.Move` | Compilation passed; scene wiring pending |
| Ground detection | Ground mask and Ground Check determine contact | `CharacterController2DP2.FixedUpdate` | Runtime wiring pending |
| Exact original code | `.bin` and `.cs` contents, including CRLF, match | `CharacterController2DP2.bin` | Byte identity verified |
| Only one custom C# script | No additional gameplay, editor, or test scripts | `Assets/Scripts/Players/CharacterController2DP2.cs` | File inventory verified |

### Recording the video (about 2 minutes)

Record the current setup accurately; movement and animation transitions are not playable yet.

1. Open **Assets → Scenes → Main** and set the Game view to **Phone 1080 × 1920**.
2. Start a screen recording on your computer. On macOS, use **Cmd + Shift + 5**.
3. Show the boy, girl, plank, large ball, clouds, and green land in the preview.
4. Open the character sheet in Sprite Editor and show its idle, walk, jump, and hurt rows.
5. Show the unchanged controller file in VS Code and explain that input and physics wiring are pending.
6. Stop recording. Record a gameplay demonstration only after those features work.

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

| Action | Supplied controller support | Current binding |
| --- | --- | --- |
| Walk left / right | `Move(move, crouch, jump)` accepts horizontal movement | Not configured |
| Jump | `Move` accepts a jump request and checks grounding | Not configured |
| Crouch | `Move` accepts a crouch request | Not configured |
| Face left / right | `Flip()` changes facing and horizontal scale | Not configured |
| Hurt animation | Sprite frames exist; controller has no damage/animation logic | Not configured |

The supplied controller needs an external call to its `Move` method; no input-calling script has been added.

## Tuning the controller

1. In the Project window, locate **Assets → Scripts → Players → CharacterController2DP2**.
2. For future gameplay wiring, attach it to a player with a **Rigidbody2D** and suitable colliders.
3. Assign **Ground Check**, **Ceiling Check**, and **What Is Ground** in its Inspector.
4. Use the existing fields for **Jump Force**, **Movement Smoothing**, **Crouch Speed**, and **Air Control**.
5. Check behavior after scene wiring is complete. Keep the supplied source unchanged.

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

| Asset | Source | License / attribution |
| --- | --- | --- |
| Characters, background, plank/ball, layout preview | Original images generated for this project using built-in imagegen | Prompts recorded in `Docs/generation_prompts.json`; no separate asset license assigned |
| UI text | Unity built-in font | Unity-provided resource |
| Project and URP configuration | Official Unity 2D template from the installed editor | Unity-provided configuration |
| CharacterController2DP2 | User-supplied file, preserved unchanged | Redistribution license not provided |

README section organization follows the supplied [CSEL Tamagotchi reference](https://github.com/Tinenen-cs/Tinenen-cs-csel-tamagotchi). No Stardew Valley game assets are included. No repository-wide license has been selected.

---

## How to run

### A. Mac (Terminal)

1. After the local commits have been published, get the project:
   ```bash
   git clone https://github.com/Tinenen-cs/Balance-with-me.git
   cd Balance-with-me
   ```
   For the existing local copy, use `/Users/imac23/Documents/TWOCHARACTERGAME_CSEL`. Once the remote is populated, update a clean checkout with `git pull origin main`.
2. Install **Unity Hub**, then editor **6000.3.24f1**. Choose that exact patch to match the reference project's version.
3. In Hub, select **Projects → Add → Add project from disk**. Choose the folder containing `Assets`, `Packages`, and `ProjectSettings`.
4. Open the project and allow package restoration and asset import to finish.
5. Open **Assets → Scenes → Main** manually. No custom scene-opening tool is included.
6. In the Game view resolution dropdown, add **Phone**, Fixed Resolution **1080 × 1920**, or select a 9:16 aspect.
7. Press **Play** to inspect the visual setup. It is not currently an interactive two-player game.
8. Open the same root folder in **VS Code**. The recommended extensions are listed in `.vscode/extensions.json`. View `CharacterController2DP2.cs` without changing it.
9. Before claiming runtime behavior, complete the scene wiring and review the Console. See [validation notes](Docs/VALIDATION.md) for the checks already performed.

### Test-run in the Unity Editor (Windows, step by step)

1. Clone the repository after publication, or use an existing copy of the source.
2. Open **Unity Hub** from the Start menu.
3. Select **Projects → Add → Add project from disk** and choose the root project folder.
4. Confirm the editor version is **6000.3.24f1**, then open the project.
5. Wait for Unity to import assets and restore packages.
6. In the Project window, open **Assets → Scenes → Main**.
7. Add a **Phone 1080 × 1920** Game view resolution; reduce the Scale slider if the preview is too large.
8. Press **Play** and inspect both characters and the background composition.
9. Open **Window → General → Console** to review messages. The unchanged controller uses obsolete `Rigidbody2D.velocity` APIs that produced warnings in local validation.
10. Press **Play** again to exit. Keyboard movement, animation changes, and balancing are not configured.
11. No custom test scripts are included under the single-script restriction; do not expect a gameplay test suite in Test Runner.

### B. Other devices

#### Browser (any phone or computer)

No hosted browser version is available. After gameplay wiring, install **WebGL Build Support** and use Unity's **File → Build Profiles** to create a WebGL build. Browser and touch behavior remain unverified.

#### Android phone

No APK is available. Future builds require **Android Build Support**, including SDK/NDK and OpenJDK, installed through Unity Hub. Configure an Android build profile after gameplay wiring. No touch controls are implemented.

#### iPhone (needs a Mac with Xcode)

No iOS build is available. Future device testing requires the Unity **iOS Build Support** module and Xcode. iOS input, signing, and device behavior have not been configured or validated.

#### Windows / Linux / macOS

Use the Unity Editor to inspect the project now. Standalone builds are pending. Once gameplay wiring is complete, install the corresponding editor build module and use **File → Build Profiles**. No custom build scripts or additional C# tools are included.

## Troubleshooting

- **No movement:** expected; the exact controller does not read keyboard input and is not wired into the visual preview.
- **No animation changes:** animation-selection code has not been added under the single-script constraint.
- **Sprites need adjustment:** use Sprite Editor to inspect slices and pivots; generated frames can need alignment work.
- **Version upgrade prompt:** use target 6000.3.24f1. Validation on 6000.3.26f1 is reported separately.
- **Missing scene:** open `Assets/Scenes/Main.unity`. No custom scene-generation script is included.

## Contributing

Keep the supplied controller unchanged and use no additional custom scripts. For each completed project change:

```bash
git pull origin main
git checkout -b feature/short-name
# Edit approved scene, artwork, or documentation files.
# Inspect the scene and Console in Unity; retain the original controller bytes.
git add README.md Docs Assets Packages ProjectSettings
git commit -m "Describe the completed change"
git push -u origin feature/short-name
# Open a pull request on GitHub.
```

The pull/push steps require a populated remote and authenticated GitHub access. Commit Unity `.meta` files alongside their assets. Exclude `Library`, `Temp`, `Logs`, `UserSettings`, local builds, and credentials. Update the status table and [validation record](Docs/VALIDATION.md) whenever behavior changes.
