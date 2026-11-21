#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Put this script in Assets/Editor. Run in Edit Mode, not Play Mode.
// Version 3: connected cylindrical handles, exact mesh-to-stand alignment.
// Existing XR components, events and information panels are not recreated.
public static class MuseumBigLayoutGenerator
{
    const float WallHeight = 3.6f;
    const float WallThickness = 0.2f;
    const float DoorWidth = 3f;
    const float DoorHeight = 2.7f;

    [MenuItem("MuseumTwin/Generate Bigger Museum Interior")]
    public static void GenerateMuseum()
    {
        if (!CanEdit()) return;
        if (!EditorUtility.DisplayDialog("MuseumTwin", "Regenerate the museum geometry? Existing custom edits inside MuseumGenerated will be replaced. The four main exhibits, their canvases and the XR rig will be preserved.", "Generate", "Cancel")) return;

        // Resolve objects before removing the previous generated layout.
        string[] exhibitNames = { "Museum_Exhibit_Sundial", "Museum_Exhibit_Amphora", "Museum_Exhibit_Mammoth", "Museum_Exhibit_Boat" };
        string[] panelNames = { "InfoCanvas_Sundial", "InfoCanvas_Amphora", "InfoCanvas_Mammoth", "InfoCanvas_Boat" };
        GameObject[] exhibits = new GameObject[4];
        GameObject[] panels = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            exhibits[i] = FindSceneObject(exhibitNames[i]);
            panels[i] = FindSceneObject(panelNames[i]);
            if (exhibits[i] == null)
            {
                Debug.LogError("MuseumTwin: missing " + exhibitNames[i] + ". Open the scene with the four exhibits first. Nothing was changed.");
                return;
            }
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Generate MuseumTwin layout");
        GameObject oldRoot = FindSceneObject("MuseumGenerated");
        if (oldRoot != null)
        {
            // Protect interactables and inactive information panels even if nested in oldRoot.
            GameObject exhibitRoot = FindSceneObject("Exhibits");
            if (exhibitRoot != null && exhibitRoot.transform.IsChildOf(oldRoot.transform))
                Undo.SetTransformParent(exhibitRoot.transform, null, "Preserve exhibits");
            GameObject panelRoot = FindSceneObject("MuseumInfoPanels");
            if (panelRoot != null && panelRoot.transform.IsChildOf(oldRoot.transform))
                Undo.SetTransformParent(panelRoot.transform, null, "Preserve panels");
            foreach (GameObject exhibit in exhibits) PreserveIfInside(exhibit, oldRoot);
            foreach (GameObject panel in panels) PreserveIfInside(panel, oldRoot);
            Undo.DestroyObjectImmediate(oldRoot);
        }
        Transform root = Group(null, "MuseumGenerated");
        Transform floors = Group(root, "Floors");
        Transform walls = Group(root, "Walls");
        Transform decor = Group(root, "Decor");
        Transform pedestals = Group(root, "GeneratedPedestals");

        SetActiveIfExists("MuseumEnvironment", false);
        SetActiveIfExists("Pedestals", false);
        SetActiveIfExists("Exhibit_01", false);
        SetActiveIfExists("Exhibit_02", false);
        SetActiveIfExists("Exhibit_03", false);
        SetActiveIfExists("ExhibitInfoCanvas", false);

        // X = east/west, Z = north/south. Room boundaries:
        // Entrance Z [-18,-10], Central [-8,4], Back [6,14].
        // Corridors fill exactly [-10,-8] and [4,6].
        BuildRoom(floors, walls, "EntranceHall", 0, -14, 10, 8, true, false, false, false);
        BuildRoom(floors, walls, "CentralHall", 0, -2, 16, 12, true, true, true, true);
        // The central room already supplies the shared walls at X=-8 and X=8.
        // Skip the duplicate wall on each side gallery.
        BuildRoom(floors, walls, "LeftGallery", -13, -2, 10, 10, false, false, true, false, skipEast: true);
        BuildRoom(floors, walls, "RightGallery", 13, -2, 10, 10, false, false, false, true, skipWest: true);
        BuildRoom(floors, walls, "BackGallery", 0, 10, 16, 8, false, true, false, false);
        BuildCorridors(root);

        // Cylinder primitive is 2 units tall; height 3.6 uses scale Y=1.8.
        Cylinder(decor, "Column_01", new Vector3(-5, 1.8f, -2), new Vector3(.8f, 1.8f, .8f));
        Cylinder(decor, "Column_02", new Vector3(5, 1.8f, -2), new Vector3(.8f, 1.8f, .8f));
        CoinStand(decor, "CoinStand_01", new Vector3(-2.8f, 0, -14));
        CoinStand(decor, "CoinStand_02", new Vector3(2.8f, 0, -14));
        VaseStand(decor, "SimpleVase_01", new Vector3(-5, 0, 1));
        VaseStand(decor, "SimpleVase_02", new Vector3(5, 0, 1));
        DisplayCase(decor, "DisplayCase_Left", new Vector3(-13, 0, 1.5f));
        DisplayCase(decor, "DisplayCase_Right", new Vector3(13, 0, 1.5f));

        GameObject[] stands = {
            Box(pedestals, "Pedestal_Sundial", new Vector3(-13, .5f, -4), new Vector3(1.4f, 1, 1.4f)),
            // Offset the central exhibit to keep the entrance-to-back route clear.
            Box(pedestals, "Pedestal_Amphora", new Vector3(2.8f, .5f, 0), new Vector3(1.4f, 1, 1.4f)),
            Box(pedestals, "Pedestal_Mammoth", new Vector3(13, .5f, -2), new Vector3(2.6f, 1, 2.6f)),
            Box(pedestals, "Platform_Boat", new Vector3(0, .35f, 10), new Vector3(6, .7f, 2.8f))
        };

        // Measure the actual generated stands and the actual visible mesh vertices.
        // No guessed Y offsets. Preserve root/child scales, rotations and XR events.
        for (int i = 0; i < exhibits.Length; i++)
            PlaceExhibit(exhibits[i], stands[i]);
        PlacePanel(panels[0], new Vector3(-10.8f, 1.8f, -4));
        PlacePanel(panels[1], new Vector3(4.4f, 1.8f, 0));
        PlacePanel(panels[2], new Vector3(15.6f, 1.8f, -2));
        PlacePanel(panels[3], new Vector3(3.6f, 1.8f, 10));

        GameObject rig = FindSceneObject("XR Origin (XR Rig)");
        if (rig != null)
        {
            Undo.RecordObject(rig.transform, "Move XR origin");
            Vector3 p = rig.transform.position;
            rig.transform.position = new Vector3(0, p.y, -16);
            rig.transform.rotation = Quaternion.identity;
        }
        Finish(root.gameObject, undoGroup);
        Debug.Log("MuseumTwin: five connected rooms generated. Save scene and test XR movement. New floors have colliders; configure Teleportation Areas separately if needed.");
    }

    [MenuItem("MuseumTwin/Fix Missing Corridors Only")]
    public static void FixCorridorsOnly()
    {
        if (!CanEdit()) return;
        GameObject root = FindSceneObject("MuseumGenerated");
        if (root == null) { Debug.LogError("MuseumGenerated not found. Generate the museum first."); return; }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Fix museum corridors");
        BuildCorridors(root.transform);
        Finish(root, group);
        Debug.Log("MuseumTwin: both missing corridors added. Existing exhibits and room geometry preserved.");
    }

    static bool CanEdit()
    {
        if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) return true;
        Debug.LogError("MuseumTwin: exit Play Mode first.");
        return false;
    }

    static void BuildCorridors(Transform root)
    {
        Transform old = root.Find("Corridors");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        Transform group = Group(root, "Corridors");
        Corridor(group, "Entrance_Corridor", -9);
        Corridor(group, "Back_Corridor", 5);
    }

    static void Corridor(Transform parent, string name, float z)
    {
        Transform group = Group(parent, name);
        // Floor bridges overlap adjacent floors by 2 cm; walls overlap by 10 cm.
        Box(group, name + "_Floor", new Vector3(0, -.05f, z), new Vector3(3.2f, .1f, 2.04f));
        Box(group, name + "_Left", new Vector3(-1.6f, WallHeight/2, z), new Vector3(WallThickness, WallHeight, 2.2f));
        Box(group, name + "_Right", new Vector3(1.6f, WallHeight/2, z), new Vector3(WallThickness, WallHeight, 2.2f));
    }

    static void BuildRoom(Transform floors, Transform walls, string name, float x, float z, float w, float d,
        bool northDoor, bool southDoor, bool eastDoor, bool westDoor, bool skipEast = false, bool skipWest = false)
    {
        Box(floors, name + "_Floor", new Vector3(x, -.05f, z), new Vector3(w, .1f, d));
        Wall(walls, name + "_North", x, z + d/2, w, northDoor, false);
        Wall(walls, name + "_South", x, z - d/2, w, southDoor, false);
        if (!skipEast) Wall(walls, name + "_East", x + w/2, z, d, eastDoor, true);
        if (!skipWest) Wall(walls, name + "_West", x - w/2, z, d, westDoor, true);
    }

    static void Wall(Transform parent, string name, float x, float z, float length, bool door, bool alongZ)
    {
        if (!door)
        {
            WallPart(parent, name, x, z, 0, length, WallHeight/2, WallHeight, alongZ);
            return;
        }
        float side = (length - DoorWidth)/2;
        float offset = DoorWidth/2 + side/2;
        WallPart(parent, name + "_A", x, z, -offset, side, WallHeight/2, WallHeight, alongZ);
        WallPart(parent, name + "_B", x, z, offset, side, WallHeight/2, WallHeight, alongZ);
        WallPart(parent, name + "_Lintel", x, z, 0, DoorWidth, (WallHeight + DoorHeight)/2, WallHeight - DoorHeight, alongZ);
    }

    static void WallPart(Transform parent, string name, float x, float z, float offset, float length, float y, float h, bool alongZ)
    {
        Vector3 p = alongZ ? new Vector3(x, y, z + offset) : new Vector3(x + offset, y, z);
        Vector3 s = alongZ ? new Vector3(WallThickness, h, length) : new Vector3(length, h, WallThickness);
        Box(parent, name, p, s);
    }

    static Transform Group(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go.transform;
    }

    static GameObject Primitive(Transform parent, string name, PrimitiveType type, Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        return go;
    }
    static GameObject Box(Transform p, string n, Vector3 pos, Vector3 size) => Primitive(p,n,PrimitiveType.Cube,pos,size);
    static GameObject Cylinder(Transform p, string n, Vector3 pos, Vector3 scale) => Primitive(p,n,PrimitiveType.Cylinder,pos,scale);

    static void CoinStand(Transform parent, string name, Vector3 p)
    {
        Transform g = Group(parent, name);
        Box(g, "Base", p + new Vector3(0,.45f,0), new Vector3(1.4f,.9f,1.1f));
        for (int i = 0; i < 3; i++)
            Cylinder(g, "Coin_0"+(i+1), p + new Vector3((i-1)*.3f,.915f,0), new Vector3(.2f,.015f,.2f));
    }

    static void VaseStand(Transform parent, string name, Vector3 p)
    {
        Transform g = Group(parent, name);
        GameObject stand = Box(g, "Base", p + new Vector3(0,.35f,0), new Vector3(1,.7f,1));
        BuildVase(g, stand.GetComponent<Renderer>().bounds, null);
    }

    static void BuildVase(Transform parent, Bounds standBounds, Material material)
    {
        // Anchor at the real pedestal surface. Unity's cylinder is 2 m tall
        // and 1 m in diameter at scale (1,1,1).
        Vector3 p = new Vector3(standBounds.center.x, standBounds.max.y, standBounds.center.z);
        Transform g = Group(parent, "VaseGeometry");
        Cylinder(g, "VaseBody", p + new Vector3(0,.25f,0), new Vector3(.38f,.25f,.38f));
        // Neck bottom = .48; body top = .50, so they overlap by 2 cm.
        Cylinder(g, "VaseNeck", p + new Vector3(0,.56f,0), new Vector3(.23f,.08f,.23f));
        Cylinder(g, "VaseRim", p + new Vector3(0,.65f,0), new Vector3(.26f,.025f,.26f));
        // Each U-shaped handle has exactly three cylinders: vertical outer bar
        // and two horizontal connectors. Inner ends penetrate the vase body;
        // outer ends overlap the vertical bar, leaving no floating pieces.
        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 lowerInner = p + new Vector3(side*.15f,.13f,0);
            Vector3 lowerOuter = p + new Vector3(side*.30f,.13f,0);
            Vector3 upperInner = p + new Vector3(side*.15f,.41f,0);
            Vector3 upperOuter = p + new Vector3(side*.30f,.41f,0);
            Rod(g, "HandleOuter_"+side, lowerOuter, upperOuter, .06f);
            Rod(g, "HandleUpper_"+side, upperInner, upperOuter, .06f);
            Rod(g, "HandleLower_"+side, lowerInner, lowerOuter, .06f);
        }
        if (material != null)
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = material;
    }

    static void Rod(Transform parent, string name, Vector3 a, Vector3 b, float diameter)
    {
        Vector3 delta = b - a;
        // Extra 2 cm at both ends avoids tiny gaps at 90-degree joints.
        GameObject rod = Cylinder(parent, name, (a+b)*.5f,
            new Vector3(diameter, (delta.magnitude+.04f)*.5f, diameter));
        rod.transform.rotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
    }

    static void DisplayCase(Transform parent, string name, Vector3 p)
    {
        Transform g = Group(parent, name);
        Box(g, "Stand", p+new Vector3(0,.45f,0), new Vector3(1.8f,.9f,.8f));
        for (int i=0; i<3; i++)
            Box(g, "Artifact_"+i, p+new Vector3((i-1)*.48f,1.05f,0), new Vector3(.25f,.3f,.25f));
    }

    static void PlaceExhibit(GameObject go, GameObject stand)
    {
        Undo.RecordObject(go, "Activate exhibit");
        go.SetActive(true);
        Bounds support = stand.GetComponent<Renderer>().bounds;
        Bounds mesh;
        if (!TryVisibleMeshBounds(go, out mesh))
        {
            Debug.LogWarning("MuseumTwin: no visible mesh found for " + go.name);
            return;
        }
        Undo.RecordObject(go.transform, "Align exhibit to pedestal");
        // Use the real world-space bottom of the geometry, not the model pivot,
        // collider padding, or a fixed Y. No deliberate air gap.
        Vector3 delta = new Vector3(support.center.x - mesh.center.x,
            support.max.y - mesh.min.y, support.center.z - mesh.center.z);
        go.transform.position += delta;
        Debug.Log("MuseumTwin: aligned " + go.name + " to " + stand.name +
            " (surface Y=" + support.max.y.ToString("F3") + ").");
    }

    static bool TryVisibleMeshBounds(GameObject exhibit, out Bounds bounds)
    {
        // Our imported OBJ is under VisualModel_*. Ignore unrelated child UI,
        // highlight geometry, hitboxes, etc. when computing the contact point.
        Transform visual = null;
        foreach (Transform t in exhibit.GetComponentsInChildren<Transform>(true))
            if (t.name.StartsWith("VisualModel_", System.StringComparison.Ordinal))
            { visual = t; break; }
        Transform target = visual != null ? visual : exhibit.transform;
        bounds = new Bounds();
        bool initialized = false;
        foreach (MeshFilter filter in target.GetComponentsInChildren<MeshFilter>())
        {
            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
            Mesh mesh = filter.sharedMesh;
            if (mesh == null || renderer == null || !renderer.enabled) continue;
            try
            {
                Vector3[] vertices = mesh.vertices;
                // Only vertices referenced by an actual mesh face: unused OBJ
                // vertices can otherwise produce a false bottom and floating mesh.
                bool[] used = new bool[vertices.Length];
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                    foreach (int index in mesh.GetIndices(sub))
                        if (index >= 0 && index < used.Length) used[index] = true;
                for (int i = 0; i < vertices.Length; i++)
                    if (used[i]) AddPoint(ref bounds, ref initialized,
                        filter.transform.TransformPoint(vertices[i]));
            }
            catch (System.Exception)
            {
                // Some imported meshes are not CPU-readable. Fall back without
                // modifying model import settings or breaking existing assets.
                AddPoint(ref bounds, ref initialized, renderer.bounds.min);
                AddPoint(ref bounds, ref initialized, renderer.bounds.max);
                Debug.LogWarning("MuseumTwin: used renderer bounds for " + filter.name +
                    ". Enable Read/Write on that imported model if alignment still looks wrong.");
            }
        }
        if (!initialized)
            foreach (MeshRenderer renderer in target.GetComponentsInChildren<MeshRenderer>())
                if (renderer.enabled)
                {
                    AddPoint(ref bounds, ref initialized, renderer.bounds.min);
                    AddPoint(ref bounds, ref initialized, renderer.bounds.max);
                }
        return initialized;
    }

    static void AddPoint(ref Bounds bounds, ref bool initialized, Vector3 point)
    {
        if (!initialized) { bounds = new Bounds(point, Vector3.zero); initialized = true; }
        else bounds.Encapsulate(point);
    }

    [MenuItem("MuseumTwin/Fix Floating Exhibits and Vase Handles Only")]
    public static void FixExhibitsOnly()
    {
        if (!CanEdit()) return;
        GameObject root = FindSceneObject("MuseumGenerated");
        if (root == null) { Debug.LogError("MuseumGenerated not found. Generate the museum first."); return; }
        string[] exhibitNames = { "Museum_Exhibit_Sundial", "Museum_Exhibit_Amphora", "Museum_Exhibit_Mammoth", "Museum_Exhibit_Boat" };
        string[] standNames = { "Pedestal_Sundial", "Pedestal_Amphora", "Pedestal_Mammoth", "Platform_Boat" };
        GameObject[] exhibits = new GameObject[4];
        GameObject[] stands = new GameObject[4];
        for (int i=0; i<4; i++)
        {
            exhibits[i] = FindSceneObject(exhibitNames[i]);
            stands[i] = FindSceneObject(standNames[i]);
            if (exhibits[i] == null || stands[i] == null || stands[i].GetComponent<Renderer>() == null)
            { Debug.LogError("MuseumTwin: missing " + exhibitNames[i] + " or " + standNames[i] + ". Nothing changed."); return; }
        }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Fix exhibit alignment and vase handles");
        for (int i=0; i<4; i++) PlaceExhibit(exhibits[i], stands[i]);
        foreach (string name in new[] { "SimpleVase_01", "SimpleVase_02" })
        {
            GameObject vase = FindSceneObject(name);
            if (vase == null) { Debug.LogWarning("MuseumTwin: " + name + " not found."); continue; }
            Transform basePart = vase.transform.Find("Base");
            if (basePart == null || basePart.GetComponent<Renderer>() == null)
            { Debug.LogWarning("MuseumTwin: missing base for " + name); continue; }
            Material material = null;
            foreach (Transform t in vase.GetComponentsInChildren<Transform>(true))
                if (t.name == "VaseBody" && t.GetComponent<Renderer>() != null)
                { material = t.GetComponent<Renderer>().sharedMaterial; break; }
            List<GameObject> oldParts = new List<GameObject>();
            foreach (Transform child in vase.transform)
                if (child.name == "VaseGeometry" || child.name.StartsWith("Vase") || child.name.StartsWith("Handle"))
                    oldParts.Add(child.gameObject);
            foreach (GameObject part in oldParts) Undo.DestroyObjectImmediate(part);
            BuildVase(vase.transform, basePart.GetComponent<Renderer>().bounds, material);
        }
        Finish(root, group);
        Debug.Log("MuseumTwin: four main exhibits aligned and both vase handles rebuilt. Save the scene.");
    }

    static void PlacePanel(GameObject go, Vector3 p)
    {
        if (go == null) return;
        Undo.RecordObject(go.transform, "Move information panel");
        go.transform.position = p;
        // Keep the existing rotation, scale, text and active state.
    }

    static void PreserveIfInside(GameObject go, GameObject oldRoot)
    {
        if (go != null && go.transform.IsChildOf(oldRoot.transform))
            Undo.SetTransformParent(go.transform, null, "Preserve exhibit or panel");
    }

    static GameObject FindSceneObject(string name)
    {
        // Unlike GameObject.Find, also finds disabled canvases.
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t.gameObject;
        }
        return null;
    }

    static void SetActiveIfExists(string name, bool active)
    {
        GameObject go = FindSceneObject(name);
        if (go == null) return;
        Undo.RecordObject(go, "Set active " + name);
        go.SetActive(active);
    }

    static void Finish(GameObject root, int group)
    {
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
    }
}
#endif
