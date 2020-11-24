# Animal API

Creates dogs, retrieves cats and dogs. Displays a different way to approach design.

## Used technologies

- MongoDB
- IntegrationTests
- NUnit
- MemoryCache

## Running

- `docker-compose up` (this spins up a mongodb database)
- `dotnet test`

You need a running mongodb database for the tests; I've only written integration tests with actual database integration.

## Accessing the application

- https://127.0.0.1:5001/swagger/v1/swagger.json
- https://127.0.0.1:5001/api-docs/index.html
