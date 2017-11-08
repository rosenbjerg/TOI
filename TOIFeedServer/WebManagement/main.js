"use strict";

// let $container = $(".container");
let $viewSpace = $("#viewSpace");
let templates = {
    createTag : JsT.loadById("create-tag-template"),
    login : JsT.loadById("login-template"),
    createEditTag : JsT.loadById("save-edit-toi-template"),
    list : JsT.loadById("list-template"),
    tag : JsT.loadById("tag-template"),
    toi : JsT.loadById("toi-template"),

};
let modalTemplates = {
    editTag : JsT.loadById("edit-tag-template")
};
let state = {

};

function post(url, data, success, error) {
    $.ajax({
        url: url,
        data: data,
        cache: false,
        processData: false,
        contentType: false,
        type: "POST",
        success: success,
        error: error
    });
}

$("#saveEditTagForm").submit(function (ev) {
    ev.preventDefault();
    var form = new FormData(this);
    post("/createtag",
        form,
        function (data) {
            console.log(data);
        },
        function (data) {
            console.log(data);
        });
});


function getMaterialIcon(input) {
    switch (input) {
        case "GPS":
            return "gps_fixed";
        default:
            return input.toLowerCase();
    }
}

function showLogin() {
    $viewSpace.empty().append(templates.login.render());
}

function showTagList(tags) {
    let l = "";
    state = { tags: [] };
    for (let i = 0; i < tags.length; i++){
        tags[i].Icon = getMaterialIcon(tags[i].TagType);
        l += templates.tag.render(tags[i]);
        state.tags[tags[i].TagId] = tags[i];
    }
    $viewSpace.empty().append(templates.list.render({
        title: "All tags",
        list: l,
        thing: "tag"
    }));
}



function showCreateTag() {
    let defaults = {
        radius: 50,
        latitude: 56.9385382,
        longitude: 9.7409053
    };
    navigator.geolocation.getCurrentPosition(function (pos) {
        console.log("got real coordinates");
        defaults.latitude = pos.coords.latitude;
        defaults.longitude = pos.coords.longitude;
        $viewSpace.empty().append(templates.createTag.render(defaults));
        initMapPicker(defaults.latitude, defaults.longitude, defaults.radius);
    }, function (err) {
        console.log(err);
        $viewSpace.empty().append(templates.createTag.render(defaults));
        initMapPicker(defaults.latitude, defaults.longitude, defaults.radius);
    }, {timeout: 500});
}

function initMapPicker(lat, lon, radius) {
    if (lat === undefined)
        lat = 57.012392;
    if (lon === undefined)
        lon = 9.991556;
    if (radius === undefined)
        radius = 50;
    let zoom = radius < 25 ? 20 : radius < 500 ? 15 : 10;
    $(".mapPicker").locationpicker({
        location: {
            latitude: lat,
            longitude: lon
        },
        radius: radius,
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

function showToiList(tois) {
    let l = "";
    state = { tois: [] };
    for (let i = 0; i < tois.length; i++){
        l += templates.toi.render(tois[i]);
        state.tois[tois[i].Id] = tois[i];
    }
    $viewSpace.empty().append(templates.list.render({
        title: "All tois",
        list: l,
        thing: "TOI"
    }));
}

function showPopup(html) {
    $.magnificPopup.open({
        items: {
            type: 'inline',
            src: "<div class='modal-popup'>" + html +"</div>"
        }
    });
}
$viewSpace.on("click", ".tag", function () {
    let id = $(this).data("id");
    let tag = state.tags[id];

    console.log(tag);
    showPopup(modalTemplates.editTag.render(tag));
    initMapPicker(tag.Latitude, tag.Longitude, tag.Radius);
});
$viewSpace.on("submit", "create-tag-form", function (ev) {
    ev.preventDefault();
    let form = new FormData(this);
    // aja
});
// showLogin();
showTagList([
    {Name: "Test tag navn 1",   TagId: "FA:C4:D1:03:8D:3D", TagType: "Bluetooth", Latitude: "56.9385382", Longitude: "9.7409053", Radius: 5000},
    {Name: "Henne på havnen",   TagId: "FB:C4:D1:03:8D:3D", TagType: "Wifi", Radius: 900},
    {Name: "Skolevej 14",   TagId: "FB:C4:D1:03:8D:3D", TagType: "GPS"},
    {Name: "Test tag navn 2",   TagId: "FB:C4:D1:03:8D:3D", TagType: "Bluetooth"},
    {                           TagId: "FC:C4:D1:03:8D:3D", TagType: "NFC"}
]);

// showCreateTag();

// showToiList([
//     {
//         Id: "FA:C4:D1:03:8D:3D",
//         Description: "This is a very descriptive description and it describes how to describe descriptions",
//         Title: "Tag 1",
//         Image: "https://i.imgur.com/gCTCL7z.jpg",
//         Url: "https://imgur.com/gallery/yWoZC",
//         TagAmount: Math.floor(Math.random() * 67),
//         Context: "Marabou ruten"
//     }
// ]);
// $viewSpace.empty().append(modalTemplates.editTag.render());
