# Release Checklist (macOS)

1. Pull latest main.
2. Verify Editor tests pass (Test Runner > EditMode).
3. Validate scenario selection works and events fire for the chosen scenario.
4. Build macOS app:
   ```bash
   export UNITY_PATH="/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity"
   scripts/build-mac.sh
   ```
5. Launch the built app and smoke test:
   - Create a city, end turns.
   - Combat against rival unit and city.
   - Open scenario panel and switch scenario.
6. Confirm save/load with F5/F9 works.
7. Archive build output (zip `.app`).
