"use strict";

let $viewSpace = $("#viewSpace");
let $body = $("body");
let templates = {
    login : JsT.loadById("login-template", true),
    createTag : JsT.loadById("create-tag-template", true),
    saveEditToi : JsT.loadById("save-edit-toi-template", true),
    list : JsT.loadById("list-template", true),
    tag : JsT.loadById("tag-template", true),
    toi : JsT.loadById("toi-template", true),
    tagCell : JsT.loadById("tag-table-cell", true),
    contextCell : JsT.loadById("context-table-cell", true),
    saveEditContext : JsT.loadById("save-edit-context-template", true),
    context : JsT.loadById("context-template", true),

};
templates.saveEditToi.setFormatter("Tags", function (tagData) {
    if (!tagData)
        return "";
    let str = "";
    let tags = tagData.map(t => state.tags[t]);
    console.log(tags);
    for (let i in tags) {
        str += templates.tagCell.render(tags[i]);
    }
    return str;
});
let modalTemplates = {
    editTag : JsT.loadById("edit-tag-template", true),
    userPrompt : JsT.loadById("user-prompt-template", true)
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
}


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
function searchInData(collection, compareFunc) {
    let result = [];
    for (let x in collection){
        if (!collection.hasOwnProperty(x) || !compareFunc(collection[x]))
            continue;
        result.push(collection[x]);
    }
    return result;
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
    showPopup(templates.saveEditContext.render({
        action: context ? "Edit" : "New",
        context: context,
        create: context ? undefined : ""
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
function showToiList() {
    loadTois(function () {
        let l = "";
        for (let x in state.tois) {
            if (state.tois.hasOwnProperty(x))
                l += templates.toi.render(state.tois[x]);
        }
        $viewSpace.empty().append(templates.list.render({
            createText: "New ToI",
            createButtonId: "create-new-toi",
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
            createText: "New tag",
            createButtonId: "create-new-tag",
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
            createText: "New context",
            createButtonId: "create-new-context",
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
function promptUser(title, question, onOk) {
    showPopup(modalTemplates.userPrompt.render({
        title: title,
        question: question
    }));
    $("#user-prompt-accept").click(function () {
        console.log("accept");
        onOk();
    });
}

$("#show-tags").click(function() {showTagList()});
$("#show-tois").click(function() {showToiList()});
$("#show-contexts").click(function() {showContextList()});

$viewSpace.on("click", "#create-new-toi", function () {showSaveEditToi()});
$viewSpace.on("click", "#create-new-tag", function () {showCreateTag()});
$viewSpace.on("click", "#create-new-context", function () {showSaveEditContext()});

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
    showSaveEditContext(context);
});

$viewSpace.on("submit", "#save-edit-toi-form", function (ev) {
    ev.preventDefault();
    let tags = $("#added-tags").find("tr").map(function (i, e) {
        return $(e).data("id");
    }).get();
    let contexts = $("#added-contexts").find("tr").map(function (i, e) {
        return $(e).data("id");
    }).get();
    let form = new FormData(this);
    form.append("tags", tags);
    form.append("contexts", contexts);
    let htmlForm = this;
    ajax("/toi", "POST", form, function (data) {
        console.log(data);
        htmlForm.reset();
    }, function (data) {
        console.log(data);
    });
});
$viewSpace.on("submit", "#create-tag-form", function (ev) {
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

// Forms in popups are "bound" to body
$body.on("submit", "#edit-tag-form", function (ev) {
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
$body.on("submit", "#save-edit-context-form", function (ev) {
    ev.preventDefault();
    let id = $(this).data("id");
    let form = new FormData(this);
    if (id) {
        form.append("id", id);
        ajax("/context", "PUT", form, function () {
            state.contexts[id].Title = form.get("title");
            state.contexts[id].Description = form.get("description");
            toastr["success"]("Context updated");
            showContextList();
            $.magnificPopup.close();
        }, function (resp) {
            toastr["error"](resp.responseText);
        });
    }
    else {
        ajax("/context", "POST", form, function (data) {
            state.contexts[data] = {
                Id: data,
                Title: form.get("title"),
                Description: form.get("description")
            };
            toastr["success"]("Context saved");
            showContextList();
            $.magnificPopup.close();
        }, function (resp) {
            toastr["error"](resp.responseText);
        });
    }
});
$body.on("click", "#remove-context", function () {
    let id = $(this.parentNode).data("id");
    promptUser("Delete context?", "Are you sure you want to delete this context?", function () {
        let form = new FormData();
        form.append("id", id);
        ajax("context", "DELETE", form, function () {
            delete state.contexts[id];
            toastr["success"]("Context deleted");
            showContextList();
            $.magnificPopup.close();
        }, function (resp) {
            console.log(resp);
            toastr["error"](resp.responseText);
        });
    })
});
$body.on("click", "#user-prompt-cancel", function () {
    $.magnificPopup.close();
});

$viewSpace.on("click", "#add-toi-tag-search button", function () {
    let searchTerm = $("#add-toi-tag-search input").val();
    let result = searchInData(state.tags, function (c) {
        return searchTerm === "" || c.Name.includes(searchTerm) || c.Id.includes(searchTerm);
    });
    let str = "";
    for (let i in result){
        str += templates.tagCell.render({action: "add_circle", tag: result[i]});
    }
    $("#tag-search-result").empty().append(str);
});
$viewSpace.on("click", "#add-toi-context-search button", function () {
    let searchTerm = $("#add-toi-context-search input").val();
    let result = searchInData(state.contexts, function (c) {
        return searchTerm === "" || c.Title.includes(searchTerm);
    });
    let str = "";
    for (let i in result){
        str += templates.contextCell.render({action: "add_circle", context: result[i]});
    }
    $("#context-search-result").empty().append(str);
});

$viewSpace.on("click", "#add-toi-context-search .context-cell i", function () {
    let context = state.contexts[$(this.parentNode.parentNode).data("id")];
    $("#added-contexts").append(templates.contextCell.render({action: "remove_circle", context: context}));
    this.parentNode.parentNode.remove();
});
$viewSpace.on("click", "#add-toi-tag-search .tag-cell i", function () {
    let tag = state.tags[$(this.parentNode.parentNode).data("id")];
    $("#added-tags").append(templates.tagCell.render({action: "remove_circle", tag: tag}));
    this.parentNode.parentNode.remove();
});

$viewSpace.on("click", "#added-contexts .context-cell i", function () {
    this.parentNode.parentNode.remove();
});
$viewSpace.on("click", "#added-tags .tag-cell i", function () {
    this.parentNode.parentNode.remove();
});


showLogin();

loadContexts(loadTois);
loadTags();