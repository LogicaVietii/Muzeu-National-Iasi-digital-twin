using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Runtime component. Keep in the project: the installed scene depends on it.
public sealed class MuseumExhibitInteractions : MonoBehaviour
{
    [Serializable]
    public sealed class Exhibit
    {
        public string title;
        public XRGrabInteractable interactable;
        public Transform visual;
        public TMP_Text existingDescription;
        public Rigidbody body;
        [NonSerialized] public Vector3 homePosition;
        [NonSerialized] public Quaternion homeRotation;
        [NonSerialized] public bool armed, held;
    }

    public Exhibit[] exhibits;
    public Camera xrCamera;
    private Exhibit current;
    private GameObject canvasObject, preview;
    private RectTransform layout;
    private Quaternion viewRotation;
    private Vector3 viewPosition;
    private Vector3 previewBaseScale;
    private float zoom = 1f, yaw, pitch;
    private TMP_FontAsset font;

    void Start()
    {
        foreach (Exhibit e in exhibits)
        {
            if (e.interactable == null || e.body == null) continue;
            e.homePosition = e.interactable.transform.position;
            e.homeRotation = e.interactable.transform.rotation;
            SetTracking(e, false);
            e.body.isKinematic = true;
            e.body.useGravity = false;
            e.interactable.selectEntered.AddListener(_ => OnSelected(e));
            e.interactable.selectExited.AddListener(_ => OnReleased(e));
            if (font == null && e.existingDescription != null) font = e.existingDescription.font;
        }
        if (font == null) font = TMP_Settings.defaultFontAsset;
        BuildCanvas();
    }

    void SetTracking(Exhibit e, bool enabled)
    {
        e.interactable.trackPosition = enabled;
        e.interactable.trackRotation = enabled;
        e.interactable.trackScale = false;
    }

    void OnSelected(Exhibit e)
    {
        if (e.armed)
        {
            e.held = true;
            HideUI();
            return;
        }
        // Do not interrupt another exhibit while actually holding it.
        foreach (var other in exhibits) if (other.held) return;
        if (current != null && current != e) CancelArmed(current);
        current = e;
        OpenMenu(true);
    }

    void OnReleased(Exhibit e)
    {
        if (!e.held) return; // Menu remains open after releasing the initial selection.
        e.held = false;
        e.armed = false;
        SetTracking(e, false);
        // XRGrabInteractable completes detach at the end of the frame.
        StartCoroutine(FinishDrop(e));
    }

    IEnumerator FinishDrop(Exhibit e)
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        if (e.armed || e.held || e.interactable.isSelected) yield break;
        // Actual object falls onto the floor/stand instead of floating mid-air.
        e.body.isKinematic = false;
        e.body.useGravity = true;
        e.body.linearVelocity = Vector3.zero;
        e.body.angularVelocity = Vector3.zero;
        // A lost object can never leave the museum permanently.
    }

    void LateUpdate()
    {
        if (exhibits == null) return;
        foreach (var e in exhibits)
            if (e.interactable != null && !e.held && !e.armed && !e.interactable.isSelected &&
                (e.interactable.transform.position.y < -3f ||
                 Vector3.Distance(e.interactable.transform.position, e.homePosition) > 45f))
                Restore(e);
    }

    void BuildCanvas()
    {
        canvasObject = new GameObject("Exhibit Interaction Menu (Runtime)", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(TrackedDeviceGraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var c = canvasObject.GetComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.worldCamera = GetCamera();
        c.sortingOrder = 40;
        layout = (RectTransform)canvasObject.transform;
        layout.sizeDelta = new Vector2(820, 440);
        layout.localScale = Vector3.one * .001f;
        canvasObject.SetActive(false);
    }

    Camera GetCamera()
    {
        if (xrCamera != null) return xrCamera;
        xrCamera = Camera.main;
        if (xrCamera == null) xrCamera = FindFirstObjectByType<Camera>();
        return xrCamera;
    }

    bool AnchorToVisitor()
    {
        Camera c = GetCamera();
        if (c == null) { Debug.LogWarning("MuseumTwin: no XR camera found."); return false; }
        viewRotation = Quaternion.LookRotation(c.transform.forward, Vector3.up);
        viewPosition = c.transform.position + c.transform.forward * 1.15f;
        canvasObject.GetComponent<Canvas>().worldCamera = c;
        return true;
    }

    void PlaceCanvas(float y, float height)
    {
        layout.sizeDelta = new Vector2(820, height);
        layout.SetPositionAndRotation(viewPosition + viewRotation * new Vector3(0, y, 0), viewRotation);
    }

    void ClearContent()
    {
        for (int i = layout.childCount - 1; i >= 0; i--)
        {
            var obj = layout.GetChild(i).gameObject;
            obj.SetActive(false);
            Destroy(obj);
        }
        DestroyPreview();
    }

    void DestroyPreview()
    {
        if (preview != null) { preview.SetActive(false); Destroy(preview); preview = null; }
    }

    void HideUI()
    {
        DestroyPreview();
        if (canvasObject != null) canvasObject.SetActive(false);
    }

    RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(.5f, .5f);
        r.pivot = new Vector2(.5f, .5f);
        r.anchoredPosition = position;
        r.sizeDelta = size;
        return r;
    }

    void Background(float height)
    {
        var r = Rect("Background", layout, Vector2.zero, new Vector2(820, height));
        var image = r.gameObject.AddComponent<Image>();
        image.color = new Color(.055f, .085f, .12f, .97f);
        image.raycastTarget = false;
    }

    TMP_Text Label(string name, Transform parent, string value, Vector2 p, Vector2 size, float fontSize,
        TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        RectTransform r = Rect(name, parent, p, size);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = value;
        t.color = Color.white;
        t.fontSize = fontSize;
        t.enableAutoSizing = true;
        t.fontSizeMin = Mathf.Min(19, fontSize);
        t.fontSizeMax = fontSize;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    void Button(string label, Vector2 p, Vector2 size, UnityAction action)
    {
        var r = Rect(label, layout, p, size);
        var img = r.gameObject.AddComponent<Image>();
        img.color = new Color(.16f, .38f, .48f, 1);
        var button = r.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(.72f, 1f, .9f);
        colors.pressedColor = new Color(.55f, .8f, .8f);
        button.colors = colors;
        var nav = button.navigation;
        nav.mode = Navigation.Mode.None;
        button.navigation = nav;
        button.onClick.AddListener(action);
        Label("Text", r, label, Vector2.zero, size - new Vector2(8, 8), 27);
    }

    string Description(Exhibit e) => e.existingDescription != null ? e.existingDescription.text : e.title;

    void OpenMenu(bool reanchor)
    {
        if (current == null || (reanchor && !AnchorToVisitor())) return;
        ClearContent();
        PlaceCanvas(-.035f, 440);
        Background(440);
        Label("Title", layout, current.title, new Vector2(0, 172), new Vector2(760, 64), 36);
        Label("Description", layout, Description(current), new Vector2(0, 28), new Vector2(750, 210), 26,
            TextAlignmentOptions.TopLeft);
        // Initial Grip must be released before using the trigger to click the UI.
        Button("View", new Vector2(-193, -145), new Vector2(176, 76), OpenViewer);
        Button("Grab", new Vector2(0, -145), new Vector2(176, 76), ArmGrab);
        Button("Close", new Vector2(193, -145), new Vector2(176, 76), Close);
        Button("Return to stand", new Vector2(0, -198), new Vector2(250, 34), ResetCurrent);
        canvasObject.SetActive(true);
    }

    void Close()
    {
        if (current != null) CancelArmed(current);
        HideUI();
        current = null;
    }

    void OpenViewer()
    {
        if (current == null || current.interactable.isSelected) return;
        ClearContent();
        PlaceCanvas(-.29f, 280);
        Background(280);
        Label("Title", layout, current.title + " | View", new Vector2(0, 100), new Vector2(780, 55), 32);
        Label("Help", layout, "Rotate the 3D copy and zoom. The original stays on display.", new Vector2(0, 56), new Vector2(780, 40), 23);
        Button("Left", new Vector2(-315, -6), new Vector2(145, 64), () => Rotate(-30, 0));
        Button("Right", new Vector2(-157, -6), new Vector2(145, 64), () => Rotate(30, 0));
        Button("Up", new Vector2(0, -6), new Vector2(145, 64), () => Rotate(0, -20));
        Button("Down", new Vector2(157, -6), new Vector2(145, 64), () => Rotate(0, 20));
        Button("Reset", new Vector2(315, -6), new Vector2(145, 64), ResetPreview);
        Button("Zoom -", new Vector2(-250, -86), new Vector2(185, 64), () => Zoom(.8f));
        Button("Zoom +", new Vector2(-50, -86), new Vector2(185, 64), () => Zoom(1.25f));
        Button("Back", new Vector2(150, -86), new Vector2(185, 64), () => OpenMenu(false));
        Button("Close", new Vector2(330, -86), new Vector2(145, 64), Close);
        canvasObject.SetActive(true);
        MakePreview();
    }

    void MakePreview()
    {
        // Copy *only* mesh geometry and materials, no collider, XR component, script, or rigidbody.
        preview = new GameObject("Exhibit Preview (Runtime)");
        var model = new GameObject("Centered Model");
        model.transform.SetParent(preview.transform, false);
        Vector3 origin = current.visual.position;
        bool valid = false;
        Bounds bounds = default;
        foreach (var mf in current.visual.GetComponentsInChildren<MeshFilter>(true))
        {
            var original = mf.GetComponent<MeshRenderer>();
            if (mf.sharedMesh == null || original == null || !original.enabled || !mf.gameObject.activeInHierarchy) continue;
            GameObject copy = new GameObject(mf.name, typeof(MeshFilter), typeof(MeshRenderer));
            copy.transform.SetParent(model.transform, false);
            copy.transform.localPosition = mf.transform.position - origin;
            copy.transform.localRotation = mf.transform.rotation;
            copy.transform.localScale = mf.transform.lossyScale;
            copy.GetComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var renderer = copy.GetComponent<MeshRenderer>();
            renderer.sharedMaterials = original.sharedMaterials;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            if (!valid) { bounds = renderer.bounds; valid = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        if (!valid) { DestroyPreview(); Debug.LogWarning("MuseumTwin: no preview mesh found."); return; }
        model.transform.localPosition = -bounds.center;
        float diameter = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        previewBaseScale = Vector3.one * (.32f / Mathf.Max(diameter, .001f));
        preview.transform.position = viewPosition + viewRotation * new Vector3(0, .22f, -.10f);
        ResetPreview();
    }

    void Rotate(float y, float p) { yaw += y; pitch = Mathf.Clamp(pitch+p, -80f,80f); ApplyPreview(); }
    void Zoom(float factor) { zoom = Mathf.Clamp(zoom*factor, .55f, 1.55f); ApplyPreview(); }
    void ResetPreview() { zoom = 1; yaw = pitch = 0; ApplyPreview(); }
    void ApplyPreview()
    {
        if (preview == null) return;
        preview.transform.rotation = viewRotation * Quaternion.Euler(pitch, yaw, 0);
        preview.transform.localScale = previewBaseScale * zoom;
        // Changing visual size simulates close inspection, without moving the user's camera.
    }

    void ArmGrab()
    {
        if (current == null || current.interactable.isSelected) return;
        current.armed = true;
        SetTracking(current, true);
        ClearContent();
        // Keep the hint below eye level so it does not block the ray towards the real exhibit.
        PlaceCanvas(-.36f, 200);
        Background(200);
        Label("Instructions", layout, "Grab ready: point at the ORIGINAL exhibit, hold Grip, move the controller, then release.",
            new Vector2(0,35), new Vector2(760,90), 28);
        Button("Cancel", new Vector2(0,-56), new Vector2(230,64), () => { CancelArmed(current); OpenMenu(false); });
        canvasObject.SetActive(true);
    }

    void CancelArmed(Exhibit e)
    {
        if (e == null || e.held) return;
        e.armed = false;
        SetTracking(e, false);
    }

    void ResetCurrent()
    {
        if (current == null || current.interactable.isSelected || current.held) return;
        CancelArmed(current);
        Restore(current);
        OpenMenu(true);
    }

    void Restore(Exhibit e)
    {
        if (!e.body.isKinematic) { e.body.linearVelocity = Vector3.zero; e.body.angularVelocity = Vector3.zero; }
        e.body.isKinematic = true;
        e.body.useGravity = false;
        e.body.position = e.homePosition;
        e.body.rotation = e.homeRotation;
        e.interactable.transform.SetPositionAndRotation(e.homePosition, e.homeRotation);
        Physics.SyncTransforms();
    }

    void OnDestroy() { DestroyPreview(); }
}
