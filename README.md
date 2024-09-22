# Xsamf

Xsamf (e**X**tenable **S**ystem and **A**pplication **M**onitoring **F**ramework) is a framework/tool for building monitoring system.

## Monitoring types

### Dead man's switch

Dead man's switch monitoring involves a monitor calling back at regular intervals.

### Activities

### Probes

## Incidents

Incidents can be generated from all monitoring types and represent a issue with an entity.

### Incident hashing

Incidents use a deterministic hash to identify them.
This means that if a incident is reported with a hash that matches a pre-existing open incident,
it will be classed as the same incident.

This is intended to reduce noise in the system while providing flexibility to implementations.

It reduces noise by making it possible to now generate multiple incidents from the same being reported multiple times.
It provides flexibility because this can be easily bypassed by providing a hash that will always be random (such as a `UUID`).

It is upto the implementation 