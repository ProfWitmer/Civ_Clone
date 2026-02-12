#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${UNITY_PATH:-}" ]]; then
  echo "UNITY_PATH is not set. Example:" >&2
  echo "  export UNITY_PATH=\"/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity\"" >&2
  exit 1
fi

PROJECT_PATH="${PROJECT_PATH:-$(pwd)}"
OUTPUT_PATH="${OUTPUT_PATH:-$PROJECT_PATH/Build/mac/CivClone.app}"

"$UNITY_PATH" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -executeMethod CivClone.Editor.BuildMac.Build \
  -buildOutput "$OUTPUT_PATH"

echo "Build completed: $OUTPUT_PATH"
