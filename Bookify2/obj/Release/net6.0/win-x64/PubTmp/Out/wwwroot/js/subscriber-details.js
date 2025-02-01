$(document).ready(function () {
    $('.js-renew').on('click', function () {
        var btn = $(this);
        bootbox.confirm({
            message: 'Are you sure that you need to renew subscription?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: `/Subscribers/RenewSubscription?sKey=${btn.data('key')}`,
                        data: {
                            "__RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (row) {

                            $('#subscriptionTable').find('tbody').append(row);

                            var activeIcon = $('#ActiveStatusIcon');
                            activeIcon.removeClass('d-none');
                            activeIcon.siblings('svg').remove();

                            $('#Cardstatus').removeClass('bg-warning').addClass('bg-success');
                            $('#badgeStatus').removeClass('badge-light-warning').addClass('badge-light-success');
                            $('.SubscriperStatus').text('Active Subscriber');
                            $('#RentalButton').removeClass('d-none');

                            ShowMessageSuccess();
                        },
                        error: function () {
                            ShowMessageError();
                        }
                    });
                }
            }
        });
    });
    $('.js-cacele-rental').on('click', function () {
        var btn = $(this);
        bootbox.confirm({
            message: 'Are you sure that you need to cancel this rental?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: `/Rentals/MarkAsDeleted/${btn.data('id')}`,
                        data: {
                            "__RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function () {
                            btn.parents('tr').remove();
                            if ($('#RentalsTable tbody tr').length == 0) {
                                $('#RentalsTable').fadeOut(function () {
                                    $('#Alert').fadeIn();
                                });
                            }

                            var oldNumberOfCopies = $('#NumberOfCopy').text();
                            $('#NumberOfCopy').text(oldNumberOfCopies - btn.data('num'));

                        },
                        error: function (row) {
                            ShowMessageError();
                        }
                    });
                }
            }
        });
    });

});