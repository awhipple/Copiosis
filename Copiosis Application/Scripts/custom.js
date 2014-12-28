// JavaScript source code
$(document).ready(function(){
    resizer();
    $(window).on('resize', resizer);
});
function resizer() {
    var windowsize = $(window).width();


    $('#navLoggedIn').toggleClass('text-right', (windowsize < 768));
    $('#navLoggedIn').toggleClass('pull-right', (windowsize < 768));

    $('.navbar-text').toggleClass('text-right', (windowsize < 768));
    $('.navbar-text').toggleClass('pull-right', (windowsize < 768));

    $('#navLoggedOut').toggleClass('text-right', (windowsize < 768));
    $('#navLoggedOut').toggleClass('pull-right', (windowsize < 768));

    $('#navLoggedIn').toggleClass('text-right', (windowsize >= 768));
    $('#navLoggedIn').toggleClass('pull-right', (windowsize >= 768));

    $('.navbar-text').toggleClass('text-right', (windowsize >= 768));
    $('.navbar-text').toggleClass('pull-right', (windowsize >= 768));

    $('#navLoggedOut').toggleClass('text-right', (windowsize >= 768));
    $('#navLoggedOut').toggleClass('pull-right', (windowsize >= 768));

    if (windowsize < 768) {
        $('#brandHolder').css('text-align', 'center');
        $('#brandHolder').css('margin-bottom', '10px');
        $('.navbar-header').css('margin-top', '20px');
        $('.navbar-brand').css('float', 'center');
        $('#navUserHolder').removeAttr('style');
        $('.navbar-brand').addClass('text-center');
        $('#navLoggedIn').addClass('text-center');
        $('.navbar-text').addClass('text-center');
        $('#navLoggedOut').addClass('text-center');
    }
    if (windowsize >= 768) {
        $('#brandHolder').removeAttr('style');
        $('.navbar-header').removeAttr('style');
        $('.navbar-brand').removeAttr('style');
        $('#navUserHolder').css('padding-right', '0');
        $('.navbar-brand').removeClass('text-center');
        $('.navbar-text').removeClass('text-center');
        $('#navLoggedIn').removeClass('text-center');
        $('#navLoggedOut').removeClass('text-center');
    }

};