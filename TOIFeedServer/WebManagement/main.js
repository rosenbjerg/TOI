"use strict";

// let $container = $(".container");
let $viewSpace = $("#viewSpace");
let templates = {
    saveEditTag : JsT.loadById("save-edit-tag-template"),
    login : JsT.loadById("login-template"),
    saveEditToi : JsT.loadById("save-edit-toi-template"),
    list : JsT.loadById("list-template"),
    tag : JsT.loadById("tag-template"),
    toi : JsT.loadById("toi-template"),

};
let modalTemplates = {
    editTag : JsT.loadById("edit-tag-template")
}

function showLogin() {
    $viewSpace.empty().append(templates.login.render());
}

let state = {

};
function getMaterialIcon(input) {
    switch (input) {
        case "GPS":
            return "gps_fixed";
        default:
            return input.toLowerCase();
    }
}

function showTagList(tags) {
    let l = "";
    state = { tags: [] };
    for (let i = 0; i < tags.length; i++){
        tags[i].TagTypeLC = getMaterialIcon(tags[i].TagType);
        l += templates.tag.render(tags[i]);
        state.tags[tags[i].TagId] = tags[i];
    }
    $viewSpace.empty().append(templates.list.render({
        title: "All tags",
        list: l,
        thing: "tag"
    }));
}



function showSaveEditTag(tag) {
    if (tag === undefined){
        $viewSpace.empty().append(templates.saveEditTag.render({
            action: "Create"
        }));
    }
    else {
        $viewSpace.empty().append(templates.saveEditTag.render({
            action: "Edit",
            hideDelete: "",
            id: tag.TagId,
            type: tag.TagType,
            title: tag.Name,
            lat: tag.Latitude,
            lon: tag.Longitude,
            radius: tag.Radius
        }));
    }
    initMapPicker();
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
        zoom: zoom,
    });
    $(".mapPicker").locationpicker("autosize");
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

// showLogin();
showTagList([
    {Name: "Test tag navn 1",   TagId: "FA:C4:D1:03:8D:3D", TagType: "Bluetooth", Latitude: "56.9385382", Longitude: "9.7409053", Radius: 5000},
    {Name: "Henne på havnen",   TagId: "FB:C4:D1:03:8D:3D", TagType: "Wifi", Radius: 900},
    {Name: "Skolevej 14",   TagId: "FB:C4:D1:03:8D:3D", TagType: "GPS"},
    {Name: "Test tag navn 2",   TagId: "FB:C4:D1:03:8D:3D", TagType: "Bluetooth"},
    {                           TagId: "FC:C4:D1:03:8D:3D", TagType: "NFC"}
]);

// showSaveEditTag();
// showSaveEditTag({
//     TagId: "FC:C4:D1:03:8D:3D",
//     Name: "Test tag",
//     TagType: "ble",
//     Latitude: 9.484,
//     Longitude: 47.45,
//     Radius: 250
// });

// $viewSpace.empty().append(modalTemplates.editTag.render());
