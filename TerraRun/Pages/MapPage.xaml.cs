using Mapsui;
using Mapsui.Extensions;
using Mapsui.Tiling;
using TerraRun.Services;
using Mapsui.Nts;
using H3;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using System.Linq;
using H3.Extensions;

namespace TerraRun.Pages;

public partial class MapPage
{
    private bool _isTracking = false;
    private List<MPoint> _routePoints = new();
    private int? _currentRunId;
    private readonly RunService _runService = new();
    private readonly Mapsui.Layers.WritableLayer _routeLayer = new();

    public MapPage()
    {
        InitializeComponent();

        Dispatcher.Dispatch(() =>
        {
            var tileLayer = OpenStreetMap.CreateTileLayer();
            MapView.Map.Layers.Add(tileLayer);
            MapView.Map.Layers.Add(_routeLayer);
            MapView.Map.Navigator.ZoomToLevel(15);
            MapView.Map.Navigator.RotationLock = false;
            RequestLocation();
            MoveCameraToCurrentLocation();
        });
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

    private async void MoveCameraToCurrentLocation()
    {
        try
        {
            var location =
                await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
            if (location != null)
            {
                var sm = Mapsui.Projections.SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                MapView.Map.Navigator.CenterOn(new MPoint(sm));
                MapView.Map.Navigator.ZoomToLevel(16);
                MapView.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position(location.Latitude,
                    location.Longitude));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GPS Error: {ex.Message}");
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
            await DisplayAlert("Финиш!", "Пробежка сохранена", "ОК");
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
                _ = Task.Run(() => StartTracking());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }

    private async Task StartTracking()
    {
        while (_isTracking)
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High));
            if (location != null && _currentRunId.HasValue)
            {
                var response = await _runService.CaptureCell(_currentRunId.Value, location.Latitude, location.Longitude);

                if (response?.Boundary != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DrawServerBoundary(response.Boundary);
                        MoveCameraToCurrentLocation();
                    });
                }
            }
            await Task.Delay(500);
        }
    }

    private void DrawServerBoundary(List<BoundaryPoint> boundary)
    {
        try
        {
            var projectedCoordinates = boundary.Select(p =>
            {
                System.Diagnostics.Debug.WriteLine($"[RENDER] Точка от сервера: Lat={p.Lat}, Lon={p.Lon}");
                var projected = Mapsui.Projections.SphericalMercator.FromLonLat(p.Lon, p.Lat);
                return new Coordinate(projected.x, projected.y);
            }).ToList();
            projectedCoordinates.Add(new Coordinate(projectedCoordinates[0].X, projectedCoordinates[0].Y));

            var feature = new GeometryFeature { 
                Geometry = new Polygon(new LinearRing(projectedCoordinates.ToArray())) 
            };
        
            feature.Styles.Add(new VectorStyle {
                Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromArgb(150, 255, 140, 0)),
                Outline = new Pen(Mapsui.Styles.Color.Orange, 2)
            });

            _routeLayer.Add(feature);
            _routeLayer.DataHasChanged();
            MapView.RefreshGraphics();
            System.Diagnostics.Debug.WriteLine($"[RENDER] Полигон добавлен. Всего на слое: {_routeLayer}");
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Render Error: {ex.Message}");
        }
    }
}





