// Admin Layout JavaScript
$(document).ready(function () {
    // Sidebar Toggle
    $('#mobileSidebarToggle, #sidebarToggle').on('click', function () {
        $('#sidebar').toggleClass('show');
    });
    
    // Close sidebar when clicking outside on mobile
    $(document).on('click', function (e) {
        if ($(window).width() <= 991) {
            if (!$(e.target).closest('#sidebar, #mobileSidebarToggle').length) {
                $('#sidebar').removeClass('show');
            }
        }
    });
    
    // Search Toggle
    $('.search-toggle').on('click', function () {
        $('.search-box').toggleClass('active');
        if ($('.search-box').hasClass('active')) {
            $('.search-box input').focus();
        }
    });
    
    $('.search-close').on('click', function () {
        $('.search-box').removeClass('active');
    });
    
    // Close search when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.header-search').length) {
            $('.search-box').removeClass('active');
        }
    });
    
    // Active menu item highlight
    var currentPath = window.location.pathname;
    $('.nav-link').each(function () {
        var linkPath = $(this).attr('href');
        if (currentPath.includes(linkPath) && linkPath !== '/') {
            $(this).addClass('active');
        }
    });
    
    // Smooth scroll for sidebar
    $('.sidebar-nav').on('scroll', function () {
        // Add any scroll behavior if needed
    });
    
    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);
});


