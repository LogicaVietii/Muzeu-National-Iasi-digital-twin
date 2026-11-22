# MuseumTwin VR — Iași Museum Digital Twin

MuseumTwin VR is an immersive educational Virtual Reality prototype inspired by the Museum of History of Moldavia at the Palace of Culture in Iași. Visitors explore a virtual museum, inspect historical objects, and interact with educational demonstrations.

## Project Information

- **Team member:** Rotariu David
- **Course:** Immersive / Mixed Reality
- **Target platform:** Meta Quest 2
- **Engine:** Unity 6.3 LTS
- **Programming language:** C#
- **XR framework:** XR Interaction Toolkit 3.3.2 and OpenXR
- **Status:** Functional Unity prototype; final standalone Meta Quest 2 validation and performance optimization remain to be completed.

## Project Documentation

### Tema 4 — Project Specification

The initial specification defines the project concept, objectives, similar applications, user experience, visual moodboard, technologies and libraries.

[View Project Specification — Google Docs](https://docs.google.com/document/d/1n_E37jPjphMbCteOcuHnphUFih9jZDZrQpTYZmo6D_Y/edit?tab=t.0)

### Tema 5 — Main Modules

Defines the application's user story map, three user journeys, eight low-fidelity storyboards, and an asset creation plan for 3D models, UI, sounds and visual effects.

[View Main Modules — Google Docs](https://docs.google.com/document/d/1f1UZ04Dr9kMBAhLeHXH1Y_np9kXkKPVA/edit?usp=drivesdk)

### Tema 6 — Initial Prototype Environment & Interactive Exhibits

- Created a first graybox museum room with floor, walls, an entrance and exhibit pedestals.
- Configured XR Origin, XR Interaction Manager, XR Device Simulator and Near-Far Interactors.
- Created/imported four approximate low-poly historical exhibit models.
- Added colliders, XR Simple Interactable components, and world-space information panels.
- Connected exhibit selection to contextual information display.
- Tested basic selection and information display in Unity Play Mode.

### Week 9 — Functional Prototype & Presentation

Week 9 presents the **main idea, design, reason for using VR, grayboxed 3D environment, asset provenance, current functional prototype, testing conclusions and planned implementation improvements**.

**Completed for Week 9:**
- Expanded the scene to **five connected rooms** with entrance, galleries, corridors, exhibits and pedestals.
- Added decorative primitive exhibits and improved object placement.
- Implemented **View, Rotate, Zoom, Grab, Return to stand and Close** controls.
- Implemented a separate educational **Explore** experience for each of the four main exhibits.
- Improved preview heights and panel positioning to reduce interference with pedestals and walls.
- Tested navigation and interaction flows in the Unity XR Device Simulator.
- Prepared a PowerPoint presentation containing **four embedded video demonstrations** of the exhibits.
- Documented team-created, externally sourced and AI-assisted parts of the prototype.

**Week 9 presentation:** [MuseumTwin_Prezentare_Video.pptx](video/MuseumTwin_Prezentare_Video.pptx) — available in the repository's **[`video/`](video/) folder**. The PowerPoint includes clips demonstrating all four exhibits.

**Earlier prototype recording:** [tema6.mkv](video/tema6.mkv).

## Project Description & Purpose of VR

The goal is to create a virtual museum inspired by the Palace of Culture in Iași, where visitors can move between exhibitions, learn about historical objects, and experience activities that would be difficult with fragile real artifacts.

VR is used for spatial presence, intuitive controller-based exploration, visualization of objects at different scales, and engaging educational demonstrations. The current environment is an **illustrative graybox**, not a dimensionally accurate museum scan.

**Audience:** students, tourists and history enthusiasts.

## Main Features

- Walkable 3D museum with five connected spaces.
- VR controller-ray exhibit selection and contextual information panels.
- View copies of exhibits, rotate and zoom.
- Grab exhibits and return them to their original stands.
- Four independent educational Explore demonstrations.
- World-space menus with improved positioning to reduce obstruction.
- Unity XR Device Simulator support for prototype testing.

## Current Prototype

### Museum Environment

**Main working scene:** `Assets/MuseumPrototype_ViewGrab_Restored.unity`

The graybox museum contains an entrance hall, a central hall, left and right galleries, and a rear gallery, connected through passages. The scene includes walls, floors, exhibit stands, decorative objects, four imported historical models and the XR interaction rig.

The simple graybox geometry prioritizes circulation, scale, accessibility and interaction testing before detailed architectural modeling.

### Historical Exhibits and Explore Interactions

| Exhibit | Implemented educational interaction |
|---|---|
| **Pocket Sundial (1591)** | Change the simulated hour and observe how the geometrically calculated shadow moves. |
| **Medieval Monoxyl Boat** | Step through four illustrative stages from a tree trunk to the finished dugout boat. |
| **Prehistoric Ceramic Amphora** | Watch an animated shattering sequence using fragments from the **original imported amphora mesh**; the exhibit on its pedestal stays intact. |
| **Mammoth Skull** | Reveal a separate full-body mammoth reconstruction to illustrate the animal's general proportions and size. |

The models and demonstrations are simplified educational approximations, not historical scans or scientifically validated simulations.

### VR Interaction Flow

1. Navigate to an exhibit and select it using an XR controller ray.
2. Read contextual information in the world-space menu.
3. Select **View** to inspect an exhibit copy with rotation and zoom.
4. Select **Grab** to manipulate the exhibit, then **Return to stand** to reset it.
5. Select **Explore** to activate the exhibit-specific demonstration.
6. Close the interaction and continue visiting the museum.

The common interaction logic is in `Assets/Scripts/MuseumTwin/Scripts/MuseumExhibitInteractions.cs`. Each Explore experience has a separate C# script.

## Technologies

- [Unity 6](https://unity.com/)
- [XR Interaction Toolkit 3.3](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.3/manual/index.html)
- [OpenXR](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.15/manual/index.html)
- C#; Unity UI; TextMesh Pro
- Unity XR Device Simulator
- Git and GitHub

## Asset Sources & AI Assistance

**Developed/integrated for the project:** five-room graybox, stands, corridors, primitive decorative exhibits, scene configuration, information panels, interaction flow, and C# exhibit demos.

**External sources:** Unity XR Interaction Toolkit, OpenXR, XR Device Simulator starter/sample assets, and hand/controller assets reused from an earlier course laboratory.

**AI-assisted development (ChatGPT):** assistance with parts of the C# scripts, debugging, troubleshooting, and creation of some simplified 3D exhibit models, including the full-body mammoth reconstruction. All components were integrated and tested iteratively by the developer. **AI is not a runtime feature of the museum application.**

## Testing and Current Limitations

**Verified in Unity Play Mode / XR Device Simulator:**
- Navigation between connected museum rooms.
- Exhibit selection, information menus, View, Rotate, Zoom, Grab and Return to stand.
- Mammoth reconstruction, monoxyl construction stages, sundial shadow demo and original-mesh amphora shattering.
- Fixes to corridor openings, floating objects, preview placement and menu accessibility.

**Remaining work:**
- Final testing of all interactions on Meta Quest 2.
- Standalone Android APK validation and frame-rate profiling.
- Detailed architecture, materials, lighting, labels and accessibility refinements.
- Historical verification of artifact descriptions and model provenance.

## Development Progress

| Week / Milestone | Work | Status |
|---|---|---|
| 1 | Repository and initial project documentation | Completed |
| 2 | XR configuration and simulator setup | Completed |
| 3 | Initial museum graybox | Completed |
| 4 | Materials, textures and lighting | Further refinement planned |
| 5 | Import historical exhibits | Four models imported |
| 6 / Tema 6 | Basic exhibit selection and information panels | Completed |
| 7 | Enhanced exhibit UI and interactions | Completed |
| 8 | Five connected rooms and View / Grab controls | Completed |
| **9** | **Functional prototype, four Explore demonstrations, Week 9 PowerPoint with videos** | **Completed in Unity simulator** |
| 10 | Meta Quest 2 validation, optimization and finishing | Planned |

## Weekly Updates

### Week 1 — Project Initialization

- Defined the concept and initial requirements.
- Created the public GitHub repository and project specification.

### Tema 5 — Main Modules and Graybox

- Prepared user story maps, journeys, storyboards and an asset checklist.
- Configured the Unity XR environment and built the initial graybox.

### Tema 6 — Interactive Museum Exhibits

- Imported four approximate historical 3D models and set up exhibit pedestals.
- Added colliders, interactable components and individual information panels.
- Tested basic selection and panel visibility in the XR simulator.

### Week 9 — Functional Prototyping and Presentation

- Reorganized the prototype into five connected museum rooms.
- Implemented reusable inspection and manipulation controls.
- Added four distinct Explore interactions: mammoth reconstruction, boat-construction stages, sundial shadows and amphora shattering.
- Improved menu and preview positioning for better reachability.
- Tested the functional prototype in Unity and recorded demonstrations.
- Prepared the [Week 9 presentation with embedded video](video/MuseumTwin_Prezentare_Video.pptx), located in the [`video/`](video/) folder.
- Presented the VR purpose, scene design, grayboxing, asset provenance, AI assistance, functional testing and next implementation steps.

## Future Improvements

- Test and optimize the complete experience for standalone Meta Quest 2.
- Improve museum architecture, material quality, lighting and signage.
- Verify historical descriptions and refine educational explanations.
- Improve world-space menu readability and accessibility.
- Expand the museum with additional exhibits after stabilizing the core prototype.

## Repository

https://github.com/LogicaVietii/Muzeu-National-Iasi-digital-twin

## Author

**Rotariu David**  
Immersive / Mixed Reality Project
