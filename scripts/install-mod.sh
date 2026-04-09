#!/usr/bin/env bash

set -euo pipefail

VERBOSE=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    -v|--verbose)
      VERBOSE=1
      shift
      ;;
    *)
      echo "Unknown option: $1" >&2
      echo "Usage: $0 [-v|--verbose]" >&2
      exit 1
      ;;
  esac
done

log_verbose() {
  if [[ "$VERBOSE" -eq 1 ]]; then
    echo "$@"
  fi
}

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd)"
ENV_FILE="${ENV_FILE:-$REPO_ROOT/.env}"

if [[ -f "$ENV_FILE" ]]; then
  log_verbose "Loading environment from $ENV_FILE"
  # shellcheck disable=SC1090
  source "$ENV_FILE"
fi

: "${STEAMAPPS_DIR:?Set STEAMAPPS_DIR in .env or the environment}"

BUILD_CONFIGURATION="${BUILD_CONFIGURATION:-Release}"
GAME_DIR="${GAME_DIR:-$STEAMAPPS_DIR/common/Nuclear Option}"
MOD_INSTALL_DIR="${MOD_INSTALL_DIR:-$GAME_DIR/BepInEx/plugins/NuclearUnits}"
BUILD_OUTPUT_DIR="$REPO_ROOT/bin/$BUILD_CONFIGURATION/netstandard2.1"

echo "Building NuclearUnits ($BUILD_CONFIGURATION)..."
dotnet build "$REPO_ROOT/NuclearUnits.csproj" -c "$BUILD_CONFIGURATION"

mkdir -p "$MOD_INSTALL_DIR"

cp "$BUILD_OUTPUT_DIR/NuclearUnits.dll" "$MOD_INSTALL_DIR/"

if [[ -f "$BUILD_OUTPUT_DIR/NuclearUnits.pdb" ]]; then
  cp "$BUILD_OUTPUT_DIR/NuclearUnits.pdb" "$MOD_INSTALL_DIR/"
fi

echo "Installed to: $MOD_INSTALL_DIR"
