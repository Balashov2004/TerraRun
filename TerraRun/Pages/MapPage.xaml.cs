using Mapsui.Tiling;
using TerraRun.Services;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using TerraRun.Models;
using Color = Mapsui.Styles.Color;

namespace TerraRun.Pages;

public partial class MapPage
{
    private bool _isTracking;
    private int? _currentRunId;
    private readonly RunService _runService = new();
    private readonly Mapsui.Layers.WritableLayer _routeLayer = new();
    
    public MapPage()
    {
        InitializeComponent();

        Dispatcher.Dispatch(async () =>
        {
            var tileLayer = OpenStreetMap.CreateTileLayer();
            MapView.Map.Layers.Add(tileLayer);
            MapView.Map.Layers.Add(_routeLayer);
            
            MapView.Map.Navigator.ZoomToLevel(15);
            MapView.MyLocationLayer.Enabled = true;
            
            RequestLocation();
            await RefreshMapCells();
            
            _ = Task.Run(() => ContinuousLocationUpdate());
        });
    }

    private async Task ContinuousLocationUpdate()
    {
        while (true)
        {
            try
            {
                var location = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.High));

                if (location != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MapView.MyLocationLayer.UpdateMyLocation(
                            new Mapsui.UI.Maui.Position(location.Latitude, location.Longitude));
                        
                        var sm = Mapsui.Projections.SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                        MapView.Map.Navigator.CenterOn(sm.x, sm.y);
                    });

                    if (_isTracking && _currentRunId.HasValue)
                    {
                        var response = await _runService.CaptureCell(_currentRunId.Value, location.Latitude, location.Longitude);
                        if (response?.Boundary != null)
                        {
                            await RefreshMapCells();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GPS Loop Error: {ex.Message}");
            }

            await Task.Delay(1000);
        }
    }

    private async Task RefreshMapCells()
    {
        try
        {
            var allCells = await _runService.GetAllCapturedCells();
            if (allCells == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _routeLayer.Clear();

                foreach (var cell in allCells)
                {
                    var cellColor = (cell.OwnerUserId == UserSession.LoggedInUserId) 
                        ? Color.Green 
                        : Color.Red;

                    AddCellToLayer(cell.Boundary, cellColor);
                }
                _routeLayer.DataHasChanged();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing cells: {ex.Message}");
        }
    }
    
    private void AddCellToLayer(List<BoundaryPoint> boundary, Color cellColor)
    {
        if (boundary == null || boundary.Count < 3) return;

        try
        {
            var points = boundary
                .Select(p => Mapsui.Projections.SphericalMercator.FromLonLat(p.Lon, p.Lat))
                .Select(pr => new Coordinate(pr.x, pr.y))
                .ToList();
            
            if (points[0] != points[^1]) 
                points.Add(new Coordinate(points[0].X, points[0].Y));

            var feature = new GeometryFeature
            {
                Geometry = new Polygon(new LinearRing(points.ToArray()))
            };

            feature.Styles.Add(new VectorStyle
            {
                Fill = new Mapsui.Styles.Brush(cellColor),
                Outline = new Pen(Color.Orange, 1)
            });

            _routeLayer.Add(feature);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Feature Add Error: {ex.Message}");
        }
    }

    private async Task RequestLocation()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Ошибка", "Без GPS мы не работаем", "ОК");
        }
    }

    private async void OnRunButtonClicked(object sender, EventArgs e)
    {
        if (_isTracking)
        {
            _isTracking = false;
            if (_currentRunId != null) await _runService.StopRun(_currentRunId.Value);
            RunButton.Text = "Начать пробежку";
            RunButton.BackgroundColor = Colors.Orange;
            return;
        }

        try
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
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }

    private async void OnMenuClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MenuPage));
    }
}