# Overview
A full stack sample that runs `grafana-stack`, a `RabbitMQ` broker and a `WeatherApp`.


# Running the sample
    docker compose up --build -d

# System Overview
## Telemetry Flow

insert-diagram from docs


# Exploring
## Front End
### WeatherApp
http://localhost:8080

Pressing the `weather` button generates metrics and correlated logs and traces which are exported to `otel-collector`.
`otel-collector` then pushes the logs to `Loki`, the traces to `Tempo`, and exposes metrics for `Prometheus` to scrape.
`Grafana` queries `Loki`, `Tempo` and `Prometheus` where the three data types can be correlated and visualised.

### Grafana
http://localhost:3000

    credentials:
    username: admin
    password: admin

Upon entering credentials to log in, skip updating the password with the "skip" button.

go to Dashboards to see a list of compiled community dashboards visualizing various metrics related to components of the system.

App telemetry data in `Prometheus`, `Loki` and `Tempo` can be viewed in the Explore section.

## Back End
### RabbitMQ
http://localhost:15672 

    credentials:
    username: guest
    password: guest

### Prometheus
http://localhost:9090

On the top banner, `Status > Targets` lists the scrape configs in `config/prometheus/prometheus.yaml` and their liveness statuses.

### otel-collector health_check
http://localhost:13133

Simple endpoint displaying app uptime in `json`.

### otel-collector zpages
http://localhost:55679/debug/servicez

WebUI displaying `otel-collector` configuration.

### otel-collector internal metrics
http://localhost:8888/metrics

### loki metrics
http://localhost:3100/metrics
