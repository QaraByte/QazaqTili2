// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {

    //$('#exampleModal').modal(options);

    //$('#exampleModal').on('show.bs.modal', function (event) {
    //    var button = $(event.relatedTarget) // Button that triggered the modal
    //    var recipient = button.data('whatever') // Extract info from data-* attributes
    //    // If necessary, you could initiate an AJAX request here (and then do the updating in a callback).
    //    // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
    //    var modal = $(this)
    //    modal.find('.modal-title').text('New message to ' + recipient)
    //    modal.find('.modal-body input').val(recipient)
    //})

    $(".edit-link").click(function () {
        let id = $(this).data('id');
        let href = $('.edit-links a.video-link').attr('href');

        let href_split = href.split('?');
        if (href_split.length > 1 && href_split[1][0]=='t') {
            let sec = href_split[1].split('=')[1];
            let t = sec / 60;
            $('#edit-time-modal').val(t);
        }

        $('#edit-link-modal').val(href);
        //$('#editModal')
        let yId = $(this).data('id');
        let action = $('#frmWord').attr('action');
        $('#frmWord').attr('action', action + '/' + yId);
    });

});

function changeLink() {
    let link = $('#edit-link-modal').val();
    let link_split = link.split('?');
    if (link_split.length > 0 && link_split[1][0] == 't') {
        let sec = link_split[1].split('=')[1];
        let t = sec / 60;
        let min = (t - t % 1);
        let sec1 = sec - min * 60;

        $('#edit-time-modal').val(min + ':' + sec1);

    }
}

