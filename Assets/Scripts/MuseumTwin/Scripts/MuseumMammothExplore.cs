using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Extension for the existing, original View & Grab menu. Do not replace its script.
[DisallowMultipleComponent]
[RequireComponent(typeof(MuseumExhibitInteractions))]
public sealed class MuseumMammothExplore : MonoBehaviour
{
    public GameObject mammothReconstruction;
    MuseumExhibitInteractions manager;
    RectTransform returnButton;
    Vector2 oldReturnPosition;
    Button exploreButton;
    TMP_Text exploreLabel;

    void Awake()
    {
        manager = GetComponent<MuseumExhibitInteractions>();
        HideMammoth();
        if (mammothReconstruction == null)
            Debug.LogWarning("Mammoth Explore: assign Mammoth Reconstruction in the Inspector.");
    }

    void LateUpdate()
    {
        Transform menu = transform.Find("Exhibit Interaction Menu (Runtime)");
        if (mammothReconstruction == null || manager == null || !manager.isActiveAndEnabled ||
            menu == null || !menu.gameObject.activeInHierarchy || !IsMammothMenu(menu))
        {
            RemoveButton();
            HideMammoth();
            return;
        }

        // The original manager recreates menu controls when opening a new view.
        var currentReturn = ActiveChild(menu, "Return to stand") as RectTransform;
        if (currentReturn == null) { RemoveButton(); HideMammoth(); return; }
        if (exploreButton != null && returnButton == currentReturn && exploreButton.gameObject.activeInHierarchy)
            return;

        RemoveButton();
        returnButton = currentReturn;
        oldReturnPosition = returnButton.anchoredPosition;
        returnButton.anchoredPosition = oldReturnPosition + new Vector2(-135f, 0f);
        var copy = Instantiate(returnButton.gameObject, menu, false);
        copy.name = "Explore Mammoth";
        ((RectTransform)copy.transform).anchoredPosition = oldReturnPosition + new Vector2(135f, 0f);
        exploreButton = copy.GetComponent<Button>();
        // Replace the cloned Return-to-stand listener; do not touch the original button.
        exploreButton.onClick = new Button.ButtonClickedEvent();
        exploreButton.onClick.AddListener(ToggleMammoth);
        exploreLabel = copy.GetComponentInChildren<TMP_Text>(true);
        RefreshLabel();
    }

    bool IsMammothMenu(Transform menu)
    {
        Transform title = ActiveChild(menu, "Title");
        if (title == null || ActiveChild(menu, "Grab") == null || manager.exhibits == null) return false;
        var label = title.GetComponent<TMP_Text>();
        if (label == null) return false;
        foreach (var e in manager.exhibits)
            if (e != null && e.interactable != null && e.interactable.name == "Museum_Exhibit_Mammoth")
                return label.text == e.title;
        return false;
    }

    static Transform ActiveChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
            if (child.name == childName && child.gameObject.activeInHierarchy) return child;
        return null;
    }

    void ToggleMammoth()
    {
        if (mammothReconstruction == null) return;
        mammothReconstruction.SetActive(!mammothReconstruction.activeSelf);
        RefreshLabel();
    }

    void RefreshLabel()
    {
        if (exploreLabel != null && mammothReconstruction != null)
            exploreLabel.text = mammothReconstruction.activeSelf ? "Hide mammoth" : "Explore";
    }

    void HideMammoth()
    {
        if (mammothReconstruction != null && mammothReconstruction.activeSelf)
            mammothReconstruction.SetActive(false);
    }

    void RemoveButton()
    {
        if (returnButton != null) returnButton.anchoredPosition = oldReturnPosition;
        if (exploreButton != null)
        {
            exploreButton.gameObject.SetActive(false);
            Destroy(exploreButton.gameObject);
        }
        returnButton = null;
        exploreButton = null;
        exploreLabel = null;
    }

    void OnDisable() { RemoveButton(); HideMammoth(); }
}
