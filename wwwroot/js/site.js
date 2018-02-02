function toTitleCase(str) {
    return str.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
}

$(document).ready(function() {
    $("#Name").keyup(function() {
        var $this = $(this);
        var name = toTitleCase($(this).val());

        if (name === "") {
            $("#NameDropdown").html("");
            $("#SymbolDropdown").html("");
        }
        else {
            $.get("Search/Name/" + name, function(data) {
                $("#NameDropdown").html("");
                $("#SymbolDropdown").html("");
                data.forEach(element => {
                    $("#NameDropdown").append(
                        '<li><span class="symbol">' + element["symbol"] +
                        '</span>, <span class="name">' +
                        element["name"] + "</span></li>");
                });
            });
        }
    });
    $("#NameDropdown").on("click", "li", function() {
        var name = $("span.name", this).text();
        var symbol = $("span.symbol", this).text();
    
        $("#NameDropdown").html("");
        $("#Name").val(name);
        $("#Symbol").val(symbol);
    });
    $("#Symbol").keyup(function () {
        var $this = $(this);
        var symbol = $(this).val().toUpperCase();

        if (symbol === "") {
            $("#NameDropdown").html("");
            $("#SymbolDropdown").html("");
        }
        else {
            $.get("Search/Symbol/" + symbol, function (data) {
                $("#NameDropdown").html("");
                $("#SymbolDropdown").html("");
                data.forEach(element => {
                    $("#SymbolDropdown").append(
                        '<li><span class="symbol">' + element["symbol"] +
                        '</span>, <span class="name">' +
                        element["name"] + "</span></li>");
                });
            });
        }
    });
    $("#SymbolDropdown").on("click", "li", function () {
        var name = $("span.name", this).text();
        var symbol = $("span.symbol", this).text();

        $("#SymbolDropdown").html("");
        $("#Name").val(name);
        $("#Symbol").val(symbol);
    });
});