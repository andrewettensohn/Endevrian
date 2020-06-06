function toggleNavBarScrollAndIcons() {

    $("#iconNavBarClose").toggleClass("d-none");
    $("#iconNavBarOpen").toggleClass("d-none");

    if ($('html, body').css("overflow") == "hidden") {

        $('html, body').css({
            overflow: 'auto',
            height: 'auto'
        });

    } else {

        $('html, body').css({
            overflow: 'hidden',
            height: '100%'
        });
    }
}