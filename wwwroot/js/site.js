function toTitleCase(str) {
    return str.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
}

$(document).ready(function () {
    $("#Name").keyup(function (e) {
        // ignore tab, enter, and arrow keys
        if (e.keyCode === 9 ||
            e.keyCode === 13 ||
            e.keyCode === 37 ||
            e.keyCode === 38 ||
            e.keyCode === 39 ||
            e.keyCode === 40)
            return;
        
        var $this = $(this);
        var name = toTitleCase($(this).val());

        if (name === "") {
            $("#NameDropdown").html("");
            $("#SymbolDropdown").html("");
        }
        else {
            $.get("Search/Name/" + name, function (data) {
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
    $("#NameDropdown").on("click", "li", function () {
        var name = $("span.name", this).text();
        var symbol = $("span.symbol", this).text();

        $("#NameDropdown").html("");
        $("#Name").val(name);
        $("#Symbol").val(symbol);
    });
    $("#NameDropdown li").on("click", "li", function () {
        $(this).parent().children().removeClass("active");
        $(this).addClass("active");
    });
    $("#Name").keydown(function (e) {
        if ($("#NameDropdown li.active").length === 0) {
            $("#NameDropdown li").first().addClass("active");
        } else if (e.keyCode === 40) { // down arrow
           var $active = $("#NameDropdown li.active");
           $active.removeClass("active")
           $active.next().addClass("active");
        } else if (e.keyCode === 38) { // up arrow
            var $active = $("#NameDropdown li.active");
            $active.removeClass("active")
            $active.prev().addClass("active");
        } else if (e.keyCode === 13) { // enter
            e.preventDefault();

            var name = $("#NameDropdown li.active span.name").text();
            var symbol = $("#NameDropdown li.active span.symbol").text();
            
            $("#NameDropdown").html("");
            $("#Name").val(name);
            $("#Symbol").val(symbol);
        }
    });
    $("#Symbol").keyup(function (e) {
        // ignore tab, enter, and arrow keys
        if (e.keyCode === 9 ||
            e.keyCode === 13 ||
            e.keyCode === 37 ||
            e.keyCode === 38 ||
            e.keyCode === 39 ||
            e.keyCode === 40)
            return;
        
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
    $("#Symbol").keydown(function (e) {
        if ($("#SymbolDropdown li.active").length === 0) {
            $("#SymbolDropdown li").first().addClass("active");
        } else if (e.keyCode === 40) { // down arrow
            var $active = $("#SymbolDropdown li.active");
            $active.removeClass("active")
            $active.next().addClass("active");
        } else if (e.keyCode === 38) { // up arrow
            var $active = $("#SymbolDropdown li.active");
            $active.removeClass("active")
            $active.prev().addClass("active");
        } else if (e.keyCode === 13) { // enter
            e.preventDefault();

            var name = $("#SymbolDropdown li.active span.name").text();
            var symbol = $("#SymbolDropdown li.active span.symbol").text();

            $("#SymbolDropdown").html("");
            $("#Name").val(name);
            $("#Symbol").val(symbol);
        }
    });
});