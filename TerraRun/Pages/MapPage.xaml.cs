using Mapsui.Tiling;
using TerraRun.Services;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
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
            await LoadAllCapturedCells();

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

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MapView.MyLocationLayer.UpdateMyLocation(
                        new Mapsui.UI.Maui.Position(location.Latitude, location.Longitude));
                    var sm = Mapsui.Projections.SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                    MapView.Map.Navigator.CenterOn(sm.x, sm.y);
                });

                if (_isTracking && _currentRunId.HasValue)
                {
                    var response =
                        await _runService.CaptureCell(_currentRunId.Value, location.Latitude, location.Longitude);
                    if (response?.Boundary != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() => { DrawServerBoundary(response.Boundary, Color.Green); });
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


    private async Task RequestLocation()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        else
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

    private void DrawServerBoundary(List<BoundaryPoint> boundary, Color cellColor)
    {
        try
        {
            var projectedCoordinates = boundary.Select(p =>
            {
                var projected = Mapsui.Projections.SphericalMercator.FromLonLat(p.Lon, p.Lat);
                return new Coordinate(projected.x, projected.y);
            }).ToList();
            projectedCoordinates.Add(new Coordinate(projectedCoordinates[0].X, projectedCoordinates[0].Y));

            var feature = new GeometryFeature
            {
                Geometry = new Polygon(new LinearRing(projectedCoordinates.ToArray()))
            };

            feature.Styles.Add(new VectorStyle
            {
                Fill = new Mapsui.Styles.Brush(cellColor),
                Outline = new Pen(Color.Orange, 2)
            });

            _routeLayer.Add(feature);
            _routeLayer.DataHasChanged();
            MapView.RefreshGraphics();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Render Error: {ex.Message}");
        }
    }
    
    
    private async Task LoadAllCapturedCells()
    {
        try
        {
            var allCells = await _runService.GetAllCapturedCells();
        
            if (allCells == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var cell in allCells)
                {
                    var cellColor = (cell.OwnerUserId == UserSession.LoggedInUserId) 
                        ? Color.Green
                        : Color.Red;

                    DrawServerBoundary(cell.Boundary, cellColor);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading world: {ex.Message}");
        }
    }
    
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }
}