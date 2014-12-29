// JavaScript source code
$(document).ready(function(){
    resizer();
    $(window).on('resize', resizer);
});
function resizer() {
    var windowsize = $(window).width();


    $('#customNav').toggleClass('text-right', (windowsize < 1200));
    $('#customNav').toggleClass('pull-right', (windowsize < 1200));

    $('.navbar-text').toggleClass('text-right', (windowsize < 1200));
    $('.navbar-text').toggleClass('pull-right', (windowsize < 1200));

    $('#customNav').toggleClass('text-right', (windowsize >= 1200));
    $('#customNav').toggleClass('pull-right', (windowsize >= 1200));

    $('.navbar-text').toggleClass('text-right', (windowsize >= 1200));
    $('.navbar-text').toggleClass('pull-right', (windowsize >= 1200));

    if (windowsize < 1200) {
        $('#brandHolder').css('text-align', 'center');
        $('#brandHolder').css('margin-bottom', '10px');
        $('.navbar-header').css('margin-top', '20px');
        $('.navbar-brand').css('float', 'center');
        $('#navUserHolder').removeAttr('style');
        $('.navbar-brand').addClass('text-center');
        $('#customNav').addClass('text-center');
        $('.navbar-text').addClass('text-center');
    }
    if (windowsize >= 1200) {
        $('#brandHolder').removeAttr('style');
        $('.navbar-header').removeAttr('style');
        $('.navbar-brand').removeAttr('style');
        $('#navUserHolder').css('padding-right', '0');
        $('.navbar-brand').removeClass('text-center');
        $('.navbar-text').removeClass('text-center');
        $('#customNav').removeClass('text-center');
    }

};