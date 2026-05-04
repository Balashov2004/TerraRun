using Mapsui.Tiling;
using TerraRun.Models;
using TerraRun.Services;
using TerraRun.Helpers;
using Mapsui.UI.Maui;

namespace TerraRun.Pages;

[QueryProperty(nameof(Tournament), "Tournament")]
public partial class TournamentMapPage : ContentPage
{
    private TournamentViewModel _tournament;
    private bool _isTracking;
    private readonly TournamentsService _tournamentsService;
    private readonly GpsService _gpsService = new();
    private readonly Mapsui.Layers.WritableLayer _cellsLayer = new();

    public TournamentViewModel Tournament
    {
        get => _tournament;
        set
        {
            _tournament = value;
            OnPropertyChanged();
            InitMap();
        }
    }
    
    public TournamentMapPage()
    {
        InitializeComponent();
        _tournamentsService = new TournamentsService();
        BindingContext = this;
    }

    private void InitMap()
    {
        Dispatcher.Dispatch(async () =>
        {
            MapView.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapView.Map.Layers.Add(_cellsLayer);
            
            MapView.Map.Navigator.ZoomToLevel(15);
            MapView.MyLocationLayer.Enabled = true;

            if (await _gpsService.CheckAndRequestPermissions())
            {
                await RefreshTournamentCells();
                _ = Task.Run(() => ContinuousUpdateLoop());
            }
        });
    }

    private async Task ContinuousUpdateLoop()
    {
        while (true)
        {
            var location = await _gpsService.GetCurrentLocation();
            if (location != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var sm = Mapsui.Projections.SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                    MapView.Map.Navigator.CenterOn(sm.x, sm.y);
                    MapView.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position(location.Latitude, location.Longitude));
                });
                
                if (_isTracking)
                {
                    var result = await _tournamentsService.CaptureTournamentCell(Tournament.Id, location.Latitude, location.Longitude);
                    
                    if (result != null)
                    {
                        await RefreshTournamentCells();
                    }
                }
            }
            
            else if (!_isTracking) 
            {
                await RefreshTournamentCells();
            }

            await Task.Delay(1000);
        }
    }

    private async Task RefreshTournamentCells()
    {
        var cells = await _tournamentsService.GetCellsByTournament(Tournament.Id);
    
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _cellsLayer.Clear();
        
            foreach (var cell in cells)
            {
                var cellColor = GetTournamentColor(cell.OwnerUserId);
                
                MapHelper.AddCellToLayer(_cellsLayer, cell.Boundary, cellColor);
            }
        
            _cellsLayer.DataHasChanged();
        });
    }

    private Mapsui.Styles.Color GetTournamentColor(int ownerId)
    {
        if (ownerId == UserSession.LoggedInUserId)
            return Mapsui.Styles.Color.Green;
        
        var r = (ownerId * 63) % 255;
        var g = (ownerId * 127) % 255;
        var b = (ownerId * 191) % 255;
        return new Mapsui.Styles.Color(r, g, b, 180);
    }

    private void OnStartClicked(object? sender, EventArgs e)
    {
        _isTracking = !_isTracking;
        StartBtn.Text = _isTracking ? "СТОП" : "СТАРТ";
        StartBtn.BackgroundColor = _isTracking ? Colors.Red : Colors.Green;
    }

    private async void OnLeaderboardClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(LeaderboardPage)}?TournamentId={Tournament.Id}");
    }
}