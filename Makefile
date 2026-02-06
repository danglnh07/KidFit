db-local-init:
	docker run --name postgres -p 1433:1433 -e "POSTGRES_USER=admin" -e "POSTGRES_PASS=12345" -d postgres:18.1-alpine3.22
db-local-start:
	docker start postgres
