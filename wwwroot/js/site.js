$(document).ready(function() {
    var labels = [];
    var values = [];
    var stock = $("#stock").text();

    $.get("https://api.iextrading.com/1.0/stock/" + stock + "/chart/1d", function(data) {
        var limit = 5;
        
        data.forEach(element => {
            if (limit > 0) {
                labels.push(element["minute"]);
                values.push(element["average"]);
                limit--;
            }
        });
    });

    console.log(labels);
    console.log(values);

    var ctx = $("#day-chart");
    var myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Price',
                data: values,
                backgroundColor: [
                    'rgba(99, 225, 132, 0.2)'
                ],
                borderColor: [
                    'rgba(99, 225, 132, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            },
            elements: {
                line: {
                    tension: 0
                }
            }
        }
    });
});