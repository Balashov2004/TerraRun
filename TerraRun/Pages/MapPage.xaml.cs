using Mapsui.Tiling;
using TerraRun.Services;
using TerraRun.Models;
using TerraRun.Helpers;
using Mapsui.UI.Maui;

namespace TerraRun.Pages;

public partial class MapPage
{
    private bool _isTracking;
    private int? _currentRunId;
    private readonly RunService _runService;
    private readonly GpsService _gpsService = new();
    private readonly Mapsui.Layers.WritableLayer _routeLayer = new();
    
    public MapPage()
    {
        InitializeComponent();
        _runService = new RunService();
        SetupMap();
    }

    private void SetupMap()
    {
        Dispatcher.Dispatch(async () =>
        {
            MapView.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapView.Map.Layers.Add(_routeLayer);
            
            MapView.Map.Navigator.ZoomToLevel(15);
            MapView.MyLocationLayer.Enabled = true;
            
            if (!await _gpsService.CheckAndRequestPermissions())
            {
                await DisplayAlert("Ошибка", "Без GPS мы не работаем", "ОК");
            }

            await RefreshMapCells();
            _ = Task.Run(() => ContinuousLocationUpdate());
        });
    }

    private async Task ContinuousLocationUpdate()
    {
        while (true)
        {
            var location = await _gpsService.GetCurrentLocation();
            if (location != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MapView.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude));
                    var sm = Mapsui.Projections.SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                    MapView.Map.Navigator.CenterOn(sm.x, sm.y);
                });

                if (_isTracking && _currentRunId.HasValue)
                {
                    var response = await _runService.CaptureCell(_currentRunId.Value, location.Latitude, location.Longitude);
                    if (response?.Boundary != null) await RefreshMapCells();
                }
            }
            await Task.Delay(5000);
        }
    }

    private async Task RefreshMapCells()
    {
        var allCells = await _runService.GetAllCapturedCells();
        if (allCells == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _routeLayer.Clear();
            foreach (var cell in allCells)
            {
                var color = MapHelper.GetCellColor(cell.OwnerUserId, UserSession.LoggedInUserId);
                MapHelper.AddCellToLayer(_routeLayer, cell.Boundary, color);
            }
            _routeLayer.DataHasChanged();
        });
    }
    
    private async void OnRunButtonClicked(object sender, EventArgs e)
    {
        if (_isTracking)
        {
            _isTracking = false;
            if (_currentRunId != null) await _runService.StopRun(_currentRunId.Value);
            RunButton.Text = "Начать пробежку";
            RunButton.BackgroundColor = Colors.Orange;
        }
        else
        {
            var runId = await _runService.StartRun(UserSession.LoggedInUserId.Value);
            if (runId.HasValue)
            {
                _currentRunId = runId.Value;
                _isTracking = true;
                RunButton.Text = "Стоп";
                RunButton.BackgroundColor = Colors.Red;
            }
        }
    }

    private async void OnProfileClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(ProfilePage));
    private async void OnMenuClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(MenuPage));
}