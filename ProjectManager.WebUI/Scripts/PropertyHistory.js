$(function () {
    var PropertyID;
    var ProjectID;
    //"add new property" dialog constructor
    $('#dialog-history').dialog({
        autoOpen: false,
        width: 400,
        modal: true,
        close: function () {
            $("#historyvalues").children().remove();
        }
    });

    $("#test1").live('click', function () {
        $("#historyvalues").children().remove();
        PropertyID = $(this).parent().parent().parent().find("div").attr("id");
        ProjectID = $("#ProjectId").val();
        $('#dialog-history').dialog('open');
        LoadHistory(PropertyID, ProjectID);
    });

    function LoadHistory(PropID, ProjID) {
        $.post(
        "/EditProperty/GetPropertyHistory",
        { Smth: PropID, PropertyID: PropID, ProjectID: ProjID },
        function (data) {
            $("#historyvalues").append(data);
        });
    }
});