using NPCChat.Core.DialogueClasses;
using NPCChat.Core.WorldClasses;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen (or anchored) dialogue panel. Shows NPC lines and player choices.
/// Wire up the Text and Buttons root in the Inspector.
///
/// Keyboard: Space/Enter advances; 1-9 select choices; Escape closes.
/// </summary>
public class DialoguePanel : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Text _speakerText;
    [SerializeField] private Text _lineText;
    [SerializeField] private Transform _choicesRoot;
    [SerializeField] private GameObject _choiceButtonPrefab;

    private DialogueSession _session;
    private WorldData _world;
    private WorldObjectMoveable _actor;

    private void Awake()
    {
        if (_root != null) _root.SetActive(false);
    }

    public void Begin(WorldObjectMoveable actor, InteractionOption option, WorldData world)
    {
        if (actor.Actor?.DialogueTree == null) return;

        _world  = world;
        _actor  = actor;
        _session = new DialogueSession(actor, option.NodeId);
        _session.Advance(BuildContext());
        Show();
    }

    private void Show()
    {
        if (_root != null) _root.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        _session = null;
        if (_root != null) _root.SetActive(false);
    }

    private void Update()
    {
        if (_session == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        if (_session.State == DialogueSessionState.NpcLine
            && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            _session.Advance(BuildContext());
            Refresh();
            return;
        }

        if (_session.State == DialogueSessionState.PlayerChoice)
        {
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    _session.Select(i, BuildContext());
                    Refresh();
                    return;
                }
            }
        }
    }

    private void Refresh()
    {
        if (_session == null) return;

        if (_session.State == DialogueSessionState.Complete)
        {
            Close();
            return;
        }

        if (_speakerText != null)
            _speakerText.text = _actor.Character?.Name ?? "???";

        if (_lineText != null)
            _lineText.text = _session.DisplayText;

        // Clear old choices.
        if (_choicesRoot != null)
        {
            foreach (Transform child in _choicesRoot)
                Destroy(child.gameObject);
        }

        if (_session.State == DialogueSessionState.PlayerChoice
            && _choicesRoot != null && _choiceButtonPrefab != null)
        {
            foreach (var (idx, choice) in _session.VisibleChoices)
            {
                var go  = Instantiate(_choiceButtonPrefab, _choicesRoot);
                var txt = go.GetComponentInChildren<Text>();
                if (txt != null) txt.text = $"[{idx}] {choice.Label}";

                var btn     = go.GetComponent<Button>();
                var captured = idx;
                if (btn != null)
                    btn.onClick.AddListener(() =>
                    {
                        _session.Select(captured, BuildContext());
                        Refresh();
                    });
            }
        }
    }

    private DialogueContext BuildContext()
    {
        var player = _world?.GetPlayer();
        var handle = player?.Handle ?? ObjectHandle.None;
        return new DialogueContext
        {
            Actor               = _actor,
            Player              = player,
            GameHour            = _world?.CurrentGameHour ?? 0,
            GameTick            = _world?.CurrentGameTick ?? 0,
            GetPlayerItemCount  = id => _world?.SnapshotInventoryCount(handle, id) ?? 0,
            GetPlayerReputation = fid => player?.ReputationLog?.GetReputation(fid) ?? 0
        };
    }
}
