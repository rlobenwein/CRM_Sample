function GetIndexChartData(url) {
    $.ajax({
        type: "POST",
        url: url,
        contextType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccessResult,
        error: OnError
    });
}

function OnSuccessResult(data) {
    var _data = data;
    var _chartLabels = _data[0];
    var _chartIncome = _data[1];
    var _chartExpense = _data[2];
    var _chartResult = _data[3];
    var _chartAccumulatedBalance = _data[4];
    var resultColor = _chartResult < 0 ? 'red' : 'blue'
    new Chart("myChart", {
        type: 'bar',
        data: {
            labels: _chartLabels,
            datasets: [{
                type: 'line',
                label: 'Saldo Acumulado',
                backgroundColor: 'black',
                data: _chartAccumulatedBalance,
            }, {
                label: 'Receita',
                backgroundColor: 'green',
                data: _chartIncome,
            }, {
                label: 'Despesa',
                backgroundColor: 'orange',
                data: _chartExpense,
            }, {
                label: 'Resultado',
                backgroundColor: 'gray',
                data: _chartResult,
            }]
        },
        options: {
            plugins: {
                legend: {
                    display: true,
                },
                title: {
                    display: false,
                },
            },
            responsive: true,
        }
    });
}
function OnError(err) {
    console.error(err);
}
