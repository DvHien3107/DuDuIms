$('nb-accordion-item.collapsed').on('click', function () {
    //$('.accordion-item-header-collapsed.collapsed').attr('type', 'hide');
    
    const currentType = $(this).attr('type') || 'hide';
    if (currentType == 'show') {
        $(this).attr('type', 'hide');
    } else {
        $(this).attr('type', 'show');
    }

})
$('.chevron').on('click', function () {
    //$('.accordion-item-header-collapsed.collapsed').attr('type', 'hide');

    const checkCollapsed = $(this).hasClass('collapsed');
    if (checkCollapsed) {
        $(this).removeClass('collapsed');
        $('.menu-sidebar.sidebar_class.left.ng-star-inserted').removeClass('compacted');
        $('.menu-sidebar.sidebar_class.left.ng-star-inserted').addClass('expanded');
    } else {
        $(this).addClass('collapsed');
        $('.menu-sidebar.sidebar_class.left.ng-star-inserted').removeClass('expanded');
        $('.menu-sidebar.sidebar_class.left.ng-star-inserted').addClass('compacted');
    }

})
//menu-sidebar sidebar_class left ng-star-inserted expanded
//menu-sidebar sidebar_class left ng-star-inserted compacted

//chevron collapsed