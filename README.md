# KidFit - Modern solution for developing thinking skills in preschool students

This is the repository of the KidFit Ed platform

## Running in local

Ensure that you have `Docker` and `.NET` (>= 8.0) installed in your machine

Run the local database

```bash
make db-local-init # First time 
make db-local-start # Or run this if you already have the container setup
```

Run the application

```bash
dotnet run
```
