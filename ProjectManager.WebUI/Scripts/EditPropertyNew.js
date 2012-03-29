$(function () {
    var cfocusid;
    var cnum = -1;
    var PropertyID;
    var ProjectID;
    var PropertyType;
    var PropertyType1;
    var MyType = "Bool";
    var PropertyValues = "";
    var xaxa = "";
    var respons = "";

    $("#values-edit :text").live('click', function () {
        cfocusid = this;
    });

    //"add new property" dialog constructor
    $('#dialog-edit').dialog({
        autoOpen: false,
        width: 400,
        modal: true,
        buttons: {
            "Save": function () {
                DoValues();
                console.log(respons);
                SendRequest(respons, PropertyID, ProjectID);                
            }
        },
        close: function () {
            $("#values-edit").children().remove();
        }
    });

    $("#editpropertybutton").live('click', function () {
        PropertyID = $(this).parent().find("div").attr("id");
        ProjectID = $("#ProjectId").val();
        GetPropertyType(PropertyID);
        $("#morelessbuttons1").hide();
        $("#values-edit").children().remove();
        GetView(PropertyID, ProjectID);
        console.log(MyType);
        if ($(this).parent().find("#testHover").find("h5").text() != "Title" && MyType != "Bool") {
            $("#morelessbuttons1").show();
        }
        $('#dialog-edit').dialog('open');
    });

    $("#more1").button().click(function () {
        cnum++;
        $('<input type="text" value="new value" name="text' + cnum + '" id=text"' + cnum + '"/>').appendTo($("#values-edit"));
    });

    $("#less1").button().click(function () {
        $(cfocusid).remove();
    });

    function GetPropertyType(PropID) {
        $.post(
        "/EditProperty/GetPropertyType",
        { ID: PropID },
        function (data) {
            MyType = data;
        });
    }

    function GetView(PropID, ProjID) {
        jQuery.ajaxSettings.traditional = true;
        $.post(
        "/EditProperty/GetResultNew",
        { PropertyID: PropID, ProjectID: ProjID },
        function (data) {
            $("#values-edit").children().remove();
            $("#values-edit").append(data);
        });
    }

    function DoValues() {
        if (MyType == "Bool") {
            respons = $("#values-edit :checkbox").is(":checked");
        }
        else {
            respons = "";
            var textboxes1 = $('#values-edit :text');
            for (i = 0; i < textboxes1.length; i++) {
                respons += '{';
                var value = textboxes1[i].value;
                respons += value;
                respons += '%';
                respons += "true";
                respons += '}';
            }
        }
    }

    function SendRequest(Val, PropID, ProjID) {
        $.post(
        "/EditProperty/SaveChangesDb",
        { types: MyType, Values: Val, PropertyID: PropID, ProjectID: ProjID },
        function (data) {
            $("#" + PropertyID).children().remove()
            $("#" + PropertyID).append(data);
            $("#dialog-edit").dialog("close");
        });
    }

});