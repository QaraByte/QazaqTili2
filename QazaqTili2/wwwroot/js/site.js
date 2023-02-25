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
        let action = $('#frmLink').attr('action');
        let actionSplit = action.split('/');
        if (actionSplit.length > 3) {
            //if (isNumeric(actionSplit[3])){
            //    actionSplit[3] = yId;
            //}
            actionSplit.splice(3, 1);
            action = actionSplit.join('/');
        }
        $('#frmLink').attr('action', action + '/' + yId);
    });

    $('.edit-word').click(function () {
        let word = $('.word-name').text();
        $('#name').val(word);
        let createTime = $('.createtime').text();
        $('#createtime').val(createTime);
    })

    $('.remove-link').click(function () {
        let id = $(this).data('id');
        $(this).parent().find('a').removeClass('selected-remove');
        $(this).addClass('selected-remove');

        Swal.fire({
            title: 'Вы точно хотите удалить ссылку? <br><span class="sure-remove-link">' + $(this).parent().find('.video-link').attr('href') + '</span>',
            //showDenyButton: true,
            showCancelButton: true,
            confirmButtonText: 'Удалить',
            denyButtonText: `Отмена`,
        }).then((result) => {
            /* Read more about isConfirmed, isDenied below */
            if (result.isConfirmed) {
                //Swal.fire('Ссылка удалена!', '', 'success')
                let id = $('.selected-remove').data('id');
                var posting = $.post('/Home/RemoveLink', { 'id': id });
                posting.done(function (data) {
                    console.log(data);
                })
                    .fail(function (data) {
                        if (data.status == 400) {
                            Swal.fire('Неверный запрос!', '', 'warning')
                        }
                        else {
                            Swal.fire('Ошибка удаления!', '', 'error')
                        }
                    });
            }
        })
    })

    $(".page").click(function () {

        SelectPage($(this));
    });

    $(document).on("click", "#submit-button", function (e) {
        e.preventDefault();

        /*var searchWord = $("#search-word").val();*/

        var formData = $('#frmWord').serializeArray();
        var data = {};
        $(formData).each(function (index, obj) {
            data[obj.name] = obj.value;
        });
        var jsonData = JSON.stringify(data);

        $.ajax({
            type: "POST",
            url: "/Home/AddWord",
            data: jsonData,
            contentType: "application/json",
            success: function (data) {
                /*$("#result").html(data);*/
                //toastr.error(data);
                location.reload();
            }
        })
            .fail(function (data) {
                //if (data.status == 400) {
                console.log(data);
                if (data.status == 0)
                    toastr.error('Ошибка приложения.')
                if (data.responseText.includes('SqlClient') || data.responseText.includes('error'))
                    toastr.error('Ошибка сервера.')
                else
                    toastr.error(data.responseText);
                //}
            });;
    });

    const input = document.querySelector('#search-word');
    //const log = document.getElementById('log');

    if (input != null)
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

    $.ajax({
        url: "/Home/SearchWord",
        data: { word: e.target.value }
    })
        .done(function (msg) {
            $('#table-words').empty();
            $('#table-words').html(msg);
        })
        .fail(function (msg) {
            console.log(msg.responseText);
        });
}

function SelectPage(page) {

    let pageN = $(page).data('page-number');

    $.ajax({
        url: "/Home/SelectPageAjax",
        data: { page: pageN }
    })
        .done(function (msg) {
            $('#table-words').empty();
            $('#table-words').html(msg);
            $(".page").click(function () {
                SelectPage($(this));
            });
        })
        .fail(function (msg) {
            console.log(msg.responseText);
        });
}

function isNumeric(s) {
    return !isNaN(s - parseFloat(s));
}

