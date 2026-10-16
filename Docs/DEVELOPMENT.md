# Development workflow

Game title: **Balance with me**. Repository: **Tinenen-cs/Balance-with-me**.

The user requires the exact contents of `CharacterController2DP2.bin` and no additional custom scripts. `Assets/Scripts/Players/CharacterController2DP2.cs` is a byte-identical copy. Git text conversion is disabled for both paths so CRLF line endings are preserved.

Earlier generated custom gameplay, editor, and test scripts were removed from the project and backed up outside the repository. The Main scene retains a static art preview with those component references removed. No input binding, animation selection, round management, or balance simulation is claimed.

Commit each completed change and push it when GitHub authentication is available. Keep asset metadata with assets. Target editor is 6000.3.24f1; the local validation editor is 6000.3.26f1.

## Desktop display

Main uses a 1920 × 1080 Canvas and DesktopFrame. HUD and footer positions fit the landscape frame. Standalone defaults are a resizable 1920 × 1080 window; browser defaults are 1280 × 720. Select 16:9 in the Editor Game view manually. The original portrait art concept remains in Docs as a design reference. No controller code changed.
