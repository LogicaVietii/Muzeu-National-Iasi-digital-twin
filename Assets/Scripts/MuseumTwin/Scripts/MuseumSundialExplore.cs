using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Independent educational extension for the original View & Grab menu.
[DisallowMultipleComponent]
[RequireComponent(typeof(MuseumExhibitInteractions))]
public sealed class MuseumSundialExplore : MonoBehaviour
{
    MuseumExhibitInteractions manager;
    MuseumExhibitInteractions.Exhibit sundial;
    RectTransform returnButton;
    Vector2 oldPosition, oldSize;
    Vector3 originalMenuPosition;
    TMP_Text description, exploreLabel;
    string oldDescription;
    readonly List<Button> buttons = new List<Button>();
    readonly List<Material> materials = new List<Material>();
    Button earlier, later;
    GameObject demo;
    Transform sun, shadow;
    bool exploring;
    int hour = 12;

    void Awake() { manager = GetComponent<MuseumExhibitInteractions>(); }

    void LateUpdate()
    {
        Transform menu = transform.Find("Exhibit Interaction Menu (Runtime)");
        if (manager == null || !manager.isActiveAndEnabled || menu == null ||
            !menu.gameObject.activeInHierarchy || !IsSundialMenu(menu))
        { Cleanup(); return; }
        var r = ActiveChild(menu, "Return to stand") as RectTransform;
        if (r == null) { Cleanup(); return; }
        if (returnButton != r || buttons.Count != 3 || buttons[0] == null || !buttons[0].gameObject.activeInHierarchy)
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
            var label = r.GetComponentInChildren<TMP_Text>();
            if (label != null) label.rectTransform.sizeDelta = new Vector2(227, oldSize.y - 4);
            earlier = CopyButton(menu, "Sundial Earlier", "Hour -", -75, 135, () => ChangeHour(-1));
            exploreLabel = CopyButton(menu, "Sundial Explore", "Explore", 90, 175, Toggle).GetComponentInChildren<TMP_Text>();
            later = CopyButton(menu, "Sundial Later", "Hour +", 265, 135, () => ChangeHour(1));
            Refresh();
        }
        if (demo != null) PlaceDemo(menu);
    }

    bool IsSundialMenu(Transform menu)
    {
        var t = ActiveChild(menu, "Title");
        if (t == null || ActiveChild(menu, "Grab") == null || manager.exhibits == null) return false;
        var label = t.GetComponent<TMP_Text>();
        if (label == null) return false;
        foreach (var e in manager.exhibits)
            if (e != null && e.interactable != null && e.interactable.name == "Museum_Exhibit_Sundial" && label.text == e.title)
            { sundial = e; return true; }
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
        var obj = Instantiate(returnButton.gameObject, menu, false);
        obj.name = name;
        var r = (RectTransform)obj.transform;
        r.anchoredPosition = new Vector2(x, oldPosition.y);
        r.sizeDelta = new Vector2(width, oldSize.y);
        var t = obj.GetComponentInChildren<TMP_Text>();
        if (t != null) { t.text = label; t.rectTransform.sizeDelta = new Vector2(width - 8, oldSize.y - 4); }
        var b = obj.GetComponent<Button>();
        b.onClick = new Button.ButtonClickedEvent();
        b.onClick.AddListener(action);
        buttons.Add(b);
        return b;
    }

    void Toggle()
    {
        if (exploring) { EndExplore(); return; }
        exploring = true;
        hour = 12;
        var menu = returnButton.parent;
        originalMenuPosition = menu.position;
        menu.position -= menu.up * .29f;
        BuildDemo();
        PlaceDemo(menu);
        Refresh();
    }

    void ChangeHour(int delta)
    {
        if (!exploring) return;
        hour = Mathf.Clamp(hour + delta, 8, 16);
        Refresh();
    }

    void PlaceDemo(Transform menu)
    {
        demo.transform.SetPositionAndRotation(menu.position + menu.up * .36f - menu.forward * .13f,
            menu.rotation * Quaternion.Euler(-32f, 0, 0));
    }

    Material ColorMaterial(Color color)
    {
        var renderer = sundial != null && sundial.visual != null ? sundial.visual.GetComponentInChildren<MeshRenderer>() : null;
        var source = renderer != null ? renderer.sharedMaterial : null;
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var mat = source != null ? new Material(source) : new Material(shader);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", null);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", null);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", .1f);
        materials.Add(mat);
        return mat;
    }

    Transform Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material mat)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(demo.transform, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        var collider = obj.GetComponent<Collider>();
        if (collider != null) { collider.enabled = false; Destroy(collider); }
        var r = obj.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return obj.transform;
    }

    void BuildDemo()
    {
        demo = new GameObject("Sundial Shadow Demo (Runtime)");
        var dial = ColorMaterial(new Color(.83f, .70f, .46f));
        var dark = ColorMaterial(new Color(.16f, .12f, .08f));
        var gold = ColorMaterial(new Color(1f, .72f, .12f));
        var shadowMat = ColorMaterial(new Color(.27f, .22f, .16f));
        // A horizontal dial at y=0, with a 6 cm vertical teaching gnomon.
        Shape("Dial", PrimitiveType.Cylinder, new Vector3(0,-.012f,0), new Vector3(.35f,.012f,.35f), dial);
        Shape("Gnomon", PrimitiveType.Cylinder, new Vector3(0,.03f,0), new Vector3(.012f,.03f,.012f), dark);
        for (int i=0;i<12;i++)
        {
            float angle = i * Mathf.PI / 6f;
            var mark = Shape("Dial mark", PrimitiveType.Cube,
                new Vector3(Mathf.Sin(angle)*.151f,.0015f,Mathf.Cos(angle)*.151f), new Vector3(.004f,.003f,.022f),dark);
            mark.localRotation = Quaternion.Euler(0,i*30f,0);
        }
        sun = Shape("Virtual Sun", PrimitiveType.Sphere, Vector3.zero, Vector3.one*.045f, gold);
        shadow = Shape("Projected shadow", PrimitiveType.Cube, Vector3.zero, Vector3.one, shadowMat);
    }

    void Refresh()
    {
        if (earlier != null) earlier.interactable = exploring && hour > 8;
        if (later != null) later.interactable = exploring && hour < 16;
        if (exploreLabel != null) exploreLabel.text = exploring ? "Hide demo" : "Explore";
        if (!exploring || sun == null || shadow == null) return;
        float a = (hour - 6) * Mathf.PI / 12f;
        Vector3 light = new Vector3(-.19f*Mathf.Cos(a), .19f*Mathf.Sin(a), -.07f);
        sun.localPosition = light;
        // Parallel light rays: project the top of the gnomon onto the dial plane.
        Vector3 end = new Vector3(-light.x,0,-light.z) * (.06f / light.y);
        shadow.localPosition = end*.5f + Vector3.up*.002f;
        shadow.localRotation = Quaternion.LookRotation(end,Vector3.up);
        shadow.localScale = new Vector3(.009f,.003f,end.magnitude);
        if (description != null) description.text = $"Solar time (illustrative): {hour:00}:00\n\nChange the hour to move the virtual Sun. The gnomon's shadow changes direction and length.\n\nA horizontal teaching model, not a calibrated reconstruction of the 1591 instrument.";
    }

    void EndExplore()
    {
        if (exploring && returnButton != null && returnButton.gameObject.activeInHierarchy)
            returnButton.parent.position = originalMenuPosition;
        exploring = false;
        if (demo != null) { demo.SetActive(false); Destroy(demo); }
        demo = null; sun = null; shadow = null;
        foreach (var material in materials) if (material != null) Destroy(material);
        materials.Clear();
        if (description != null) description.text = oldDescription;
        Refresh();
    }

    void Cleanup()
    {
        EndExplore();
        if (returnButton != null)
        {
            returnButton.anchoredPosition = oldPosition;
            returnButton.sizeDelta = oldSize;
            var label = returnButton.GetComponentInChildren<TMP_Text>();
            if (label != null) label.rectTransform.sizeDelta = oldSize - new Vector2(8,8);
        }
        foreach (var b in buttons) if (b != null) { b.gameObject.SetActive(false); Destroy(b.gameObject); }
        buttons.Clear(); returnButton = null; description = null; exploreLabel = null; earlier = null; later = null;
    }

    void OnDisable() { Cleanup(); }
}
