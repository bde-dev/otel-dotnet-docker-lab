# Observability
Piecing together a Grafana Labs stack for monitoring distributed systems.

# System Overview
## Components

WeatherApp "weather" button is a GET endpoint which fires an HTTP GET request to openweatherapi, then sends a message via MassTransit to a local RabbitMQ broker.

WeatherApp is configured to send the above trace to an opentelemetry-collector-contrib container, and export that trace to Tempo.

WeatherApp is configured so that when the "weather" button is pressed, a simple integer counter metric called forecast_counter is incremented, then exported to the otel-collector-contrib container, where a Prometheus container can scrape it.

WeatherApp is configured to use Serilog to export all logs to the opentelemetry-collector-contrib container, and then exported on again to Loki.

WeatherApp is configured so that the logs and traces are correlated throughout a trace. Any logs that happen on a trace (should) be linked by traceid.

Grafana is configured to visualize Prometheus, Loki and Tempo data.

# Requirements
## Environment
The demo can be run from the following environments:
1. Windows / Linux desktop workstation
2. Ubuntu server

When exploring endpoints, be sure to use the correct hostname, i.e. if using a local desktop environment, a web browser can be opened locally to access components using localhost. If using a remote headless Ubuntu server to run the demo, be sure to enter the IP address of the remote host instead of localhost.

## Docker
These configurations wil allow Prometheus to scrape the Docker Swarm.

### All Platforms
Run the demo on a Swarm Manager host: docker swarm init

### Linux
Ensure the following properties are set in the daemon config:
{
  "metrics-addr" : "0.0.0.0:9323",
  "experimental" : true,
  "hosts": ["unix:///var/run/docker.sock", "tcp://0.0.0.0:2375"]
}

### Windows
Expose the TCP socket:

1. OPen Docker Desktop GUI
2. Settings > General
3. Tick the "Expose daemon on tcp://localhost:2375 without TLS" checkbox
4. Quit Docker Desktop and start it again

WARNING: This is a pretty serious security vulnerability - do not under any circumstances leave these configs set on hosts that are accessible from the internet, and do not set them at all in production environments. Remove / undo these configs once finished with the demo.

# Running the demo

docker compose up -d

For a full experience of the demo, it is recommended to run host-metrics on at least one other Linux host and update targets.json with its IP address.
See Bonus Experimentation for more detailed instructions.

# Exploring
## Front End
### Grafana
http://localhost:3000

### WeatherApp
http://localhost:8080

## Back End
### RabbitMQ
http://localhost:15672

### Prometheus
http://localhost:9090

### otel-collector health_check
http://localhost:13133

### otel-collector zpages
http://localhost:55679/debug/servicez

### otel-collector internal metrics
http://localhost:8888/metrics

### loki metrics
http://localhost:3100/metrics

# Bonus experimentation
With a bit of configuration, host-metrics can be run on Linux hosts so Prometheus can scrape them.

See host-metrics README.md for instructions on running that demo.

In config/prometheus update targets.json to include the IP address and corresponding port.

Example:
[
    {
        "targets": [ "192.168.252.66:9323", "192.168.252.100:9323" ], # Added "192.168.252.100:9323"
        "labels": {
            "job": "docker_engine"
        }
    },
    {
        "targets": [ "192.168.252.66:9100", "192.168.252.100:9100" ],  # Added "192.168.252.100:9100"
        "labels": {
            "job": "node_exporter"
        }
    },
    {
        "targets": [ "192.168.252.66:8085", "192.168.252.100:8085" ],  # Added "192.168.252.100:8085"
        "labels": {
            "job": "cadvisor"
        }
    }
]
