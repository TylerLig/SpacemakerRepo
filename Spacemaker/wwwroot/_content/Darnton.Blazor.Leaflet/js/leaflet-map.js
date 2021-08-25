
var stylelayer = {
    reset: {
        color: "#3DA2FF",
        opacity: 1,
        fillOpacity: 0.2,
        weight: 3
    },
    selected: {
        color: "red",
        opacity: 1,
        fillOpacity: 0.4,
        weight: 3
    }
}

export let LeafletMap = {

    Map: {

        setView: function (map, center, zoom) {
            map.setView(center, zoom);
        }

    },

    Layer: {

        addTo: function (layer, map) {
            layer.addTo(map);
        },

        remove: function (layer) {
            layer.remove();
        },

        bindOnClick: function (layer) {
            layer.on('click', function () {
                layer.setStyle(stylelayer.highlight);
            });
        },

        onClick: function (layer, callBack) {
            layer.on('click', function (evt) {
                callBack(evt.target.result);
            });

            //layer.onclick = function (evt) {
            //    callBack(evt.target.result);
            //}
        },

        // Proxy function
        // blazorInstance: A reference to the actual C# class instance, required to invoke C# methods inside it
        // blazorCallbackName: parameter that will get the name of the C# method used as callback
        onClickProxy: function (layer, instance, callbackMethod) {
            this.onClick(layer, result => {
                instance.invokeMethodAsync(callbackMethod);
            });
        },

        fitBounds: function (layer, map) {
            map.fitBounds(layer.getBounds());
        },

        setSelectedStyle: function (layer, selected) {
            if (selected) {
                layer.setStyle(stylelayer.selected);
            }
            else {
                layer.setStyle(stylelayer.reset);
            }
        }

    },

    Polyline: {

        addLatLng: function (polyline, latlng, latlngs) {
            polyline.addLatLng(latlng, latlngs);
        }

    }

}