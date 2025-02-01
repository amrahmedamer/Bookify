var UpdateRow;
var table;
var datatable;
var exportedCols = [];
function Select2(message = "Saved Successfuly!") {
    $('.js-select2').select2();
    $('.js-select2').on('select2:select', function (e) {
        $('form').validate().element('#' + $(this).attr('id'));
    });
}
function ShowMessageSuccess(message = "Saved Successfuly!") {
    Swal.fire({
        icon: 'success',
        title: 'Good Job', 
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    })
}
function ShowMessageError(message = "Something went wrong!") {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message.responseText != undefined ? message.responseText : message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    })
}
function DisabledSubmitButtom(btn) {
    $(btn).attr('disabled', 'disabled').attr('data-kt-indicator', 'on');
}

function onModalBegin() {
    DisabledSubmitButtom($('#Modal').find(':submit'));
}
function OnModalSuccess(row) {
    ShowMessageSuccess();
    $('#Modal').modal("hide");

    if (UpdateRow !== undefined) {
        datatable.row(UpdateRow).remove().draw();
        UpdateRow = undefined;
    }
    var newRow = $(row);
    datatable.row.add(newRow).draw();
}
function onModalCompleted() {
    $('body :submit').removeAttr('disabled').removeAttr('data-kt-indicator');
}

//DataTables
var headers = $('th');
$.each(headers, function (i) {
    if (!$(this).hasClass('js-no-export'))
        exportedCols.push(i);
});

// Class definition
var KTDatatablesExample = function () {

    // Private functions
    var initDatatable = function () {

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            'info': false,
            'pageLength': 10,
            'drawCallback': function () {
                KTMenu.createInstances();
            }
        });
    }
    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('.js-datatable').data('document-title');
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    },
                     customize: function (doc) {
                        pdfMake.fonts = {
                            Arial: {
                                normal: "arial",
                                bold: "arial",
                                italics: "arial",
                                bolditalics: "arial"
                            }
                        }
                        doc.defaultStyle.font = "Arial";
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }


    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-datatable');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();
//datePicker
$(document).ready(function () {
    $('.js-datepicker').daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: 'down',
        maxDate: new Date()
    });
});
$(document).ready(function () {
    //Disable submit button
    $('form').not('#SignOut').not('.js-excluded-validation').on('submit', function () {

        //TinyMce
        if ($('.js-tiny').length > 0) {
            $('.js-tiny').each(function () {

                var input = $(this);
                var content = tinyMCE.get(input.attr('id')).getContent();
                input.val(content);

            })
        }

        var isValid = $(this).valid();
        if (isValid) DisabledSubmitButtom($(this).find(':submit'));
    })

    //TinyMce
    if ($('.js-tiny').length > 0) {
        var options = { selector: ".js-tiny", height: "377" };

        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }

        tinymce.init(options); var options = { selector: "#kt_docs_tinymce_basic", height: "480" };

        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }

        tinymce.init(options);
    }
    //select2
    Select2();

    //Apply datatable
    KTUtil.onDOMContentLoaded(function () {
        KTDatatablesExample.init();
    });

    //Handle bootStrap Modal
    $('body').delegate('.js-render-modal', 'click', function () {

        var btn = $(this);
        var modal = $('#Modal');
        modal.find('#ModalTitle').text(btn.data('title'));

        if (btn.data('update') !== undefined) {
            UpdateRow = btn.parents('tr');
        }

        $.get({
            url: btn.data('url'),
            success: function (form) {
                modal.find('.modal-body').html(form);
                $.validator.unobtrusive.parse(modal);
                Select2();

            },
            error: function () {
                ShowMessageError();
            },

        })

        modal.modal('show');

    })
    //Handle Toggle Status
    $('body').delegate('.js-toggle-status', 'click', function () {
        var btn = $(this);
        bootbox.confirm({
            message: 'Are you sure that you need to toggle this item status?',
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
                        url: btn.data('url'),
                        data: {
                            "__RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (LastUpdatedOn) {
                            var row = btn.parents('tr');
                            var status = row.find('.js-status');
                            var newStatus = status.text().trim() === 'Deleted' ? 'Available' : 'Deleted';
                            status.text(newStatus).toggleClass('badge-light-danger badge-light-success');
                            row.find('.js-updatedate').html(LastUpdatedOn);
                            row.addClass("animate__animated animate__flash");

                            ShowMessageSuccess();
                        },
                        error: function () {
                            ShowMessageError();

                        }
                    });
                }
            }
        });

    })
    //Handle confirm lockout
    $('body').delegate('.js-confirm', 'click', function () {
        var btn = $(this);
        bootbox.confirm({
            message: btn.data('message'),
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
                        url: btn.data('url'),
                        data: {
                            "__RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (LastUpdatedOn) {

                            ShowMessageSuccess();
                        },
                        error: function () {
                            ShowMessageError();

                        }
                    });
                }
            }
        });

    })
    //Handel Submit Sign Out
    $('.js-signout').on('click', function () {
        $(this).parents('form').submit();
        //$('#SignOut').submit();
    });

}
);