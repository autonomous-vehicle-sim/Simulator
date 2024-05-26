# Autonomous Vehicle Simulator - Unity Simulator

The Unity part of the simulator. It's responsible for running the simulations. 
Upon receiving configuration or status messages through a websocket, it spawns tracks and vehicles for them to ride on it.

## Scenes

There are four scenes included in the project:
- `MainScene` - main menu scene, allows to launch other scenes
- `GameScene` - simulator scene with manual driving and data collection
- `ClientScene` - client for the web API
- `EditorScene` - track editor, allows for placing tiles and exporting to a PNG file

## Unity project structure

- `Assets/` - all of the project's assets (e.g. scripts, models, scenes, prefabs)
- `Packages/` - project packages, manifest file
- `ProjectSettings/` - Unity's project settings
