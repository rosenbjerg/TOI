"use strict";
let _map = null;

function initMap() {
    _map = new google.maps.Map(document.getElementById('context-map'), {
        zoom: 4,
        center: {
            lat: 57,
            lng: 9.9
        }
    });

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
    let mapCentered = false;
    for (let i in tois) {
        let url = "/tags?ids=" + tois[i].Tags.join(",");
        ajax(url, "GET", null,
            function (tags) {
                console.log(tags);
                if(!mapCentered)
                    _map.center = {lat: tags[0].Latitude, lng: tags[0].Longitude};
                addMarkers(tois[i], tags);
            },
            function () {
                console.error("TAG ERROR!");
            }
        );
    }
}

function addMarkers(toi, tags) {
    for (let i in tags) {
        new google.maps.Marker({
            position: {
                lat: tags[i].Latitude,
                lng: tags[i].Longitude
            },
            map: _map
        });
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