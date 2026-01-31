# TechAssistPro Solution Architecture

## Overview

TechAssistPro is an event-driven system designed to manage customer support ticketing and agent scheduling. It leverages a **Clean Architecture** and **Domain-Driven Design (DDD)** approach to ensure maintainability, scalability, and clear separation of concerns. Communication within services is handled by MediatR, while cross-service communication utilizes RabbitMQ for robust and asynchronous event propagation.

## Core Architectural Principles

*   **Clean Architecture**: Emphasizes separation of concerns, making the system independent of frameworks, UI, and databases. Business rules are central, with infrastructure details relegated to outer layers.
*   **Domain-Driven Design (DDD)**: Focuses on modeling the business domain precisely. Aggregates, Entities, Value Objects, and Domain Events are core constructs that reflect the ubiquitous language of the business.
*   **Event-Driven Architecture**: The system reacts to significant business events, enabling loose coupling between services and promoting asynchronous processing.

## Communication Patterns

### In-process Communication (within a service)

*   **MediatR**: Used for dispatching commands, queries, and domain events within a single service boundary. This facilitates a clear request-response or notification pattern without direct dependencies between handlers.

### Cross-service Communication

*   **RabbitMQ**: Employed as the message broker for asynchronous communication between different microservices (e.g., Ticketing and Scheduling).
    *   **Topic Exchanges**: Used for routing integration events.
    *   **Routing Keys**: Include schema version (e.g., `event.name.v{version}`).
    *   **Consumers**: Bind to exchanges using wildcards where appropriate, consuming messages based on their interests.

## Eventing Strategy

### Domain Events

*   **Nature**: Pure business facts, representing something that happened in the domain (e.g., `TicketCreatedDomainEvent`, `SupportAgentAssignedDomainEvent`).
*   **Immutability**: Domain events are immutable.
*   **Raising**: Aggregates raise domain events after persistence, which are then handled by MediatR.
*   **No Infrastructure Concerns**: Domain events should not contain any infrastructure-specific details.

### Integration Events

*   **Nature**: Immutable DTOs representing business occurrences that are relevant to other services (e.g., `TicketCreatedIntegrationEvent`, `SupportAgentAssignedIntegrationEvent`).
*   **Versioning**: Integration events are versioned (e.g., `v1`, `v2`) to allow for schema evolution. The schema version is conveyed via RabbitMQ headers.
*   **JSON Schema Validation**: All integration events are validated against predefined JSON schemas to ensure contract adherence and data integrity.
*   **Ownership**: The producing service owns the contract (schema) of the integration event.
*   **Consumer Contract**: Consumers never mutate the payload of an integration event.

## Observability

TechAssistPro prioritizes robust observability to facilitate debugging, monitoring, and understanding system behavior.

*   **Structured Logging**: Utilizes Serilog (or a similar structured logging library) for consistent and machine-readable logs.
*   **Correlation IDs**:
    *   Propagated via HTTP request headers (e.g., `X-Correlation-ID`) and then stored in an `ExecutionContext` (AsyncLocal).
    *   Included in integration event headers when publishing to RabbitMQ.
    *   Automatically added to logging scopes for tracing requests across service boundaries.
    *   **Note**: `IDomainEvent` explicitly does *not* contain a `CorrelationId`, as correlation and causation tracking are considered infrastructure concerns.
*   **Schema Validation**: Ensures all integration events conform to their contracts.
*   **Dead Letter Queues (DLQ)**: Implemented for RabbitMQ consumers to capture messages that cannot be processed successfully, enabling manual inspection and re-processing.
*   **RabbitMQ Routing Diagnostics**: Extensive logging around RabbitMQ message routing, publishing, and consumption to aid in troubleshooting messaging flow.

## Project Structure (High-Level)

The solution is organized into several projects, each with a distinct responsibility:

*   **TechAssistPro.SharedKernel**: Contains shared abstractions, domain primitives, common events (like `IDomainEvent`), and utility classes used across multiple services.
*   **TechAssistPro.Ticketing**: A microservice responsible for managing the Ticket aggregate. It raises domain events, publishes integration events (e.g., `TicketCreatedIntegrationEvent`), and subscribes to relevant events from other services (e.g., `SupportAgentAssignedIntegrationEvent`).
*   **TechAssistPro.Scheduling**: A separate microservice with its own database, managing `SupportAgent` and `Assignment` aggregates. It subscribes to `TicketCreatedIntegrationEvent` to perform intelligent auto-assignment and publishes `SupportAgentAssignedIntegrationEvent`.
*   **TechAssistPro.Infrastructure**: Houses common infrastructure concerns such as MediatR wiring, RabbitMQ publisher/subscriber implementations, schema registry, and shared messaging abstractions. Crucially, it contains no business logic.
*   **TechAssistPro.CustomerManagement**: (Details not fully defined, but expected to manage customer-related entities).
*   **TechAssistPro.Gateway**: Acts as an API Gateway, potentially handling cross-cutting concerns like authentication, rate limiting, and request routing.