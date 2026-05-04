using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using TerraRun.Models;
using Color = Mapsui.Styles.Color;

namespace TerraRun.Helpers;

public static class MapHelper
{
    public static void AddCellToLayer(Mapsui.Layers.WritableLayer layer, List<BoundaryPoint> boundary, Color cellColor)
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

            layer.Add(feature);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Feature Add Error: {ex.Message}");
        }
    }
    
    public static Color GetCellColor(int cellOwnerId, int? currentUserId)
    {
        return (cellOwnerId == currentUserId) ? Color.Green : Color.Red;
    }
}