'use strict';
$(document).ready(function() {
    if (window.InvitationCode) {
        testRefresh(window.InvitationCode);
    }
});
function testRefresh(codeId) {
    if (codeId) {
        var headerTemplate = kendo.template($("#headerStartSurvey").html());
        kendo.ui.progress($('.myclassselector'), true);
        $.ajax({
            url: "/invitation/bycode",
            dataType: "json",
            data: { codeId: codeId },
            type: "GET",
            traditional: true,
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function(result) {
                if (result && result.success && result.success === true) {
                 var res = headerTemplate(result.data);
                    $("#contentStartUser").html(res);
                }
                kendo.ui.progress($('.myclassselector'), false);
            },
            error: function(response) {
                kendo.ui.progress($('.myclassselector'), false);
                window.popupNotification.show("Ошибка в запросе / Error get data", "error");
            }
        });
    } else {
        window.popupNotification.show("Не указан индентификатор теста / Invalid test ID", "error");
    }
}