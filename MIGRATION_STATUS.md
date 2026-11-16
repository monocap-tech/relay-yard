# Migration verification and outstanding work

| Item | State | Notes |
|---|---|---|
| Destination | Staging branch | `monocap-tech/relay-yard` / `migration/relay-yard-source` |
| Source snapshot | Fixed | `monocap-tech/reimagined-fishstick` release tree `dca236c1f5beea1c5d203bc3459193d91358f3b3` |
| Unity text files | Copied | Scenes, C# source, animations, data, URP settings, project settings, manifest and `.meta` |
| PNG textures | Copied | `Square.png` and `ResetRing.png` |
| Modified font | **Missing** | Restore exact `Assets/Font/DotGothic16-Regular.ttf`; keep committed `.meta` GUID |
| External music | Intentionally omitted | `MusMus-BGM-134.ogg`, `MusMus-BGM-172.ogg`, and their `.meta` files |
| External clear cue | Intentionally omitted | `PuzzleClear.wav` and its `.meta` file |
| Audio fallback | Code present, **untested** | Existing `AudioDirector.cs` synthesizes music/cues when external clips are absent |
| New Unity import | **Untested** | Open with Unity `6000.6.4f1` after restoring font |
| New Windows/Web builds | **Not produced** | Previous validated binaries were built from the original release, not this branch |
| License and instructor consent | **Not fully resolved** | SIL OFL license retained; prior instructor consent not established |
| Documentation | Migration-specific | Older reports discuss imported BGM/jingle and must be revised before submission |

## Restoring the exact modified font

From a local checkout of the original repository, copy `Assets/Font/DotGothic16-Regular.ttf` from its `cpsc386-project2-release` branch to the same path in this repository, without replacing the existing `.meta` file. It is the modified font, not necessarily byte-identical to upstream DotGothic16. Its license text is in `licenses/DotGothic16-OFL.txt`.

A possible local command, run **inside the new repository** when a checkout of the old repository is available at `../reimagined-fishstick`:

```bash
git -C ../reimagined-fishstick show cpsc386-project2-release:Assets/Font/DotGothic16-Regular.ttf > Assets/Font/DotGothic16-Regular.ttf
```

Review instructor permissions separately before redistribution. Reimport, run the complete gameplay/audio regression, rebuild both targets, and then merge/promote to `main`. Do not treat an earlier binary as validation of this modified source.

The previous source-specific grading guide, submission checklist and MLA report have not been copied unchanged, because their claims about included recordings would be inaccurate for this migration.
