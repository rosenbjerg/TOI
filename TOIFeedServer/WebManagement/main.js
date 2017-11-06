
let $container = $(".container");
let templates = {
    saveEditTag : JsT.loadById("saveEditTag")
};

function showSaveEditTag(tag) {
    if (arguments.length === 0){
        $container.empty().append(templates.saveEditTag.render({
            action: "Create"
        }));
    }
    else {

        $container.empty().append(templates.saveEditTag.render({
            action: "Edit"
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
}

showSaveEditTag();