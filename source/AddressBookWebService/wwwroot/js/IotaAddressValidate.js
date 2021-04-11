jQuery.validator.addMethod("iotaaddress",
    function (value, element, param) {
        //two checks:
        //1. verify if correct address and length
        console.log("starting validating address");
        var response;

        //try {
        //    var decodedAddress = Base58.decode(value);
        //    console.log(decodedAddress);
        //    console.log(decodedAddress.length);
        //    if (decodedAddress.length != 33) return false;
        //} catch (e) {
        //    return false;
        //}

        //if (result) {
        //2. verify if address online
        $.when($.ajax({
            type: "GET",
            async: false, //important!
            url: "https://localhost:44383/api/address/verify/" + value,
            cache: false
        })).done(function (data, textStatus, jqXHR) {
            console.log("background call done. Response: " + data);
            console.log(data.result);
            console.log(data.errorDescription);
            response = data;
        });
        //}

        if (response.result) $("#addressSubmit").removeAttr("disabled");
        else $("#addressSubmit").prop("disabled", true);

        console.log("finished validating address " + response);

        return response.result;
    });

jQuery.validator.unobtrusive.adapters.addBool("iotaaddress");