jQuery.validator.addMethod("iotaaddress",
    function (value, element, param) {
        console.log("starting validating address");
        var response;
        $.when($.ajax({
            type: "GET",
            async: false, //important!
            url: "https://localhost:44383/api/address/validate/" + value,
            cache: false
        })).done(function (data, textStatus, jqXHR) {
            console.log("background call done. Response: " + data);
            console.log(data.result);
            console.log(data.errorDescription);
            response = data;
        });

        if (response.result) $("#addressSubmit").removeAttr("disabled");
        else $("#addressSubmit").prop("disabled", true);

        console.log("finished validating address " + response);

        return response.result;
    });

jQuery.validator.unobtrusive.adapters.addBool("iotaaddress");