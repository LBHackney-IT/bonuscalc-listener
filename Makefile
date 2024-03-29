.PHONY: setup
setup:
	docker-compose build

.PHONY: build
build:
	docker-compose build bonus-calc-listener

.PHONY: build-test
build-test:
	docker-compose build bonus-calc-listener-test

.PHONY: serve
serve: build
	docker-compose up -d bonus-calc-listener

.PHONY: shell
shell: build
	docker-compose run --rm bonus-calc-listener bash

.PHONY: test
test: test-db build-test
	docker-compose run --rm bonus-calc-listener-test

.PHONY: lint
lint:
	-dotnet tool install -g dotnet-format
	dotnet tool update -g dotnet-format
	dotnet format

.PHONY: stop
stop:
	docker-compose down

.PHONY: dev-db
dev-db:
	docker-compose up -d dev-database

.PHONY: test-db
test-db:
	docker-compose up -d test-database

.PHONY: restart-db
restart-db:
	docker stop $$(docker ps -q --filter ancestor=test-database -a)
	-docker rm $$(docker ps -q --filter ancestor=test-database -a)
	docker rmi test-database
	docker-compose up -d test-database
