
$(document).ready(function () {
    //DOMSubtreeModified
    $('#DateRange').on("click", function () {
        console.log("change");
    });
});

//line chart 
drawRentalChart();
function drawRentalChart() {
    var element = document.getElementById('RentalPerDay');

    var height = parseInt(KTUtil.css(element, 'height'));
    var labelColor = KTUtil.getCssVariableValue('--kt-gray-500');
    var borderColor = KTUtil.getCssVariableValue('--kt-gray-200');
    var baseColor = KTUtil.getCssVariableValue('--kt-primary');
    var lightColor = KTUtil.getCssVariableValue('--kt-primary-light');

    if (!element) {
        return;
    }
    $.get({
        url: '/Dashboard/GetRentalPerDay',
        success: function (data) {

            var options = {
                series: [{
                    name: 'Books',
                    data: data.map(i => i.value),
                }],
                chart: {
                    fontFamily: 'inherit',
                    type: 'area',
                    height: height,
                    toolbar: {
                        show: false
                    }
                },
                plotOptions: {

                },
                legend: {
                    show: false
                },
                dataLabels: {
                    enabled: false
                },
                fill: {
                    type: 'solid',
                    opacity: 1
                },
                stroke: {
                    curve: 'smooth',
                    show: true,
                    width: 3,
                    colors: [baseColor]
                },
                xaxis: {
                    categories: data.map(i => i.label),
                    axisBorder: {
                        show: false,
                    },
                    axisTicks: {
                        show: false
                    },
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    },
                    crosshairs: {
                        position: 'front',
                        stroke: {
                            color: baseColor,
                            width: 1,
                            dashArray: 3
                        }
                    },
                    tooltip: {
                        enabled: true,
                        formatter: undefined,
                        offsetY: 0,
                        style: {
                            fontSize: '12px'
                        }
                    }
                },
                yaxis: {
                    tickAmount: Math.max(...data.map(i => i.value)),
                    min: 0,
                    labels: {
                        style: {
                            colors: labelColor,
                            fontSize: '12px'
                        }
                    }
                },
                states: {
                    normal: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    hover: {
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    },
                    active: {
                        allowMultipleDataPointsSelection: false,
                        filter: {
                            type: 'none',
                            value: 0
                        }
                    }
                },
                tooltip: {
                    style: {
                        fontSize: '12px'
                    }
                },
                colors: [lightColor],
                grid: {
                    borderColor: borderColor,
                    strokeDashArray: 4,
                    yaxis: {
                        lines: {
                            show: true
                        }
                    }
                },
                markers: {
                    strokeColor: baseColor,
                    strokeWidth: 3
                }
            };

            var chart = new ApexCharts(element, options);
            chart.render();

        }
    });

}


//pie chart 

drawCityChart();
function drawCityChart() {

    var ctx = document.getElementById('PieChart');

    //// Define colors
    var primaryColor = KTUtil.getCssVariableValue('--kt-primary');
    var dangerColor = KTUtil.getCssVariableValue('--kt-danger');
    var successColor = KTUtil.getCssVariableValue('--kt-success');
    var warningColor = KTUtil.getCssVariableValue('--kt-warning');
    var infoColor = KTUtil.getCssVariableValue('--kt-info');

    $.get({
        url: '/Dashboard/GetRentalPerCity',
        success: function (data) {
            var data = {
                labels: data.map(i => i.label),
                datasets: [{
                    label: 'My First Dataset',
                    data: data.map(i => i.value),
                    backgroundColor: [
                        primaryColor,
                        dangerColor,
                        successColor,
                        warningColor,
                        infoColor,
                    ],
                    hoverOffset: 4,
                   borderRadius:8
                }]
            };

            // Chart config
            const config = {
                //if you want pie
                type: 'doughnut',
                data: data,
            };

            //Init ChartJS -- for more info, please visit: https://www.chartjs.org/docs/latest/
            var myChart = new Chart(ctx, config);

        }
    });


}

