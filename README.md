# Rebyu
![image](https://github.com/user-attachments/assets/5403c49d-1582-40b7-9ad6-1fc79323ede2)

Rebyu is an intelligent SQLite viewer application designed to enable seamless, natural interaction with your databases through a multi-agent system.
At its core, Rebyu performs three key functions:
- **Schema-Aware Chat Initiation**: Rebyu starts by analysing your current database schema.
- **Autonomous Request Routing**: Rebyu interprets the user's intent and silently routes the request to the correct internal agent.

![Rebyu-AI-Agents](https://github.com/user-attachments/assets/e019c5ad-5e40-49da-97a0-e844214faf6a)

- **Natural Language to SQL Translation**: A dedicated NL2SQL Agent translates user queries from plain English into precise, SQLite-compatible SQL using only the validated database schema.

Perfect For:
1. Developers needing a quick, AI-enhanced way to explore SQLite databases.
2. Analysts who prefer natural language over SQL syntax.
3. Anyone who wants accurate, schema-safe SQL queries without writing code.

## Dependencies
1. .NET 9
2. Semantic Kernel
3. Avalonia UI

## How to run
1. Clone this repository
2. Set your Openai API key as an environment variable
  ```bash
   setx OPENAI_API_KEY "<YOUR-API-KEY>"
   ```
4. Build and run the project.
