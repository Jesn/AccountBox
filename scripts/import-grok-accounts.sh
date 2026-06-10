#!/usr/bin/env bash

set -euo pipefail

INPUT_DIR="${INPUT_DIR:-/Users/darren/Downloads/grok_register (3)/output}"
API_URL="${API_URL:-http://192.168.2.200:5095/api/external/accounts}"
WEBSITE_ID="${WEBSITE_ID:-6}"
TAGS="${TAGS:-}"
NOTES="${NOTES:-捡的}"
API_KEY="${API_KEY:-}"
CURL_NO_PROXY="${CURL_NO_PROXY:-*}"
DRY_RUN=0

usage() {
  cat <<'USAGE'
Usage:
  API_KEY='YOUR_API_KEY' ./scripts/import-grok-accounts.sh

Options:
  --api-key VALUE       外部 API Key，也可以使用环境变量 API_KEY
  --input-dir PATH      输入目录，默认 /Users/darren/Downloads/grok_register (4)/output
  --api-url URL         导入接口地址，默认 http://192.168.2.200:5095/api/external/accounts
  --website-id ID       网站 ID，默认 6
  --tags VALUE          tags 字段，默认空字符串
  --notes VALUE         notes 字段，默认 捡的
  --dry-run             只解析和打印待导入账号，不发送请求
  --curl-no-proxy VALUE curl 绕过代理规则，默认 *，如需走系统代理可设为空字符串
  -h, --help            显示帮助

Input format:
  username---password---token
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --api-key)
      API_KEY="${2:-}"
      shift 2
      ;;
    --input-dir)
      INPUT_DIR="${2:-}"
      shift 2
      ;;
    --api-url)
      API_URL="${2:-}"
      shift 2
      ;;
    --website-id)
      WEBSITE_ID="${2:-}"
      shift 2
      ;;
    --tags)
      TAGS="${2:-}"
      shift 2
      ;;
    --notes)
      NOTES="${2:-}"
      shift 2
      ;;
    --dry-run)
      DRY_RUN=1
      shift
      ;;
    --curl-no-proxy)
      CURL_NO_PROXY="${2:-}"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

if [[ ! -d "$INPUT_DIR" ]]; then
  echo "Input directory not found: $INPUT_DIR" >&2
  exit 1
fi

if [[ "$DRY_RUN" -ne 1 && -z "$API_KEY" ]]; then
  echo "API_KEY is required. Example: API_KEY='YOUR_API_KEY' $0" >&2
  exit 1
fi

if ! command -v python3 >/dev/null 2>&1; then
  echo "python3 is required to safely generate JSON payloads." >&2
  exit 1
fi

success_count=0
failed_count=0
skipped_count=0
processed_count=0

while IFS= read -r -d '' file; do
  processed_count=$((processed_count + 1))

  line="$(tr -d '\r\n' < "$file")"
  if [[ -z "$line" ]]; then
    echo "SKIP empty file: $file" >&2
    skipped_count=$((skipped_count + 1))
    continue
  fi

  rest="${line#*---}"
  if [[ "$rest" == "$line" ]]; then
    echo "SKIP invalid format: $file" >&2
    skipped_count=$((skipped_count + 1))
    continue
  fi

  username="${line%%---*}"
  password="${rest%%---*}"
  token="${rest#*---}"

  if [[ "$token" == "$rest" || "$token" == *---* || -z "${username:-}" || -z "${password:-}" || -z "${token:-}" ]]; then
    echo "SKIP invalid format: $file" >&2
    skipped_count=$((skipped_count + 1))
    continue
  fi

  payload="$(
    WEBSITE_ID="$WEBSITE_ID" \
    USERNAME="$username" \
    PASSWORD="$password" \
    TAGS="$TAGS" \
    NOTES="$NOTES" \
    TOKEN="$token" \
    python3 - <<'PY'
import json
import os

payload = {
    "websiteId": int(os.environ["WEBSITE_ID"]),
    "username": os.environ["USERNAME"],
    "password": os.environ["PASSWORD"],
    "tags": os.environ["TAGS"],
    "notes": os.environ["NOTES"],
    "extend": json.dumps({"token": os.environ["TOKEN"]}, ensure_ascii=False),
}

print(json.dumps(payload, ensure_ascii=False))
PY
  )"

  if [[ "$DRY_RUN" -eq 1 ]]; then
    echo "DRY-RUN $processed_count: $username"
    continue
  fi

  response_file="$(mktemp)"
  http_code="$({
    curl --noproxy "$CURL_NO_PROXY" -sS -o "$response_file" -w '%{http_code}' -X POST "$API_URL" \
      -H 'Content-Type: application/json' \
      -H "X-API-Key: $API_KEY" \
      -d "$payload"
  } || true)"

  if [[ "$http_code" =~ ^2[0-9][0-9]$ ]]; then
    echo "OK $processed_count: $username"
    success_count=$((success_count + 1))
  else
    echo "FAIL $processed_count: $username HTTP $http_code" >&2
    cat "$response_file" >&2
    echo "" >&2
    failed_count=$((failed_count + 1))
  fi

  rm -f "$response_file"
done < <(find "$INPUT_DIR" -type f -name '*.txt' -print0 | sort -z)

echo ""
echo "Import finished. processed=$processed_count success=$success_count failed=$failed_count skipped=$skipped_count"

if [[ "$failed_count" -gt 0 ]]; then
  exit 1
fi