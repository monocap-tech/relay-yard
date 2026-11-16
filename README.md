# Relay Yard

**CPSC 386 — Project 2 | Unity 6000.6.4f1 | URP 2D**

Relay Yard is a single-room, six-switch puzzle-platformer. Touching a pressure plate toggles its indicator and its immediate neighbors. Light all six indicators to unlock the exit. Jumping lets the player bypass switches.

**Status: migration candidate.** This `migration/relay-yard-source` branch contains the Unity text assets, scripts, scenes, project settings, PNGs, and their associated Unity metadata. The **exact modified DotGothic16 TTF is missing** and must be restored. It is not yet a verified build or a replacement for the tested original release. See [MIGRATION_STATUS.md](MIGRATION_STATUS.md).

## Opening in Unity

After restoring the missing font, open this repository as a Unity project using **Unity 6000.6.4f1**. Open `Assets/Scenes/MainMenu.unity` and press Play. The three scenes are `MainMenu`, `Game`, and `Win`, in that order in Build Profiles.

| Control | Action |
|---|---|
| A/D or Left/Right | Move |
| Space | Jump |
| Esc | Pause / resume |
| Hold R (about 0.9 s) | Reset puzzle |
| Mouse | UI buttons, audio settings, immediate reset |

The initial switch state is OFF, OFF, ON, OFF, ON, OFF. A known solution is to activate switches **2, 4, and 6**, each once, avoiding accidental plate contacts.

## Implementation

| Component | Files |
|---|---|
| Movement/input | `PlayerMotor.cs`, `Assets/Input/RelayControls.inputactions` |
| Puzzle model | `LightsOutBoard.cs` (pure C#), `LightsOutController.cs`, `PressurePlate.cs` |
| Presentation | `SwitchStyle.cs`, two style assets, 2D URP lights, Animator and particle effects |
| Goal and scenes | `ExitGate.cs`, `SceneLoader.cs` |
| UI/persistence | `SceneUI.cs`, `RelayRunClock.cs`, `ResetHoldController.cs`, `PauseManager.cs` |
| Audio | `AudioDirector.cs`, `AudioSettingsPanel.cs`, `RelayMixer.mixer` |
| Procedural effect | `SeededSignal.cs` (Perlin/noise signal trace) |

## Redistribution and audio differences

The three third-party audio recordings from the original release were **not** copied here: `MusMus-BGM-134.ogg`, `MusMus-BGM-172.ogg`, and `PuzzleClear.wav`. Their redistribution or prior instructor-consent status was unresolved. The current `AudioDirector` source can synthesize a music loop and an unlock cue when those files are absent, but it will log warnings, and this version's audio has **not been playtested**. The old Windows and Unity Play binaries are not proof of the migrated version.

The exact previously modified `Assets/Font/DotGothic16-Regular.ttf` could not be transferred through this GitHub connection because it is larger than the GitHub Contents API's inline file limit. The original `.meta` and OFL license are retained. Restore the exact modified file before importing the source. The root Apache-2.0 license does not replace the font's separate SIL OFL 1.1 terms.

**Original source:** [reimagined-fishstick / cpsc386-project2-release](https://github.com/monocap-tech/reimagined-fishstick/tree/cpsc386-project2-release), reference snapshot `dfe65d908a0b7a8e90451b3fdd93428d31aafd74`.

**Previously validated build:** [Relay Yard on Unity Play](https://play.unity.com/en/games/9dd35762-b4d6-48e1-8354-5b90646dbff1/relayyard-web) — points to the prior build, not this altered migration.
