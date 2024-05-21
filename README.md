# Autonomous Vehicle Simulator

Unity simulator with a web-based interface intended for real-life vehicle training.

## Table of contents

- [Description](#description)
- [Usage](#usage)
- [Project structure](#project-structure)
- [Dependencies and requirements](#dependencies-and-requirements)
  - [Unity simulator requirements](#unity-simulator-requirements)
  - [Web server dependencies](#web-server-dependencies)

## Description

*brief simulator description, use cases, motivation, etc.*

## Usage

*here explain the different methods for running the simulator/server - from main menu, from a file, etc.*

## Project structure

- `Car Simulator/` - Unity simulator files
- `docs/` - project documentation
- `resources/` - graphics project files
- `server/` - simulator's API, web interface

## Dependencies and requirements

### Unity simulator requirements

- 64-bit version of Windows 7/8.1/10/11
- x64 CPU architecture with SSE2 support

For more details visit the official [Unity documentation](https://docs.unity3d.com/Manual/system-requirements.html)

### Web server dependencies

```
flask==3.0.2
click==8.1.7
websockets~=12.0
waitress==3.0.0
flask-restx==1.3.0
SQLAlchemy~=2.0.29
flask_sqlalchemy==3.1.1
requests~=2.31.0
```
