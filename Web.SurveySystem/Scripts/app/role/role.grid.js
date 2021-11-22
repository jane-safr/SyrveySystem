"use strict";
$(document).ready(function () {
    function createColumns() {
        var cols = [
            {
                field: "Name",
                title: "Наименование <br/>Name",
                width: 30,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false
            },
            {
                field: "Description",
                title: "Описание <br/>Description",
                width: 150,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false
            },
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

    $("#gridRoles").kendoGrid({
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
                        url: "/roles/all",
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
                                window.popupNotification.show("Ошибка при получении данных / Error get Roles", "error");
                            }
                        },
                        error: function (response) {
                            window.popupNotification.show("Ошибка при получении данных / Error get Roles", "error");
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
                if (e.type === "create") {
                    // Update the Yardage list
                    $("#gridRoles").data("kendoGrid").dataSource.read();
                    $("#gridRoles").data("kendoGrid").refresh();
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
                            editable: false,
                            validation: { required: true, min: 2, max: 1000 }
                        },
                        Description: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 2, max: 3500 }
                        }
                    }
                }
            }
        },
        edit: false,
        dataBound: onDataBound,
        selectable: "row",
        scrollable: true,
        filterable: { mode: "row" },
        columns: createColumns(),
        editable: "inline"
    });
    $("#gridRoles").find(".k-dropdown-wrap:last").width(35);
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.76;
    $("#gridRoles").height(windowHeight);
});