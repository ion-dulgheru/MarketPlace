#!/usr/bin/env bash
# Single responsibility: fail the CI job if any NuGet package (direct or transitive)
# has a known High or Critical vulnerability.
# Usage: bash .github/scripts/check-nuget-vulnerabilities.sh <path-to-solution-or-project>
# Requires `dotnet restore` to have been run first.
set -euo pipefail

target="${1:?Usage: check-nuget-vulnerabilities.sh <path-to-solution-or-project>}"

report="$(dotnet list "$target" package --vulnerable --include-transitive)"
echo "$report"

if grep -qE '\b(High|Critical)\b' <<< "$report"; then
  echo "::error::High or Critical NuGet vulnerabilities found. Update the packages listed above."
  exit 1
fi

echo "No High or Critical NuGet vulnerabilities found."
