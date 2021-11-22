'use strict';
$(document).ready(function () {

    var dsCriterions = new kendo.data.DataSource({
        serverFiltering: true,
        transport: {
            read: function (options) {
                $.ajax({
                    url: "/criterion/all",
                    dataType: "json",
                    type: "GET",
                    async: false,
                    cache: false,
                  //  data: { searchtxt: $("#cbCriterions").data("kendoComboBox").input.val() },
                    contentType: "application/json; charset=utf-8",
                    success: function (result) {
                        if (result && result.success) {
                            options.success(result);
                        }
                        else {
                            window.popupNotification.show("Ошибка при получении критериев / Error get criterions", "error");
                            options.error();
                        }
                    },
                    error: function (response) {
                        window.popupNotification.show("Ошибка при получении критериев / Error get criterions", "error");
                        options.error();
                    }
                });
            }
        },
        schema: {
            data: "data"
        }
    });
    function comboBox(container, options) {
        $('<input id="cbCriterions" style="width: 100%; max-width: 100%"  name="' + options.field + '"/>')
            .appendTo(container)
            .kendoComboBox({
                dataSource: dsCriterions,
                dataValueField: "CriterionId",
                dataTextField: "FullName",
                filter: "contains",
                minLength: 3,
                placeholder: "Выберите критерий / Select criterion",
                autoBind: true
            });
    }

    function createColumns() {
        var cols = [
            {
                field: "CriterionId",
                title: "Критерий<br/>Criterion",
                template: '<span>#=kendo.toString(Criterion.FullName, "")#</span>',
                width: 100,
                editable: false,
                sortable: false,
                editor: comboBox,
                headerAttributes: { style: 'vertical-align: top;text-align: center;' },
                attributes: { style: 'vertical-align: middle; text-align:left;' },
                filterable: false
            },
            {
                field: "Order",
                title: "Порядок<br/>Sorting",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '#= kendo.toString(Order, "n0")#',
                format: "{0:n0}",
                width: 15,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false,
                hidden: true
            },
            {
                field: "FullName",
                title: "Показатель<br/>Parameter",
                template: "<a href='/parameter/edit/#=ParameterId#'>#=FullName#</a>",
                editor: textareaEditorRusAndEng,
                width: 100,
                attributes: { style: 'vertical-align: middle; text-align:left;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
            {
                field: "Name",
                title: "Наименование<br/>Name",
                editor: textareaEditorRusAndEng,
                width: 50,
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
                width: 20,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
            {
                field: "CreatedOn",
                title: "Создан<br/>Created on",
                width: 35,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                format: "{0: dd-MM-yyyy HH:mm}",
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false
            },
            {
                field: "CreatedBy",
                title: "Создал<br/>Created by",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                width: 20,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            }
        ];
        if (window.permissions.Edit) {
            var comm = [];
            comm.push({
                name: "edit",
                text: " ",
                className: "btn btn-warning btn-sm mr-1 btn-rounded",
                iconClass: "fa fa-wrench"
            });
            comm.push({
                name: "destroy",
                text: " ",
                className: "btn btn-danger btn-sm mr-1 btn-rounded",
                title: "Удалить / Delete",
                iconClass: "fa fa-times"
            });
            cols.push({
                command: comm,
                width: 30,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                title: "Действия<br/>Actions"
            });
        }
        return cols;
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
        }
        return toolbar;
    }

    function onDataBoundParameters() {
        this.expandRow(this.tbody.find("tr.k-master-row").first());
        $(".btn-success").removeClass("k-button");
        $(".btn-danger").removeClass("k-button");
        $(".btn-primary").removeClass("k-button");
        $(".btn-warning").removeClass("k-button");
        $(".btn-info").removeClass("k-button");
        var items = this._data;
        var tableRows = $(this.table).find("tr").not(".k-grouping-row");
        tableRows.each(function(index) {
            var row = $(this);
            var item = items[index];
            if (!item.IsActive) {
                row.addClass("noActive");
            }
        });
    }

    $("#gridParameters").kendoGrid({
        toolbar: createToolbar(),
        autoBind: true,
        batch: false,
        sortable: true,
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
            serverSorting: true,
            serverFiltering: false,
            groupable: true,
            transport:
            {
                read: function (options) {
                    kendo.ui.progress($('.myclassselector'), true);
                    $.ajax({
                        url: "/parameter/all",
                        dataType: "json",
                        type: "GET",
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
                            window.popupNotification.show("Ошибка получения показателей / Error get parameters", "error");
                        }
                    });
                },
                create: function (e) {
                    if (e && e.data) {
                        var createDate = e.data;
                        if (createDate) {
                            kendo.ui.progress($('.myclassselector'), true);
                            $.ajax({
                                url: "/parameter/add",
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
                                url: "/parameter/edit",
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
                        if (deleteDate && deleteDate.ParameterId) {
                            $.ajax({
                                url: "/parameter/remove",
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify({ id: deleteDate.ParameterId }),
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
                                    $("#gridParameters").data("kendoGrid").dataSource.read();
                                    $("#gridParameters").data("kendoGrid").refresh();
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
                    }
                    else if (type === "create") {
                        return kendo.stringify(data);
                    }
                }
            },
            requestEnd: function (e) {
                if (e.type === "create" || e.type === "destroy" || e.type === "update") {
                    $("#gridParameters").data("kendoGrid").dataSource.read();
                    $("#gridParameters").data("kendoGrid").refresh();
                }
            },
            batch: false,
            schema: {
                total: "data.length",
                data: "data",
                model: {
                    id: "ParameterId",
                    fields:
                    {
                        ParameterId: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 1, max: 38 }
                        },
                        Name: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 3, max: 200 }
                        },
                        FullName: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 3, max: 200 }
                        },
                        Order: {
                            type: "number",
                            editable: true,
                            validation: { required: true, min: 0 }
                        },
                        IsActive: {
                            type: "boolean",
                            editable: true,
                            defaultValue: true
                        },
                        CreatedBy: {
                            type: "string",
                            editable: false
                        },
                        CreatedOn: {
                            type: "date",
                            editable: false
                        },
                        CriterionId: {
                            type: "string",
                            editable: true,
                            validation: { required: true }
                        },
                        Criterion:
                        {
                            FullName: {
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
            $("#gridParameters").data("kendoGrid").dataSource.read();
            $("#gridParameters").data("kendoGrid").refresh();
        },
        dataBound: onDataBoundParameters,
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
    $("#gridParameters").kendoTooltip({
        filter: ".k-grid-edit",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "🔨Изменить / Edit";
        }
    });
    $("#gridParameters").kendoTooltip({
        filter: ".k-grid-delete",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "❌Удалить / Delete";
        }
    });
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.85;
    $("#gridParameters").height(windowHeight);
});