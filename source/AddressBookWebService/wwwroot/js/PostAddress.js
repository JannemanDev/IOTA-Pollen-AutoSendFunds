function postAddress(jsonAddressRequestModel) {
    console.log("starting post address");
    var response;
    $.when($.ajax({
        type: "POST",
        dataType: "json",
        contentType: 'application/json',
        data: jsonAddressRequestModel,
        async: false, //important!
        url: "https://localhost:44383/api/address/",
        cache: false
    })).done(function (data, textStatus, jqXHR) {
        console.log("background call done. Response: " + data);
        console.log(data.result);
        response = data;
    });

    if (response.result) {

    }

    console.log("finished  post address" + response);
};