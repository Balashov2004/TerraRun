using H3;
using H3.Extensions;
using H3.Model;
using TerraRun.Api.DTO;

namespace TerraRun.Api.Services;

public interface IGeoCellService
{
    string GetCellIndex(double latitude, double longitude, int resolution = 12);
    List<BoundaryPointDto> GetBoundary(string h3Index);
}

public class GeoCellService : IGeoCellService
{
    public string GetCellIndex(double latitude, double longitude, int resolution = 12)
    {
        var latRad = latitude * (Math.PI / 180.0);
        var lonRad = longitude * (Math.PI / 180.0);
        var point = new GeoCoord(latRad, lonRad);
        return H3Index.FromGeoCoord(point, resolution).ToString();
    }

    public List<BoundaryPointDto> GetBoundary(string h3Index)
    {
        var indexObj = new H3Index(h3Index);
        return indexObj.GetCellBoundaryVertices()
            .Select(v => new BoundaryPointDto
            {
                Lat = v.Latitude * (180.0 / Math.PI),
                Lon = v.Longitude * (180.0 / Math.PI)
            })
            .ToList();
    }
}
