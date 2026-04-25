using System.Collections.Generic;
using NPCChat.Core.DialogueClasses;
using NPCChat.Core.WorldClasses;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays an interaction popup near each NPC the player can interact with.
/// One popup per NPC; hides all when no snapshots are present.
///
/// Setup: assign the Panel prefab and the DialoguePanel in the Inspector.
/// The Panel prefab must have: a "Title" Text child and a "Buttons" VerticalLayoutGroup child.
/// </summary>
public class InteractionMenu : MonoBehaviour
{
    [SerializeField] private GameObject _popupPrefab;
    [SerializeField] private Canvas _worldCanvas;
    [SerializeField] private DialoguePanel _dialoguePanel;
    [SerializeField] private Camera _cam;

    private WorldData _world;
    // Reuse popups per actor to avoid alloc each frame.
    private readonly Dictionary<ObjectHandle, GameObject> _popups = new();

    private void Awake()
    {
        if (_cam == null) _cam = Camera.main;
    }

    private void Start()
    {
        _world = SimulationBootstrap.Instance.World;
    }

    /// <summary>Called by WorldView.Update() with the latest interaction snapshots.</summary>
    public void Refresh(InteractionSnapshot[] snapshots, Grid grid)
    {
        if (snapshots == null || snapshots.Length == 0)
        {
            HideAll();
            return;
        }

        // Track which handles are still active this frame.
        var active = new HashSet<ObjectHandle>();

        foreach (var snap in snapshots)
        {
            active.Add(snap.ActorHandle);

            if (!_popups.TryGetValue(snap.ActorHandle, out var popup) || popup == null)
            {
                popup = CreatePopup();
                _popups[snap.ActorHandle] = popup;
            }

            PositionPopup(popup, snap.ActorBounds, grid);
            PopulatePopup(popup, snap);
            popup.SetActive(true);
        }

        // Hide popups for actors no longer in range.
        foreach (var (handle, popup) in _popups)
        {
            if (!active.Contains(handle))
                popup.SetActive(false);
        }
    }

    private void HideAll()
    {
        foreach (var p in _popups.Values)
            if (p != null) p.SetActive(false);
    }

    private GameObject CreatePopup()
    {
        var popup = Instantiate(_popupPrefab, _worldCanvas.transform);
        return popup;
    }

    private void PositionPopup(GameObject popup, Bounds bounds, Grid grid)
    {
        var worldPos = IsometricUtil.BoundsCenterToWorld(bounds, grid);
        // Offset upward so the popup sits above the character sprite.
        worldPos.y += 1.5f;

        var screenPos = _cam.WorldToScreenPoint(worldPos);
        var rt = popup.GetComponent<RectTransform>();
        if (rt != null)
            rt.position = screenPos;
    }

    private void PopulatePopup(GameObject popup, InteractionSnapshot snap)
    {
        var title = popup.transform.Find("Title")?.GetComponent<Text>();
        if (title != null)
            title.text = snap.ActorName ?? "NPC";

        var buttonsRoot = popup.transform.Find("Buttons");
        if (buttonsRoot == null) return;

        // Clear old buttons.
        foreach (Transform child in buttonsRoot)
            Destroy(child.gameObject);

        foreach (var option in snap.Options)
        {
            var btnGO = new GameObject(option.Label);
            btnGO.transform.SetParent(buttonsRoot);

            var btn = btnGO.AddComponent<Button>();
            var txt = btnGO.AddComponent<Text>();
            txt.text = $"[{option.Index}] {option.Label}";
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 14;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;

            // Layout
            var le = btnGO.AddComponent<LayoutElement>();
            le.minHeight = 24;

            var captured = option;
            var handle   = snap.ActorHandle;
            btn.onClick.AddListener(() => OnOptionSelected(handle, captured));
        }
    }

    private void OnOptionSelected(ObjectHandle actorHandle, InteractionOption option)
    {
        if (_dialoguePanel == null) return;
        if (!_world.TryGetObject(actorHandle, out var obj) || obj is not WorldObjectMoveable actor) return;
        _dialoguePanel.Begin(actor, option, _world);
    }
}
