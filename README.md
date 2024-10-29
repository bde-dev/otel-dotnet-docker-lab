# grafana-stack
Piecing together a Grafana Labs stack for monitoring distributed systems.

# Overview
This repository is split into two sections:

| Component      | Description                              |
|----------------|------------------------------------------|
| host-metrics   | node-exporter and cadvisor exports hosts metrics to observability. Runs on Linux hosts. |
| observability  | A boxed demo for collecting and visualizing Open Telemetry data exported by a .NET 8.0 application. |
