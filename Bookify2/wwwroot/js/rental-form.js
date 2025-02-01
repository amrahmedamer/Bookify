var CurrentCopies;
var SelectedCopies = [];
var isEditMode = false;
$(document).ready(function () {

    if ($('.js-copy').length > 0) {
       var x= prepareInput();
        CurrentCopies = SelectedCopies;
        isEditMode = true;
    }
    $('.js-search').on('click', function (e) {
        e.preventDefault();
        var serial = $('#value').val();
        if (SelectedCopies.find(c => c.serial == serial)) {
            ShowMessageError("You cannot add the same copy");
            return;
        }

        if (SelectedCopies.length >= maxAllowedCopies) {
            ShowMessageError(`You cannot add more than ${maxAllowedCopies} copy(s)`);
            return;
        }

        $('#SearchForm').submit();
    });

    $('body').delegate('.js-remove', 'click', function () {
        var btn = $(this);
        var container = btn.parents('.js-copy-container');
        if (isEditMode) {
            btn.toggleClass('btn-light-danger btn-light-success js-remove js-readd').text('Re-Add');
            container.find('img').css("opacity", 0.5);
            container.find('h4').css("text-decoration", "line-through");
            container.find('.js-copy').toggleClass('js-copy js-removed').removeAttr('name').removeAttr('id');
        } else {
            container.remove()
        }

        prepareInput();
        if (!$.isEmptyObject(SelectedCopies) || JSON.stringify(CurrentCopies) == JSON.stringify(SelectedCopies))
            $('#CopiesForm').find(':submit').addClass('d-none');
        else
            $('#CopiesForm').find(':submit').removeClass('d-none');
    });

    $('body').delegate('.js-readd', 'click', function () {
        var btn = $(this);
        var container = btn.parents('.js-copy-container');

        btn.toggleClass('btn-light-danger btn-light-success js-remove js-readd').text('Remove');
        container.find('img').css("opacity", 1);
        container.find('h4').css("text-decoration", "none");
        container.find('.js-removed').toggleClass('js-copy js-removed');

        prepareInput();

        if (JSON.stringify(CurrentCopies) == JSON.stringify(SelectedCopies))
            $('#CopiesForm').find(':submit').addClass('d-none');
        else
            $('#CopiesForm').find(':submit').removeClass('d-none');
    });

});

function onAddCopySuccess(copy) {
    $('#value').val('');

    var bookId = $(copy).find('.js-copy').data('book-id');

    if (SelectedCopies.find(c => c.bookId == bookId)) {
        ShowMessageError("You cannot add more than one copy for the same book");
        return;
    }

    $('#CopiesForm').prepend(copy);
    $('#CopiesForm').find(":submit").removeClass('d-none');
    prepareInput();
};
function prepareInput() {
    var copies = $('.js-copy');
    SelectedCopies = [];
    $.each(copies, function (i, input) {
        var $input = $(input);
        SelectedCopies.push({ serial: $input.val(), bookId: $input.data('book-id') });
        $input.attr('name', `SelectedCopies[${i}]`).attr('id', `SelectedCopies_${i}_`);
    })
}
