#!/bin/sh
set -e

mkdir -p /app/data
chown -R accountbox:accountbox /app/data

exec su-exec accountbox "$@"