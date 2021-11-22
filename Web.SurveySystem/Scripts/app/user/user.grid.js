"use strict";
function closeWinUploadFile() {
    try {
        var kendoWindow = $("#win").data("kendoWindow").close();
    }
    catch (err) { window.popupNotification.show(err.message, "error"); }
}

$(document).ready(function () {
    function createColumns() {
        var cols = [
            {
                field: "Name",
                title: "Пользователь / User",
                width: 150,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: {
                    cell: {
                        operator: "contains",
                        showOperators: true
                    }
                }
            },
            {
                field: "Email",
                title: "Эл.почта / Email",
                width: 100,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                }
            }
        ];
        return cols;
    }

    function onDataBound() {
        $(".btn-success").removeClass("k-button");
        $(".btn-danger").removeClass("k-button");
        $(".btn-primary").removeClass("k-button");
        $(".btn-warning").removeClass("k-button");
        $(".btn-info").removeClass("k-button");
    }

    $("#gridUsers").kendoGrid({
        autoBind: true,
        batch: false,
        sortable: true,
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
            serverSorting: true,
            serverFiltering: false,
            transport:
            {
                read: function (options) {
                    kendo.ui.progress($('.myclassselector'), true);
                    $.ajax({
                        url: "/users/all",
                        dataType: "json",
                        type: "GET",
                        traditional: true,
                        cache: false,
                        contentType: "application/json; charset=utf-8",
                        success: function (result) {
                            kendo.ui.progress($('.myclassselector'), false);
                            if (result && result.success) {
                                options.success(result);
                            }
                            else {
                                window.popupNotification.show("Ошибка при получении данных Users/ Error Rows", "error");
                            }
                        },
                        error: function (response) {
                            window.popupNotification.show("Ошибка при получении данных Users / Error Rows", "error");
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
                    $("#gridUsers").data("kendoGrid").dataSource.read();
                    $("#gridUsers").data("kendoGrid").refresh();
                }
            },
            batch: true,
            schema: {
                total: "data.length",
                data: "data",
                model: {
                    id: "Id",
                    fields:
                    {
                        Id: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 1, max: 120 }
                        },
                        Name: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 2, max: 4000 }
                        },
                        Email: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 2, max: 4000 }
                        }
                    }
                }
            }
        },
        edit: function (e) {
            $(".k-grid-update").removeClass("k-button k-primary").addClass("btn btn-success btn-sm marginRigh");
            $(".k-grid-cancel").removeClass("k-button").addClass("btn btn-danger btn-sm marginRigh");
        },
        dataBound: onDataBound,
        selectable: "row",
        change: function (e) {
            if (e) {
                window.selectUserId = this.dataItem(this.select()).Id;
                if (window.selectUserId) {
                    window.location.href = "/users/edit/" + selectUserId;
                }
            }
        },
        scrollable: true,
        filterable: { mode: "row" },
        columns: createColumns(),
        editable: "inline"
    });
    $("#gridUsers").find(".k-dropdown-wrap:last").width(40);
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.78;
    $("#gridUsers").height(windowHeight);
    $('input[data-text-field="Name"').prop('placeholder', "Поиск / Search");
    $('input[data-text-field="Email"').prop('placeholder', "Поиск / Search");
});