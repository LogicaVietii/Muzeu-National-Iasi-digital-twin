using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// Separate extension: does not change the original View & Grab or Mammoth scripts.
[DisallowMultipleComponent]
[RequireComponent(typeof(MuseumExhibitInteractions))]
public sealed class MuseumBoatExplore : MonoBehaviour
{
    MuseumExhibitInteractions manager;
    MuseumExhibitInteractions.Exhibit boat;
    RectTransform returnButton;
    Vector2 oldPosition, oldSize;
    Vector3 originalMenuPosition;
    bool menuLowered;
    public float panelDrop = .28f;
    public float previewHeight = .32f;
    TMP_Text description, exploreLabel;
    string oldDescription;
    readonly List<GameObject> buttons = new List<GameObject>();
    Button previous, next;
    GameObject preview;
    Mesh generatedMesh;
    Material wood;
    int stage = -1;
    static readonly string[] stages = {
        "1/4 — Select a trunk\nA suitable tree trunk forms the starting material.",
        "2/4 — Rough shaping\nThe outside is shaped and a shallow cavity is cut.",
        "3/4 — Hollowing\nMore wood is removed until the internal cavity is deep enough.",
        "4/4 — Finished monoxyl\nA dugout boat made from a single trunk."
    };

    void Awake() { manager = GetComponent<MuseumExhibitInteractions>(); }

    void LateUpdate()
    {
        Transform menu = transform.Find("Exhibit Interaction Menu (Runtime)");
        if (manager == null || !manager.isActiveAndEnabled || menu == null ||
            !menu.gameObject.activeInHierarchy || !IsBoatMenu(menu))
        { Cleanup(); return; }

        var r = ActiveChild(menu, "Return to stand") as RectTransform;
        if (r == null) { Cleanup(); return; }
        if (returnButton != r || buttons.Count != 3 || buttons[0] == null || !buttons[0].activeInHierarchy)
        {
            Cleanup();
            returnButton = r;
            oldPosition = r.anchoredPosition;
            oldSize = r.sizeDelta;
            var d = ActiveChild(menu, "Description");
            description = d != null ? d.GetComponent<TMP_Text>() : null;
            oldDescription = description != null ? description.text : "";
            r.anchoredPosition = new Vector2(-270, oldPosition.y);
            r.sizeDelta = new Vector2(235, oldSize.y);
            // The original label already fits its slightly smaller button.
            previous = CopyButton(menu, "Boat Previous", "Previous", -75, 135, () => ChangeStage(-1));
            Button explore = CopyButton(menu, "Boat Explore", "Explore", 90, 175, Toggle);
            exploreLabel = explore.GetComponentInChildren<TMP_Text>(true);
            next = CopyButton(menu, "Boat Next", "Next", 265, 135, () => ChangeStage(1));
            RefreshButtons();
        }
        if (preview != null)
            PlacePreview(menu);
    }

    bool IsBoatMenu(Transform menu)
    {
        Transform t = ActiveChild(menu, "Title");
        if (t == null || ActiveChild(menu, "Grab") == null || manager.exhibits == null) return false;
        TMP_Text label = t.GetComponent<TMP_Text>();
        if (label == null) return false;
        foreach (var e in manager.exhibits)
            if (e != null && e.interactable != null && e.interactable.name == "Museum_Exhibit_Boat" && label.text == e.title)
            { boat = e; return true; }
        return false;
    }

    static Transform ActiveChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
            if (child.name == name && child.gameObject.activeInHierarchy) return child;
        return null;
    }

    Button CopyButton(Transform menu, string name, string label, float x, float width, UnityAction action)
    {
        GameObject obj = Instantiate(returnButton.gameObject, menu, false);
        obj.name = name;
        buttons.Add(obj);
        var r = (RectTransform)obj.transform;
        r.anchoredPosition = new Vector2(x, oldPosition.y);
        r.sizeDelta = new Vector2(width, oldSize.y);
        var text = obj.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = label;
            text.rectTransform.sizeDelta = new Vector2(width - 8, oldSize.y - 4);
        }
        var b = obj.GetComponent<Button>();
        b.onClick = new Button.ButtonClickedEvent();
        b.onClick.AddListener(action);
        return b;
    }

    void Toggle()
    {
        if (stage >= 0) EndExplore();
        else
        {
            Transform menu = returnButton.parent;
            originalMenuPosition = menu.position;
            menu.position -= menu.up * panelDrop;
            menuLowered = true;
            stage = 0; ShowStage();
        }
    }
    void ChangeStage(int offset)
    {
        if (stage < 0) return;
        stage = Mathf.Clamp(stage + offset, 0, 3);
        ShowStage();
    }
    void RefreshButtons()
    {
        if (previous != null) previous.interactable = stage > 0;
        if (next != null) next.interactable = stage >= 0 && stage < 3;
        if (exploreLabel != null) exploreLabel.text = stage < 0 ? "Explore" : "Hide demo";
    }

    void ShowStage()
    {
        RemovePreview();
        if (description != null) description.text = stages[stage] + "\n\nIllustrative reconstruction, not a documented exact manufacturing sequence.";
        preview = new GameObject("Monoxyl Construction Preview (Runtime)");
        if (stage == 0)
        {
            var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = "Tree trunk";
            log.transform.SetParent(preview.transform, false);
            log.transform.localRotation = Quaternion.Euler(0, 0, 90);
            log.transform.localScale = new Vector3(.10f, .20f, .10f);
            var collider = log.GetComponent<Collider>();
            if (collider != null) { collider.enabled = false; Destroy(collider); }
            log.GetComponent<MeshRenderer>().sharedMaterial = WoodMaterial();
        }
        else if (stage < 3)
        {
            generatedMesh = MakeDugout(stage == 1 ? .026f : .070f);
            var hull = new GameObject("Carved trunk", typeof(MeshFilter), typeof(MeshRenderer));
            hull.transform.SetParent(preview.transform, false);
            hull.GetComponent<MeshFilter>().sharedMesh = generatedMesh;
            hull.GetComponent<MeshRenderer>().sharedMaterial = WoodMaterial();
        }
        else CopyFinishedBoat();
        Transform menu = returnButton != null ? returnButton.parent : null;
        if (menu != null) PlacePreview(menu);
        RefreshButtons();
    }

    void PlacePreview(Transform menu)
    {
        preview.transform.SetPositionAndRotation(menu.position + menu.up * previewHeight - menu.forward * .10f,
            menu.rotation * Quaternion.Euler(-22f, 0f, 0f));
    }

    Material WoodMaterial()
    {
        if (wood != null) return wood;
        // Clone a material already used in the scene to retain URP compatibility in builds.
        var renderer = boat != null && boat.visual != null ? boat.visual.GetComponentInChildren<MeshRenderer>() : null;
        if (renderer != null && renderer.sharedMaterial != null) wood = new Material(renderer.sharedMaterial);
        else wood = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        if (wood.HasProperty("_BaseColor")) wood.SetColor("_BaseColor", new Color(.43f,.25f,.12f));
        if (wood.HasProperty("_Color")) wood.SetColor("_Color", new Color(.43f,.25f,.12f));
        if (wood.HasProperty("_BaseMap")) wood.SetTexture("_BaseMap", null);
        if (wood.HasProperty("_MainTex")) wood.SetTexture("_MainTex", null);
        if (wood.HasProperty("_Metallic")) wood.SetFloat("_Metallic", 0);
        if (wood.HasProperty("_Smoothness")) wood.SetFloat("_Smoothness", .15f);
        return wood;
    }

    // Longitudinal sections, with a concave inner trough and a solid wooden exterior.
    // Dimensions are in meters in this small educational preview, not historical scale.
    Mesh MakeDugout(float depth)
    {
        float[] xs = {-.20f, -.16f, -.10f, .10f, .16f, .20f};
        float[] taper = {.24f, .78f, 1f, 1f, .78f, .24f};
        const int arc = 8;
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        for (int i=0;i<xs.Length;i++)
        {
            for (int j=0;j<=arc;j++)
            {
                float a = Mathf.PI * j / arc;
                vertices.Add(new Vector3(xs[i], .035f - .083f * Mathf.Sin(a), -.059f*taper[i]*Mathf.Cos(a)));
            }
            for (int j=arc;j>=0;j--)
            {
                float a = Mathf.PI * j / arc;
                float d = (i==0 || i==xs.Length-1) ? .004f : depth;
                vertices.Add(new Vector3(xs[i], .035f - d*Mathf.Sin(a), -.044f*taper[i]*Mathf.Cos(a)));
            }
        }
        int n = 2*(arc+1);
        for (int i=0;i<xs.Length-1;i++)
            for (int j=0;j<n;j++)
            {
                int a=i*n+j, b=i*n+(j+1)%n, c=(i+1)*n+j, d=(i+1)*n+(j+1)%n;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
        // Close the ends with strips between the outside and inside curves.
        for (int end=0;end<2;end++)
        {
            int start = end==0 ? 0 : (xs.Length-1)*n;
            for (int j=0;j<arc;j++)
            {
                int a=start+j, b=start+j+1, c=start+n-1-j, d=start+n-2-j;
                if(end==0) { triangles.Add(a); triangles.Add(b); triangles.Add(c); triangles.Add(b); triangles.Add(d); triangles.Add(c); }
                else { triangles.Add(a); triangles.Add(c); triangles.Add(b); triangles.Add(b); triangles.Add(c); triangles.Add(d); }
            }
        }
        var mesh = new Mesh { name = "Dugout stage" };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }

    void CopyFinishedBoat()
    {
        if (boat == null || boat.visual == null) return;
        var model = new GameObject("Finished boat model");
        model.transform.SetParent(preview.transform, false);
        bool found=false;
        Bounds bounds=default;
        Vector3 origin=boat.visual.position;
        foreach(var mf in boat.visual.GetComponentsInChildren<MeshFilter>())
        {
            var src=mf.GetComponent<MeshRenderer>();
            if (mf.sharedMesh==null || src==null || !src.enabled) continue;
            var obj=new GameObject(mf.name,typeof(MeshFilter),typeof(MeshRenderer));
            obj.transform.SetParent(model.transform,false);
            obj.transform.localPosition=mf.transform.position-origin;
            obj.transform.localRotation=mf.transform.rotation;
            obj.transform.localScale=mf.transform.lossyScale;
            obj.GetComponent<MeshFilter>().sharedMesh=mf.sharedMesh;
            var dst=obj.GetComponent<MeshRenderer>(); dst.sharedMaterials=src.sharedMaterials;
            if(!found) { bounds=dst.bounds; found=true; } else bounds.Encapsulate(dst.bounds);
        }
        if(!found) return;
        // Same centering technique as the working View mode, using a separate copy.
        float scale=.40f/Mathf.Max(.001f, Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z));
        model.transform.localScale=Vector3.one*scale;
        model.transform.localPosition=-bounds.center*scale;
        model.transform.localRotation=Quaternion.identity;
    }

    void RemovePreview()
    {
        if(preview!=null) { preview.SetActive(false); Destroy(preview); preview=null; }
        if(generatedMesh!=null) { Destroy(generatedMesh); generatedMesh=null; }
    }
    void EndExplore()
    {
        stage=-1;
        if (menuLowered && returnButton != null && returnButton.gameObject.activeInHierarchy)
            returnButton.parent.position = originalMenuPosition;
        menuLowered = false;
        RemovePreview();
        if(description!=null) description.text=oldDescription;
        RefreshButtons();
    }
    void Cleanup()
    {
        EndExplore();
        if(returnButton!=null) { returnButton.anchoredPosition=oldPosition; returnButton.sizeDelta=oldSize; }
        foreach(var b in buttons) if(b!=null) { b.SetActive(false); Destroy(b); }
        buttons.Clear(); returnButton=null; description=null; exploreLabel=null; previous=null; next=null;
    }
    void OnDisable() { Cleanup(); }
    void OnDestroy() { if(wood!=null) Destroy(wood); }
}
