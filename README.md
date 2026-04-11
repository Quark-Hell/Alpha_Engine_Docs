# Alpha Engine Documentation

<p align="center">
      <img src="https://i.ibb.co/LprNYRR/Alpha-Engine-2.png" alt="Alpha-Engine-Logo" border="0">
</p>

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Automated documentation site for **Alpha Engine** — a high-performance C# 9 project.

This repository contains the documentation infrastructure and generated docs for [Alpha_Engine](https://github.com/Quark-Hell/Alpha_Engine).

## Overview

The project automatically generates and hosts up-to-date technical documentation using **Doxygen**, served via **Nginx**. The entire stack runs in Docker containers for easy deployment and development.

### Screenshots

<div align="center">

**General View**  
<img src="https://i.ibb.co/Kz7M3ypC/General.png" alt="General" width="80%" style="margin: 8px; border-radius: 8px;">

**Class Graph**  
<img src="https://i.ibb.co/wNwcvJc6/Graph.png" alt="Graph" width="80%" style="margin: 8px; border-radius: 8px;">

**Description Page**  
<img src="https://i.ibb.co/939R1BSs/Description.png" alt="Description" width="80%" style="margin: 8px; border-radius: 8px;">

</div>

## Tech Stack

- **Documentation Generator**: Doxygen
- **Web Server**: Nginx
- **Database**: PostgreSQL + pgAdmin
- **.NET**: C# 9 + .NET Aspire (for orchestration)
- **Containerization**: Docker Compose

## Services

| Service     | Description                    | Port     |
|-------------|--------------------------------|----------|
| **Site**    | Nginx documentation website    | `8080`   |
| **Postgres**| Database                       | `5432`   |
| **pgAdmin** | Database management            | `8081`   |
| **Generator**| Doxygen documentation builder | —        |

## Quick Start

```bash
git clone https://github.com/Quark-Hell/Alpha_Engine_Docs.git
cd Alpha_Engine_Docs

docker compose up -d
