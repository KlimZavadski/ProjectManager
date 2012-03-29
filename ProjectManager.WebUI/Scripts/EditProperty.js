$(function () {
    var PropertyID;
    var ProjectID;
    var PropertyType;
    var PropertyValues = "";
    //"add new property" dialog constructor
    $('#dialog-edit').dialog({
        autoOpen: false,
        width: 400,
        modal: true,
        buttons: {
            "Save": function () {
                DoValues();
                SendRequest(PropertyValues, PropertyID, ProjectID);
            }
        },
        close: function () {
            $("#values-edit").children().remove();
        }
    });

    $("#editpropertybutton").live('click', function () {
        $("#values-edit").children().remove();
        PropertyID = $(this).parent().find("div").attr("id");
        ProjectID = $("#ProjectId").val();
        $('#dialog-edit').dialog('open');
        GetPropertyType(PropertyID);
        GetView(PropertyID, ProjectID);
    });

    function GetPropertyType(PropID) {
        $.post(
        "/EditProperty/GetPropertyType",
        { ID: PropID },
        function (data) {
            PropertyType = data;
        });
    }

    function GetView(PropID, ProjID) {
        $.post(
        "/EditProperty/GetViewForProperty",
        { PropertyID: PropID, ProjectID: ProjID },
        function (data) {
            $("#values-edit").children().remove();
            $("#values-edit").append(data);
        });
    }

    function SendRequest(Val, PropID, ProjID) {
        $.post(
        "/EditProperty/GetViewNew",
        { Values: Val, PropertyID: PropID, ProjectID: ProjID },
        function (data) {
            $("#testhere").children().remove();
            $("#testhere").hide();
            $("#testhere").append(data);
            if ($("#testhere").find("div").attr("name") == "true") {
                $("#" + PropertyID).children().remove()
                $("#" + PropertyID).append(data);
                $("#dialog-edit").dialog("close");
            }
            else {
                $("#testhere").children().remove();
                $("#values-edit").children().remove();
                $("#values-edit").append(data);
            }
        });
    }

    function DoValues() {
        switch (PropertyType) {
            case "Bool": { break; }
            case "Date": { break; }
            case "Number": { break; }
            case "NumberArray": { break; }
            case "String":
                {
                    PropertyValues = $("#values-edit :text").val();
                    console.log(PropertyValues);
                    break;
                }
            case "StringArray": { break; }
        }
    }
});