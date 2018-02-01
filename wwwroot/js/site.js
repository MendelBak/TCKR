function data() {
    var highs = [];
    var avgs = [];
    var lows = [];
    var stock = $("#stock").text();

    $.get("https://api.iextrading.com/1.0/stock/" + stock + "/chart/1d?filter=minute,high,low,average", function (data) {
        var x = 0;
        data.forEach(element => {
            var time = element["minute"];
            var timeSplit = time.split(":");
            var minute

            highs.push({x: x, y: element["high"]});
            avgs.push({x: x, y: element["average"]});
            lows.push({x: x++, y: element["low"]});
        });
    });

    return [
        {
            values: highs,
            key: 'High',
            color: '#00ff00'
        },
        {
            values: avgs,
            key: 'Average',
            color: '#0000ff'
        },
        {
            values: lows,
            key: 'Low',
            color: '#ff0000'
        }
    ];
}

function moreData() {
    var sin = [],
        cos = [];

    for (var i = 0; i < 100; i++) {
        sin.push({ x: i, y: Math.sin(i / 10) });
        cos.push({ x: i, y: .5 * Math.cos(i / 10) });
    }

    return [
        {
            values: sin,
            key: 'Sine Wave',
            color: '#ff7f0e'
        },
        {
            values: cos,
            key: 'Cosine Wave',
            color: '#2ca02c'
        }
    ];
}


$(document).ready(function() {
    // var labels = [];
    // var values = [];
    // var stock = $("#stock").text();

    // $.get("https://api.iextrading.com/1.0/stock/" + stock + "/chart/1d", function(data) {
    //     var limit = 5;
        
    //     data.forEach(element => {
    //         labels.push(element["minute"]);
    //         values.push(element["average"]);
    //     });
    // });

    // console.log(labels);
    // console.log(values);

    console.log(data());

    nv.addGraph(function () {
        var chart = nv.models.lineChart()
            .useInteractiveGuideline(true);

        chart.xAxis
            .axisLabel('Time (HH:MM)')
            .tickFormat(d3.format(',r'));

        chart.yAxis
            .axisLabel('Price ($)')
            .tickFormat(d3.format('.02f'));

        d3.select('#chart svg')
            .datum(data())
            .transition().duration(500)
            .call(chart);

        nv.utils.windowResize(chart.update);

        return chart;
    });

});