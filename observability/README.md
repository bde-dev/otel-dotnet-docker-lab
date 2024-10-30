# Observability
Piecing together a Grafana Labs stack for monitoring distributed systems.

# Requirements
## Environment
The demo has worked in the following environments:
1. Windows / Linux desktop workstation
2. Ubuntu server 22.04

When exploring endpoints, be sure to use the correct hostname, i.e. if using a local desktop environment, a web browser can be opened locally to access components using localhost. If using a remote headless Ubuntu server to run the demo, be sure to enter the IP address of the remote host instead of localhost.

> The rest of this document will assume the demo is running on a
> workstation, therefore all subsequent references to "the host running
> the demo" will be written as localhost.

## Docker
The demo uses `docker` and `docker compose` - ensure they are installed.

### All Platforms
Run the demo on a Swarm Manager host

    docker swarm init

### Linux
Ensure the following properties are set in the daemon config:

    {
      "metrics-addr" : "0.0.0.0:9323",
      "experimental" : true,
      "hosts": ["unix:///var/run/docker.sock", "tcp://0.0.0.0:2375"] # WARNING: Can be a security issue
    }

### Windows
Expose the TCP socket:

1. Open Docker Desktop GUI
2. Settings > General
3. Tick the `Expose daemon on tcp://localhost:2375 without TLS` checkbox
4. Quit Docker Desktop and start it again

> WARNING: This is a pretty serious security vulnerability - do not
> under any circumstances leave these configs enabled on hosts that are
> accessible from the internet, and do not set them at all in production
> environments. Remove / undo these configs once finished with the demo.

TLS integration further down the line.

# Running the demo
 
    docker compose up -d

This will start the observability stack. For a full experience of the demo, it is recommended to run `host-metrics` on at least one other Linux host and update `targets.json` with its IP address so it can be scraped by `Prometheus`.

> See Bonus Experimentation for more detailed instructions.

One Linux host `192.168.252.66` has been pre-configured in `config/prometheus/` for demonstrative purposes.

> Please be aware this host may not be running or may not be running the
> `host-metrics` apps at any given time.

# Exploring
## Front End
### Grafana
http://localhost:3000

    credentials:
    username: admin
    password: admin

Upon entering credentials to log in, skip updating the password with the "skip" button.

go to Dashboards to see a list of compiled community dashboards visualizing various metrics related to components of the system.

App telemetry data in `Prometheus`, `Loki` and `Tempo` can be viewed in the Explore section.

### WeatherApp
http://localhost:8080

Press the "Weather" button on the side panel to start a trace and increment the `forecast_counter` meter by 1.

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

# System Overview
## Telemetry Flow

insert-image


# Bonus experimentation
With a bit of configuration to `Prometheus`, host-metrics can be run on Linux hosts so `Prometheus` can scrape them.

See `host-metrics` README.md for instructions on running that demo.

In `config/prometheus` update `targets.json` to include the IP address(es) and corresponding port(s).

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

> This scrape job has been configured to refresh every 5 seconds.

# Known quirks
## otel-collector
Sometimes the `health_check` endpoint doesn't work.

This can be resolved by bringing the down the compose apps, pruning the docker volumes, then starting the compose apps again.

    docker compose down
    docker volume prune
    docker compose up -d

http://localhost:13133 should work again.
