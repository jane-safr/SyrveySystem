'use strict';
$(document).ready(function () {
    function createColumns() {
        var cols = [
            {
                field: "Order",
                title: "Порядок<br/>Sorting",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '#= kendo.toString(Order, "n0")#',
                format: "{0:n0}",
                width: 10,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false,
                hidden: true
            },
            {
                field: "FullName",
                title: "Индикатор<br/>Indicator",
                editor: textareaEditorRusAndEng,
                width: 100,
                attributes: { style: 'vertseleical-align: middle; text-align:left;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false,
                editable: false
            },
            {
                field: "Name",
                title: "Индикатор<br/>Indicator",
                editor: textareaEditorRusAndEng,
                width: 100,
                attributes: { style: 'vertical-align: middle; text-align:left;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false,
                hidden: true
            },
            {
                field: "IsActive",
                title: "Активный<br/>Active",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsActive ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 15,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
            {
                field: "CreatedOn",
                title: "Создан<br/>Created on",
                width: 25,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                format: "{0: dd-MM-yyyy HH:mm}",
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false,
                exportable: { excel: false }
            },
            {
                field: "CreatedBy",
                title: "Создал<br/>Created by",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                width: 20,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false,
                exportable: { excel: false }
            }
        ];
        var comm = [];
        comm.push({
            name: "edit",
            text: " ",
            className: "btn btn-warning btn-sm mr-1",
            iconClass: "fa fa-wrench"
        });
        if (window.permissions.Edit) {
            comm.push({
                name: "destroy",
                text: " ",
                className: "btn btn-danger btn-sm mr-1",
                title: "Удалить / Delete",
                iconClass: "fa fa-times"
            });
        }
        cols.push({
            command: comm,
            width: 20,
            attributes: { style: 'vertical-align: middle; text-align:center;' },
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            },
            title: "Действия<br/>Actions"
        });

        return cols;
    }
    
    function onDataBoundInd(e) {
        e.sender.element.find(".k-group-col,.k-group-cell").css('width', 2);
        $(".btn-success").removeClass("k-button");
        $(".btn-danger").removeClass("k-button");
        $(".btn-primary").removeClass("k-button");
        $(".btn-warning").removeClass("k-button");
        $(".btn-info").removeClass("k-button");
        $('.k-group-cell').css('width', '1px');
        var items = this._data;
        var tableRows = $(this.table).find("tr").not(".k-grouping-row");
        tableRows.each(function (index) {
            var row = $(this);
            var item = items[index];
            if (!item.IsActive) {
                row.addClass("noActive");
                }
        });
    }
    function createToolbar() {
        var toolbar = [];
        if (window.permissions.Add) {
            toolbar.push({
                name: "create",
                text: " Добавить / Add",
                className: "btn btn-success btn-sm mr-1 btn-rounded",
                iconClass: "fa fa-plus"
            });
            toolbar.push({
                name: "excel",
                text: " Экспорт в Excel / Export to Excel",
                className: "btn btn-info btn-sm mr-1 "
            });
        }
        return toolbar;
    }

    $("#gridIndicators").kendoGrid({
        toolbar: createToolbar(),
        excel: {
            allPages: true,
            filename: "parameters.xslx",
            filterable: true
        },
        autoBind: true,
        batch: false,
        sortable: true,
        mobile: true,
        pageable: {
            refresh: true,
            pageSizes: true,
            buttonCount: 100,
            pageSize: 100
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
                    if (options.data.searchtxt) {
                        modelsFilter.push({ Field: "searchtxt", Value: options.data.searchtxt });
                    }
                    if (window.ParameterId) {
                        $.ajax({
                            url: "/indicator/byparameter",
                            dataType: "json",
                            type: "POST",
                            data: JSON.stringify({ parameterId: window.ParameterId }),
                            traditional: true,
                            cache: false,
                            contentType: "application/json; charset=utf-8",
                            success: function (result) {
                                kendo.ui.progress($('.myclassselector'), false);
                                if (result && result.success && result.success === true) {
                                    $("#countIndicators").text(result.data.length);
                                   /* if (result.data.length < 1) {
                                        window.popupNotification.show("Нет д / No employees available in charter", "success");
                                    } */
                                    options.success(result);
                                } else {
                                    window.popupNotification.show(result.message, "error");
                                }
                            },
                            error: function (response) {
                                window.popupNotification.show("Ошибка получения списка сотрудников / Error get employees", "error");
                            }
                        });
                    }
                },
                create: function (e) {
                    if (e && e.data) {
                        var createDate = e.data;
                        createDate.ParameterId = window.ParameterId;
                        if (createDate) {
                            kendo.ui.progress($('.myclassselector'), true);
                            $.ajax({
                                url: "/indicator/add",
                                dataType: "json",
                                type: "POST",
                                traditional: true,
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify(createDate),
                                success: function (response) {
                                    if (response && response.success) {
                                        e.success();
                                        window.popupNotification.show(response.message, "success");
                                    }
                                    else {
                                        kendo.ui.progress($('.myclassselector'), false);
                                        window.popupNotification.show(response.message, "error");
                                        e.error();
                                    }
                                },
                                error: function (response) {
                                    kendo.ui.progress($('.myclassselector'), false);
                                    window.popupNotification.show(response.responseText, "error");
                                    e.error();
                                }
                            });
                        }
                        else {
                            window.popupNotification.show("Укажите данные для создания / No data to create", "error");
                        }
                    }
                },
                update: function (e) {
                    if (e && e.data) {
                        var updateDate = e.data;
                        if (updateDate) {
                            kendo.ui.progress($('.myclassselector'), true);
                            $.ajax({
                                url: "/indicator/edit",
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
                                    }
                                    else {
                                        window.popupNotification.show(response.message, "error");
                                        e.error();
                                    }
                                },
                                error: function (response) {
                                    kendo.ui.progress($('.myclassselector'), false);
                                    window.popupNotification.show(response.responseText, "error");
                                    e.error();
                                }
                            });
                        }
                        else {
                            kendo.ui.progress($('.myclassselector'), false);
                            window.popupNotification.show("Укажите данные для обновления / No data to edit", "error");
                        }
                    }
                },
                destroy: function (e) {
                    if (e && e.data) {
                        kendo.ui.progress($('.myclassselector'), true);
                        var deleteDate = e.data;
                        if (deleteDate && deleteDate.IndicatorId) {
                            $.ajax({
                                url: "/indicator/remove",
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify({ id: deleteDate.IndicatorId }),
                                success: function (response) {
                                    kendo.ui.progress($('.myclassselector'), false);
                                    if (response && response.success) {
                                        e.success();
                                        window.popupNotification.show(response.message, "success");
                                    }
                                    else {
                                        window.popupNotification.show(response.message, "error");
                                        e.error();
                                    }
                                },
                                error: function (response) {
                                    kendo.ui.progress($('.myclassselector'), false);
                                    window.popupNotification.show(response.responseText, "error");
                                    e.error();
                                    $("#gridIndicators").data("kendoGrid").dataSource.read();
                                    $("#gridIndicators").data("kendoGrid").refresh();
                                }
                            });
                        }
                        else {
                            kendo.ui.progress($('.myclassselector'), false);
                            window.popupNotification.show("Данные не удалены / Delete not completed", "error");
                        }
                    }
                    else {
                        window.popupNotification.show("Ошибка при удалении / Delete error", "error");
                        e.error();
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
                    } else if (type === "create") {
                        return kendo.stringify(data);
                    }
                }
            },
            requestEnd: function (e) {
                if (e.type === "create" || e.type === "destroy" || e.type === "update") {
                    // Update the Yardage list
                    $("#gridIndicators").data("kendoGrid").dataSource.read();
                    $("#gridIndicators").data("kendoGrid").refresh();
                }
            },
            batch: false,
            schema: {
                total: "data.length",
                data: "data",
                model: {
                    id: "IndicatorId",
                    fields:
                    {
                        IndicatorId: {
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
                        IsActive: {
                            type: "boolean",
                            editable: true,
                            defaultValue: true
                        },
                        Name: {
                            type: "string",
                            editable: true,
                            validation: { required: true}
                        },
                        ParameterId: {
                            type: "string",
                            editable: false
                        },
                        Order: {
                            type: "number",
                            editable: true,
                            validation: { required: true, min: 0 }
                        }
                    }
                }
            }
        },
        edit: function (e) {
            $(".k-grid-update").removeClass("k-button k-primary").addClass("btn btn-success btn-sm mr-1");
            $(".k-grid-cancel").removeClass("k-button").addClass("btn btn-danger btn-sm mr-1");
            window.popupNotification.show("✔️ Загрузка.../ Loading...", "success");
            $(".k-window .k-grid-cancel").html("<span class=\"fa fa-times-circle\"></span> Отменить / Cancel");
            e.container.find("label[for=ParameterId]").parent("div .k-edit-label").hide();
            e.container.find("label[for=ParameterId]").parent().next("div .k-edit-field").hide();
            e.container.find("label[for=CreatedOn]").parent("div .k-edit-label").hide();
            e.container.find("label[for=CreatedOn]").parent().next("div .k-edit-field").hide();
            e.container.find("label[for=FullName]").parent("div .k-edit-label").hide();
            e.container.find("label[for=FullName]").parent().next("div .k-edit-field").hide();
            //up
            if (!e.model.isNew()) {
                $(".k-window-title").text("✔️ Обновить / Update");
                $(".k-window .k-grid-update").html("<span class=\"fa fa-check-circle\"></span> Обновить / Update");
            }
            //add
            else {
                $(".k-window-title").text("✔️ Добавить / Add ");
                $(".k-window .k-grid-update").html("<span class=\"fa fa-plus\"></span> Добавить / Add");
                e.container.find("label[for=CreatedBy]").parent("div .k-edit-label").hide();
                e.container.find("label[for=CreatedBy]").parent().next("div .k-edit-field").hide();
                e.container.find("label[for=FullName]").parent("div .k-edit-label").hide();
                e.container.find("label[for=FullName]").parent().next("div .k-edit-field").hide();
                var dataAll = this.dataSource.data();
                var lastNum = 0;
                for (var i = 0; i < dataAll.length; i++) {
                    var thisNum = dataAll[i].Order;
                    if (thisNum - lastNum > 1) {
                        break;
                    }
                    lastNum = thisNum;
                }
                var nextNum = lastNum + 1;
                e.model.set("Order", nextNum);
            }
        },
        cancel: function (e) {
            e.preventDefault();
            $("#gridIndicators").data("kendoGrid").dataSource.read();
            $("#gridIndicators").data("kendoGrid").refresh();
        },
        dataBound: onDataBoundInd,
        selectable: "row",
        scrollable: true,
        filterable: false,
        columns: createColumns(),
        editable: {
            mode: "popup",
            window: {
                animation: false,
                width: "50%",
                maxHeight: 900,
                open: function () {
                    this.center();
                }
            }
        }
    });

    $("#gridIndicators").kendoTooltip({
        filter: ".k-grid-edit",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "🔨Изменить / Edit";
        }
    });
    $("#gridIndicators").kendoTooltip({
        filter: ".k-grid-remove",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "❌Удалить / Delete";
        }
    });
    
    $("#gridIndicators").find(".k-dropdown-wrap:last").width(35);
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.67;
    $("#gridIndicators").height(windowHeight);
});
