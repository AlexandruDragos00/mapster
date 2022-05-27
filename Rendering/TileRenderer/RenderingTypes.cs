using Mapster.Common.MemoryMappedTypes;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace Mapster.Rendering;

public struct GeoFeature : BaseShape
{
    public enum GeoFeatureType
    {
        Plain,
        Hills,
        Mountains,
        Forest,
        Desert,
        Unknown,
        Water,
        Residential
    }

    public int ZIndex
    {
        get
        {
            switch (Type)
            {
                case GeoFeatureType.Plain:
                    return 10;
                case GeoFeatureType.Hills:
                    return 12;
                case GeoFeatureType.Mountains:
                    return 13;
                case GeoFeatureType.Forest:
                    return 11;
                case GeoFeatureType.Desert:
                    return 9;
                case GeoFeatureType.Unknown:
                    return 8;
                case GeoFeatureType.Water:
                    return 40;
                case GeoFeatureType.Residential:
                    return 41;
            }

            return 7;
        }
        set { }
    }

    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }
    public GeoFeatureType Type { get; set; }

    public void Render(IImageProcessingContext context)
    {
        var color = Color.Magenta;
        switch (Type)
        {
            case GeoFeatureType.Plain:
                color = Color.LightGreen;
                break;
            case GeoFeatureType.Hills:
                color = Color.DarkGreen;
                break;
            case GeoFeatureType.Mountains:
                color = Color.LightGray;
                break;
            case GeoFeatureType.Forest:
                color = Color.Green;
                break;
            case GeoFeatureType.Desert:
                color = Color.SandyBrown;
                break;
            case GeoFeatureType.Unknown:
                color = Color.Magenta;
                break;
            case GeoFeatureType.Water:
                color = Color.LightBlue;
                break;
            case GeoFeatureType.Residential:
                color = Color.LightCoral;
                break;
        }

        if (!IsPolygon)
        {
            var pen = new Pen(color, 1.2f);
            context.DrawLines(pen, ScreenCoordinates);
        }
        else
        {
            context.FillPolygon(color, ScreenCoordinates);
        }
    }

    public GeoFeature(ReadOnlySpan<Coordinate> c, GeoFeatureType type)
    {
        IsPolygon = true;
        Type = type;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public GeoFeature(ReadOnlySpan<Coordinate> c, MapFeatureData feature)
    {
        IsPolygon = feature.Type == GeometryType.Polygon;
        var natural = feature.Properties.natural;
        Type = GeoFeatureType.Unknown;
        if (natural != PropertyManager.Natural.UNSET)
        {
            if (natural == PropertyManager.Natural.FELL ||
                natural == PropertyManager.Natural.GRASSLAND ||
                natural == PropertyManager.Natural.HEATH ||
                natural == PropertyManager.Natural.MOOR ||
                natural == PropertyManager.Natural.SCRUB ||
                natural == PropertyManager.Natural.WETLAND)
            {
                Type = GeoFeatureType.Plain;
            }
            else if (natural == PropertyManager.Natural.WOOD ||
                     natural == PropertyManager.Natural.TREE_ROW)
            {
                Type = GeoFeatureType.Forest;
            }
            else if (natural == PropertyManager.Natural.BARE_ROCK ||
                     natural == PropertyManager.Natural.ROCK ||
                     natural == PropertyManager.Natural.SCREE)
            {
                Type = GeoFeatureType.Mountains;
            }
            else if (natural == PropertyManager.Natural.BEACH ||
                     natural == PropertyManager.Natural.SAND)
            {
                Type = GeoFeatureType.Desert;
            }
            else if (natural == PropertyManager.Natural.WATER)
            {
                Type = GeoFeatureType.Water;
            }
        }

        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public static bool isNatural(MapFeatureData feature)
    {
        return feature.Type == GeometryType.Polygon && feature.Properties.natural != PropertyManager.Natural.UNSET;
    }

    public static bool isForest(MapFeatureData feature)
    {
        return feature.Properties.boundary == PropertyManager.Boundary.Forest;
    }

    public static bool isLanduseForestOrOrchad(MapFeatureData feature)
    {
        return feature.Properties.landuse == PropertyManager.Landuse.FOREST || feature.Properties.landuse == PropertyManager.Landuse.ORCHARD;
    }

    public static bool isLanduseResidential(MapFeatureData feature)
    {
        PropertyManager.Landuse landuse = feature.Properties.landuse;
        return landuse == PropertyManager.Landuse.RESIDENTIAL || landuse == PropertyManager.Landuse.CEMETERY || landuse == PropertyManager.Landuse.INDUSTRIAL ||
          landuse == PropertyManager.Landuse.COMMERCIAL || landuse == PropertyManager.Landuse.SQUARE || landuse == PropertyManager.Landuse.CONSTRUCTION ||
          landuse == PropertyManager.Landuse.MILITARY || landuse == PropertyManager.Landuse.QUARRY || landuse == PropertyManager.Landuse.BROWNFIELD;
    }

    public static bool isLandusePlain(MapFeatureData feature)
    {
        PropertyManager.Landuse landuse = feature.Properties.landuse;
        return landuse == PropertyManager.Landuse.FARM || landuse == PropertyManager.Landuse.MEADOW || landuse == PropertyManager.Landuse.GRASS ||
          landuse == PropertyManager.Landuse.GREENFIELD || landuse == PropertyManager.Landuse.RECREATION_GROUND || landuse == PropertyManager.Landuse.WINTER_SPORTS ||
          landuse == PropertyManager.Landuse.ALLOTMENTS;
    }

    public static bool isWater(MapFeatureData feature)
    {
        PropertyManager.Landuse landuse = feature.Properties.landuse;
        return landuse == PropertyManager.Landuse.RESERVOIR || landuse == PropertyManager.Landuse.BASIN;
    }

    public static bool isBuilding(MapFeatureData feature)
    {
        return feature.Properties.building != PropertyManager.Building.UNSET && feature.Type == GeometryType.Polygon;
    }

    public static bool isLeisure(MapFeatureData feature)
    {
        return feature.Properties.leisure != PropertyManager.Leisure.UNSET && feature.Type == GeometryType.Polygon;
    }

    public static bool isAmenity(MapFeatureData feature)
    {
        return feature.Properties.amenity != PropertyManager.Amenity.UNSET && feature.Type == GeometryType.Polygon;
    }
}

public struct Railway : BaseShape
{
    public int ZIndex { get; set; } = 45;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        var penA = new Pen(Color.DarkGray, 2.0f);
        var penB = new Pen(Color.LightGray, 1.2f, new[]
        {
            2.0f, 4.0f, 2.0f
        });
        context.DrawLines(penA, ScreenCoordinates);
        context.DrawLines(penB, ScreenCoordinates);
    }

    public Railway(ReadOnlySpan<Coordinate> c)
    {
        IsPolygon = false;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public static bool isRailway(MapFeatureData feature)
    {
        return feature.Properties.railway != PropertyManager.Railway.UNSET;
    }
}

public struct PopulatedPlace : BaseShape
{
    public int ZIndex { get; set; } = 60;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }
    public string Name { get; set; }
    public bool ShouldRender { get; set; }

    public void Render(IImageProcessingContext context)
    {
        if (!ShouldRender)
        {

            return;
        }

        var font = SystemFonts.Families.First().CreateFont(12, FontStyle.Bold);
        context.DrawText(Name, font, Color.Black, ScreenCoordinates[0]);
    }

    public PopulatedPlace(ReadOnlySpan<Coordinate> c, MapFeatureData feature)
    {
        IsPolygon = false;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
        var name = feature.Properties.name;
        if (feature.Label.IsEmpty)
        {
            ShouldRender = false;
            Name = "Unknown";
        }
        else
        {

            Name = string.IsNullOrWhiteSpace(name) ? feature.Label.ToString() : name;
            ShouldRender = true;
        }
    }

    public static bool ShouldBePopulatedPlace(MapFeatureData feature)
    {
        // https://wiki.openstreetmap.org/wiki/Key:place
        if (feature.Type != GeometryType.Point)
        {
            return false;
        }
        PropertyManager.Place place = feature.Properties.place;
        return place == PropertyManager.Place.CITY || place == PropertyManager.Place.TOWN || place == PropertyManager.Place.LOCALITY || place == PropertyManager.Place.HAMLET;
    }
}

public struct Border : BaseShape
{
    public int ZIndex { get; set; } = 30;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        var pen = new Pen(Color.Gray, 2.0f);
        context.DrawLines(pen, ScreenCoordinates);
    }

    public Border(ReadOnlySpan<Coordinate> c)
    {
        IsPolygon = false;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public static bool ShouldBeBorder(MapFeatureData feature)
    {
        return feature.Properties.boundary == PropertyManager.Boundary.Administrative && feature.Properties.adminLevel == PropertyManager.AdminLevel.LEVEL_2;
    }
}

public struct Waterway : BaseShape
{
    public int ZIndex { get; set; } = 40;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        if (!IsPolygon)
        {
            var pen = new Pen(Color.LightBlue, 1.2f);
            context.DrawLines(pen, ScreenCoordinates);
        }
        else
        {
            context.FillPolygon(Color.LightBlue, ScreenCoordinates);
        }
    }

    public static bool isWaterway(MapFeatureData feature)
    {
        return feature.Properties.water != PropertyManager.Water.Unset && feature.Type != GeometryType.Point;
    }

    public Waterway(ReadOnlySpan<Coordinate> c, bool isPolygon = false)
    {
        IsPolygon = isPolygon;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }
}

public struct Road : BaseShape
{
    public int ZIndex { get; set; } = 50;
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context)
    {
        if (!IsPolygon)
        {
            var pen = new Pen(Color.Coral, 2.0f);
            var pen2 = new Pen(Color.Yellow, 2.2f);
            context.DrawLines(pen2, ScreenCoordinates);
            context.DrawLines(pen, ScreenCoordinates);
        }
    }

    public Road(ReadOnlySpan<Coordinate> c, bool isPolygon = false)
    {
        IsPolygon = isPolygon;
        ScreenCoordinates = new PointF[c.Length];
        for (var i = 0; i < c.Length; i++)
            ScreenCoordinates[i] = new PointF((float)MercatorProjection.lonToX(c[i].Longitude),
                (float)MercatorProjection.latToY(c[i].Latitude));
    }

    public static bool isRoad(MapFeatureData feature)
    {
        return feature.Properties.highway != PropertyManager.Highway.Other && feature.Properties.highway != PropertyManager.Highway.Unset;
    }

}

public interface BaseShape
{
    public int ZIndex { get; set; }
    public bool IsPolygon { get; set; }
    public PointF[] ScreenCoordinates { get; set; }

    public void Render(IImageProcessingContext context);

    public void TranslateAndScale(float minX, float minY, float scale, float height)
    {
        for (var i = 0; i < ScreenCoordinates.Length; i++)
        {
            var coord = ScreenCoordinates[i];
            var newCoord = new PointF((coord.X + minX * -1) * scale, height - (coord.Y + minY * -1) * scale);
            ScreenCoordinates[i] = newCoord;
        }
    }
}