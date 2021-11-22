'use strict';
function paramRefresh(ParameterId) {
    if (ParameterId) {
        kendo.ui.progress($('.myclassselector'), true);
        $.ajax({
            url: "/parameter/byid",
            dataType: "json",
            data: { id: ParameterId },
            type: "GET",
            traditional: true,
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (result) {
                if (result && result.success && result.success === true) {
                    //Criterion
                    var cName = $("#criterionName");
                    if (cName && result.data.Criterion.FullName) {
                        cName.val(result.data.Criterion.FullName);
                    }
                    //Par
                    var dir = $("#paramName");
                    if (dir && result.data.FullName) {
                        dir.val(result.data.FullName);
                    }
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
    if (window.ParameterId) {
        paramRefresh(window.ParameterId);
        var tabstripBlockP = $("#divtabIndicators");
        if (tabstripBlockP) {
            window.tabstrip = $("#tabIndic").kendoTabStrip(
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