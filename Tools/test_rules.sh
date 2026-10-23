#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
UNITY_MONO="${UNITY_MONO:-/Applications/Unity/Hub/Editor/6000.2.2f1/Unity.app/Contents/MonoBleedingEdge/bin}"
TEST_OUT="$(mktemp -d /tmp/hexbound-rules.XXXXXX)"
"$UNITY_MONO/mcs" -out:"$TEST_OUT/rules.exe" Assets/Scripts/Runtime/DuelModel.cs Assets/Scripts/Runtime/RuleChecks.cs Tools/RuleCheckRunner.cs
"$UNITY_MONO/mono" "$TEST_OUT/rules.exe"
