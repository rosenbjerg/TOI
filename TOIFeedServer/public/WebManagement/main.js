"use strict";

const FeedRepo = "http://ssh.windelborg.info:7575/";
let $viewSpace = $("#viewSpace"), $body = $("body");
let cache = {
    tags: {},
    tois: {},
    contexts: {},
    feed: {}
};
let templates = {
    login : JsT.loadById("login-template", true),
    register: JsT.loadById("register-template", true),
    createTag : JsT.loadById("create-tag-template", true),
    saveEditToi : JsT.loadById("save-edit-toi-template", true),
    list : JsT.loadById("list-template", true),
    tag : JsT.loadById("tag-template", true),
    toi : JsT.loadById("toi-template", true),
    tagCell : JsT.loadById("tag-table-cell", true),
    contextCell : JsT.loadById("context-table-cell", true),
    saveEditContext : JsT.loadById("save-edit-context-template", true),
    context : JsT.loadById("context-template", true),
    profile : JsT.loadById("profile-template", true),
    file : JsT.loadById("file-box-template", true),
    fileUploadBox: JsT.loadById("file-upload-box-template", true),
    fileUploadHeader: JsT.loadById("file-upload-header", true),
    fileUploadBatch: JsT.loadById("file-upload-batch", true)
};
let modalTemplates = {
    editTag : JsT.loadById("edit-tag-template", true),
    userPrompt : JsT.loadById("user-prompt-template", true),
    fileEdit: JsT.loadById("file-edit-template", true),
    fileSelect: JsT.loadById("choose-file-template", true),
    feedLocationPicker: JsT.loadById("feed-pick-location", true),
    apiKeyRegister: JsT.loadById("api-key-template", true)
};
templates.saveEditToi.setFormatter("toi.Tags", function (tags) {
    if (!tags)
        return "";
    return tags.reduce(function (acc, curr) {
        return acc + templates.tagCell.render({action: "remove_circle", tag: cache.tags[curr]})
    }, "");
});
templates.saveEditToi.setFormatter("toi.Contexts", function (contexts) {
    if (!contexts)
        return "";
    return contexts.reduce(function (acc, curr) {
        return acc + templates.contextCell.render({action: "remove_circle", context: cache.contexts[curr]})
    }, "");
});
templates.toi.setFormatter("Contexts", function (contexts) {
    return contexts.map(c => cache.contexts[c].Title).join(', ');
});
templates.saveEditContext.setFormatter("create", function (data) {
    if (data)
        return '<input style="margin-left: 0" class="six columns" type="button" id="remove-context" value="Delete context"/>';
    return "";
});
templates.saveEditToi.setFormatter("create", function (data) {
    if (data)
        return '<input style="margin-left: 0" class="six columns" type="button" id="remove-toi" value="Delete ToI"/>';
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
        error: function (jqXHR) {
            if(jqXHR.status === 401) {
                return;
            }
            error(jqXHR);
        },
        statusCode: {
            401: function () {
                showLogin();
            }
        }
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
function getMaterialFileIcon(input) {
    switch (input.toLowerCase()) {
        case "png":
        case "jpg":
        case "jpeg":
        case "bmp":
        case "svg":
            return "photo";
        case "mp4":
        case "webm":
            return "ondemand_video";
        case "mp3":
        case "ogg":
            return "audiotrack";
        case "txt":
            return "description";
        default:
            return "insert_drive_file";
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
    if (!cache[updated] || diffMinutes(new Date(), cache[updated]) > 1){
        ajax("/" + resource, "GET", null,
            function (data) {
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
        },
        function() {
            toastr["error"]("Could not fetch " + resource);
        });
    }
    else {
        if (doneCallback)
            doneCallback();
    }
}
function renderAll(array, template) {
    let str = "";
    for (let i in array){
        if (array.hasOwnProperty(i))
            str += template.render(array[i]);
    }
    return str;
}
function loadAll() {
    getResource("files", function () {
        getResource("contexts", function () {
            getResource("tags", function() {
                getResource("tois", function () {
                    showToiList();
                });
            });
        });
        ajax("/feed", "GET", null,
            function (feed) {
                cache.feed = feed;
            },
            function (resp) {
                toastr["error"](resp);
            });
    }, function(files) {
        for(let i in files) {
            files[i].Icon = getMaterialFileIcon(files[i].Filetype);
        }
        return files;
    });
}

function showLogin() {
    $(".header-menu").hide();
    $viewSpace.empty().append(templates.login.render());
}
function showToiList() {
    getResource("tois", function () {
        $viewSpace.empty().append(templates.list.render({
            createText: "New ToI",
            createButtonId: "create-new-toi",
            title: "ToIs",
            list: renderAll(cache.tois, templates.toi),
            thing: "ToI"
        }));
        $(".header-menu-button.active").removeClass("active");
        $("#show-tois").addClass("active");
    });
}
function showSaveEditToi(toi) {
    $viewSpace.empty().append(templates.saveEditToi.render({
        toi: toi,
        action: toi ? "Edit" : "New",
        create: !!toi,
    }));
    if (toi)
        $("#save-edit-toi-form").find("select").val(toi.InformationType);

    let allTags = searchInData(cache.tags, function (){return true;});
    let allContexts = searchInData(cache.contexts, function() {return true;});
    let str = "";
    for (let i in allTags){
        str += templates.tagCell.render({action: "add_circle", tag: allTags[i]});
    }
    $("#tag-search-result").empty().append(str);
    str = "";
    for (let i in allContexts){
        str += templates.contextCell.render({action: "add_circle", context: allContexts[i]});
    }
    $("#context-search-result").empty().append(str);
}
function showContextList() {
    getResource("contexts", function () {
        $viewSpace.empty().append(templates.list.render({
            createText: "New context",
            createButtonId: "create-new-context",
            title: "Contexts",
            list: renderAll(cache.contexts, templates.context),
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
        $viewSpace.empty().append(templates.list.render({
            createText: "New tag",
            createButtonId: "create-new-tag",
            title: "Tags",
            list: renderAll(cache.tags, templates.tag),
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
        defaults.latitude = pos.coords.latitude;
        defaults.longitude = pos.coords.longitude;
        showPopup(templates.createTag.render(defaults), function () {
            $(".pac-container").remove();
        });
        initMapPicker(defaults.latitude, defaults.longitude, defaults.radius);
    }, function (err) {
        showPopup(templates.createTag.render(defaults), function () {
            $(".pac-container").remove();
        });
        initMapPicker(defaults.latitude, defaults.longitude, defaults.radius);
    }, {timeout: 500});
}
function showProfile() {
    $viewSpace.empty().append(templates.profile.render({
        user: cUser,
        feed:cache.feed,
        deactivateStyle: cache.feed && cache.feed.IsActive ? "block" : "none",
        activateStyle: !cache.feed || cache.feed.IsActive ? "none" : "block"
    }));
    if (cUser.Type === "admin"){
        $.get("/users", function (users) {
            console.log(users);
        });
    }
}
function showFiles() {
    $viewSpace.empty().append(templates.list.render({
        createText: "Upload File",
        createButtonId: "upload-file",
        title: "Files",
        list: renderAll(cache.files, templates.file),
        thing: "file"
    }));
    $(".header-menu-button.active").removeClass("active");
    $("#show-files").addClass("active");
}
function showFilesUpload() {
    showPopup(templates.fileUploadBox.render({
        uploadHeader: templates.fileUploadHeader.render()
    }));
}
function showPopup(html, onClose) {
    $.magnificPopup.open({
        items: {
            type: 'inline',
            src: "<div class='modal-popup'>" + html +"</div>"
        },
        callbacks: {
            close: onClose
        }
    });
}
function showAdministration() {
    $viewSpace.empty().append(templates.administration.render());
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
$("#show-files").click(showFiles);
$("#show-administration").click(showAdministration);

$viewSpace.on("submit", "#login-form", function (ev) {
    ev.preventDefault();
    this.disabled = true;
    let form = new FormData(this);
    ajax("/login", "POST", form,
        feedInfo => {
            console.log(feedInfo);
            $(".header-menu").show();
            loadAll();
        },
        () => {
            toastr["error"]("Wrong username or password");
            this.disabled = true;
        });
});
$viewSpace.on("submit", "#save-edit-toi-form", function (ev) {
    ev.preventDefault();
    let tags = $("#added-tags").find("tr").map(function (i, e) { return $(e).data("id") }).get();
    let contexts = $("#added-contexts").find("tr").map(function (i, e) { return $(e).data("id") }).get();
    let form = new FormData(this);
    let id = $(this).data("id");
    if(id)
        form.append("id", id);
    form.append("tags", tags);
    form.append("contexts", contexts);
    if (id) {
        ajax("/toi", "PUT", form, function (toi) {
            cache.tois[toi.Id] = toi;
            toastr["success"]("ToI updated");
            showToiList();
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
$viewSpace.on("submit", "#update-feed-form", function(ev) {
    ev.preventDefault();
    let form = new FormData(this);
    ajax("/feed", "PUT", form,
        function (resp) {
            showProfile();
            cache.feed = resp.Result;
            toastr["success"](resp.Message);
        },
        function (resp) {
            toastr["error"](resp.responseText);
        });
});

$viewSpace.on("click", "#create-new-toi", function () {showSaveEditToi()});
$viewSpace.on("click", "#create-new-tag", function () {showCreateTag()});
$viewSpace.on("click", "#create-new-context", function () {showSaveEditContext()});
$viewSpace.on("click", "#create-new-context-inline", function () {
    showSaveEditContext(undefined, function (newContext) {
        $("#added-contexts").append(templates.contextCell.render({action: "remove_circle", context: newContext}));
    });
});
$viewSpace.on("click", "#create-user", function() {
    $viewSpace.empty().append(templates.register.render());
});
$viewSpace.on("click", "#upload-file", function() {showFilesUpload()});
$viewSpace.on("click", "#remove-toi", function () {
    promptUser("Delete ToI?", "Are you sure you want to delete this ToI?", function () {
        let form = new FormData();
        form.append("id", $(this).parent().data("id"));

        ajax("/toi", "DELETE", form,
            function () {
                showToiList();
                toastr["success"]("The ToI was deleted");
            },
            function (resp) {
                toastr["error"](resp.responseText);
            })
    });
});
$viewSpace.on("click", "#delete-file", function() {
    promptUser("Delete file?", "Are you sure you want to delete this file?", function () {
        let form = new FormData();
        form.append("id", $(this).parent().data("id"));

        ajax("/files", "DELETE", form,
            function () {
                showFiles();
                toastr["success"]("The file was deleted");
            },
            function (resp) {
                toastr["error"](resp.responseText);
            })
    });
});
$viewSpace.on("click", "#choose-file-button", function() {
    showPopup(modalTemplates.fileSelect.render({
        list: renderAll(cache.files, templates.file),
    }));
    $(".file-select-list").on("click", ".file", function () {
        let file = cache.files[$(this).data("id")];
        $("#information-url").val(`${window.location.protocol}//${window.location.host}/uploads/${file.Id}.${file.Filetype}`);

        $.magnificPopup.close();
    });
});
$viewSpace.on("click", ".tag", function () {
    let id = $(this).data("id");
    let tag = cache.tags[id];
    showPopup(modalTemplates.editTag.render({tag: tag, icon: getMaterialIcon(tag.Type)}), function () {
        $(".pac-container").remove();
    });
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
$viewSpace.on("click", ".file", function () {
    let id = $(this).data("id");
    let file = cache.files[id];
    showPopup(modalTemplates.fileEdit.render(file));

    document.getElementById("copy-url").onclick = function () {
        let id = $("#copy-url").closest("form").data("id");
        let file = cache.files[id];
        let url = `${window.location.protocol}//${window.location.host}/${file.Id}.${file.Filetype}`;
        let temp = $("<input>");
        $body.append(temp);
        temp.val(url).select();
        let copied = document.execCommand('copy');
        temp.remove();
        copied ? toastr["success"]("Copied to clipboard") : toastr["error"]("Could not copy");
    };

});
<<<<<<< HEAD
=======
$viewSpace.on("submit", "#save-edit-toi-form", function (ev) {
    ev.preventDefault();
    let tags = $("#added-tags").find("tr").map(function (i, e) { return $(e).data("id") }).get();
    let contexts = $("#added-contexts").find("tr").map(function (i, e) { return $(e).data("id") }).get();
    let form = new FormData(this);
    let id = $(this).data("id");
    if(id)
        form.append("id", id);
    form.append("tags", tags);
    form.append("contexts", contexts);
    if (id) {
        ajax("/toi", "PUT", form, function (toi) {
            cache.tois[toi.Id] = toi;
            toastr["success"]("ToI updated");
            showToiList();
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
$viewSpace.on("submit", "#update-feed-form", function(ev) {
    ev.preventDefault();
    let form = new FormData(this);
    ajax("/feed", "PUT", form,
        function (resp) {
            cache.feed = resp.Result;
            toastr["success"](resp.Message);
            showProfile();
        },
        function (resp) {
            toastr["error"](resp.responseText);
        });
});
>>>>>>> 5652fdba7250c41bc11c46987e129282dd0d522f
$viewSpace.on("click", "#feed-change-location", function() {
    showPopup(modalTemplates.feedLocationPicker.render(cache.feed));
    initMapPicker(cache.feed);
});
$viewSpace.on("click", "#feed-deactivate", function() {
    promptUser("Deactivate?", "Are you sure you wish to deactivate your feed?", function () {
        ajax("/feed/deactivate", "POST", null,
            function(resp) {
                $.magnificPopup.close();
                cache.feed = resp.Result;
                toastr["success"](resp.Message);
                showProfile();
            },
            function (resp) {
                toastr["error"](resp.responseText);
            });
    });
});
$viewSpace.on("click", "#feed-activate", function () {
    if(cache.feed.Id) {
        ajax("/feed/activate", "POST", null,
            function (resp) {
                $.magnificPopup.close();
                cache.feed = resp.Result;
                toastr["success"](resp.Message);
                showProfile();
            },
            function (resp) {
                toastr["error"](resp.responseText);
            });
    }
    else {
        showPopup(modalTemplates.apiKeyRegister.render());
    }
});
<<<<<<< HEAD

$viewSpace.on("input", "#filter-ToI", function () {
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
=======
$body.on("submit", "#pick-feed-location", function (ev) {
    ev.preventDefault();

    let form = new FormData(this);
    ajax("/feed/location", "PUT", form,
        function (resp) {
            showProfile();
            $.magnificPopup.close();
            cache.feed = resp.Result;
            toastr["success"](resp.Message);
        },
        function (resp) {
            toastr["error"](resp.responseText);
        });
>>>>>>> 5652fdba7250c41bc11c46987e129282dd0d522f
});


// Forms in popups are "bound" to body
$body.on("submit", "#add-file-to-batch-form", function (ev) {
    ev.preventDefault();
    let $this = $(this);
    let batchList = $("#file-upload-list");

    let title = $this.find("input[type=text]");
    let desc = $this.find("textarea");
    let file = $this.find("input[type=file]");

    if(!title.val() || !file.val())
        return;

    let count = batchList.find("li").length;
    let hiddenForm = $("#file-upload-form");

    title.attr("name", "title" + count);
    desc.attr("name", "description" + count);
    file.attr("name", "file" + count);

    title.appendTo(hiddenForm);
    desc.appendTo(hiddenForm);
    file.appendTo(hiddenForm);
    $this.empty().append(templates.fileUploadHeader.render());
    batchList.append(templates.fileUploadBatch.render({
        Number: count,
        Title: title.val(),
        Description: desc.val(),
        Filename: file.val()
    }))
});
$body.on("submit", "#file-upload-form", function(ev) {
    ev.preventDefault();
    $("#add-file-to-batch-form").submit();
    let form = new FormData(this);
    ajax("/files", "POST", form,
        function(fileUpload) {
            console.log(fileUpload);
            let files = fileUpload.Result;
            //Cache all the files that was created
            for (let i in files) {
                files[i].Icon = getMaterialFileIcon(files[i].Filetype);
                cache.files[files[i].Id] = files[i];
            }
            toastr["success"](fileUpload.Message);
            $.magnificPopup.close();
            showFiles();
        },
        function() {
            toastr["error"]("An error occured");
        })
});
$body.on("submit", "#api-key-form", function(ev) {
    ev.preventDefault();

    let form = new FormData(this);
    ajax("/feed/registerowner", "POST", form,
        function(feed) {
            cache.feed = feed;
            toastr["success"]("Your feed is now active");
            showProfile();
            $.magnificPopup.close();
        },
        function (resp) {
            toastr["error"](resp);
        });
});
$body.on("submit", "#create-tag-form", function (ev) {
    ev.preventDefault();
    let form = new FormData(this);
    if (form.get("type") === "none"){
        toastr["error"]("You must select the type");
        return;
    }
    ajax("/tag", "POST", form, function (tag) {
        cache.tags[tag.Id] = tag;
        toastr["success"]("Tag created");
        showTagList();
        $.magnificPopup.close();
    }, function (data) {
        toastr["error"](data.responseText)
    });
});
$body.on("submit", "#edit-tag-form", function (ev) {
    ev.preventDefault();
    let form = new FormData(this);
    form.append("id", $(this).data("id"));
    form.append("type", $(this).data("type"));
    ajax("/tag", "PUT", form, function (tag) {
        cache.tags[tag.Id] = tag;
        toastr["success"]("Changes to tag has been saved");
        $.magnificPopup.close();
        showTagList();
    }, function (data) {
        toastr["error"](data.responseText);
    })
});
$body.on("submit", "#edit-file-form", function (ev) {
    ev.preventDefault();
    let form = new FormData(this);
    let id = $(this).data("id");
    form.append("id", id);
    form.append("filetype", cache.files[id].Filetype);
    ajax("/files", "PUT", form,
        function (resp) {
            resp.Result.Icon = getMaterialFileIcon(resp.Result.Filetype);
            cache.files[resp.Result.Id] = resp.Result;
            toastr["success"](resp.Message);
            $.magnificPopup.close();
            showFiles();
        },
        function (resp) {
            toastr["error"](resp.responseText);
        })
});
$body.on("submit", "pick-feed-location", function () {
    let form = new FormData(this);
    ajax("/feed/location", "PUT", null,
        function (resp) {
            showProfile();
            $.magnificPopup.close();
            cache.feed = resp.Result;
            toastr["success"](resp.Message);
        },
        function (resp) {
            toastr["error"](resp.responseText);
        });
});

$body.on("click", "#remove-context", function () {
    let id = $(this.parentNode).data("id");
    promptUser("Delete context?", "Are you sure you want to delete this context?", function () {
        let form = new FormData();
        form.append("id", id);
        ajax("/context", "DELETE", form, function () {
            delete cache.contexts[id];
            toastr["success"]("Context deleted");
            showContextList();
            searchInData(cache.tois, function (toi) {
                let index = toi.Contexts.indexOf(id);
                if (index === -1)
                    return false;
                toi.Contexts.splice(index, 1);
                return true;
            });
            $.magnificPopup.close();
        }, function (resp) {
            toastr["error"](resp.responseText);
        });
    })
});
$body.on("click", "#remove-tag", function () {
    let id = $(this.parentNode).data("id");
    promptUser("Delete tag?", "Are you sure you want to delete this tag?", function () {
        let form = new FormData();
        form.append("id", id);
        ajax("/tag", "DELETE", form, function () {
            delete cache.tags[id];
            toastr["success"]("Tag deleted");
            showTagList();
            searchInData(cache.tois, function (toi) {
                let index = toi.Tags.indexOf(id);
                if (index === -1)
                    return false;
                toi.Tags.splice(index, 1);
                return true;
            });
            $.magnificPopup.close();
        }, function (resp) {
            toastr["error"](resp.responseText);
        });
    })
});
$body.on("click", "#user-prompt-cancel", function () {
    $.magnificPopup.close();
});
$body.on("click", "#add-toi-context-search .context-cell .action-button", function () {
    let context = cache.contexts[$(this.parentNode.parentNode).data("id")];
    $("#added-contexts").append(templates.contextCell.render({action: "remove_circle", context: context}));
    this.parentNode.parentNode.remove();
});
$body.on("click", "#add-toi-tag-search .tag-cell .action-button", function () {
    let tag = cache.tags[$(this.parentNode.parentNode).data("id")];
    $("#added-tags").append(templates.tagCell.render({action: "remove_circle", tag: tag}));
    this.parentNode.parentNode.remove();
});
$body.on("click", "#added-contexts .action-button, #added-tags .action-button", function () {
    this.parentNode.parentNode.remove();
});
$body.on("click", "#save-edit-toi-form .info-button", function () {
    let $this = $(this).closest("tr");
    let id = $this.data("id");
    let type = $this.data("type");
    let item = cache[type + "s"][id];
    if (type === "tag"){
        showPopup(modalTemplates.editTag.render({
            icon: getMaterialIcon(item.Type),
            tag: item
        }), function () {
            $(".pac-container").remove();
        });
        initMapPicker(item);
    }
    else {
        showSaveEditContext(item);
    }
});
$body.on("click", "#copy-url", function () {

});

$body.on("input", "#add-toi-tag-search input", function () {
    let searchTerm = this.value.toLowerCase();
    let result = searchInData(cache.tags, function (c) {
        return searchTerm === "" || c.Title.toLowerCase().includes(searchTerm) || c.Id.toLowerCase().includes(searchTerm);
    });

    let str = "";
    for (let i in result){
        str += templates.tagCell.render({action: "add_circle", tag: result[i]});
    }
    $("#tag-search-result").empty().append(str);
});
$body.on("input", "#add-toi-context-search input", function () {
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
$body.on("input", "#create-tag-form select", function () {
    let hid = $("#hardware-id-wrapper");
    let hidInput = hid.find("input");
    if (this.value === "Gps"){
        hidInput.val("none");
        hid.hide(100);
    }
    else if (hidInput.val() === "none") {
        hidInput.val("");
        hid.show(100);
    }
});

loadAll();