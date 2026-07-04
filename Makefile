PG_BIN := /opt/homebrew/opt/postgresql@17/bin
NODE_BIN := /Users/nikhil/.nvm/versions/node/v24.18.0/bin
export PATH := $(NODE_BIN):$(PG_BIN):$(PATH)

API_DIR := backend/Current.Api
UI_DIR := frontend/current-ui

.PHONY: db-up db-down db-create migrate api ui dev build build-ui

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

build-ui:
	cd $(UI_DIR) && npm run build

ui:
	cd $(UI_DIR) && npm start

dev: db-up migrate api
