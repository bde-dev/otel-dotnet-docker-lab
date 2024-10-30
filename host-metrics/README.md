# Overview
Runs node-exporter and cadvisor to expose host and container performance related metrics for Prometheus to scrape.

See observability/README.md for starting a Grafana stack that includes Prometheus to scrape these metrics.

# Running the demo
On a Linux host:

docker compose up -d
