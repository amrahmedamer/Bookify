$(document).ready(function () {
    $('.page-link').on('click', function () {

        var btn = $(this);
        var pageNumber = btn.data('page-number');

        if (btn.parent().hasClass('active')) return;

        $('#PageNumber').val(pageNumber);
        $('#Filters').submit();

    });

    $(".js-date-range").daterangepicker({
        "autoUpdateInput": false,
        "showDropdowns": true,
        "autoApply": true,
        "minYear": 2020,
        "maxDate": new Date(),
    });
    $('.js-date-range').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
    });

})