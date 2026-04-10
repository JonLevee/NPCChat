using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core.DialogueClasses;
using NPCChat.Core.SupportClasses;
using NPCChat.Core.Validation;
using NPCChat.Editor;
using NPCChat.Editor.Persistence;
using NPCChat.Editor.UIClasses;
using NPCChat.Editor.UserControls;
using NPCChat.Core.Builders;
using NPCChat.Core.Extensions;
using NPCChat.Core.WorldBuilderTemplates;
using NPCChat.Core.WorldClasses;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Windows.Point;

namespace NPCChat
{
    public partial class MainWindow : Window
    {
        private WorldData _world = null!;
        private WorldDataBuilder _worldBuilder = null!;
        private IServiceScope serviceScope = null!;

        private readonly DispatcherTimer _gameTimer = new();

        private bool _autoStartGame = true;
        private string originalTitle = string.Empty;
        private Point lastMousePosition;
        private Point lastMouseDownPosition;

        private readonly UserSettingsRepository _userSettingsRepository;
        private readonly ChunkInfo _chunkInfo;
        private GridRenderer _gridRenderer = null!;
        private readonly Dictionary<ObjectHandle, CharacterSummary> _summaryMap = new();

        // Dialogue state
        private DialogueSession? _dialogueSession;


        public MainWindow(UserSettingsRepository userSettingsRepository, ChunkInfo chunkInfo)
        {
            _userSettingsRepository = userSettingsRepository;
            _chunkInfo = chunkInfo;
            InitializeComponent();

            Loaded += (s, e) => _userSettingsRepository.RestoreWindow(this);
            Closing += (s, e) => _userSettingsRepository.SaveWindow(this);

            originalTitle = Title;
            this.LocationChanged += (s, e) => UpdateTitle();
            this.SizeChanged += (s, e) => UpdateTitle();

            DataContext = this;
            _gridRenderer = new GridRenderer(this, chunkInfo);
            _gridRenderer.ObjectHovered += obj =>
            {
                foreach (var cs in _summaryMap.Values)
                    cs.SetHighlighted(false);
                if (obj is not null && _summaryMap.TryGetValue(obj.Handle, out var summary))
                    summary.SetHighlighted(true);
            };

            _gridRenderer.InteractionOptionSelected += OnInteractionOptionSelected;

            Title = "NPCChat Sandbox - Map View";
            _gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            _gameTimer.Tick += _gameTimer_Tick;

            CellSizeListBox.Items.Clear();
            _chunkInfo.ChunkSizes.ForEach(size => CellSizeListBox.Items.Add(size));
            CellSizeListBox.SelectedItem = _chunkInfo.ChunkSize;

            _gameTimer.Start();
        }

        private void UpdateTitle()
        {
            Title = $"{originalTitle} - {_userSettingsRepository.GetUserSettingsText(this)} Clicked = {GridCoordLabel(lastMouseDownPosition)} Mouse = {GridCoordLabel(lastMousePosition)}";
        }

        private void _gameTimer_Tick(object? sender, EventArgs e)
        {
            if (_autoStartGame)
            {
                _autoStartGame = false;
                StartStopButton_Click(this, new RoutedEventArgs());
                BuildDemoTownButton_Click(this, new RoutedEventArgs());
            }

            // Surface any simulation fault on the UI thread so it is not silently swallowed.
            if (_world?.SimulationFault is { } fault)
                throw new AggregateException("Simulation loop faulted.", fault);

            if (_world is not null)
            {
                _gridRenderer.UpdateMoveablePositions(_world.SnapshotMoveablePositions());

                var selected = _gridRenderer.SelectedHandle;
                if (selected != ObjectHandle.None)
                    _gridRenderer.DrawPathPreview(_world.SnapshotPath(selected));
                else
                    _gridRenderer.ClearPathPreview();

                // Interaction overlays — hidden while a dialogue is open.
                if (_dialogueSession is null)
                    _gridRenderer.UpdateInteractionOverlays(_world.SnapshotInteractions());
                else
                    _gridRenderer.ClearInteractionOverlays();

                // Update actor debug info (Mode/Task) for all NPC summaries.
                foreach (var (handle, cs) in _summaryMap)
                {
                    if (_world.TryGetObject(handle, out var obj) && obj is WorldObjectMoveable m)
                        cs.UpdateActorDebugInfo(m.Actor);
                }
            }
        }

        private void ClearWorld()
        {
            _world.Clear();
        }

        private void CreateWorldScope()
        {
            Require.IsNull(serviceScope);

            serviceScope = App.Services.CreateScope();
            _worldBuilder = serviceScope.ServiceProvider.GetRequiredService<WorldDataBuilder>();
            _world = serviceScope.ServiceProvider.GetRequiredService<WorldData>();
            _world.StartSimulationProcessing();
            RefreshWorld();
        }

        private void RefreshWorld()
        {
            _gridRenderer.RenderWorld(_world);

            var objects = _world.EnumerateWorldObjects();
            var player = objects.FirstOrDefault(o => o.Kind == WorldObjectKind.Player);
            var npcs = objects
                .Where(o => o.Kind == WorldObjectKind.NPC)
                .OrderBy(o => o.Category);

            _summaryMap.Clear();
            WorldObjectPanel.Children.Clear();

            if (player is not null)
                AddSummary(player);
            foreach (var obj in npcs)
                AddSummary(obj);
        }

        private void AddSummary(WorldObject obj)
        {
            var cs = new CharacterSummary(obj);
            cs.MouseEnter += (s, e) => { _gridRenderer.HighlightObject(obj.Handle); cs.SetHighlighted(true); };
            cs.MouseLeave += (s, e) => { _gridRenderer.ClearHighlight(); cs.SetHighlighted(false); };
            _summaryMap[obj.Handle] = cs;
            WorldObjectPanel.Children.Add(cs);
        }

        private void ClearWorldScope()
        {
            if (serviceScope != null)
            {
                serviceScope?.Dispose();
                serviceScope = null!;
                MapCanvas.Children.Clear();
                StatusTextBlock.Text = string.Empty;
                WorldObjectPanel.Children.Clear();
                _summaryMap.Clear();
                _world.Dispose();
                _world = null!;
                _worldBuilder = null!;
                BuildDemoTownButton.IsEnabled = false;
                RedrawButton.IsEnabled = false;
                CellSizeListBox.IsEnabled = true;
            }
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            switch (StartStopButton.Content)
            {
                case "Start":
                    CreateWorldScope();
                    BuildDemoTownButton.IsEnabled = true;
                    RedrawButton.IsEnabled = true;
                    CellSizeListBox.IsEnabled = false;
                    StartStopButton.Content = "Stop";
                    break;
                case "Stop":
                    ClearWorldScope();
                    BuildDemoTownButton.IsEnabled = false;
                    RedrawButton.IsEnabled = false;
                    CellSizeListBox.IsEnabled = true;
                    StartStopButton.Content = "Start";
                    break;
            }
        }

        private void BuildDemoTownButton_Click(object sender, RoutedEventArgs e)
        {
            ClearWorld();

            using var templates = _worldBuilder.GetTemplates();
            templates.AddSmallTown();

            RefreshWorld();
        }

        private void RedrawButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshWorld();
        }

        private void CellSizeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CellSizeListBox.SelectedItem is int size)
                _chunkInfo.ChunkSize = size;
        }

        private void MapCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lastMouseDownPosition = e.GetPosition(this.MapCanvas);
            UpdateTitle();
        }

        private void MapCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lastMousePosition = e.GetPosition(this.MapCanvas);
            UpdateTitle();
        }

        private void MapCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_world is null || _gridRenderer.IsoTransform is null) return;

            var selected = _gridRenderer.SelectedHandle;
            if (selected == ObjectHandle.None) return;
            if (!_world.TryGetObject(selected, out var obj) || obj is not WorldObjectMoveable) return;

            var screenPos = e.GetPosition(MapCanvas);
            var (gx, gy) = _gridRenderer.IsoTransform.ScreenToGrid(screenPos);
            _world.EnqueueMoveCommand(new MoveCommand(selected, new System.Drawing.Point(gx, gy)));

            e.Handled = true;
        }

        private string GridCoordLabel(Point screenPoint)
        {
            if (_gridRenderer.IsoTransform is null) return screenPoint.ToString();
            var (gx, gy) = _gridRenderer.IsoTransform.ScreenToGrid(screenPoint);
            return $"({gx}, {gy})";
        }

        // ── Dialogue ──────────────────────────────────────────────────────────

        private void OnInteractionOptionSelected(ObjectHandle actorHandle, InteractionOption option)
        {
            if (_world is null) return;
            if (!_world.TryGetObject(actorHandle, out var obj) || obj is not WorldObjectMoveable actor) return;
            if (actor.Actor?.DialogueTree is null) return;

            // Close any existing session before starting a new one.
            CloseDialogue();

            _dialogueSession = new DialogueSession(actor, option.NodeId);
            var ctx = BuildDialogueContext(actor);
            _dialogueSession.Advance(ctx);
            RefreshDialoguePanel();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_dialogueSession is null) return;

            var ctx = BuildDialogueContext(_dialogueSession.Actor);

            switch (e.Key)
            {
                case Key.Escape:
                    CloseDialogue();
                    e.Handled = true;
                    break;

                case Key.Space:
                case Key.Enter:
                    if (_dialogueSession.State == DialogueSessionState.NpcLine)
                    {
                        _dialogueSession.Advance(ctx);
                        RefreshDialoguePanel();
                        e.Handled = true;
                    }
                    break;

                case >= Key.D1 and <= Key.D9:
                    if (_dialogueSession.State == DialogueSessionState.PlayerChoice)
                    {
                        int idx = e.Key - Key.D1 + 1;
                        _dialogueSession.Select(idx, ctx);
                        RefreshDialoguePanel();
                        e.Handled = true;
                    }
                    break;

                case >= Key.NumPad1 and <= Key.NumPad9:
                    if (_dialogueSession.State == DialogueSessionState.PlayerChoice)
                    {
                        int idx = e.Key - Key.NumPad1 + 1;
                        _dialogueSession.Select(idx, ctx);
                        RefreshDialoguePanel();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void RefreshDialoguePanel()
        {
            if (_dialogueSession is null) return;

            if (_dialogueSession.State == DialogueSessionState.Complete)
            {
                CloseDialogue();
                return;
            }

            DialoguePanelControl.Refresh(_dialogueSession);
        }

        private void CloseDialogue()
        {
            _dialogueSession = null;
            DialoguePanelControl.Hide();
        }

        private DialogueContext BuildDialogueContext(WorldObjectMoveable actor)
        {
            return new DialogueContext
            {
                Actor    = actor,
                Player   = _world?.GetPlayer(),
                GameHour = _world?.CurrentGameHour ?? 0,
                GameTick = _world?.CurrentGameTick ?? 0
            };
        }
    }
}
