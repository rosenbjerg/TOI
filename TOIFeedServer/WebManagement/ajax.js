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

$("#saveEditTagForm").submit(function (ev) {
    ev.preventDefault();
    var form = new FormData(this);
    post("/createtag",
        form,
        function (data) {
            console.log(data);
        },
        function (data) {
            console.log(data);
        });
});