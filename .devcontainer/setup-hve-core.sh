#!/usr/bin/env bash
#
# setup-hve-core.sh
#
# Best-effort, non-fatal, idempotent installation of the GitHub Copilot CLI and
# the hve-core-all plugin (https://github.com/microsoft/hve-core).
#
# This script is shared by:
#   * the Copilot cloud sandbox setup workflow
#     (.github/workflows/copilot-setup-steps.yml), and
#   * the dev container (.devcontainer/devcontainer.json).
#
# Every step is best-effort: the sandbox and dev container MUST still start even
# if the Copilot CLI or the plugin cannot be installed (for example when the
# environment is offline or the CLI has not been authenticated). Failures are
# therefore downgraded to warnings and never abort the caller.
set -euo pipefail

echo "==> [hve-core] Ensuring GitHub Copilot CLI is installed"
command -v copilot >/dev/null 2>&1 || npm install -g @github/copilot || echo "::warning::copilot CLI install failed"

echo "==> [hve-core] Adding microsoft/hve-core plugin marketplace"
copilot plugin marketplace add microsoft/hve-core || echo "::warning::marketplace add failed"

echo "==> [hve-core] Installing hve-core-all plugin"
copilot plugin install hve-core-all@hve-core || echo "::warning::hve-core-all install failed"

echo "==> [hve-core] Installed plugins:"
copilot plugin list || true

echo "==> [hve-core] Setup script complete"
