jQuery.validator.addMethod("addressexist",
    function (value, element, param) {
        console.log("starting addressexist: " + hostname + "api/address/addressexist/" + value);
        var response;
        $.when($.ajax({
            type: "GET",
            async: false, //important!
            url: hostname + "api/address/addressexist/" + value,
            cache: false
        })).done(function (data, textStatus, jqXHR) {
            console.log("background call done. Response: " + data);
            console.log(data);
            response = data;
        });

        if (response) $("#addressSubmit").removeAttr("disabled");
        else $("#addressSubmit").prop("disabled", true);

        console.log("finished addressexist" + response);

        return response.result;
    });

jQuery.validator.unobtrusive.adapters.addBool("addressexist");