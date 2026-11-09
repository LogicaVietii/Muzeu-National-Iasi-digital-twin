
# MuseumTwin VR — Iași Museum Digital Twin

An immersive Virtual Reality application that allows visitors to explore a digital reconstruction of a museum in Iași, interact with exhibits, and discover their historical significance.

## Project Information

- **Team member:** Rotariu David
- **Course:** Immersive / Mixed Reality
- **Platform:** Meta Quest 2
- **Engine:** Unity 6
- **Programming Language:** C#
- **Status:** Prototype Development

## Project Documentation

### Tema 4 — Project Specification

The initial project definition includes:
- Application description and objectives
- Comparison with similar applications
- Main features and user journey
- Visual moodboard
- Technologies and libraries

[View Project Specification — Google Drive](https://docs.google.com/document/d/1n_E37jPjphMbCteOcuHnphUFih9jZDZrQpTYZmo6D_Y/edit?tab=t.0)

### Tema 5 — Main Modules

This assignment focuses on defining the main modules and user interactions of MuseumTwin VR.

The document includes:
- User Story Map with goals, activities, and features
- Three User Journey Maps
- Eight low-fidelity storyboard sketches
- Asset creation plan for 3D models, UI, sounds, and VFX

[View Tema 5 — Main Modules (Google Drive)](https://docs.google.com/document/d/1f1UZ04Dr9kMBAhLeHXH1Y_np9kXkKPVA/edit?usp=drivesdk)

### Tema 6 — Prototype Environment & Interactive Exhibits

This assignment focuses on building a functional museum prototype in Unity, importing historical exhibit models, and experimenting with VR interactions.

**Objectives:**
- Create a graybox prototype of the museum environment.
- Introduce 3–4 representative historical exhibits.
- Implement basic VR interactions for the exhibits.
- Design educational interactions that reveal interesting or unexpected information about historical objects.
- Test object scale, navigation, and interactions in VR.

**Completed:**

- Created a graybox museum room using Unity primitives.
- Added walls, a floor, an entrance, and exhibit pedestals.
- Configured XR Origin and XR Device Simulator.
- Configured Near-Far Interactors for controller-based interaction.
- Created and imported four approximate low-poly historical exhibit models.
- Added colliders and XR Simple Interactable components.
- Configured exhibit selection events.
- Created world-space information panels.
- Implemented showing and hiding exhibit descriptions through XR selection.
- Tested basic exhibit interaction in the XR Device Simulator.

**Current limitations:**

- The museum currently contains one prototype exhibition room.
- The 3D models are approximate reconstructions, not accurate historical scans.
- Basic exhibit selection is implemented, but advanced educational interactions are still planned.
- Real headset testing and performance optimization remain to be completed.

## Project Description

MuseumTwin VR aims to create an immersive digital reconstruction of a museum located in the Palace of Culture in Iași.

Visitors will be able to navigate through virtual exhibition rooms, interact with historical artifacts, and access educational information about each exhibit.

The long-term goal is to reconstruct as much of the museum as possible while maintaining a comfortable VR experience.

## Main Features

- Immersive 3D museum reconstruction
- VR navigation using controllers
- Interactive historical exhibits
- Information panels with historical descriptions
- Navigation between exhibition rooms
- Realistic lighting, materials, and textures
- Educational interactions with selected exhibits

## Current Prototype

### Museum Environment

The current Unity scene is named:

`MuseumPrototype_4Exhibits`

It contains:
- One graybox museum room
- Walls and floor
- Four exhibit pedestals
- Four imported historical exhibit models
- XR Origin and XR Device Simulator
- Controller-based interactions
- Information panels for the exhibits

### Historical Exhibits

Four approximate 3D models have been created for the prototype.

#### 1. Pocket Sundial (1591)

A historical portable sundial representing an early method of measuring time.

**Implemented:**
- Low-poly 3D model
- Collider and XR Simple Interactable
- Basic information panel

**Planned special interaction:**
The visitor will be able to manipulate a virtual light source to understand how the sundial measures time through shadows.

#### 2. Medieval Monoxyl Boat

A historical boat carved from a single tree trunk.

**Implemented:**
- Low-poly 3D boat model
- Collider and XR Simple Interactable
- Basic information panel

**Planned special interaction:**
The visitor will be able to switch between a tree trunk and the reconstructed boat, visualizing how such a vessel could have been created.

#### 3. Prehistoric Ceramic Vessel

A ceramic vessel inspired by prehistoric archaeological discoveries.

**Implemented:**
- Low-poly 3D ceramic model
- Collider and XR Simple Interactable
- Basic information panel

**Planned special interaction:**
The visitor will reconstruct a broken ceramic vessel by grabbing fragments and placing them in their correct positions.

#### 4. Mammoth Skull

An approximate 3D representation of a prehistoric mammoth skull.

**Implemented:**
- Low-poly 3D skull model
- Collider and XR Simple Interactable
- Basic information panel

**Planned special interaction:**
Selecting the skull will reveal a full-scale 3D reconstruction of a mammoth, allowing visitors to understand its real dimensions.

### VR Interaction System

The prototype uses Unity's XR Interaction Toolkit.

**Implemented components:**
- XR Origin (XR Rig)
- XR Interaction Manager
- XR Device Simulator
- Near-Far Interactor
- XR Simple Interactable
- Object Colliders
- World Space Canvas
- TextMesh Pro

**Basic interaction flow:**

1. The visitor approaches an exhibit.
2. The visitor points a controller toward the object.
3. The visitor selects the exhibit.
4. An information panel appears.
5. The visitor reads the description.
6. The panel can be hidden, allowing exploration to continue.

## Technologies

- [Unity 6](https://unity.com/)
- [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.3/manual/index.html)
- [OpenXR](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.15/manual/index.html)
- [Blender](https://www.blender.org/)
- C#
- TextMesh Pro
- Git & GitHub

## Development Progress

This section will be updated after each laboratory session.

| Week | Planned Work | Status |
|---|---|---|
| 1 | Repository and project documentation | Completed |
| 2 | XR configuration and simulator setup | Completed |
| 3 | Museum graybox environment | Initial room completed |
| 4 | Materials, textures, and lighting | Planned |
| 5 | Import historical 3D exhibits | 4 models imported |
| 6 | Exhibit interactions | Basic interactions implemented |
| 7 | Information panels | Basic panels implemented |
| 8 | Additional museum rooms | Planned |
| 9 | VR testing and optimization | Pending |
| 10 | Final presentation | Planned |

## Weekly Updates

### Week 1 — Project Initialization

**Completed:**
- Defined the project concept.
- Created a public GitHub repository.
- Prepared the initial project specification.
- Established the main project requirements.

### Tema 5 — Main Modules and Graybox

**Completed:**
- Defined the application's main modules.
- Created User Story Maps and Journey Maps.
- Designed low-fidelity storyboards.
- Prepared an asset creation checklist.
- Configured the Unity XR environment.
- Built the initial museum graybox.
- Added three primitive exhibits and pedestals.
- Tested the first interactive information panel.

### Tema 6 — Interactive Museum Exhibits

**Completed:**
- Created four approximate historical 3D models.
- Imported the models into Unity.
- Positioned the exhibits on museum pedestals.
- Added a fourth pedestal.
- Added Box Colliders to the exhibit objects.
- Configured XR Simple Interactable components.
- Created individual exhibit information panels.
- Connected exhibit selection events to information panels.
- Tested basic selection and information display in the simulator.

**Next Steps:**
- Implement special educational interactions for the exhibits.
- Add more accurate exhibit descriptions and references.
- Improve exhibit positioning and visual presentation.
- Extend the museum environment.
- Test the prototype on Meta Quest 2.
- Optimize the application for standalone VR.

## Future Improvements

- More detailed museum architecture
- Accurate historical exhibit models
- Interactive ceramic reconstruction puzzle
- Sundial lighting simulation
- Mammoth reconstruction visualization
- Medieval boat transformation demonstration
- Additional museum rooms
- Improved UI and visual effects
- Performance optimization for Meta Quest 2

## Repository

https://github.com/LogicaVietii/Muzeu-National-Iasi-digital-twin

## Author

**Rotariu David**

Immersive / Mixed Reality Project
