var slidecheck = false;

$("legend").children().hover(
function () {
    $(this).append($('<img src="../../Content/client_basics_icons_edit.png" id="test1"/>'));
},
function () {
    $("#test1").remove();
});

$(function () {
    $("#splittingproperties").click(function () {
        if (slidecheck) {
            $("legend").each(function (index) {
                $(this).siblings().slideDown("slow");
            });
            slidecheck = false;
        }
        else {
            $("legend").each(function (index) {
                $(this).siblings().slideUp("slow");
            });
            slidecheck = true;
        }

    });
});

$(function () {
    $('legend').live('click', function () {
        $(this).siblings().slideToggle("slow");
    });
});


/*
$(function () {
    $("legend").each(function (index) {
        $(this).siblings().slideUp();
    });
});*/