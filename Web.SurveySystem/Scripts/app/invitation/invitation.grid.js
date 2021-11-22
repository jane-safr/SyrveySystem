'use strict';
function closeWinModal() {
    try {
        var kendoWindow = $("#winModal").data("kendoWindow").close();
    }
    catch (err) { window.popupNotification.show(err.message, "error"); }
}

function startTest(e) { 
    if (e) {
        debugger;
        var dataItemApp = this.dataItem($(e.currentTarget).closest("tr"));
        if (dataItemApp && dataItemApp.InvitationCode) {
            window.popupNotification.show("Загрузка ... / Loading ...", "success");
            window.open("/invitation/start/" + dataItemApp.InvitationCode + "", "_blank");
        } else {
            kendo.ui.progress($('.myclassselector'), false);
            window.popupNotification.show("Не указан индентификатор / select ID", "error");
        }
    }
    else {
        window.popupNotification.show("Неверная ссылка на строку / Error Rows", "error");
    }
}

function resultTest(e) {
    if (e) {
        var dataItemApp = this.dataItem($(e.currentTarget).closest("tr"));
        if (dataItemApp && dataItemApp.InvitationCode) {
            window.popupNotification.show("Загрузка ... / Loading ...", "success");
            window.open("/useranswer/result/" + dataItemApp.InvitationCode + "", "_blank");
        } else {
            kendo.ui.progress($('.myclassselector'), false);
            window.popupNotification.show("Не указан индентификатор / select ID", "error");
        }
    }
    else {
        window.popupNotification.show("Неверная ссылка на строку / Error Rows", "error");
    }
}

$(document).ready(function () {
    function createColumns() {
        var colsFull = [];
        colsFull.push({
            field: "IsFinished",
            hidden: true,
            groupHeaderTemplate: "#=value ? 'Пройдено' : 'Не пройдено' # : #=count #",
            width: 0,
            attributes: { style: 'vertical-align: middle; text-align:center;' },
            headerAttributes: { style: 'vertical-align: top;text-align: center;' },
            filterable: false
        });

        colsFull.push({
            field: "DateEnd",
            title: "Пройти до<br/>Deadline",
            width: 20,
            attributes: { style: 'vertical-align: middle; text-align:center;' },
            format: "{0: dd-MM-yyyy HH:mm}",
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            },
            filterable: false
        });
        if (window.permissions.Edit) {
            colsFull.push({
                field: "UserName",
                title: "Сотрудник<br/>Employee",
                width: 30,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false
            });
            colsFull.push({
                field: "UserEmail",
                title: "Эл.почта<br/>Email",
                width: 30,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false
            });
        }
        colsFull.push({
                field: "SurveyCode",
                title: "Код<br/>Survey",
                width: 20,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false
        });
        colsFull.push({
            field: "FullName",
            title: "Наименование<br/>Name",
            width: 30,
            attributes: { style: 'vertical-align: middle; text-align:center;' },
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            },
            filterable: false
        });
        colsFull.push({
            field: "DateStart",
            title: "Дата начала<br/>Date Start",
            width: 20,
            format: "{0: dd-MM-yyyy HH:mm}",
            attributes: { style: 'vertical-align: middle; text-align:center;' },
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            },
            filterable: false
        });
        colsFull.push({
            field: "ActualCompleteDate",
            title: "Дата окончания<br/>Date end",
            width: 20,
            format: "{0: dd-MM-yyyy HH:mm}",
            attributes: { style: 'vertical-align: middle; text-align:center;' },
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            },
            filterable: false
        });
        colsFull.push({
            field: "Percent",
            template: "#=kendo.format('{0:p0}', Percent/100)#",
            title: "Процент прохождения<br/>Percent",
            width: 10,
            attributes: { style: 'vertical-align: middle; text-align:center;' },
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            },
            filterable: false
        });
        var comm = [];
        comm.push({
            name: "answer",
            text: " ",
            title: "Пройти тест / Answer",
            click: startTest,
            className: "btn btn-success btn-sm mr-1",
            iconClass: "fa fa-arrow-circle-right"
        });
        comm.push({
            name: "result",
            text: " ",
            title: "Результаты / Results",
            click: resultTest,
            className: "btn btn-info btn-sm mr-1",
            iconClass: "fa fa-search"
        });
        colsFull.push({
            command: comm,
            width: 15,
            attributes: { style: 'vertical-align: middle; text-align:center;' },
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            },
            title: "Действия<br/>Actions"
        });
        return colsFull;
    }

    function onDataBoundSurvey(e) {
        e.sender.element.find(".k-group-col,.k-group-cell").css('width', 2);
        $(".btn-success").removeClass("k-button");
        $(".btn-danger").removeClass("k-button");
        $(".btn-primary").removeClass("k-button");
        $(".btn-warning").removeClass("k-button");
        $(".btn-info").removeClass("k-button");
        $('.k-group-cell').css('width', '1px');
        var items = this._data;
        var tableRows = $(this.table).find("tr").not(".k-grouping-row");
        tableRows.each(function(index) {
            var row = $(this);
            var item = items[index];
           
            if (item && !item.IsFinished && (item.DateEnd < new Date().setHours(0, 0, 0, 0))) {
                row.addClass("table-danger");
                $(this).find('.k-grid-answer').hide();
                $(this).find('.k-grid-result').hide();
            }
            else if (!item.IsFinished && item.IsAccepted) {
                row.addClass("row_warning");
                $(this).find('.k-grid-result').hide();
            }
            else if (!item.IsFinished) {
                $(this).find('.k-grid-answer').show();
                $(this).find('.k-grid-result').hide();
            }
            else {
               // row.addClass("row_success");
                $(this).find('.k-grid-answer').hide();
                $(this).find('.k-grid-result').show();
            }
        });
    }
    $("#gridInvitations").kendoGrid({
       //toolbar: kendo.template($("#templateToolBar").html()),
        autoBind: true,
        batch: false,
        mobile: true,
        pageable: {
            refresh: true,
            pageSizes: true,
            buttonCount: 200,
            pageSize: 200
        },
        dataSource:
        {
            serverPaging: false,
            serverSorting: false,
            serverFiltering: false,
            transport:
            {
                read: function (options) {
                    kendo.ui.progress($('.myclassselector'), true);
                    var modelsFilter = [];
                    if (options.data.searchtxt)
                        modelsFilter.push({ Field: "searchtxt", Value: options.data.searchtxt });
                    $.ajax({
                        url: "/invitation/invitefilter",
                        dataType: "json",
                        type: "POST",
                        data: JSON.stringify({ filterModels: modelsFilter }),
                        traditional: true,
                        cache: false,
                        contentType: "application/json; charset=utf-8",
                        success: function (result) {
                            kendo.ui.progress($('.myclassselector'), false);
                            if (result && result.success && result.success === true) {
                                options.success(result);
                            }
                            else {
                                window.popupNotification.show(result.message, "error");
                            }
                        },
                        error: function (response) {
                            window.popupNotification.show("error get", "error");
                        }
                    });
                },
                parameterMap: function (data, type) {
                    if (type === "read") {
                        if (data.filter && data.filter.filters && data.filter.filters.length) {
                            for (let i = 0; i < data.filter.filters.length; ++i) {
                                var filter = data.filter.filters[i];
                                if (filter.field === "CreatedDate") {
                                    data.CreatedDate = kendo.toString(filter.value, "dd-MM-yyyy");
                                    data.filter.filters.splice(i, 1);
                                    break;
                                }
                            }
                        }
                        return data;
                    }
                    else if (type === "create") {
                        return kendo.stringify(data);
                    }
                }
            },
            requestEnd: function (e) {
                if (e.type === "create" || e.type === "destroy" || e.type === "update") {
                    // Update the Yardage list
                    $("#gridInvitations").data("kendoGrid").dataSource.read();
                    $("#gridInvitations").data("kendoGrid").refresh();
                }
            },
            batch: true,
            group: {
                field: "IsFinished",
                //dir: "desc",
                aggregates: [
                    {
                        field: "IsFinished",
                        aggregate: "count"
                    }
                ]
            },
            schema: {
                total: "data.length",
                data: "data",
                model: {
                    id: "InvitationId",
                    fields:
                    {
                        InvitationId: {
                            type: "string",
                            editable: false,
                            validation: { required: false, min: 1, max: 120 }
                        },
                        CreatedBy: {
                            type: "string",
                            editable: false
                        },
                        CreatedOn: {
                            type: "date",
                            editable: false
                        },
                        UserName: {
                            type: "string",
                            editable: false
                        },
                        UserEmail: {
                            type: "string",
                            editable: false
                        },
                        UserId: {
                            type: "string",
                            editable: false
                        },
                        DateEnd: {
                            type: "date",
                            editable: false
                        },
                        DateStart: {
                            type: "date",
                            editable: false
                        },
                        Percent: {
                            type: "number",
                            editable: false
                        },
                        ActualCompleteDate: {
                            type: "date",
                            editable: false
                        },
                        SurveyId: {
                            type: "string",
                            editable: false,
                            validation: { required: false, min: 1, max: 120 }
                        },
                        Survey:
                        {
                            SurveyCode: {
                                type: "number",
                                editable: false
                            },
                            FullName: {
                                type: "string",
                                editable: false
                            },
                            IsActive: {
                                type: "boolean",
                                editable: false
                            }
                        },
                        IsActive: {
                            type: "boolean",
                            editable: false,
                            defaultValue: true
                        },
                        IsAccepted : {
                            type: "boolean",
                            editable: false,
                            defaultValue: true
                        },
                        IsFinished : {
                            type: "boolean",
                            editable: false,
                            defaultValue: true
                        }
                    }
                }
            }
        },
        edit: function (e) {
            $(".k-grid-update").removeClass("k-button k-primary").addClass("btn btn-success btn-sm marginRigh");
            $(".k-grid-cancel").removeClass("k-button").addClass("btn btn-danger btn-sm marginRigh");
        },
        dataBound: onDataBoundSurvey,
        selectable: true,
        scrollable: true,
        filterable: false,
        columns: createColumns(),
        editable: false
    });

    var timeout = null;
    $("#txtSearchSurvey").keyup(function () {
        var value = $(this).val();
        var grid = $("#gridInvitations").data("kendoGrid");
        if (grid) {
            if (value && value.length > 1) {
                // Clear the timeout if it has already been set.
                clearTimeout(timeout);
                // Make a new timeout set to go off in 800ms
                timeout = setTimeout(function () {
                    grid.dataSource.read({ searchtxt: value });
                    grid.refresh();
                }, 300);
                $("#txtSearchClearSurvey").show();
            }
            else if (!value) {
                grid.dataSource.read();
                grid.refresh();
            }
        }
    });

    $("#txtSearchClearSurvey").click(function (e) {
        e.preventDefault();
        $("#txtSearchSurvey").val(null);
        var grid = $("#gridInvitations").data("kendoGrid");
        if (grid) {
            grid.dataSource.read();
            grid.refresh();
        }
        $("#txtSearchClearSurvey").hide();
    });

    $("#gridInvitations").kendoTooltip({
        filter: ".k-grid-answer",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "📝Пройти тест / Answer test";
        }
    });
    $("#gridInvitations").kendoTooltip({
        filter: ".k-grid-result",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "☑ Просмотреть результаты / Get results";
        }
    });
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.74;
    $("#gridInvitations").height(windowHeight);
});