function GetDetailsChartData(url) {
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
    var _chartIncomeIncurred = _data[0];
    var _chartExpenseIncurred = _data[1];
    var _chartResultIncurred = _data[2];
    var _chartPreviousBalance = _data[3];
    var _chartAccumutaledBalanceIncurred = _data[4];
    var _chartIncomeForecast = _data[5];
    var _chartExpenseForecast = _data[6];
    var _chartResultForecast = _data[7];
    var _chartAccumutaltedBalanceForecast = _data[8];
    var resultIncurredColor = _chartResultIncurred < 0 ? '#fc051a' : '#0d05f2'
    var resultForecastColor = _chartResultForecast < 0 ? '#f06975' : '#8985f2'
    var barColor = ['#8c8e91', '#1b9427', '#14661c', '#edbb74', '#e67017', resultForecastColor, resultIncurredColor, 'black', '#929199'];
    new Chart("myChart", {
        type: 'bar',
        data: {
            datasets: [{
                backgroundColor: barColor,
                data: [
                    { x: 'Saldo Anterior', y: _chartPreviousBalance },
                    { x: 'Receita Prevista', y: _chartIncomeForecast },
                    { x: 'Receita Realizada', y: _chartIncomeIncurred },
                    { x: 'Despesa Prevista', y: _chartExpenseForecast },
                    { x: 'Despesa Realizada', y: _chartExpenseIncurred },
                    { x: 'Resultado Previsto', y: _chartResultForecast },
                    { x: 'Resultado Realizado', y: _chartResultIncurred },
                    { x: 'Saldo Acumulado Previsto', y: _chartAccumutaltedBalanceForecast },
                    { x: 'Saldo Acumulado Realizado', y: _chartAccumutaledBalanceIncurred }
                ]
            }]
        },
        options: {
            plugins: {
                legend: {
                    display: false,
                },
                title: {
                    display: false,
                },
            },
            responsive: true,
            scales: {
                x: {
                    stacked: true,
                },
                y: {
                    stacked: true
                }
            }
        }
    });
}
function OnError(err) {
    console.error(err);
}
