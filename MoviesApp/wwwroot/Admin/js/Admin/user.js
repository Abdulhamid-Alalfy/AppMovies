$(document).ready(function () {
    $('#searchInput').on('keyup', function () {
        var filter = $(this).val().toLowerCase();
        $('#userTable tr').filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(filter) > -1);
        });
    });
});