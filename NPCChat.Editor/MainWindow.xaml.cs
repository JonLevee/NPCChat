using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core.SupportClasses;
using NPCChat.Core.Validation;
using NPCChat.Editor;
using NPCChat.Editor.Persistence;
using NPCChat.Editor.UIClasses;
using NPCChat.Editor.UserControls;
using NPCChatLib.Builders;
using NPCChatLib.Extensions;
using NPCChatLib.WorldBuilderTemplates;
using NPCChatLib.WorldClasses;
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

        private string GridCoordLabel(Point screenPoint)
        {
            if (_gridRenderer.IsoTransform is null) return screenPoint.ToString();
            var (gx, gy) = _gridRenderer.IsoTransform.ScreenToGrid(screenPoint);
            return $"({gx}, {gy})";
        }
    }
}