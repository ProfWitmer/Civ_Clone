# Packaging (macOS)

## Build Script
The build script uses Unity batchmode and an editor build method.

### Requirements
- Unity installed (same version as the project).
- `UNITY_PATH` environment variable set to the Unity executable.

Example:
```bash
export UNITY_PATH="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"
```

### Build
```bash
scripts/build-mac.sh
```

### Optional Overrides
- `PROJECT_PATH`: project root (defaults to current directory).
- `OUTPUT_PATH`: output `.app` path (defaults to `Build/mac/CivClone.app`).

## Notes
- Ensure the correct scenes are enabled in Build Settings.
- Build output is a macOS `.app` bundle.
