# FilesApi
## Description
Web application for file storage. Uses ASP.NET and MongoDB for the backend and React for the frontend.

- FilesApi: Web API code
- files-frontend: React app that consumes the API.

## Deployment
Docker images for the API and the frontend are available in Docker Hub (linux/amd64 architecture). To deploy a local version of both the API and the frontend, create the following docker-compose.yaml (or download the one available):
 ```yaml
 version: '2.2'
services:
  frontend:
    container_name: files-frontend
    image: enriquebarba97/files-frontend
    networks:
      network1:
        aliases:
          - frontend
    ports:
      - '1337:80'
  backend:
    container_name: files-api
    image: enriquebarba97/files-api
    networks:
      network1:
        aliases:
          - backend
    environment: 
        - ASPNETCORE_ENVIRONMENT=Production
    ports:
      - '8014:80'
    depends_on:
      - mongodb
  mongodb:
    container_name: mongo
    image: mongo
    networks:
      network1:
        aliases:
          - mongo
    ports:
        - '27017:27017'
    volumes:
      - 'db_volume:/data/db'
networks:
  network1:
    external: false
volumes:
  db_volume: null
 ```
 
 After that, run:
 ```bash
 docker-compose up
 ```
