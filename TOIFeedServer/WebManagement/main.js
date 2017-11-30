"use strict";

let $viewSpace = $("#viewSpace");
let $body = $("body");
let cache = {
    tags: {},
    tois: {},
    contexts: {},
};
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
    profile : JsT.loadById("profile-template")

};
let modalTemplates = {
    editTag : JsT.loadById("edit-tag-template", true),
    userPrompt : JsT.loadById("user-prompt-template", true)
};
templates.saveEditToi.setFormatter("Tags", function (tags) {
    if (!tags)
        return "";
    return tags.reduce(function (acc, curr) {
        return acc + templates.tagCell.render({action: "remove_circle", tag: cache.tags[curr]})
    }, "");
});
templates.saveEditToi.setFormatter("Contexts", function (contexts) {
    if (!contexts)
        return "";
    return contexts.reduce(function (acc, curr) {
        return acc + templates.contextCell.render({action: "remove_circle", context: cache.contexts[curr]})
    }, "");
});
templates.toi.setFormatter("Tags", function (tags) {
    return tags.length;
});
templates.toi.setFormatter("Contexts", function (contexts) {
    return contexts.map(c => cache.contexts[c].Title).join(', ');
});
templates.saveEditContext.setFormatter("create", function (data) {
    if (data)
        return '<input style="margin-left: 0" class="six columns" type="button" id="remove-context" value="Delete context"/>';
    return "";
});
templates.tag.setFormatter("Type", function (type) {
    return getMaterialIcon(type);
});
templates.tagCell.setFormatter("tag.Type", function (type) {
    return getMaterialIcon(type);
});


let cUser = {
    Username: "admin",
    Email: "test@test.dk",
    Title: "admin"
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
        error: error
    });
}
function searchInData(collection, compareFunc, max) {
    let result = [];
    let c = 0;
    if (max === undefined)
        max = collection.length;
    for (let x in collection){
        if (!collection.hasOwnProperty(x) || !compareFunc(collection[x]))
            continue;
        result.push(collection[x]);
        if (++c === max) return result;
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
            locationTitleInput: $(".locationTitleInput")
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
    if (!cache[updated] || diffMinutes(new Date(), cache[updated]) > 1){
        $.get("/" + resource, function (data) {
            if (processData)
                data = processData(data);

            cache[resource] = {};
            cache[updated] = new Date();
            for (let i in data){
                let element = data[i];
                cache[resource][element.Id] = element;
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

function showLogin() {
    $viewSpace.empty().append(templates.login.render());
}
function showToiList() {
    getResource("tois", function () {
        let l = "";
        for (let x in cache.tois) {
            if (cache.tois.hasOwnProperty(x))
                l += templates.toi.render(cache.tois[x]);
        }
        $viewSpace.empty().append(templates.list.render({
            createText: "New ToI",
            createButtonId: "create-new-toi",
            title: "ToIs",
            list: l,
            thing: "TOI"
        }));
        $(".header-menu-button.active").removeClass("active");
        $("#show-tois").addClass("active");
    });
}
function showSaveEditToi(toi) {
    $viewSpace.empty().append(templates.saveEditToi.render(toi));
    $viewSpace.find("select").val(toi.InformationType);
}
function showContextList() {
    getResource("contexts", function () {
        let l = "";
        for (let x in cache.contexts) {
            if (cache.contexts.hasOwnProperty(x))
                l += templates.context.render(cache.contexts[x]);
        }
        $viewSpace.empty().append(templates.list.render({
            createText: "New context",
            createButtonId: "create-new-context",
            title: "Contexts",
            list: l,
            thing: "context"
        }));
        $(".header-menu-button.active").removeClass("active");
        $("#show-contexts").addClass("active");
    });
}
function showSaveEditContext(context, onSaveCallback) {
    showPopup(templates.saveEditContext.render({
        action: context ? "Edit" : "New",
        create: !!context,
        context: context,
    }));
    $("#save-edit-context-form").submit(function (ev) {
        ev.preventDefault();
        let id = $(this).data("id");
        let form = new FormData(this);
        if (id) {
            form.append("id", id);
            ajax("/context", "PUT", form, function (context) {
                cache.contexts[context.Id] = context;
                toastr["success"]("Context updated");
                showContextList();
                $.magnificPopup.close();
            }, function (resp) {
                toastr["error"](resp.responseText);
            });
        }
        else {
            ajax("/context", "POST", form, function (context) {
                console.log(context);
                cache.contexts[context.Id] = context;
                toastr["success"]("Context saved");
                if (onSaveCallback)
                    onSaveCallback(context);
                else
                    showContextList();
                $.magnificPopup.close();
            }, function (resp) {
                toastr["error"](resp.responseText);
            });
        }
    })
}
function showTagList() {
    getResource("tags", function () {
        let l = "";
        for (let x in cache.tags){
            if (cache.tags.hasOwnProperty(x))
                l += templates.tag.render(cache.tags[x]);
        }
        $viewSpace.empty().append(templates.list.render({
            createText: "New tag",
            createButtonId: "create-new-tag",
            title: "Tags",
            list: l,
            thing: "tag"
        }));
        $(".header-menu-button.active").removeClass("active");
        $("#show-tags").addClass("active");
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
function showProfile() {
    $viewSpace.empty().append(templates.profile.render({user: cUser}));
    if (cUser.Type === "admin"){
        $.get("/users", function (users) {
            console.log(users);
        });
    }
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
    $("#user-prompt-accept").click(onOk);
}

$("#show-tags").click(showTagList);
$("#show-tois").click(showToiList);
$("#show-contexts").click(showContextList);
$("#show-profile").click(showProfile);

$viewSpace.on("click", "#create-new-toi", function () {showSaveEditToi()});
$viewSpace.on("click", "#create-new-tag", function () {showCreateTag()});
$viewSpace.on("click", "#create-new-context", function () {showSaveEditContext()});
$viewSpace.on("click", "#create-new-context-inline", function () {
    showSaveEditContext(undefined, function (newContext) {
        $("#added-contexts").append(templates.contextCell.render({action: "remove_circle", context: newContext}));
    });
});

$viewSpace.on("click", ".tag", function () {
    let id = $(this).data("id");
    let tag = cache.tags[id];
    showPopup(modalTemplates.editTag.render(tag));
    initMapPicker(tag);
});
$viewSpace.on("click", ".toi", function () {
    let id = $(this).data("id");
    showSaveEditToi(cache.tois[id]);
});
$viewSpace.on("click", ".context", function () {
    let id = $(this).data("id");
    showSaveEditContext(cache.contexts[id]);
});
$viewSpace.on("submit", "#save-edit-toi-form", function (ev) {
    ev.preventDefault();
    let tags = $("#added-tags").find("tr").map(function (i, e) { return $(e).data("id") }).get();
    let contexts = $("#added-contexts").find("tr").map(function (i, e) { return $(e).data("id") }).get();
    let form = new FormData(this);
    let id = $(this).data("id");
    if (id)
        form.append("id", id);
    form.append("tags", tags);
    form.append("contexts", contexts);
    if (id) {
        ajax("/toi", "PUT", form, function (toi) {
            cache.tois[toi.Id] = toi;
            toastr["success"]("ToI updated");
            showSaveEditToi();
        }, function (data) {
            toastr["error"](data.responseText);
        });
    }
    else {
        ajax("/toi", "POST", form, function (toi) {
            cache.tois[toi.Id] = toi;
            toastr["success"]("ToI created");
            showSaveEditToi();
        }, function (data) {
            toastr["error"](data.responseText);
        });
    }

});
$viewSpace.on("submit", "#create-tag-form", function (ev) {
    ev.preventDefault();
    let form = new FormData(this);
    if (form.get("type") === "none"){
        toastr["error"]("You must select the type");
        return;
    }
    ajax("/tag", "POST", form, function (tag) {
        cache.tags[tag.Id] = tag;
        toastr["success"]("Tag created");
        $.magnificPopup.close();
    }, function (data) {
        toastr["error"](data.responseText)
    });
});

// Forms in popups are "bound" to body
$body.on("submit", "#edit-tag-form", function (ev) {
    ev.preventDefault();
    let form = new FormData(this);
    form.append("id", $(this).data("id"));
    form.append("type", $(this).data("type"));
    ajax("/tag", "PUT", form, function (tag) {
        cache.tags[tag.Id] = tag;
        toastr["success"]("Changes to tag has been saved");
        $.magnificPopup.close();
    }, function (data) {
        toastr["error"](data.responseText);
    })
});
$body.on("click", "#remove-context", function () {
    let id = $(this.parentNode).data("id");
    promptUser("Delete context?", "Are you sure you want to delete this context?", function () {
        let form = new FormData();
        form.append("id", id);
        ajax("context", "DELETE", form, function () {
            delete cache.contexts[id];
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

$viewSpace.on("input", "#add-toi-tag-search input", function () {
    let searchTerm = this.value;
    let result = searchInData(cache.tags, function (c) {
        return searchTerm === "" || c.Title.toLowerCase().includes(searchTerm) || c.Id.toLowerCase().includes(searchTerm);
    });
    let str = "";
    for (let i in result){
        str += templates.tagCell.render({action: "add_circle", tag: result[i]});
    }
    $("#tag-search-result").empty().append(str);
});
$viewSpace.on("input", "#add-toi-context-search input", function () {
    let searchTerm = this.value;
    let result = searchInData(cache.contexts, function (c) {
        return searchTerm === "" || c.Title.toLowerCase().includes(searchTerm);
    });
    let str = "";
    for (let i in result){
        str += templates.contextCell.render({action: "add_circle", context: result[i]});
    }
    $("#context-search-result").empty().append(str);
});


$viewSpace.on("click", "#add-toi-context-search .context-cell .action-button", function () {
    let context = cache.contexts[$(this.parentNode.parentNode).data("id")];
    $("#added-contexts").append(templates.contextCell.render({action: "remove_circle", context: context}));
    this.parentNode.parentNode.remove();
});
$viewSpace.on("click", "#add-toi-tag-search .tag-cell .action-button", function () {
    let tag = cache.tags[$(this.parentNode.parentNode).data("id")];
    $("#added-tags").append(templates.tagCell.render({action: "remove_circle", tag: tag}));
    this.parentNode.parentNode.remove();
});

$viewSpace.on("click", "#added-contexts .action-button, #added-tags .action-button", function () {
    this.parentNode.parentNode.remove();
});

$viewSpace.on("click", "#save-edit-toi-form .info-button", function () {
    let $this = $(this).closest("tr");
    let id = $this.data("id");
    let type = $this.data("type");
    let item = cache[type + "s"][id];
    if (type === "tag"){
        showPopup(modalTemplates.editTag.render(item));
        initMapPicker(item);
    }
    else {
        showSaveEditContext(item);
    }
});

$viewSpace.on("input", "#filter-TOI", function () {
    let searchTerm = this.value;
    let result = searchInData(cache.tois, function (toi) {
        return toi.Title.toLowerCase().includes(searchTerm.toLowerCase());
    });
    let l = renderAll(result, templates.toi);
    $("#list-ul").empty().append(l);
});
$viewSpace.on("input", "#filter-context", function () {
    let searchTerm = this.value;
    let result = searchInData(cache.contexts, function (ctx) {
        return ctx.Title.toLowerCase().includes(searchTerm.toLowerCase());
    });
    let l = renderAll(result, templates.context);
    $("#list-ul").empty().append(l);
});
$viewSpace.on("input", "#filter-tag", function () {
    let searchTerm = this.value;
    let result = searchInData(cache.tags, function (tag) {
        return  tag.Id.toLowerCase().includes(searchTerm.toLowerCase()) ||
                tag.Title.toLowerCase().includes(searchTerm.toLowerCase());
    });
    let l = renderAll(result, templates.tag);
    $("#list-ul").empty().append(l);
});


function renderAll(array, template) {
    let str = "";
    for (let i = 0, max = array.length; i < max; i++) {
        str += template.render(array[i]);
    }
    return str;
}

showLogin();

getResource("contexts", function () {
    getResource("tois")
});
getResource("tags");