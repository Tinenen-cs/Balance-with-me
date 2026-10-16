# Validation — 16 October 2026

- Unity 6000.3.26f1 batch import and C# compilation completed successfully (exit 0).
- CharacterController2DP2.cs is the only custom C# file in Assets.
- Original .bin and .cs remain byte-identical after compilation, including CRLF line endings.
- SHA-256: `113c9394fe1d9b8fd0fcc56b91eeca7cc3cb9044ba90f48afc1d89392d49e959`.
- Unity reports obsolete Rigidbody2D.velocity warnings. The supplied code remains unchanged.
- Prior prototype component references were removed from Main. Local scene object references were checked for dangling IDs.
- ProjectVersion restored to requested target 6000.3.24f1 after newer-patch validation.
- Exact-target-version compilation, interactive gameplay, animation playback, and builds remain unverified. Main is a visual setup preview, not wired gameplay.
