PG_BIN := /opt/homebrew/opt/postgresql@17/bin
export PATH := $(PG_BIN):$(PATH)

API_DIR := backend/Current.Api

.PHONY: db-up db-down db-create migrate api dev build

db-up:
	@pg_isready -q 2>/dev/null || brew services start postgresql@17
	@$(MAKE) db-create

db-create:
	@$(PG_BIN)/psql -lqt | cut -d \| -f 1 | tr -d ' ' | grep -qx CurrentDb || $(PG_BIN)/createdb CurrentDb

db-down:
	@brew services stop postgresql@17

migrate:
	cd $(API_DIR) && dotnet ef database update

api:
	cd $(API_DIR) && dotnet watch run --launch-profile http

build:
	cd $(API_DIR) && dotnet build

dev: db-up migrate api
