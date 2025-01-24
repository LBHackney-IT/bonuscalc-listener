#!/bin/bash
set -e

host="$1"
shift
cmd="$@"

until PGPASSWORD=$DB_PASSWORD psql -h "$host" -U "$DB_USERNAME" -d "$DB_DATABASE" -c '\q'; do
  sleep 1
done

exec $cmd