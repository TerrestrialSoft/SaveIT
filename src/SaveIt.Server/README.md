# SaveIt - Server

## 🐋 Docker setup

### Build the image

```bash
docker build -f .\SaveIt.Server.UI\Dockerfile -t saveit-server:latest .
```

### Run the container

```bash
docker run -d -p 5001:8080 -p 5002:8081 --name saveit-server --rm saveit-server:latest
```