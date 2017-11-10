"use strict";

// let $container = $(".container");
let $viewSpace = $("#viewSpace");
let templates = {
    login : JsT.loadById("login-template"),
    createTag : JsT.loadById("create-tag-template"),
    saveEditToi : JsT.loadById("save-edit-toi-template"),
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

function showSaveEditToi(toi) {
    loadTags(function () {
        $viewSpace.empty().append(templates.saveEditToi.render(toi));
    });
}

function loadTags(callback) {
    $.get("/tags", function (tags) {
        state = { tags: [] };
        for (let i = 0, max = tags.length; i < max; i++){
            let tag = tags[i];
            tag.Icon = getMaterialIcon(tag.TagType);
            state.tags.push(tag);
        }
        callback();
    });
}

function showTagList() {
    loadTags(function () {
        let l = "";
        for (let i = 0, max = state.tags.length; i < max; i++){
            l += templates.tag.render(state.tags[i]);
        }
        $viewSpace.empty().append(templates.list.render({
            title: "All tags",
            list: l,
            thing: "tag"
        }));
    });
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

function showToiList() {
    $.get("/tois", function (tags) {
        let l = "";
        state = { tags: [] };
        for (let i = 0; i < tags.length; i++){
            tags[i].Icon = getMaterialIcon(tags[i].TagType);
            l += templates.tag.render(tags[i]);
            state.tags[tags[i].TagId] = tags[i];
        }
        $viewSpace.empty().append(templates.list.render({
            title: "All tois",
            list: l,
            thing: "TOI"
        }));
    });
}

function showPopup(html) {
    $.magnificPopup.open({
        items: {
            type: 'inline',
            src: "<div class='modal-popup'>" + html +"</div>"
        }
    });
}

// showLogin();
// showTagList([
//     {Name: "Test tag navn 1",   TagId: "FA:C4:D1:03:8D:3D", TagType: "Bluetooth", Latitude: "56.9385382", Longitude: "9.7409053", Radius: 5000},
//     {Name: "Henne på havnen",   TagId: "FB:C4:D1:03:8D:3D", TagType: "Wifi", Radius: 900},
//     {Name: "Skolevej 14",   TagId: "FB:C4:D1:03:8D:3D", TagType: "GPS"},
//     {Name: "Test tag navn 2",   TagId: "FB:C4:D1:03:8D:3D", TagType: "Bluetooth"},
//     {                           TagId: "FC:C4:D1:03:8D:3D", TagType: "NFC"}
// ]);

$("#show-tags").click(showTagList);
$("#create-tag").click(showCreateTag);
$("#show-tois").click(showToiList);
$("#create-toi").click(showSaveEditToi);


$viewSpace.on("click", ".tag", function () {
    let id = $(this).data("id");
    let tag = state.tags[id];
    showPopup(modalTemplates.editTag.render(tag));
    initMapPicker(tag.Latitude, tag.Longitude, tag.Radius);
});
$viewSpace.on("click", ".toi", function () {
    let id = $(this).data("id");
    let toi = state.tois[id];
    showSaveEditToi(toi);
});
$viewSpace.on("submit", "#save-edit-toi-form", function (ev) {
    ev.preventDefault();
    var form = new FormData(this);
    post("/createtoi", form, function (data) {
        console.log(data);
    }, function (data) {
        console.log(data);
    });
});
$viewSpace.on("submit", "create-tag-form", function (ev) {
    ev.preventDefault();
    let form = new FormData(this);
    if (form.get("typeInput") === "none"){
        showPopup()
        return;
    }
    post("/createtag", form, function () {

    })
    // aja
});
$viewSpace.on("submit", "#edit-tag-form", function (ev) {
    ev.preventDefault();
    let form = new FormData(this);
    post("/edittag", form, function (data) {
        console.log(data);
    }, function (data) {
        console.log(data);
    });
});
$("#add-toi-tag-search button").click(function () {
    let searchTerm = $("#add-toi-tag-search input").val();
    if (searchTerm === "")
        return;
    let result = state.tags.filter(function (t) { t.Name.contains(searchTerm) || t.TagId.contains(searchTerm) });
    for (let i = 0, max = result.length; i < max; i++) {

    }
});

showLogin();

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
