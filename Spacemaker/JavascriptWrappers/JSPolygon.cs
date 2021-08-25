using Darnton.Blazor.Leaflet.LeafletMap;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ClipperLib;
using MightyLittleGeodesy.Positions;

namespace Spacemaker.JavascriptWrappers
{
    public class SelectionChangedEvent
    {
        public JSPolygon polygon { get; set; }
        public bool newSelectionState { get; set; }
    }
    public class JSPolygon : InteractiveLayer
    {
        [JsonIgnore] public List<LatLng> LatLngs { get; }

        [JsonIgnore] public PolylineOptions Options { get; }

        [JsonIgnore] public bool IsSelected;

        public event EventHandler<SelectionChangedEvent> SelectedChanged;

        public JSPolygon(List<LatLng> latLngs, PolylineOptions options)
        {
            LatLngs = latLngs;
            Options = options;
            IsSelected = false;
        }

        public double CalculateArea()
        {
            List<IntPoint> polygon = new List<IntPoint>();

            WGS84Position wgsPos;
            RT90Position rtPos;
            foreach (LatLng latlng in LatLngs)
            {
                wgsPos = new WGS84Position()
                {
                    Latitude = latlng.Lat,
                    Longitude = latlng.Lng
                };
                rtPos = new RT90Position(wgsPos, RT90Position.RT90Projection.rt90_2_5_gon_v);
                polygon.Add(new IntPoint((long)rtPos.Latitude, (long)rtPos.Longitude));
            }
            return Math.Abs(Clipper.Area(polygon));
        }

        public async Task SetIsSelected(bool newState)
        {
            IsSelected = newState;
            await SetSelectedStyle();
            SelectedChanged?.Invoke(this, new SelectionChangedEvent() { polygon = this, newSelectionState = IsSelected });
        }

        public async Task SetSelectedStyle()
        {
            IJSObjectReference module = await JSBinder.GetLeafletMapModule();
            await module.InvokeVoidAsync("LeafletMap.Layer.setSelectedStyle", this.JSObjectReference, IsSelected);
        }

        public async Task BindToClickEvent(IJSObjectReference jsObject)
        {
            IJSObjectReference module = await JSBinder.GetLeafletMapModule();
            await module.InvokeVoidAsync("LeafletMap.Layer.onClickProxy", jsObject, DotNetObjectReference.Create(this), "ClickCallback");
        }

        [JSInvokable]
        public async void ClickCallback()
        {
            await SetIsSelected(!IsSelected);
        }

        public async Task FitBounds(Map map)
        {
            IJSObjectReference module = await JSBinder.GetLeafletMapModule();
            await module.InvokeVoidAsync("LeafletMap.Layer.fitBounds", this.JSObjectReference, map.JSObjectReference);
        }

        protected override async Task<IJSObjectReference> CreateJsObjectRef()
        {
            IJSObjectReference jsObject = await JSBinder.JSRuntime.InvokeAsync<IJSObjectReference>("L.polygon", LatLngs.ToArray(), Options);
            await BindToClickEvent(jsObject);
            return jsObject;
        }
    }
}