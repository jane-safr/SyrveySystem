'use strict';
$(document).ready(function () {
    function createColumns() {
        var cols = [
            {
                field: "Name",
                title: "Наименование<br/>Name",
                width: 80,
                editor: textareaEditorRusAndEng,
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
                field: "Value",
                title: "Значение<br/>Value",
                width: 100,
                editor: textareaEditorRusAndEng,
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
                field: "Description",
                title: "Описание<br/>Description",
                width: 120,
                editor: textareaEditorRusAndEng,
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
        ];
        var commRep = [];
        if (window.permissions.Edit) {
            commRep.push({
                name: "edit",
                text: " ",
                className: "btn btn-warning btn-sm mr-1",
                iconClass: "fa fa-wrench"
            });
        }
        cols.push({
            command: commRep,
            width: 40,
            title: "Действия<br/>Actions",
            attributes: { style: 'vertical-align: middle; text-align:center;' },
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            }
        });
        return cols;
    }

    function onDataBoundSettings() {
        $(".btn-success").removeClass("k-button");
        $(".btn-danger").removeClass("k-button");
        $(".btn-primary").removeClass("k-button");
        $(".btn-warning").removeClass("k-button");
        $(".btn-info").removeClass("k-button");
    }

    $("#gridSettings").kendoGrid({
        autoBind: true,
        batch: false,
        sortable: true,
        mobile: true,
        pageable: {
            refresh: true,
            pageSizes: true,
            buttonCount: 50,
            pageSize: 50
        },
        dataSource: {
            serverPaging: false,
            serverSorting: true,
            serverFiltering: false,
            transport:
            {
                read: function (options) {
                    kendo.ui.progress($('.myclassselector'), true);
                    $.ajax({
                        url: "/settings/all",
                        dataType: "json",
                        type: "POST",
                        traditional: true,
                        cache: false,
                        contentType: "application/json; charset=utf-8",
                        success: function (result) {
                            kendo.ui.progress($('.myclassselector'), false);
                            if (result && result.success) {
                                options.success(result);
                            }
                            else {
                                window.popupNotification.show("Ошибка при получении данных / Error getting data", "error");
                            }
                        },
                        error: function (response) {
                            alert(response);
                        }
                    });
                },
                update: function (e) {
                    if (e && e.data.models) {
                        kendo.ui.progress($('.myclassselector'), true);
                        var updateDate = e.data.models[0];
                        if (updateDate) {
                            $.ajax({
                                url: "/settings/update",
                                dataType: "json",
                                type: "POST",
                                traditional: true,
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify(updateDate),
                                success: function (response) {
                                    kendo.ui.progress($('.myclassselector'), false);
                                    if (response && response.success) {
                                        e.success();
                                        window.popupNotification.show(response.message, "success");
                                        $("#gridSettings").data("kendoGrid").dataSource.read();
                                    }
                                    else {
                                        window.popupNotification.show(response.message, "error");
                                    }
                                },
                                error: function (response) {
                                    kendo.ui.progress($('.myclassselector'), false);
                                    window.popupNotification.show("Ошибка при получении данных / Error getting data", "error");
                                }
                            });
                        }
                        else {
                            kendo.ui.progress($('.myclassselector'), false);
                            window.popupNotification.show("Error: Укажите данные для обновления", "error");
                        }
                    }
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
            batch: true,
            schema: {
                total: "data.length",
                data: "data",
                model: {
                    id: "SettingId",
                    fields:
                    {
                        SettingId: {
                            type: "string",
                            editable: false,
                            defaultValue: ""

                        },
                        Name: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 2, max: 4000 }
                        },
                        Value: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 2, max: 4000 }
                        },
                        Description: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 2, max: 4000 }
                        }
                    }
                }
            }
        },
        dataBound: onDataBoundSettings,
        selectable: "row",
        scrollable: true,
        filterable: { mode: "row" },
        columns: createColumns(),
        editable: {
            mode: "popup",
            window: {
                animation: false,
                width: "50%",
                maxHeight: "80%",
                open: function () {
                    this.center();
                }
            }
        },
        edit: function (e) {
            e.preventDefault();
            $(".k-grid-update").removeClass("k-button k-primary").addClass("btn btn-success btn-sm mr-1");
            $(".k-grid-cancel").removeClass("k-button").addClass("btn btn-danger btn-sm mr-1");
            $(".k-window .k-grid-cancel").html("<span class=\"fa fa-times-circle\"></span> Отменить / Cancel");
            //up
            if (!e.model.isNew()) {
                $(".k-window-title").text("✔️ Обновить / Update");
                $(".k-window .k-grid-update").html("<span class=\"fa fa-check-circle\"></span> Обновить / Update");
            }
            //add
            else {
                $(".k-window-title").text("✔️ Добавить / Add ");
                $(".k-window .k-grid-update").html("<span class=\"fa fa-plus\"></span> Добавить / Add");
            }
        },
        cancel: function (e) {
            e.preventDefault();
            $("#gridSettings").data("kendoGrid").dataSource.read();
            $("#gridSettings").data("kendoGrid").refresh();
        }
    });
    //add placeholder
    $('input[data-text-field="SettingId"').prop('placeholder', "Search");
    $('input[data-text-field="Description"').prop('placeholder', "Поиск / Search");
    $('input[data-text-field="Name"').prop('placeholder', "Поиск / Search");
    $('input[data-text-field="Value"').prop('placeholder', "Поиск / Search");

    $("#gridSettings").kendoTooltip({
        filter: ".k-grid-edit",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "🔨 Изменить / Edit";
        }
    });
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.65;
    $("#gridSettings .k-grid-content").height(windowHeight);
});