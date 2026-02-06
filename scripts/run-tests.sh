#!/usr/bin/env bash
set -euo pipefail

# Usage:
#   UNITY_PATH=/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity \
#     ./scripts/run-tests.sh
#
# Optionally set:
#   TEST_PLATFORM=playmode|editmode (default: editmode)
#   RESULTS_PATH=./TestResults.xml

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_PATH="${UNITY_PATH:-/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity}"
TEST_PLATFORM="${TEST_PLATFORM:-editmode}"
RESULTS_PATH="${RESULTS_PATH:-${PROJECT_ROOT}/TestResults.xml}"

if [[ ! -x "$UNITY_PATH" ]]; then
  echo "Unity executable not found or not executable: $UNITY_PATH" >&2
  echo "Set UNITY_PATH to your Unity Editor executable." >&2
  exit 1
fi

"$UNITY_PATH" \
  -batchmode \
  -nographics \
  -projectPath "$PROJECT_ROOT" \
  -runTests \
  -testPlatform "$TEST_PLATFORM" \
  -testResults "$RESULTS_PATH" \
  -logFile -
