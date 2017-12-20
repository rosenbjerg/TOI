"use strict";

let $viewSpace = $('#viewSpace');
let $body = $("body");
let cache = {
    feeds: []
};
let templates = {
    login: JsT.loadById("login-template", true),
    register: JsT.loadById("register-template", true)
};
toastr.options = {
    "closeButton": false,
    "debug": false,
    "newestOnTop": false,
    "progressBar": false,
    "positionClass": "toast-bottom-right",
    "preventDuplicates": false,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "5000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};

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

function initMapPicker(pos) {
    if (pos === undefined || !pos.Latitude || !pos.Longitude || !pos.Radius) {
        pos = {
            Latitude: 57.012392,
            Longitude: 9.991556,
            Radius: 50
        };
    }
    let zoom = pos.Radius < 25 ? 20 : pos.Radius < 500 ? 15 : 10;
    $(".mapPicker").locationpicker({
        location: {
            latitude: pos.Latitude,
            longitude: pos.Longitude
        },
        radius: pos.Radius,
        enableAutocomplete: true,
        inputBinding: {
            latitudeInput: $(".latitudeInput"),
            longitudeInput: $(".longitudeInput"),
            radiusInput: $(".radiusInput"),
            locationNameInput: $(".locationNameInput")
        },
        zoom: zoom
    });
    $(".mapPicker").locationpicker("autosize");
}

function showLogin() {
    $viewSpace.empty().append(templates.login.render());
}

$viewSpace.on("click", "#create-key-button", function() {
    $viewSpace.empty().append(templates.register.render());
});
$viewSpace.on("click", "#register-cancel", function () {
    showLogin();
});
$viewSpace.on("submit", "#register-form", function (ev) {
    ev.preventDefault();

    let form = new FormData(this);
    ajax("/feed", "POST", form,
        function(feed) {
            toastr["succes"]("Your feed has been created.");
            showFeed(feed);
        },
        function (error) {
            toast["error"](error);
        }
    );
});

showLogin();