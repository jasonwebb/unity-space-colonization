## Requirements

* [TubeRenderer](https://assetstore.unity.com/packages/tools/modeling/tuberenderer-3281) ($5) by Sixth Sensor


## Interface

### Algorithm parameters

1. Attraction distance
2. Kill distance
3. Segment length


### Branch rendering

1. Material
2. Enable vein thickening

**When vein thickening is enabled...**

1. Minimum radius
2. Maximum radius
3. Radius increment

**When vein thickening is NOT enabled...**

1. Radius


### Attractor generation

1. Attractor placement - dropdown with the following options:
    * SPHERE
    * GRID
    * MESH

#### SPHERE options...

1. Radius
2. Attractor count

#### GRID options...

1. Dimensions
2. Resolution
3. Jitter amount

#### MESH options...

1. Target mesh
2. Raycast attempts
3. Raycasting direction

#### For all placement options...

1. Attractor gizmo radius

#### Buttons

1. Generator attractors
2. Clear


### Root node(s)

1. Type of root node(s) - dropdown with the following options:
    * INPUT
    * MESH

#### INPUT options...

1. Root node object

#### MESH options...

1. Target mesh
2. Number of root nodes


### Bounds

1. Use bounds

#### When bounds are enabled...

1. Bounding mesh


### Obstacles

1. Use obstacles

#### When obstacles are enabled...

1. Obstacle meshes


### Run controls

1. Iterations to run
2. Run button
3. Reset button


### Export

1. Filename
2. Export button