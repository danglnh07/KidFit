db-local-init:
	docker run --name postgres -p 5432:5432 -e "POSTGRES_USER=admin" -e "POSTGRES_PASSWORD=12345" -d postgres:18.1-alpine3.22
db-local-start:
	docker start postgres
