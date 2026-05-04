namespace TerraRun.Services;

public class GpsService
{
    public async Task<bool> CheckAndRequestPermissions()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        return status == PermissionStatus.Granted;
    }

    public async Task<Location> GetCurrentLocation()
    {
        try
        {
            return await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5)));
        }
        catch { return null; }
    }
}