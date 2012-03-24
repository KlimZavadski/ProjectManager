$(function () {

    var num = -1;

    var focusid;

    $("#values :text").live('click', function () {
        focusid = this;
        console.log(this);
    });

    //"add new property" dialog constructor
    $('#dialog-addnew').dialog({
        autoOpen: false,
        width: 400,
        modal: true,
        buttons: {
            "Add": function () {
                $(this).dialog("close");
            }
        },
        close: function () {
            var checkboxes = $('#values :checkbox');
            var textboxes = $('#values :text');
            var resp = "";
            for (i = 0; i < textboxes.length; i++) {
                resp += '{';
                var value = textboxes[i].value;
                resp += value;
                resp += '%';
                var check = checkboxes[i].checked;
                resp += check;
                resp += '}';
                resp += ',';
            }
            refreshHomes($("#property_name").val(), $("#ispublic").is(":checked"), $("#isnumber").is(":checked"), resp, $("#ProjectId").val());
        }
    });

    //handler open "add new property" dialog
    $("#property-addnew").button().click(function () {
        $('#dialog-addnew').dialog('open');
    });

    //handler add textbox+checkbox
    $("#more").button().click(function () {
        num++;
        $('<input type="text" value="new value" name="text' + num + '" id=text"' + num + '"/>').appendTo($("#values"));
        //$('<input type="text" id="text"' + num + '"/>').appendTo($("#values"));
        $('<input type="checkbox" checked="checked" name="check' + num + '" id="check' + num + '">').appendTo($("#values"));
    });

    $("#less").button().click(function () {
        var textboxes = $("#values :text");
        var checkboxes = $("#values :checkbox");
        for (i = 0; i < textboxes.length; i++) {
            console.log(i);
            console.log(focusid);
            if (textboxes[i].id == focusid.id) {
                console.log("catch focus");
                $(textboxes[i]).remove();
                $(checkboxes[i]).remove();
                console.log("true");
                break;
            }
        }
    });

});

function refreshHomes(propertyname, ispublic, isnumber, values, id) {
    jQuery.ajaxSettings.traditional = true;
    $("#loading-content").slideDown('fast', function () {
        $.post(
                    "/Project/Test2",
                    { PropertyName: propertyname, PropertyIsPublic: ispublic, PropertyIsNumber: isnumber, PropertyValues: values, ProjectId: id },
                    function (data) {
                        //$("#home-list").html(data);
                        $("#loading-content").slideUp();
                    });
    });
}