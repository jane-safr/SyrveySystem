'use strict';
function testRefresh(codeId) {
    if (codeId) {
        kendo.ui.progress($('.myclassselector'), true);
        var headerTemplate = kendo.template($("#headerStartSurvey").html());
        $.ajax({
            url: "/survey/bycode",
            dataType: "json",
            data: { codeId: codeId },
            type: "GET",
            traditional: true,
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (result) {
                if (result && result.success && result.success === true) {
                    var res = headerTemplate(result.data);
                    $("#contentStartUser").html(res);
                }
                kendo.ui.progress($('.myclassselector'), false);
            },
            error: function (response) {
                kendo.ui.progress($('.myclassselector'), false);
                window.popupNotification.show("Ошибка в запросе / Error get data", "error");
            }
        });
    } else {
        window.popupNotification.show("Не указан индентификатор / Select ID", "error");
    }
}

$(document).ready(function () {
    if (window.SurveyCode) {
        testRefresh(window.SurveyCode);
        let tabstripBlockP = $("#divtabSurveys");
        if (tabstripBlockP) {
            window.tabstrip = $("#tabSurvey").kendoTabStrip(
                {
                    animation: {
                        open: {
                            effects: "fadeIn"
                        }
                    },
                    tabPosition: "top"
                }
            ).data("kendoTabStrip");
            window.tabstrip.select(window.TabPId ? window.TabPId : 0);
        } else {
            tabstripBlockP.css({ 'display': "none" });
        }
    }
});