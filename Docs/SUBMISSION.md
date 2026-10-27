# Windows / Canvas submission

The professor requests a portable Windows executable, landscape mode and fullscreen. The submission build targets **Windows 10/11 x64** and starts in fullscreen with a landscape 16:9 game view. Unity does not need to be installed on the receiving PC.

## Exact upload file

Use **`Submission/BalanceWithMe-Portable-Windows-x64.exe`** after your playtest. This is the entire game in one executable, not the small Unity launcher separated from its data. It temporarily extracts its bundled engine/data, runs the game, then removes that launch's temporary directory. It requires the Windows .NET Framework used by the included launcher; a standard Unity ZIP is also supplied below.

If Canvas does not allow `.exe` attachments, use **`Submission/BalanceWithMe-Windows-x64.zip`**. This contains the complete standard Unity Windows build. The professor should extract all files into one folder and run `BalanceWithMe.exe`. Keep its `_Data` directory, `UnityPlayer.dll` and `MonoBleedingEdge` folder together. Never submit only the small `BalanceWithMe.exe` from the uncompressed build folder.

Canvas's assignment-specific file extension allowlist could not be inspected because Computer Use stopped when the current browser URL could not be verified. The professor's written request explicitly says portable EXE; the ZIP is the complete-build fallback. **Nothing has been uploaded or submitted.**

## Test before uploading

1. Copy the portable EXE into a new folder (or onto a USB drive), then double-click it. Confirm the controls menu appears in landscape fullscreen. Choose Play / Resume.
2. Test both players: Boy uses **A/D, W, F, G, E** for move/jump/punch/push/emote. Girl uses **Left/Right, Up, K, L, O**. Check jumping, attack direction, life percentages and uphill movement to resist sliding.
3. Plug in the actual controller(s). The menu shows device assignments. First available device controls Boy; second controls Girl. Xbox / PlayStation: left stick or D-pad moves, **A/Cross** jumps, **X/Square** punches, **B/Circle** pushes, **Y/Triangle** emotes. **R3** opens the menu. For raw USB joysticks, choose **Set up Boy USB / Set up Girl USB** and press a button for each requested action (Jump, Punch, Push, Emote, Menu).
4. Test unplugging/reconnecting a controller and mixed keyboard/controller play. Verify the other player keeps their controller. Hardware testing is required: simulated-device tests cannot establish compatibility with a specific physical controller or driver.
5. Play three rounds. Check the 20-second and 10-second sound warnings during each 25-second round, fall/stamina defeats, earned stars and automatic next rounds. At match end, **Enter / A / Cross** plays again. **Start / Options** restarts. A timed draw awards no star.
6. Press **Esc / R3** to pause. Try Resume, New Match, sound and display. Hover over X to confirm its red highlight; click it to resume. Quit should ask Yes, quit / No, stay: No returns to the paused menu, Yes exits. Alt-tab should pause the game. Relaunch to confirm fullscreen startup. Only upload after you are satisfied.

## Suggested Canvas comment

> Submitted: Balance with me, a portable Windows x64 two-player balancing game. It opens in landscape fullscreen with an in-game controls/menu screen. Boy: A/D move, W jump, F punch, G push. Girl: arrow keys move, Up jump, K punch, L push. Xbox/PS4: left stick/D-pad move, A/Cross jump, X/Square punch, B/Circle push. Esc/R3 opens the menu; raw USB joysticks can be mapped there. Please run the portable EXE, or extract the entire ZIP before running BalanceWithMe.exe.

## Rebuild

Use Unity **6000.3.24f1** with Windows Mono build support. Run `BalanceWithMe.Editor.BuildSubmission.Build` in batchmode or choose **Balance with me > Build Windows Submission** in the Editor. `BALANCE_BUILD_OUTPUT` can select a fresh build directory. The builder sets native fullscreen, landscape orientation and Direct3D 11, builds Main, and writes a build summary under Logs.

Run `Tools/Package-Submission.ps1 -BuildDirectory <build folder> -OutputDirectory <new output folder>` to generate the EXE, complete-build ZIP and SHA256 checksums. Packaging excludes debug symbols and `DoNotShip` directories. Generated binaries remain local and are ignored by Git; source and build scripts are committed.

## Evidence and limits

See `Docs/VALIDATION.md` and `Docs/submission_controls.png`. Automated tests cover gameplay, pause/resume, score reset, assets, keyboard input, simulated Xbox-compatible/PlayStation/USB devices and controller hotplug. Physical controllers were not attached during the recorded initial player sample. The initial Canvas format inspection stopped under Computer Use URL-verification policy, so the assignment-specific extension allowlist remains unverified. Later desktop mouse checks verified the packaged menu; Unity tests/rendered previews and opt-in player diagnostics cover the other recorded checks. See Docs/VALIDATION.md for each run and its limits.
