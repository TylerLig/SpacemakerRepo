using Darnton.Blazor.Leaflet.LeafletMap;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Spacemaker.JavascriptWrappers;
using System;
using System.Collections.Generic;

namespace Spacemaker.Data
{
    public class Solution
    {
        public FeatureCollection Features { get; set; }
        public List<JSPolygon> Polygons { get; set; }
        public List<JSPolygon> SelectedPolygons { get; set; } = new List<JSPolygon>();
        public string Title { get; set; }

        public void ParsePolygons()
        {
            Polygons = new List<JSPolygon>();
            foreach (Feature feature in Features.Features)
            {
                if (feature.Geometry.Type.Equals(GeoJSONObjectType.Polygon))
                {
                    List<LatLng> latLongs = new List<LatLng>();
                    Polygon poly = feature.Geometry as Polygon;
                    foreach (LineString coord in poly.Coordinates)
                    {
                        foreach (IPosition position in coord.Coordinates)
                        {
                            latLongs.Add(new LatLng(position.Latitude, position.Longitude));
                        }
                    }
                    Polygons.Add(new JSPolygon(latLongs, new PolylineOptions()));
                }
            }
        }
    }
}
