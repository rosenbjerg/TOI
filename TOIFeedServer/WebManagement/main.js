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