/* 
   JKTurnos - Layout Scripts
*/

$(document).ready(function () {
    // Toggle Mobile Menu
    $('#mobile-menu-toggle').click(function () {
        $('#sidebar').toggleClass('d-none');
        // If sidebar is visible, position it as an overlay on mobile
        if (!$('#sidebar').hasClass('d-none')) {
            $('#sidebar').css({
                'position': 'fixed',
                'z-index': '1000',
                'height': '100vh',
                'background': 'white',
                'width': '260px',
                'display': 'flex'
            });
            // Add overlay to close when clicking outside
            if ($('.mobile-overlay').length === 0) {
                $('body').append('<div class="mobile-overlay" style="position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.5);z-index:999;"></div>');
            }
        }
    });

    $(document).on('click', '.mobile-overlay', function () {
        $('#sidebar').addClass('d-none');
        $('.mobile-overlay').remove();
    });
});
