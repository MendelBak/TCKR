function toTitleCase(str) {
    return str.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
}

// var values;

$(document).ready(function() {
    $("#Name").keyup(function() {
        var $this = $(this);
        var name = toTitleCase($(this).val());

        if (name === "") {
            $("#NameDropdown").html("");
        }
        else {
            $.get("Search/Name/" + name, function(data) {
                console.log(data);
                // value = data;
                $("#NameDropdown").show();
                $("#NameDropdown").html("");
                data.forEach(element => {
                    $("#NameDropdown").append(
                        '<li><span class="symbol">' + element["symbol"] +
                        '</span>, <span class="name">' +
                        element["name"] + "</span></li>");
                    // $("#NameDropdown").append(data);
                });
            });
        }
    });
    $("#NameDropdown").on("click", "li", function() {
        console.log("click");
        var name = $("span.name", this).text();
        var symbol = $("span.symbol", this).text();
    
        $("#NameDropdown").html("");
        $("#Name").val(name);
        $("#Symbol").val(symbol);
    });
});