"use strict";
let _map = null;

let templates = {
    infobox: JsT.loadById("marker-infowindow-template", true)
};

function initMap() {
    loadContextToIs(onToILoaded);
}

function loadContextToIs(callback) {
    let url = new URL(window.location);
    let contextId = url.searchParams.get("context");
    if(!contextId)
    {
        console.error("No context id was found in the url!");
        return;
    }

    ajax("/tois?contexts=" + contextId, "GET", null,
        function (toi) {
            onToILoaded(toi);
        },
        function() {
            console.error("ERROR");
        }
    );
}

function onToILoaded(tois) {
    let bounds = new google.maps.LatLngBounds();
    _map = new google.maps.Map(document.getElementById('context-map'));
    google.maps.event.addListenerOnce(_map, "idle", function() {
        console.log("Zooming to ToI location");
        _map.fitBounds(bounds);
        _map.panToBounds(bounds);
    });

    console.log(tois);
    for (let i in tois) {
        let url = "/tags?ids=" + tois[i].Tags.join(",");
        ajax(url, "GET", null,
            function (tags) {
                console.log(tags);
                addMarkers(tois[i], tags, bounds);
            },
            function () {
                console.error("TAG ERROR!");
            }
        );
    }
}

function addMarkers(toi, tags, bounds) {
    for (let i in tags) {
        let toiInfoWindow = new google.maps.InfoWindow({
            content: templates.infobox.render( toi )
        });

        let loc = new google.maps.LatLng (tags[i].Latitude, tags[i].Longitude);
        let marker = new google.maps.Marker({
            position: loc,
            title: toi.title,
            map: _map
        });

        marker.addListener('click', function () {
            toiInfoWindow.open(_map, marker);
        });

        bounds.extend(loc);
    }
}

function ajax(url, method, data, success, error) {
    $.ajax({
        url: url,
        data: data,
        cache: false,
        processData: false,
        contentType: false,
        type: method.toUpperCase(),
        success: success,
        error: function (jqXHR) {
            if(jqXHR.status === 401) {
                return;
            }
            error();
        },
        statusCode: {
            401: function () {
                showLogin();
            }
        }
    });
}