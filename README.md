# Temperance.Ephemeris
An ephemeris is a table or data set that gives the calculated positions (coordinates) of celestial bodies, 
such as the Sun, Moon, planets, asteroids, and satellites, at specific, regular intervals of time.


The Ephemeris project serves as the foundational Data Access Layer (DAL) for the entire Temperance system. 
Its sole responsibility is the persistent storage, retrieval, and structural modeling of data, keeping it entirely
agnostic of business logic, trading strategy, or message queues

Core Purpose
To provide a clean, robust, and highly optimized interface for all microservices to interact with the SQL Server Historical Database.

Architectural Principles (Separation of Concerns)
Data Purity: This library must not contain external API logic, rate limiters, HTTP clients, or RabbitMQ messaging.

Schema Ownership: Ephemeris owns the schema (e.g., [Prices].[StockSymbol_Interval]) and the logic to query it.

Bounded Contexts: Data is segregated into specialized domains, preventing the creation of a fragile "God Repository."