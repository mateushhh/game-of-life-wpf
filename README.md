# Game of Life - Ecosystem Simulation

A WPF application simulating an ecosystem of plants and animals.

## Features
- Configurable simulation parameters using numerical vectors and values.
- Interactive board editing, real-time animation with speed control, and zoom capabilities.
- Statistics tracking (alive, births, deaths).
- Save and load simulation state to/from a text file.

## Simulation Parameters
The ecosystem behavior is defined by the following parameters:
- **R**: Number of neighboring plants required for a new plant to grow.
- **W**: Number of neighboring plants required for an existing plant to survive.
- **B**: Number of neighboring animals required for a new animal to be born.
- **P**: Number of neighboring plants required for a new animal to be born.
- **Z**: Number of neighboring animals required for an animal to survive.
- **M**: Movement distance of an animal.
- **L**: Maximum lifespan of an animal.