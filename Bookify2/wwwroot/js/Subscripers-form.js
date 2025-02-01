$(document).ready(function () {
    $('.governoratesId').on('change', function () {
        var GovernorateId = $(this).val();
        console.log(GovernorateId);

        var areaList = $('.AreaId');
        areaList.empty();
        areaList.append("<option>Select Area</option>");

        $.get({
            url: "/Subscribers/GetArea?governratesId=" + GovernorateId,
            success: function (areas) {
                $.each(areas, function (i, area) {
                    areaList.append($("<option></option>").attr("value", area.value).text(area.text));
                })
            },
            error: function () {
                console.log("error");
            }

        });
    });
});