﻿$(function () {

    var num = -1;

    var focusid;
    var radiofocus;
    var needtoaddnum = true;
    var needtoaddbool = true;
    var needtoadddatetime = true;
    var needtoaddstring = true;
    var mytype = "";

    $("#values :text").live('click', function () {
        focusid = this;
    });

    $("#radios :radio").live('click', function () {
        radiofocus = this;
        console.log("radios click " + $(this).val());
        switch ($(this).val()) {
            case "Number":
                {
                    needtoaddbool = true;
                    needtoadddatetime = true;
                    needtoaddstring = true;
                    if (needtoaddnum) {
                        $("#values").children().remove();
                        $("#more").click()
                        $("#morelessbuttons").show();
                        needtoaddnum = false;
                    }
                    break;
                }
            case "Bool":
                {
                    needtoaddnum = true;
                    needtoadddatetime = true;
                    needtoaddstring = true;
                    if (needtoaddbool) {
                        $("#morelessbuttons").hide();
                        $("#values").children().remove();
                        $('<input type="checkbox" id="boolcheckbox" checked="checked"/>').appendTo($("#values"));
                        needtoaddbool = false;
                    }
                    break;
                }
            case "DateTime":
                {
                    console.log("in DateTime");
                    needtoaddnum = true;
                    needtoaddbool = true;
                    needtoaddstring = true;
                    if (needtoadddatetime) {
                        $("#values").children().remove();
                        $("#more").click();
                        $("#morelessbuttons").show();
                        //$("#values :checkbox").remove();
                        needtoadddatetime = false;
                    }
                    break;
                }
            case "String":
                {
                    needtoaddnum = true;
                    needtoaddbool = true;
                    needtoadddatetime = true;
                    if (needtoaddstring) {
                        $("#values").children().remove();
                        $("#morelessbuttons").show();
                        $("#more").click();
                        needtoaddstring = false;
                    }
                    break;
                }
            default: { break; }
        }
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
            if ($(radiofocus).val() == "Bool") {
                var resp = $("#boolcheckbox").is(":checked").toString();
            }
            else {
                var resp = "";
                var checkboxes = $('#values :checkbox');
                var textboxes = $('#values :text');
                for (i = 0; i < textboxes.length; i++) {
                    resp += '{';
                    var value = textboxes[i].value;
                    resp += value;
                    resp += '%';
                    var check = checkboxes[i].checked;
                    resp += check;
                    resp += '}';
                }
            }
            switch ($(radiofocus).val()) {
                case "Number":
                    {
                        refreshHomes($("#property_name").val(), $("#ispublic").is(":checked"), resp, $("#ProjectId").val(), "Number");
                        break;
                    }
                case "Bool":
                    {
                        refreshHomes($("#property_name").val(), $("#ispublic").is(":checked"), resp, $("#ProjectId").val(), "Bool");
                        break;
                    }
                case "DateTime":
                    {
                        console.log("processing DateTime");
                        console.log(resp);
                        refreshHomes($("#property_name").val(), $("#ispublic").is(":checked"), resp, $("#ProjectId").val(), "DateTime");
                        break;
                    }
                case "String":
                    {
                        refreshHomes($("#property_name").val(), $("#ispublic").is(":checked"), resp, $("#ProjectId").val(), "String");
                        break;
                    }
            }
        }
    });

    //handler open "add new property" dialog
    $("#property-addnew").button().click(function () {
        $('#dialog-addnew').dialog('open');
    });

    $("#property-addnew2").click(function () {
        $('#dialog-addnew').dialog('open');
    });

    //handler add textbox+checkbox
    $("#more").button().click(function () {
        num++;
        $('<div id="text' + num + '"><input type="checkbox" checked="checked" name="check' + num + '" id="check' + num + '"><input type="text" value="new value" name="text' + num + '" id=text"' + num + '"/></div>').appendTo($("#values"));
        if ($(radiofocus).val().toString() == "Number" || $(radiofocus).val().toString() == "DateTime") {
            $("#values :checkbox").hide();
        }
    });

    $("#less").button().click(function () {
        $("#" + focusid.name).remove();
    });
});

function refreshHomes(propertyname, ispublic, values, id, mynewtype) {
    jQuery.ajaxSettings.traditional = true;
    $("#loading-content").slideDown('fast', function () {
        $.post(
                    "/Project/AddNewPropertyDialog",
                    { PropertyName: propertyname, PropertyIsPublic: ispublic, PropertyValues: values, PropertyType: mynewtype, ProjectId: id },
                    function (data) {
                        $("#inserthere").after().append(data);
                        $("#loading-content").slideUp("slow");
                    });
    });
}