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

This application was created with autonomous vehicle training in mind.

It presents the user with a friendly interface to create a simulated vehicle on a simulated track. 
The user then receives images from vehicle's cameras, which can be used as training data for the vehicle.

The simulator was written in C# in Unity, the web server is running on Flask written in python, and the interface is a Flask template.
For more details check the modules' subdirectiories: [server](https://github.com/autonomous-vehicle-sim/Simulator/tree/main/server), [simulator](https://github.com/autonomous-vehicle-sim/Simulator/tree/main/Car%20Simulator).

## Usage

There are a few ways of running our solution:
- Through the main .exe file (the built .exe file opens a main menu scene with scene slection)
- Running the server through the .bat file

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
