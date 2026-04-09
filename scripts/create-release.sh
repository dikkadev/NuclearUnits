#!/usr/bin/env bash

set -euo pipefail

usage() {
  echo "Usage: $0 [--dry-run] <tag>" >&2
  echo "Example: $0 v0.1.0" >&2
}

DRY_RUN=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run|-n)
      DRY_RUN=1
      shift
      ;;
    -* )
      echo "Unknown option: $1" >&2
      usage
      exit 1
      ;;
    *)
      break
      ;;
  esac
done

if [[ $# -ne 1 ]]; then
  usage
  exit 1
fi

TAG="$1"
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd)"
DIST_DIR="$REPO_ROOT/dist"
STAGE_ROOT="$DIST_DIR/stage"
PACKAGE_ROOT="$STAGE_ROOT/NuclearUnits"
ARCHIVE_PATH="$DIST_DIR/NuclearUnits-${TAG}.7z"

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing required command: $1" >&2
    exit 1
  fi
}

require_command dotnet
require_command gh
require_command 7z

run() {
  if [[ "$DRY_RUN" -eq 1 ]]; then
    printf '[dry-run] '
    printf '%q ' "$@"
    printf '\n'
    return 0
  fi

  "$@"
}

if [[ ! -f "$REPO_ROOT/README.md" ]]; then
  echo "Missing README.md. Add project documentation before publishing a release." >&2
  exit 1
fi

run mkdir -p "$DIST_DIR"
run rm -rf "$STAGE_ROOT"
run mkdir -p "$PACKAGE_ROOT"

echo "Building Release..."
run dotnet build "$REPO_ROOT/NuclearUnits.csproj" -c Release

run cp "$REPO_ROOT/bin/Release/netstandard2.1/NuclearUnits.dll" "$PACKAGE_ROOT/NuclearUnits.dll"
run cp "$REPO_ROOT/README.md" "$PACKAGE_ROOT/README.md"

run rm -f "$ARCHIVE_PATH"

echo "Packaging $ARCHIVE_PATH..."
if [[ "$DRY_RUN" -eq 1 ]]; then
  printf '[dry-run] %q %q %q %q > /dev/null\n' 7z a -t7z "$ARCHIVE_PATH" "$PACKAGE_ROOT"
else
  7z a -t7z "$ARCHIVE_PATH" "$PACKAGE_ROOT" >/dev/null
fi

echo "Creating GitHub release $TAG..."
run gh release create "$TAG" "$ARCHIVE_PATH" --title "$TAG" --generate-notes

if [[ "$DRY_RUN" -eq 1 ]]; then
  echo "Dry run complete."
  echo "Archive would be: $ARCHIVE_PATH"
else
  echo "Release created: $TAG"
  echo "Archive: $ARCHIVE_PATH"
fi
