'use strict';
$(document).ready(function () {
    function createColsNotifications() {
        var cols = [
            {
                field: "DateSend",
                title: "Дата отправки<br/>Date Send",
                format: "{0: dd-MM-yyyy HH:mm}",
                width: 20,
                headerAttributes: { style: 'vertical-align: top;text-align: center;' },
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                filterable: false
            },
            {
                field: "NotificationTypeId",
                title: "Тип<br/>Type",
                template: '<span>#=kendo.toString(NotificationType.NameRus, "")# / #=kendo.toString(NotificationType.NameEng, "")#</span>',
                width: 30,
                editable: false,
                sortable: false,
                headerAttributes: { style: 'vertical-align: top;text-align: center;' },
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                filterable: false
            },
            {
                field: "EmailTo",
                title: "Получатель <br/> Recipient",
                width: 25,
                headerAttributes: { style: 'vertical-align: top;text-align: center;' },
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                filterable: false
            },
            {
                field: "EmailText",
                title: "Текст письма <br/> Email Text",
                width: 120,
                headerAttributes: { style: 'vertical-align: top;text-align: center;' },
                filterable: false
            },
            {
                field: "IsSend",
                title: "Отправлено<br/>Is Sent",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsSend ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 20,
                headerAttributes: { style: 'vertical-align:top;text-align: center;' },
                filterable: false
            }
        ];
        return cols;
    }

    function onDataBoundNotifications() {
        $(".btn-success").removeClass("k-button");
        $(".btn-danger").removeClass("k-button");
        $(".btn-primary").removeClass("k-button");
        $(".btn-warning").removeClass("k-button");
        $(".btn-info").removeClass("k-button");
        var items = this._data;
        var tableRows = $(this.table).find("tr");
        tableRows.each(function (index) {
            var row = $(this);
            var item = items[index];
            if (item.IsSend) {
                row.addClass("table-success");
            } else {
                row.addClass("table-danger");
            }
        });
    }

    var timeout = null;
    $("#txtSearchEmail").keyup(function () {
        var value = $(this).val();
        var grid = $("#gridNotifications").data("kendoGrid");
        if (grid) {
            if (value && value.length > 1) {
                // Clear the timeout if it has already been set.
                clearTimeout(timeout);
                // Make a new timeout set to go off in 800ms
                timeout = setTimeout(function () {
                    grid.dataSource.read({ searchtxt: value });
                    grid.refresh();
                }, 300);
                $("#clearSearchEmail").show();
            }
            else if (!value) {
                grid.dataSource.read();
                grid.refresh();
            }
        }
    });

    $("#clearSearchEmail").click(function (e) {
        e.preventDefault();
        $("#txtSearchEmail").val("");
        var grid = $("#gridNotifications").data("kendoGrid");
        if (grid) {
            grid.dataSource.read();
            grid.refresh();
        }
        $("#clearSearchEmail").hide();
    });

    $("#gridNotifications").kendoGrid({
        autoBind: true,
        batch: false,
        sortable: false,
        mobile: true,
        pageable: {
            refresh: true,
            pageSizes: true,
            buttonCount: 20,
            pageSize: 20
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
                    $.ajax({
                        url: "/notification/all",
                        dataType: "json",
                        type: "GET",
                        traditional: true,
                        data: { searchtxt: options.data.searchtxt ? options.data.searchtxt : "" },
                        cache: false,
                        contentType: "application/json; charset=utf-8",
                        success: function (result) {
                            kendo.ui.progress($('.myclassselector'), false);
                            if (result && result.success) {
                                options.success(result);
                            } else {
                                window.popupNotification.show("Ошибка при получении уведомлений / Error receiving notifications", "error");
                            }
                        },
                        error: function (response) {
                            window.popupNotification.show("Ошибка при получении уведомлений / Error receiving notifications", "error");
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
                }
            },
            batch: true,
            schema: {
                total: "data.length",
                data: "data",
                model: {
                    id: "NotificationId",
                    fields:
                    {
                        NotificationId: {
                            type: "string",
                            editable: false
                        },
                        EmailTo: {
                            type: "string",
                            editable: false
                        },
                        EmailText: {
                            type: "string",
                            editable: false
                        },
                        DateSend: {
                            type: "Date",
                            editable: false
                        },
                        IsSend: {
                            type: "boolean",
                            editable: false
                        },
                        CreatedOn: {
                            type: "date",
                            editable: false
                        },
                        NotificationTypeId: {
                            type: "string",
                            editable: false
                        },
                        NotificationType:
                        {
                            NameRus: {
                                type: "string",
                                editable: false
                            },
                            NameEng: {
                                type: "string",
                                editable: false
                            },
                            IsActive: {
                                type: "boolean",
                                editable: false
                            }
                        }
                    }
                }
            }
        },
        cancel: function (e) {
            e.preventDefault();
            $("#gridNotifications").data("kendoGrid").dataSource.read();
            $("#gridNotifications").data("kendoGrid").refresh();
        },
        dataBound: onDataBoundNotifications,
        scrollable: true,
        filterable: false,
        columns: createColsNotifications()
    });
    $("#gridNotifications").find(".k-dropdown-wrap:last").width(40);
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.72;
    $("#gridNotifications").height(windowHeight);
});