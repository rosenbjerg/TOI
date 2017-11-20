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
    saveEditContext : JsT.loadById("save-edit-context-template"),
    context : JsT.loadById("context-template"),

};
// templates.saveEditToi.setFormatter("Tags", function (tagData) {
//     if (!tagData)
//         return "";
//     let str = "";
//     let tags = tagData.map(t => state.tags[t]);
//     for (let i in tags) {
//         str += templates.tagCell.render(tags[i]);
//     }
//     return str;
// });
let modalTemplates = {
    editTag : JsT.loadById("edit-tag-template")
};
let state = {
    tags: {},
    tois: {},
    contexts: {},
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
        error: error
    });
}
function getMaterialIcon(input) {
    switch (input) {
        case "Gps":
            return "gps_fixed";
        default:
            return input.toLowerCase();
    }
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
function diffMinutes(dt2, dt1) {
    let diff =(dt2.getTime() - dt1.getTime()) / 1000;
    diff /= 60;
    return Math.abs(Math.round(diff));
}
function getResource(resource, doneCallback, processData) {
    let updated = resource + "Updated";
    if (!state[updated] || diffMinutes(new Date(), state[updated]) > 1){
        $.get("/" + resource, function (data) {
            if (data.Status !== "Ok")
            {
                console.log("/tag error");
                return;
            }
            if (processData)
                data = processData(data.Result);
            else
                data = data.Result;

            state[resource] = {};
            state[updated] = new Date();
            for (let i in data){
                let element = data[i];
                state[resource][element.Id] = element;
            }
            if (doneCallback)
                doneCallback();
        });
    }
    else {
        if (doneCallback)
            doneCallback();
    }
}

function loadTags(callback) {
    getResource("tags", callback, function (data) {
        for (let i in data){
            data[i].Icon = getMaterialIcon(data[i].TagType);
        }
        return data;
    });


}
function loadTois(callback) {
    getResource("tois", callback, function (data) {
        for (let i in data){
            let toi = data[i];
            toi.TagAmount = toi.Tags.length;
            toi.ContextString = toi.Contexts.map(c => state.contexts[c].Title).join(', ');
        }
        return data;
    });
}
function loadContexts(callback) {
    getResource("contexts", callback);
}

function showLogin() {
    $viewSpace.empty().append(templates.login.render());
}
function showSaveEditToi(toi) {
    $viewSpace.empty().append(templates.saveEditToi.render(toi));
}
function showSaveEditContext(context) {
    $viewSpace.empty().append(templates.saveEditContext.render(toi));
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
function showContextList() {
    loadContexts(function () {
        let l = "";
        for (let x in state.contexts) {
            if (state.contexts.hasOwnProperty(x))
                l += templates.context.render(state.contexts[x]);
        }
        $viewSpace.empty().append(templates.list.render({
            title: "All Contexts",
            list: l,
            thing: "context"
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


$("#show-tags").click(showTagList);
$("#create-tag").click(showCreateTag);
$("#show-tois").click(showToiList);
$("#create-toi").click(showSaveEditToi);
$("#show-contexts").click(showContextList);
$("#create-context").click(showSaveEditContext);


$viewSpace.on("click", ".tag", function () {
    let id = $(this).data("id");
    console.log(id);
    let tag = state.tags[id];
    console.log(tag);
    showPopup(modalTemplates.editTag.render(tag));
    initMapPicker(tag);
});
$viewSpace.on("click", ".toi", function () {
    let id = $(this).data("id");
    let toi = state.tois[id];
    console.log(toi);
    showSaveEditToi(toi);
});
$viewSpace.on("click", ".context", function () {
    let id = $(this).data("id");
    let context = state.contexts[id];
    console.log(context);
    // showSaveEditContext(context);
});
$viewSpace.on("submit", "#save-edit-toi-form", function (ev) {
    ev.preventDefault();
    var form = new FormData(this);
    ajax("/toi", form, function (data) {
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
    ajax("/tag", form, function () {

    })
    // aja
});
$viewSpace.on("submit", "#edit-tag-form", function (ev) {
    ev.preventDefault();
    let form = new FormData(this);
    form.append("id", $(this).data("tag-id"));
    form.append("type", $(this).data("tag-type"));
    put("/tag", form, function (data) {
        console.log(data);
    }, function (data) {
        console.log(data);
    });
});
$viewSpace.on("click", "#add-toi-tag-search button", function () {
    // let searchTerm = $("#add-toi-tag-search input").val();
    // let result = [];
    // if (searchTerm === ""){
    //     for (let x in state.tags){
    //         if (state.tags.hasOwnProperty(x))
    //             result.push(state.tags[x]);
    //     }
    // }
    // else {
    //     for (let x in state.tags){
    //         if (state.tags.hasOwnProperty(x) && (state.tags[x].Name.indexOf(searchTerm) !== -1 || state.tags[x].Id.contains(searchTerm)))
    //             result.push(state.tags[x]);
    //     }
    // }
    // let str = "";
    // for (let i = 0, max = result.length; i < max; i++) {
    //     str += templates.tagCell.render(result[i]);
    // }
    // $("#tag-search-result").empty().append(str);
    //
    //

    let searchTerm = $("add-toi-tag-search input").val();
    console.log(searchTerm);
    let result = searchInData(state.tags, function (c) {
        console.log(c);
        return searchTerm === "" || c.Name.includes(searchTerm) || c.Id.includes(searchTerm);
    });
    let str = "";
    for (let i in result){
        str += templates.tagCell.render(result[i]);
    }
    $("#tag-search-result").empty().append(str);
});
$viewSpace.on("click", "#add-toi-context-search button", function () {
    let searchTerm = $("add-toi-context-search input").val();
    console.log(searchTerm == "");
    console.log(searchTerm);
    let result = searchInData(state.contexts, function (c) {
        return searchTerm === "" || c.Title.includes(searchTerm) || c.Id.includes(searchTerm);
    });
    let str = "";
    for (let i in result){
        str += templates.tagCell.render(result[i]);
    }
    console.log(result);
    $("#context-search-result").empty().append(str);
});
function searchInData(collection, compareFunc) {
    let result = [];
    for (let x in collection){
        if (!collection.hasOwnProperty(x) || !compareFunc(collection[x]))
            continue;
        console.log("added");
        result.push(collection[x]);
    }
    return result;
}

showLogin();

loadContexts(loadTois);
loadTags();