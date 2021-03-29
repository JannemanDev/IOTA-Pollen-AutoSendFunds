jQuery.validator.addMethod("iotaaddress",
    function (value, element, param) {
        //two checks:
        //1. verify if correct address and length
        try {
            var decodedAddress = Base58.decode(value);
            //console.log(decodedAddress);
            //console.log(decodedAddress.length);
            if (decodedAddress.length != 33) return false;
        } catch (e) {
            return false;
        }

        var result;
        //2. verify if address online
        $.when($.ajax({
            type: "GET",
            async: false, //important!
            url: "https://localhost:44383/api/address/verify/" + value,
            cache: false
        })).done(function (data, textStatus, jqXHR) {
            console.log("background call done. Received: " + data);
            result = data;
        });
        console.log("done");
        return result;
    });

jQuery.validator.unobtrusive.adapters.addBool("iotaaddress");