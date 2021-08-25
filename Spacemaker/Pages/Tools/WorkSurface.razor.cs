using Darnton.Blazor.Leaflet.LeafletMap;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Spacemaker.Data;
using System.Collections.Generic;
using System.Linq;
using Spacemaker.JavascriptWrappers;
using ClipperLib;
using MightyLittleGeodesy.Positions;

namespace Spacemaker.Pages.Tools
{
    public partial class WorkSurface : ComponentBase
    {
        [Parameter]
        public string Style { get; set; }

        [Inject]
        public WorkSurfaceState WorkSurfaceState { get; set; }

        private List<JSPolygon> SelectedPolygons => WorkSurfaceState.SelectedPolygons;
        private Solution SelectedSolution => WorkSurfaceState.SelectedSolution;

        private Map PositionMap;
        private TileLayer OpenStreetMapsTileLayer;
        private int MaxSelected = 2;
        protected override void OnInitialized()
        {
            WorkSurfaceState.PreSolutionStateChangedDelegate += HandlePreStateChange;
            WorkSurfaceState.PostSolutionStateChangedDelegate += HandlePostStateChange;
            var mapCentre = new LatLng(-42, 175); //New Zealand
            PositionMap = new Map("WorkAreaMap", new MapOptions
            {
                Center = mapCentre,
                Zoom = 4
            });
            OpenStreetMapsTileLayer = new TileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", new TileLayerOptions());
        }

        private async void HandlePreStateChange()
        {
            if (SelectedSolution != null && SelectedSolution.Polygons != null)
            {
                foreach (JSPolygon polygon in WorkSurfaceState.SelectedSolution.Polygons)
                {
                    polygon.SelectedChanged -= SelectionChanged;
                    await polygon.SetIsSelected(false);
                    await polygon.Remove();
                }
                SelectedSolution.SelectedPolygons = new List<JSPolygon>();
                WorkSurfaceState.PolygonsStateChangedDelegate.Invoke();
            }
        }
        private void HandlePostStateChange()
        {
            StateHasChanged();
            DrawPolygons();
            BindToPolygonSelectionChangedEvent();
        }

        private async void DrawPolygons()
        {
            if (SelectedSolution == null || SelectedSolution.Polygons == null || SelectedSolution.Polygons.Count < 1)
                return;

            foreach (JSPolygon polygon in SelectedSolution.Polygons)
                await polygon.AddTo(PositionMap);

            if(SelectedSolution.Polygons.Count > 0)
            {
                //await SelectedSolution.Polygons[0].FitBounds(PositionMap);
                await PositionMap.SetView(SelectedSolution.Polygons[0].LatLngs.First(), 15);
            }
        }

        private async void Redraw()
        {
            foreach (JSPolygon polygon in SelectedSolution.Polygons)
            {
                await polygon.Remove();
            }
            DrawPolygons();
        }


        private void AddNewPolygonsAndRedraw(List<JSPolygon> newPolygons)
        {
            foreach(JSPolygon newPolygon in newPolygons)
            {
                if (newPolygon.LatLngs.Count > 0)
                {
                    SelectedSolution.Polygons.Add(newPolygon);
                    newPolygon.SelectedChanged += SelectionChanged;
                }
            }

            SelectedSolution.SelectedPolygons = new List<JSPolygon>();

            WorkSurfaceState.PolygonsStateChangedDelegate.Invoke();
            StateHasChanged();
            Redraw();
        }

        private void UnionOperation()
        {
            Clipper clipper = new Clipper();
            PopulateClipperObjectAndRemovePolygons(clipper);

            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            AddNewPolygonsAndRedraw(ConvertClipperSolution(solution));
        }
        private void IntersectOperation()
        {
            Clipper clipper = new Clipper();
            PopulateClipperObjectAndRemovePolygons(clipper);

            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            AddNewPolygonsAndRedraw(ConvertClipperSolution(solution));
        }

        private List<JSPolygon> ConvertClipperSolution(List<List<IntPoint>> solution)
        {
            List<JSPolygon> output = new List<JSPolygon>();
            if (solution.Count < 1)
                return output;

            RT90Position position;
            WGS84Position wgsPos;

            foreach (List<IntPoint> polygon in solution)
            {
                JSPolygon newPolygon = new JSPolygon(new List<LatLng>(), new PolylineOptions());
                foreach (IntPoint point in polygon)
                {
                    position = new RT90Position(point.X, point.Y);
                    wgsPos = position.ToWGS84();
                    newPolygon.LatLngs.Add(new LatLng(wgsPos.Latitude, wgsPos.Longitude));
                }
                if(newPolygon.LatLngs.Count > 0)
                    output.Add(newPolygon);
            }
            return output;
        }

        private void PopulateClipperObjectAndRemovePolygons(Clipper clipper)
        {
            int i = 1;
            foreach (JSPolygon polygon in SelectedPolygons)
            {
                List<IntPoint> clipperPoly = new List<IntPoint>();
                WGS84Position wgsPos;
                RT90Position rtPos;
                foreach (LatLng latlng in polygon.LatLngs)
                {
                    wgsPos = new WGS84Position()
                    {
                        Latitude = latlng.Lat,
                        Longitude = latlng.Lng
                    };
                    rtPos = new RT90Position(wgsPos, RT90Position.RT90Projection.rt90_2_5_gon_v);
                    clipperPoly.Add(new IntPoint((long)rtPos.Latitude, (long)rtPos.Longitude));
                }
                clipper.AddPolygon(clipperPoly,  i++ % 2 ==0? PolyType.ptClip : PolyType.ptSubject);
                polygon.Remove();
                SelectedSolution.Polygons.Remove(polygon);
            }
        }

        private async void SelectionChanged(object sender, SelectionChangedEvent e)
        {
            if (e.newSelectionState == true)
            {
                if (SelectedPolygons.Count >= MaxSelected)
                {
                    await SelectedPolygons[0].SetIsSelected(false);
                }
                SelectedPolygons.Add(e.polygon);
            }
            else
            {
                if (SelectedPolygons.Contains(e.polygon))
                {
                    SelectedPolygons.Remove(e.polygon);
                }
            }

            WorkSurfaceState.PolygonsStateChangedDelegate.Invoke();
            StateHasChanged();
        }

        private void BindToPolygonSelectionChangedEvent()
        {
            foreach (JSPolygon polygon in WorkSurfaceState.SelectedSolution.Polygons)
            {
                polygon.SelectedChanged += SelectionChanged;
            }
        }
    }
}
