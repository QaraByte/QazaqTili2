// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {

    $(".edit-link").click(function () {
        //let id = $(this).data('id');
        let href = $('.edit-links a.video-link').attr('href');

        let time = GetUnderstandableTime(href);
        if (time) {
            $('#edit-time-modal').val(time);
        }

        //let href_split = href.split('?');
        //if (href_split.length > 1 && href_split[1][0]=='t') {
        //    let sec = href_split[1].split('=')[1];
        //    let t = sec / 60;
        //    $('#edit-time-modal').val(t);
        //}

        let title = $(this).parent().find('span').first().text();
        if (title) {
            $('#name-modal').val(title);
        }
        
        $('#edit-link-modal').val(href);
        //$('#editModal')
        let yId = $(this).data('id');
        let action = $('#frmWord').attr('action');
        $('#frmWord').attr('action', action + '/' + yId);
    });


    const input = document.querySelector('#search-word');
    //const log = document.getElementById('log');

    input.addEventListener('change', SearchWord);
});

function changeLink() {
    let link = $('#edit-link-modal').val();

    let time = GetUnderstandableTime(link);
    if (time) {
        $('#edit-time-modal').val(time);
    }
    

    //let link_split = link.split('?');
    //if (link_split.length > 1 && link_split[1][0] == 't') {
    //    let sec = link_split[1].split('=')[1];
    //    let t = sec / 60;
    //    let hour = 0;
    //    let min = 0;
    //    let sec1 = 0;
    //    if (t > 60) {
    //        hour = Math.trunc(t / 60);
    //        min = Math.trunc(t - hour * 60);
    //        sec1 = sec - (hour * 3600 + min * 60);
    //    }
    //    else {
    //        min = (t - t % 1);
    //        sec1 = sec - min * 60;
    //    }
        
    //    if (sec1.toString().length == 1)
    //        sec1 = '0' + sec1;

    //    if (t > 60)
    //        $('#edit-time-modal').val(hour + ':' + min + ':' + sec1);
    //    else
    //        $('#edit-time-modal').val(min + ':' + sec1);

    //}
}

function GetUnderstandableTime(link) {
    let link_split = link.split('?');
    if (link_split.length > 1 && link_split[1][0] == 't') {
        let sec = link_split[1].split('=')[1];
        let t = sec / 60;
        let hour = 0;
        let min = 0;
        let sec1 = 0;
        if (t > 60) {
            hour = Math.trunc(t / 60);
            min = Math.trunc(t - hour * 60);
            sec1 = sec - (hour * 3600 + min * 60);
        }
        else {
            min = (t - t % 1);
            sec1 = sec - min * 60;
        }

        if (sec1.toString().length == 1)
            sec1 = '0' + sec1;

        if (t > 60)
            return hour + ':' + min + ':' + sec1;
        else
            return min + ':' + sec1;
    }
    return '';
}

function SearchWord(e) {
    //log.textContent = e.target.value;
    console.log(e.target.value);
}

