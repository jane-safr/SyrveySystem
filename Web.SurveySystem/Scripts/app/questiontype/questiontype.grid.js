'use strict';
$(document).ready(function () {
    function createColumns() {
        var cols = [
            {
                field: "TypeName",
                title: "Наименование<br/>Name",
                editor: textareaEditorRusAndEng,
                width: "35%",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
           {
               field: "IsFixedAnswer",
               hidden: true,
                title: "Фиксированный ответ<br/>Fixed Answer",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                //template: '<input type="checkbox" #=IsFixedAnswer ? "checked=checked" : "" # disabled="disabled"></input>',
               groupHeaderTemplate: "#=value ? 'Шаблонный ответ / Fixed Answers' : 'Ответы добавляются вручную / Manual answer'# : #=count # ",
               aggregates: ["count"],
                headerAttributes: {style: 'vertical-align: top;text-align: center;'},
               filterable: false
            },
            {
                field: "IsOpenAnswer",
                title: "Открытый ответ<br/>Open Answer",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsOpenAnswer ? "checked=checked" : "" # disabled="disabled"></input>',
                width: "15%",
               // groupHeaderTemplate: "#: value ? 'Шаблонный ответ / Fixed Answers' : 'Ответы добавляются вручную / Manual answer' #",
               // aggregates: ["count"],
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
            {
                field: "Comment",
                title: "Пояснение<br/>Comment",
                editor: textareaEditorRusAndEng,
                width: "50%",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
            {
                field: "IsActive",
                title: "Активный<br/>Active",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsActive ? "checked=checked" : "" # disabled="disabled"></input>',
                width: "10%",
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
            {
                field: "CreatedOn",
                title: "Создан<br/>Created on",
                width: "15%",
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
                width: "15%",
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
                width: "15%",
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
                className: "btn btn-success btn-sm",
                iconClass: "fa fa-plus"
            });
        }
        return toolbar;
    }

    function onDataBoundQTypes(e) {
       // e.sender.element.find(".k-group-col,.k-group-cell").css('width', '2%');
        $(".btn-success").removeClass("k-button");
        $(".btn-danger").removeClass("k-button");
        $(".btn-primary").removeClass("k-button");
        $(".btn-warning").removeClass("k-button");
        $(".btn-info").removeClass("k-button");
       // $('.k-group-cell').css('width', '1px');

        var items = this._data;
        var tableRows = $(this.table).find("tr").not(".k-grouping-row");
        tableRows.each(function (index) {
            var row = $(this);
            var item = items[index];
            /*  if (item.IsFixedAnswer) {
                    row.addClass("table-info");
                } */
            if (!item.IsActive)
                row.addClass("noActive");
            if(!item.IsFixedAnswer)
                row.find(".k-hierarchy-cell").html("");
        });
        
        var grid = e.sender;
        grid.tbody.find("tr.k-master-row").click(function(e) {
            var target = $(e.target);
            if ((target.hasClass("k-i-expand")) || (target.hasClass("k-i-collapse"))) {
                return;
            }
            var row = target.closest("tr.k-master-row");
            var icon = row.find(".k-i-expand");

            if (icon.length) {
                grid.expandRow(row);
            } else {
                grid.collapseRow(row);
            }
        });
    }

    var preventBinding = false;
    $("#gridQuestionTypes").kendoGrid({
        toolbar: createToolbar(),
        autoBind: true,
        batch: false,
        sortable: true,
        mobile: true,
        pageable: {
            refresh: true,
            pageSizes: true,
            buttonCount: 15,
            pageSize: 15
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
                        url: "/questiontype/all",
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
                            window.popupNotification.show("Ошибка получения типов / Error get question types", "error");
                        }
                    });
                },
                create: function (e) {
                    if (e && e.data) {
                        var createDate = e.data;
                        if (createDate) {
                            kendo.ui.progress($('.myclassselector'), true);
                            $.ajax({
                                url: "/questiontype/add",
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
                                url: "/questiontype/edit",
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
                        if (deleteDate && deleteDate.QuestionTypeId) {
                            $.ajax({
                                url: "/questiontype/remove",
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify({ id: deleteDate.QuestionTypeId }),
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
                                    $("#gridQuestionTypes").data("kendoGrid").dataSource.read();
                                    $("#gridQuestionTypes").data("kendoGrid").refresh();
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
                    $("#gridQuestionTypes").data("kendoGrid").dataSource.read();
                    $("#gridQuestionTypes").data("kendoGrid").refresh();
                }
            },
            batch: false,
            group: {
                field: "IsFixedAnswer",
                dir: "desc",
                aggregates: [
                    {
                        field: "IsFixedAnswer",
                        aggregate: "count"
                    }
                ]
            },
            schema: {
                total: "data.length",
                data: "data",
                groups: 'groups',
                aggregates: 'aggregates',
                model: {
                    id: "QuestionTypeId",
                    fields:
                    {
                        QuestionTypeId: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 1, max: 38 }
                        },
                        TypeName: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 3, max: 200 }
                        },
                        IsActive: {
                            type: "boolean",
                            editable: true,
                            defaultValue: true
                        },
                        IsFixedAnswer: {
                            type: "boolean",
                            editable: true,
                            defaultValue: false
                        },
                        IsOpenAnswer: {
                            type: "boolean",
                            editable: true,
                            defaultValue: false
                        },
                        Comment: {
                            type: "string",
                            editable: true,
                            validation: { required: false, min: 3, max: 100 }
                        },
                        CreatedBy: {
                            type: "string",
                            editable: false
                        },
                        CreatedOn: {
                            type: "date",
                            editable: false
                        }
                    }
                }
            }
        },
        //dataBinding: function (e) {
        //    if (preventBinding) {
        //        e.preventDefault();
        //    }
        //    preventBinding = false;
        //},
        detailInit: detailInit,
        detailExpand: function (e) {
            var grid = e.sender;
            var rows = grid.element.find(".k-master-row").not(e.masterRow);
            rows.each(function (e) {
                $(".k-master-row").removeClass("selected");
                grid.collapseRow(this);
            });
            e.masterRow.addClass("selected");
        },
        edit: function (e) {
            $(".k-grid-update").removeClass("k-button k-primary").addClass("btn btn-success btn-sm mr-1");
            $(".k-grid-cancel").removeClass("k-button").addClass("btn btn-danger btn-sm mr-1");
            window.popupNotification.show("✔️ Загрузка.../ Loading...", "success");
            $(".k-window .k-grid-cancel").html("<span class=\"fa fa-times-circle\"></span> Отменить / Cancel");
          //  e.container.find("label[for=IsFixedAnswer]").parent("div .k-edit-label").hide();
           // e.container.find("label[for=IsFixedAnswer]").parent().next("div .k-edit-field").hide();
            e.container.find("label[for=QuestionTypeId]").parent("div .k-edit-label").hide();
            e.container.find("label[for=QuestionTypeId]").parent().next("div .k-edit-field").hide();
            e.container.find("label[for=CreatedOn]").parent("div .k-edit-label").hide();
            e.container.find("label[for=CreatedOn]").parent().next("div .k-edit-field").hide();
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
               }
        },
        cancel: function (e) {
            e.preventDefault();
            $("#gridQuestionTypes").data("kendoGrid").dataSource.read();
            $("#gridQuestionTypes").data("kendoGrid").refresh();
        },
        dataBound: onDataBoundQTypes,
        //selectable: "row",
        selectable: true,
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
    $("#gridQuestionTypes").kendoTooltip({
        filter: ".k-grid-edit",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "🔨Изменить / Edit";
        }
    });
    $("#gridQuestionTypes").kendoTooltip({
        filter: ".k-grid-delete",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "❌Удалить / Delete";
        }
    });
    
    function createToolbarAnswer() {
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
    function createColumnsAnswers() {
        var cols = [
            {
                field: "FixAnswerRus",
                title: "Ответ (рус.) / Answer (rus.)",
                editor: textareaEditorRusAndEng,
                width: 60,
                attributes: { style: 'vertical-align: middle; text-align:left;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
            {
                field: "FixAnswerEng",
                title: "Ответ (англ.) / Answer (eng.)",
                editor: textareaEditorEn,
                width: 60,
                attributes: { style: 'vertical-align: middle; text-align:left;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
            {
                field: "Credit",
                title: "Балл / Credit",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '#= kendo.toString(Credit, "n0")#',
                format: "{0:n0}",
                width: 15,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                filterable: false
            },
            {
                field: "IsActive",
                title: "Актив. / Active",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsActive ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 15,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false
            },
        ];
        if (window.permissions.Edit) {
            var comm = [];
            comm.push({
                name: "edit",
                text: " ",
                className: "btn btn-warning btn-sm mr-1",
                iconClass: "fa fa-wrench"
            });
            comm.push({
                name: "destroy",
                text: " ",
                className: "btn btn-danger btn-sm mr-1",
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
                title: "Действия / Actions"
            });
        }
        return cols;
    }

    function detailInit(e) {
        // var grid = this.wrapper.closest("[data-role=grid]").data("kendoGrid");
        // grid.dataSource.read();
        var id = this.dataItem(e.masterRow).QuestionTypeId;

        $('<div id="child-grid-' + id + '"/>').appendTo(e.detailCell).kendoGrid({
            dataSource: {
                transport: {
                    read: function (options) {
                        kendo.ui.progress($('.myclassselector'), true);
                        var modelsFilter = [];
                       // modelsFilter.push({ Field: "IsActive", Value: true });
                        modelsFilter.push({ Field: "QuestionTypeId", Value: id });
                        $.ajax({
                            url: "/fixedanswer/active",
                            data: JSON.stringify({ filterModels: modelsFilter }),
                            dataType: "json",
                            type: "POST",
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
                                window.popupNotification.show("Ошибка получения ответов / Error get fixed answers types", "error");
                            }
                        });
                    },
                    create: function (e) {
                        if (e && e.data && id) {
                            e.data.QuestionTypeId = id;
                            var createDate = e.data;
                            if (createDate) {
                                kendo.ui.progress($('.myclassselector'), true);
                                $.ajax({
                                    url: "/fixedanswer/add",
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
                                    url: "/fixedanswer/edit",
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
                            if (deleteDate && deleteDate.FixedAnswerId) {
                                $.ajax({
                                    url: "/fixedanswer/remove",
                                    dataType: "json",
                                    type: "POST",
                                    contentType: "application/json; charset=utf-8",
                                    data: JSON.stringify({ id: deleteDate.FixedAnswerId }),
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
                edit: function (e) {
                    debugger;
                    $(".k-grid-update").removeClass("k-button k-primary").addClass("btn btn-success btn-sm mr-1");
                    $(".k-grid-cancel").removeClass("k-button").addClass("btn btn-danger btn-sm mr-1");
                    window.popupNotification.show("✔️ Загрузка.../ Loading...", "success");
                    $(".k-window .k-grid-cancel").html("<span class=\"fa fa-times-circle\"></span> Отменить / Cancel");
                    e.container.find("label[for=CreatedOn]").parent("div .k-edit-label").hide();
                    e.container.find("label[for=CreatedOn]").parent().next("div .k-edit-field").hide();

                    if (!e.model.isNew()) {
                        $(".k-window-title").text("✔️ Обновить / Update");
                        $(".k-window .k-grid-update").html("<span class=\"fa fa-check-circle\"></span> Обновить / Update");
                    }
                    //    //add
                    else {
                        $(".k-window-title").text("✔️ Добавить / Add ");
                        $(".k-window .k-grid-update").html("<span class=\"fa fa-plus\"></span> Добавить / Add");

                    }
                },
                cancel: function (e) {
                    debugger;
                    e.preventDefault();
                    $("#child-grid-" + id).data("kendoGrid").dataSource.read();
                    $("#child-grid-" + id).data("kendoGrid").refresh();
                },
                requestEnd: function (e) {
                    if (e.type === "create" || e.type === "destroy" || e.type === "update") {
                        $("#child-grid-" + id).data("kendoGrid").dataSource.read();
                        $("#child-grid-" + id).data("kendoGrid").refresh();
                    }
                },
                //pageSize: 10,
                schema: {
                    total: "data.length",
                    data: "data",
                    model: {
                        id: "FixedAnswerId",
                        fields: {
                            FixedAnswerId: {
                                type: "string",
                                editable: false,
                                validation: { required: true, min: 1, max: 38 }
                            },
                            QuestionTypeId: {
                                type: "string",
                                editable: false,
                                validation: { required: true, min: 3, max: 200 }
                            },
                            FixAnswerRus: {
                                type: "string",
                                editable: true,
                                validation: { required: true, min: 3, max: 200 }
                            },
                            FixAnswerEng: {
                                type: "string",
                                editable: true,
                                validation: { required: true, min: 3, max: 200 }
                            },
                            Credit: {
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
                            }
                        }
                    }
                }
            },
            scrollable: false,
            sortable: true,
            pageable: false,
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
            },
            selectable: "row",
            dataBound: onDataBound,
            toolbar: createToolbarAnswer(),
            columns: createColumnsAnswers()
        });

        function onDataBound() {
            this.expandRow(this.tbody.find("tr.k-master-row").first());
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
                row.addClass("table-warning");
            });
        }

        $("#child-grid-" + id).kendoTooltip({
            filter: ".k-grid-edit",
            position: 'bottom',
            showAfter: 1,
            widht: 45,
            offset: 10,
            content: function (e) {
                return "🔨Изменить / Edit";
            }
        });
        $("#child-grid-" + id).kendoTooltip({
            filter: ".k-grid-delete",
            position: 'bottom',
            showAfter: 1,
            widht: 45,
            offset: 10,
            content: function (e) {
                return "❌Удалить / Delete";
            }
        });
    }
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.8;
    $("#gridQuestionTypes").height(windowHeight);
});