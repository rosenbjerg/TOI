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

function showLogin() {
    $viewSpace.empty().append(templates.login.render());
}

function showTagList(tags) {
    let l = "";
    for (let i = 0; i < tags.length; i++){
        l += templates.tag.render(tags[i]);
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

function initMapPicker() {
    $("#mapPicker").locationpicker({
        location: {
            latitude: 57.012392,
            longitude: 9.991556
        },
        radius: 200,
        enableAutocomplete: true,
        inputBinding: {
            latitudeInput: $("#latitudeInput"),
            longitudeInput: $("#longitudeInput"),
            radiusInput: $("#radiusInput"),
            locationNameInput: $("#locationNameInput")
        }
    });
    $("#mapPicker").locationpicker("autosize");
}

// showLogin();
// showTagList([
//     {Name: "Test tag navn 1",   TagId: "FA:C4:D1:03:8D:3D", TagType: "Bluetooth"},
//     {Name: "Test tag navn 2",   TagId: "FB:C4:D1:03:8D:3D", TagType: "Bluetooth"},
//     {                  TagId: "FC:C4:D1:03:8D:3D", TagType: "NFC"}
// ]);
showSaveEditTag();
showSaveEditTag({
    TagId: "FC:C4:D1:03:8D:3D",
    Name: "Test tag",
    TagType: "ble",
    Latitude: 9.484,
    Longitude: 47.45,
    Radius: 250
});