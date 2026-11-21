using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Independent extension: visual fracture using the actual imported amphora meshes.
// No changes to View/Grab, the original artifact or other exhibits.
[DisallowMultipleComponent]
[RequireComponent(typeof(MuseumExhibitInteractions))]
public sealed class MuseumAmphoraExplore : MonoBehaviour
{
    public float panelDrop = .20f;
    public float demoHeight = .45f;
    MuseumExhibitInteractions manager;
    MuseumExhibitInteractions.Exhibit amphora;
    RectTransform returnButton;
    Vector2 oldPosition, oldSize;
    Vector3 originalMenuPosition;
    TMP_Text description, exploreLabel;
    string oldDescription;
    Button explore;
    GameObject demo;
    Coroutine animationRoutine;
    bool exploring;
    readonly List<Mesh> meshes = new List<Mesh>();
    Material trayMaterial;
    readonly List<Shard> shards = new List<Shard>();

    sealed class Shard
    {
        public Transform transform;
        public Vector3 start, end;
        public Quaternion rotation;
    }

    void Awake() { manager = GetComponent<MuseumExhibitInteractions>(); }

    void LateUpdate()
    {
        Transform menu = transform.Find("Exhibit Interaction Menu (Runtime)");
        if (manager == null || !manager.isActiveAndEnabled || menu == null ||
            !menu.gameObject.activeInHierarchy || !IsAmphoraMenu(menu)) { Cleanup(); return; }
        var r = ActiveChild(menu, "Return to stand") as RectTransform;
        if (r == null) { Cleanup(); return; }
        if (returnButton == r && explore != null && explore.gameObject.activeInHierarchy) return;
        Cleanup();
        returnButton = r;
        oldPosition = r.anchoredPosition; oldSize = r.sizeDelta;
        var d = ActiveChild(menu, "Description");
        description = d != null ? d.GetComponent<TMP_Text>() : null;
        oldDescription = description != null ? description.text : "";
        r.anchoredPosition = new Vector2(-140, oldPosition.y);
        r.sizeDelta = new Vector2(250, oldSize.y);
        var obj = Instantiate(r.gameObject, menu, false);
        obj.name = "Amphora Explore";
        var rect = (RectTransform)obj.transform;
        rect.anchoredPosition = new Vector2(140, oldPosition.y);
        explore = obj.GetComponent<Button>();
        explore.onClick = new Button.ButtonClickedEvent();
        explore.onClick.AddListener(Toggle);
        exploreLabel = obj.GetComponentInChildren<TMP_Text>(true);
        if (exploreLabel != null) exploreLabel.text = "Explore";
    }

    bool IsAmphoraMenu(Transform menu)
    {
        var t = ActiveChild(menu, "Title");
        if (t == null || ActiveChild(menu, "Grab") == null || manager.exhibits == null) return false;
        var label = t.GetComponent<TMP_Text>();
        if (label == null) return false;
        foreach (var e in manager.exhibits)
            if (e != null && e.interactable != null &&
                e.interactable.name == "Museum_Exhibit_Amphora" && label.text == e.title)
            { amphora = e; return true; }
        return false;
    }

    static Transform ActiveChild(Transform p, string name)
    {
        foreach (Transform child in p)
            if (child.name == name && child.gameObject.activeInHierarchy) return child;
        return null;
    }

    void Toggle()
    {
        if (exploring) { EndExplore(); return; }
        exploring = true;
        Transform menu = returnButton.parent;
        originalMenuPosition = menu.position;
        menu.position -= menu.up * panelDrop;
        if (exploreLabel != null) exploreLabel.text = "Hide demo";
        if (description != null) description.text = "Watch an exact visual copy of this exhibit break into shards.\nThe original artifact remains safe on its stand.";
        if (!BuildDemo(menu))
        {
            EndExplore();
            if (description != null) description.text = "Cannot read the amphora mesh. Enable Read/Write in the OBJ Model import settings and try again.";
            return;
        }
        animationRoutine = StartCoroutine(Shatter());
    }

    // Partition original triangles, not a replacement model. Every source face,
    // normal, UV and material is preserved. Cuts are visual (not solid fracture).
    sealed class Piece
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<Vector3> normals = new List<Vector3>();
        public readonly List<Vector2> uv = new List<Vector2>();
        public readonly Dictionary<int, List<int>> triangles = new Dictionary<int, List<int>>();
    }

    bool BuildDemo(Transform menu)
    {
        if (amphora == null || amphora.visual == null) return false;
        var sources = new List<MeshFilter>();
        Bounds bounds = default;
        bool valid = false;
        // Keep source orientation and all child transforms, but not its scene location.
        Matrix4x4 frame = Matrix4x4.TRS(amphora.visual.position, amphora.visual.rotation, Vector3.one).inverse;
        foreach (var mf in amphora.visual.GetComponentsInChildren<MeshFilter>(true))
        {
            var mr = mf.GetComponent<MeshRenderer>();
            var mesh = mf.sharedMesh;
            if (mesh == null || mr == null || !mr.enabled || !mf.gameObject.activeInHierarchy) continue;
            if (!mesh.isReadable)
            {
                Debug.LogWarning("MuseumTwin: enable Read/Write in the amphora OBJ importer: " + mesh.name);
                return false;
            }
            sources.Add(mf);
            Matrix4x4 matrix = frame * mf.transform.localToWorldMatrix;
            foreach (Vector3 v in mesh.vertices)
            {
                Vector3 p = matrix.MultiplyPoint3x4(v);
                if (!valid) { bounds = new Bounds(p, Vector3.zero); valid = true; }
                else bounds.Encapsulate(p);
            }
        }
        if (!valid) return false;

        const float trayTop = -.18f;
        float scale = .36f / Mathf.Max(.001f, Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z));
        Vector3 origin = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        const int rows = 4, sectors = 4;
        Piece[] pieces = new Piece[rows * sectors];
        for (int i = 0; i < pieces.Length; i++) pieces[i] = new Piece();
        var sourceMaterials = new List<Material>();
        foreach (var mf in sources)
        {
            Mesh mesh = mf.sharedMesh;
            Vector3[] v = mesh.vertices, n = mesh.normals;
            Vector2[] uv = mesh.uv;
            Material[] mats = mf.GetComponent<MeshRenderer>().sharedMaterials;
            Matrix4x4 matrix = frame * mf.transform.localToWorldMatrix;
            Matrix4x4 normalMatrix = matrix.inverse.transpose;
            bool mirrored = matrix.determinant < 0f;
            for (int i = 0; i < v.Length; i++)
                v[i] = (matrix.MultiplyPoint3x4(v[i]) - origin) * scale + Vector3.up * trayTop;
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                if (mesh.GetTopology(sub) != MeshTopology.Triangles) continue;
                Material mat = mats.Length > 0 ? mats[Mathf.Min(sub, mats.Length - 1)] : null;
                int materialIndex = sourceMaterials.IndexOf(mat);
                if (materialIndex < 0) { materialIndex = sourceMaterials.Count; sourceMaterials.Add(mat); }
                int[] indices = mesh.GetTriangles(sub);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    int ia = indices[i], ib = indices[i+1], ic = indices[i+2];
                    if (mirrored) { int swap = ib; ib = ic; ic = swap; }
                    Vector3 center = (v[ia] + v[ib] + v[ic]) / 3f;
                    float fraction = (center.y - trayTop) / Mathf.Max(.001f, bounds.size.y * scale);
                    int row = Mathf.Clamp(Mathf.FloorToInt(fraction * rows), 0, rows - 1);
                    float angle = Mathf.Atan2(center.z, center.x) + Mathf.PI + row * .29f;
                    int sector = Mathf.FloorToInt(Mathf.Repeat(angle, 2f * Mathf.PI) / (2f * Mathf.PI / sectors));
                    Piece piece = pieces[row * sectors + Mathf.Min(sector, sectors - 1)];
                    if (!piece.triangles.TryGetValue(materialIndex, out var tris))
                    { tris = new List<int>(); piece.triangles.Add(materialIndex, tris); }
                    Vector3 faceNormal = Vector3.Cross(v[ib] - v[ia], v[ic] - v[ia]).normalized;
                    AddVertex(piece, tris, v, n, uv, normalMatrix, faceNormal, ia);
                    AddVertex(piece, tris, v, n, uv, normalMatrix, faceNormal, ib);
                    AddVertex(piece, tris, v, n, uv, normalMatrix, faceNormal, ic);
                }
            }
        }

        demo = new GameObject("Original Amphora Fracture (Runtime)");
        demo.transform.SetPositionAndRotation(menu.position + menu.up * demoHeight - menu.forward * .12f, menu.rotation);
        var tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tray.name = "Fragment display tray";
        tray.transform.SetParent(demo.transform, false);
        tray.transform.localPosition = new Vector3(0, trayTop - .008f, 0);
        tray.transform.localScale = new Vector3(.79f, .016f, .65f);
        tray.GetComponent<Collider>().enabled = false;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader != null)
        {
            trayMaterial = new Material(shader);
            if (trayMaterial.HasProperty("_BaseColor")) trayMaterial.SetColor("_BaseColor", new Color(.24f,.29f,.33f));
            else if (trayMaterial.HasProperty("_Color")) trayMaterial.color = new Color(.24f,.29f,.33f);
            tray.GetComponent<MeshRenderer>().sharedMaterial = trayMaterial;
        }
        for (int i = 0; i < pieces.Length; i++)
        {
            Piece piece = pieces[i];
            if (piece.vertices.Count == 0) continue;
            Bounds pb = new Bounds(piece.vertices[0], Vector3.zero);
            foreach (var vertex in piece.vertices) pb.Encapsulate(vertex);
            Vector3 center = pb.center;
            for (int j = 0; j < piece.vertices.Count; j++) piece.vertices[j] -= center;
            var mesh = new Mesh { name = "Original ceramic fragment " + (i + 1) };
            if (piece.vertices.Count > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(piece.vertices); mesh.SetNormals(piece.normals); mesh.SetUVs(0, piece.uv);
            mesh.subMeshCount = piece.triangles.Count;
            var usedMaterials = new List<Material>();
            int sub = 0;
            foreach (var entry in piece.triangles)
            {
                mesh.SetTriangles(entry.Value, sub++);
                usedMaterials.Add(sourceMaterials[entry.Key]);
            }
            mesh.RecalculateBounds(); meshes.Add(mesh);
            GameObject obj = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer));
            obj.transform.SetParent(demo.transform, false);
            obj.transform.localPosition = center;
            obj.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = obj.GetComponent<MeshRenderer>();
            renderer.sharedMaterials = usedMaterials.ToArray();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            int row = i / sectors, sector = i % sectors;
            float angle = -Mathf.PI + (sector + .5f) * (2f * Mathf.PI / sectors) - row * .29f;
            float distance = .11f + row * .058f;
            Quaternion rotation = Quaternion.Euler(65f + row * 17f, sector * 82f + row * 29f, 20f + sector * 19f);
            Vector3 end = new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance * .75f);
            float minY = float.PositiveInfinity;
            float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;
            foreach (var vertex in piece.vertices)
            {
                Vector3 p = rotation * vertex;
                minY = Mathf.Min(minY, p.y);
                minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                minZ = Mathf.Min(minZ, p.z); maxZ = Mathf.Max(maxZ, p.z);
            }
            end.x = Mathf.Clamp(end.x, -.39f - minX, .39f - maxX);
            end.z = Mathf.Clamp(end.z, -.32f - minZ, .32f - maxZ);
            end.y = trayTop - minY + .001f;
            shards.Add(new Shard { transform = obj.transform, start = center, end = end, rotation = rotation });
        }
        return shards.Count > 0;
    }

    static void AddVertex(Piece piece, List<int> triangles, Vector3[] v, Vector3[] n,
        Vector2[] uv, Matrix4x4 normalMatrix, Vector3 fallback, int index)
    {
        triangles.Add(piece.vertices.Count);
        piece.vertices.Add(v[index]);
        piece.normals.Add(n.Length == v.Length ? normalMatrix.MultiplyVector(n[index]).normalized : fallback);
        piece.uv.Add(uv.Length == v.Length ? uv[index] : Vector2.zero);
    }

    IEnumerator Shatter()
    {
        // Short intact preview, then a controlled break animation. No physics/XR dependencies.
        yield return new WaitForSeconds(.85f);
        const float duration = .8f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            foreach (var shard in shards)
            {
                shard.transform.localPosition = Vector3.Lerp(shard.start, shard.end, t) +
                    Vector3.up * (.10f * Mathf.Sin(t * Mathf.PI));
                shard.transform.localRotation = Quaternion.Slerp(Quaternion.identity, shard.rotation, t);
            }
            yield return null;
        }
        foreach (var shard in shards)
        {
            shard.transform.localPosition = shard.end;
            shard.transform.localRotation = shard.rotation;
        }
        if (description != null) description.text = "The original exhibit model has been visually separated into ceramic fragments.\nArchaeologists often recover fragments rather than complete vessels.\nSelect Hide demo to return; Explore replays the break.";
        animationRoutine = null;
    }

    void EndExplore()
    {
        if (animationRoutine != null) { StopCoroutine(animationRoutine); animationRoutine = null; }
        if (exploring && returnButton != null && returnButton.gameObject.activeInHierarchy)
            returnButton.parent.position = originalMenuPosition;
        exploring = false;
        if (demo != null) { demo.SetActive(false); Destroy(demo); }
        demo = null; shards.Clear();
        foreach (var mesh in meshes) if (mesh != null) Destroy(mesh);
        meshes.Clear();
        if (trayMaterial != null) Destroy(trayMaterial);
        trayMaterial = null;
        if (description != null) description.text = oldDescription;
        if (exploreLabel != null) exploreLabel.text = "Explore";
    }

    void Cleanup()
    {
        if (returnButton == null && explore == null && demo == null && !exploring) return;
        EndExplore();
        if (returnButton != null) { returnButton.anchoredPosition = oldPosition; returnButton.sizeDelta = oldSize; }
        if (explore != null) { explore.gameObject.SetActive(false); Destroy(explore.gameObject); }
        returnButton = null; description = null; exploreLabel = null; explore = null;
    }

    void OnDisable() { Cleanup(); }
}
