"use strict";

let $viewSpace = $("#viewSpace");
let templates = {
    login : JsT.loadById("login-template"),
    createTag : JsT.loadById("create-tag-template"),
    saveEditToi : JsT.loadById("save-edit-toi-template"),
    list : JsT.loadById("list-template"),
    tag : JsT.loadById("tag-template"),
    toi : JsT.loadById("toi-template"),
    tagCell : JsT.loadById("tag-table-cell"),

};
let modalTemplates = {
    editTag : JsT.loadById("edit-tag-template")
};
let state = {
    tags: {}
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
function diffMinutes(dt2, dt1)
{
    let diff =(dt2.getTime() - dt1.getTime()) / 1000;
    diff /= 60;
    return Math.abs(Math.round(diff));
}
function loadTags(callback) {

    if (!state.tagsUpdated || diffMinutes(new Date(), state.tagsUpdated) > 1){
        $.post("/tags", function (tagResult) {
            if (tagResult.Status !== "Ok")
            {
                console.log("/tags error");
                return;
            }
            state.tags = {};
            state.tagsUpdated = new Date();
            for (let i = 0, max = tagResult.Result.length; i < max; i++){
                let tag = tagResult.Result[i];
                tag.Icon = getMaterialIcon(tag.TagType);
                state.tags[tag.TagId] = tag;
            }
            callback();
        });
    }
    else {
        callback();
    }
}
function loadTois(callback) {
    console.log(state);
    if (!state.toisUpdated || diffMinutes(new Date(), state.toisUpdated) > 1){
        $.get("/tois", function (toiResult) {
            console.log(toiResult);
            if (toiResult.Status !== "Ok")
            {
                console.log("/tois error");
                return;
            }
            state.tois = {};
            state.toisUpdated = new Date();
            console.log(toiResult);
            for (let i = 0, max = toiResult.Result.length; i < max; i++){
                let toi = toiResult.Result[i];
                state.tois[toi.Id] = toi;
            }
            callback();
        });
    }
    else {
        callback();
    }
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

function showTagList() {
    loadTags(function () {
        let l = "";
        for (let x in state.tags){
            if (state.tags.hasOwnProperty(x))
                l += templates.tag.render(state.tags[x]);
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

function showToiList() {
    loadTois(function () {
        let l = "";
        console.log(state);
        for (let x in state.tois) {
            if (state.tois.hasOwnProperty(x))
                l += templates.toi.render(state.tois[x]);
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
    console.log(tag);
    showPopup(modalTemplates.editTag.render(tag));
    initMapPicker(tag);
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
    form.append("id", $(this).data("tag-id"));
    form.append("type", $(this).data("tag-type"));
    post("/edittag", form, function (data) {
        console.log(data);
    }, function (data) {
        console.log(data);
    });
});
$viewSpace.on("click", "#add-toi-tag-search button", function () {
    let searchTerm = $("#add-toi-tag-search input").val();
    let result = [];
    if (searchTerm === ""){
        for (let x in state.tags){
            if (state.tags.hasOwnProperty(x))
                result.push(state.tags[x]);
        }
    }
    else {
        for (let x in state.tags){
            if (state.tags.hasOwnProperty(x) && (state.tags[x].Name.contains(searchTerm) || state.tags[x].TagId.contains(searchTerm)))
                result.push(state.tags[x]);
        }
    }
    let str = "";
    for (let i = 0, max = result.length; i < max; i++) {
        str += templates.tagCell.render(result[i]);
    }
    $("#tag-search-result").empty().append(str);
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
