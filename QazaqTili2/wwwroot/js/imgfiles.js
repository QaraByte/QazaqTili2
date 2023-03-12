let slideIndex = 1;

$(document).ready(function () {
    ShowSlides(slideIndex);
});

function plusSlides(n) {
    ShowSlides(slideIndex += n);
}

function currentSlide(n) {
    ShowSlides(slideIndex = n);
}

function ShowSlides(n) {
    let i;
    let slides = $('.mySlide');
    let dots = $('.dot');

    if (n > slides.length) {
        slideIndex = 1;
    }
    if (n < 1) {
        slideIndex = slides.length;
    }

    $('.mySlide').css('display', 'none');
    $('.dot').removeClass('active');
    slides[slideIndex - 1].style.display = 'block';
    dots[slideIndex - 1].className += ' active';
}
