'use strict';
$(document).ready(function () {
    debugger;
    var dsIndicators = new kendo.data.DataSource({
        serverFiltering: true,
        transport: {
            read: function (options) {
                $.ajax({
                    url: "/indicator/active",
                    dataType: "json",
                    type: "POST",
                    async: false,
                    cache: true,
                    data: JSON.stringify({ searchtxt: $("#cbIndicators").data("kendoComboBox").input.val() }),
                    contentType: "application/json; charset=utf-8",
                    success: function (result) {
                        if (result && result.success) {
                            options.success(result);
                        }
                        else {
                            window.popupNotification.show("Ошибка при получении индикаторов / Error get indicators", "error");
                            options.error();
                        }
                    },
                    error: function (response) {
                        window.popupNotification.show("Ошибка при получении индикаторов / Error get indicators", "error");
                        options.error();
                    }
                });
            }
        },
        schema: {
            data: "data"
   }
    });
    function comboBoxInd(container, options) {
        $('<input id="cbIndicators" style="width: 95%; max-width: 100%"  name="' + options.field + '"/>')
            .appendTo(container)
            .kendoComboBox({
                dataSource: dsIndicators,
                dataValueField: "IndicatorId",
                dataTextField: "FullName",
                filter: "contains",
                minLength: 3,
                placeholder: "Выберите индикатор / Select indicator",
                autoBind: true,
                enable: false
            });
    }

    var dsTypes = new kendo.data.DataSource({
        serverFiltering: true,
        transport: {
            read: function (options) {
                $.ajax({
                    url: "/questiontype/all",
                    dataType: "json",
                    type: "GET",
                    async: false,
                    cache: false,
                    data: { searchtxt: $("#cbTypes").data("kendoComboBox").input.val() },
                    contentType: "application/json; charset=utf-8",
                    success: function (result) {
                        if (result && result.success) {
                            options.success(result);
                        }
                        else {
                            window.popupNotification.show("Ошибка при получении типов / Error get types", "error");
                            options.error();
                        }
                    },
                    error: function (response) {
                        window.popupNotification.show("Ошибка при получении типов / Error get types", "error");
                        options.error();
                    }
                });
            }
        },
        schema: {
            data: "data"
        }
    });
    function comboBoxTypes(container, options) {
        $('<input id="cbTypes" style="width: 95%; max-width: 100%"  name="' + options.field + '"/>')
            .appendTo(container)
            .kendoComboBox({
                dataSource: dsTypes,
                dataValueField: "QuestionTypeId",
                dataTextField: "TypeName",
                filter: "contains",
                minLength: 3,
                placeholder: "Выберите тип вопроса / Select question type",
                autoBind: true
            });
    }

    function createColumns() {
        var cols = [
            {
                field: "Group",
                title: "Номер<br/>#",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '#= kendo.toString(Group, "n0")#',
                format: "{0:n0}",
                width: "5%",
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                filterable: false
            },
            {
                field: "QuestionRus",
                title: "Вопрос (рус.)<br/>Question (rus.)",
                editor: textareaEditorRusAndEng,
                width: "32%",
                attributes: { style: 'vertical-align: middle; text-align:left;font-size: 11pt;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false
            },
            {
                field: "QuestionEng",
                title: "Вопрос (англ.)<br/>Question (eng.)",
                editor: textareaEditorEn,
                width: "32%",
                attributes: { style: 'vertical-align: middle; text-align:left;font-size: 11pt;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false
            },
            {
                field: "QuestionTypeId",
                title: "Тип вопроса<br/>Question type",
                template: '<span>#=kendo.toString(QuestionType.TypeName, "")#</span>',
                width: "15%",
                editable: false,
                sortable: false,
                editor: comboBoxTypes,
                headerAttributes: { style: 'vertical-align: top;text-align: center;font-size: 11pt;' },
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                filterable: false
            },
            {
                field: "IsCriterion",
                title: "Индикатор<br/>Indicator",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsCriterion ? "checked=checked" : "" # disabled="disabled"></input>',
                width: "10%",
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false
            },
            {
                field: "IndicatorId",
                title: "Индикатор<br/>Indicator",
                template: '<span>#= Indicator? kendo.toString(Indicator.FullNumber, ""): ""#</span>',
                width: "8%",
                editable: false,
                sortable: false,
                editor: comboBoxInd,
                headerAttributes: { style: 'vertical-align: top;text-align: center;font-size: 11pt;' },
                attributes: { style: 'vertical-align: middle; text-align:left;font-size: 11pt;' },
                filterable: false,
                //hidden: true
            },
            {
                field: "IsInReport",
                title: "Для отчета<br/>In Report",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsInReport ? "checked=checked" : "" # disabled="disabled"></input>',
                width: "8%",
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false
            },
            {
                field: "IsActive",
                title: "Активный<br/>Active",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsActive ? "checked=checked" : "" # disabled="disabled"></input>',
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false,
                hidden: true
            }
        ];
        if (window.permissions.Edit && window.IsActiveSurvey) {
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
                width: "8%",
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                title: "Действия<br/>Actions"
            });
        }
        return cols;
    }

    function createToolbar() {
        var toolbar = [];
        if (window.permissions.Add && window.IsActiveSurvey) {
            toolbar.push({
                name: "create",
                text: " Добавить / Add",
                className: "btn btn-success",
                iconClass: "fa fa-plus mt-1 mr-1"
            });
        }
        return toolbar;
    }

    function onDataBoundQuestions(e) {
        $(".btn-success").removeClass("k-button");
        $(".btn-danger").removeClass("k-button");
        $(".btn-primary").removeClass("k-button");
        $(".btn-warning").removeClass("k-button");
        $(".btn-info").removeClass("k-button");

        var items = this._data;
        var tableRows = $(this.table).find("tr").not(".k-grouping-row");
        tableRows.each(function (index) {
            var row = $(this);
            var item = items[index];
            /*  if (item.IsFixedAnswer) {
                    row.addClass("table-info");
                } */
            if (item && item.QuestionType.IsOpenAnswer)
                row.find(".k-hierarchy-cell").html("");

            if (!window.IsActiveSurvey) {
                $(".k-grid-toolbar").hide();
            }
        });

       /* var items = this._data;
        var tableRows = $(this.table).find("tr");
        tableRows.each(function (index) {
            var row = $(this);
            var item = items[index];
            if ((item.Group % 2) === 0) {
                row.addClass("table-info");
            } else {
                row.addClass("table-info-dark");
            }
        });*/

        var grid = e.sender;
        grid.tbody.find("tr.k-master-row").click(function (e) {
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
    $("#gridQuestions").kendoGrid({
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
            transport:
            {
                read: function (options) {
                    kendo.ui.progress($('.myclassselector'), true);
                    var modelsFilter = [];
                    modelsFilter.push({ Field: "SurveyCode", Value: window.SurveyCode });
                    $.ajax({
                        url: "/question/active",
                        dataType: "json",
                        type: "POST",
                        data: JSON.stringify({ filterModels: modelsFilter }),
                        traditional: true,
                        cache: false,
                        contentType: "application/json; charset=utf-8",
                        success: function (result) {
                            kendo.ui.progress($('.myclassselector'), false);
                            if (result && result.success && result.success === true) {
                                $("#countQuestions").text(result.data.length);
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
                        createDate.SurveyId = window.SurveyId;
                        if (createDate) {
                            kendo.ui.progress($('.myclassselector'), true);
                            $.ajax({
                                url: "/question/add",
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
                        updateDate.SurveyId = window.SurveyId;
                        var currentlySelectedValue = $("#cbIndicators").data("kendoComboBox").value(); // get dataValueField (guid)
                        updateDate.IndicatorId = currentlySelectedValue;
                       if (updateDate) {
                            kendo.ui.progress($('.myclassselector'), true);
                            $.ajax({
                                url: "/question/edit",
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
                        if (deleteDate && deleteDate.QuestionId) {
                            $.ajax({
                                url: "/question/remove",
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify({ id: deleteDate.QuestionId }),
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
                                    window.popupNotification.show(response.message, "error");
                                    e.error();
                                    $("#gridQuestions").data("kendoGrid").dataSource.read();
                                    $("#gridQuestions").data("kendoGrid").refresh();
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
                    $("#gridQuestions").data("kendoGrid").dataSource.read();
                    $("#gridQuestions").data("kendoGrid").refresh();
                }
            },
            batch: false,
            schema: {
                total: "data.length",
                data: "data",
                model: {
                    id: "QuestionId",
                    fields:
                    {
                        QuestionId: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 1, max: 38 }
                        },
                        QuestionRus: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 3, max: 200 }
                        },
                        QuestionEng: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 3, max: 200 }
                        },
                        IsCriterion: {
                            type: "boolean",
                            editable: true,
                            defaultValue: false
                        },
                        SurveyId: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 1, max: 38 }
                        },
                        QuestionTypeId: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 1, max: 38 }
                        },
                        QuestionType:
                        {
                            TypeName: {
                                type: "string",
                                editable: false
                            },
                            IsActive: {
                                type: "boolean",
                                editable: false
                            }
                        },
                        IndicatorId: {
                            type: "string",
                            editable: true,
                            validation: { required: true }
                        },
                        Indicator:
                        {
                            FullName: {
                                type: "string",
                                editable: false
                            },
                            FullNumber: {
                                type: "string",
                                editable: false
                            },
                            IsActive: {
                                type: "boolean",
                                editable: false
                            }
                        },
                        Group: {
                            type: "number",
                            editable: true,
                            validation: { required: true, min: 0 }
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
                        IsInReport: {
                            type: "boolean",
                            editable: true,
                            defaultValue: true
                        }
                    }
                }
            }
        },
       /* change: function (e) {
            debugger;
            var grid = this.wrapper.closest("[data-role=grid]").data("kendoGrid");
            grid.dataSource.read();
            if (e.field && e.field.indexOf("Answers.results") >= 0) {
                preventBinding = true;
            }
        },*/
        dataBinding: function (e) {
            if (preventBinding) {
                e.preventDefault();
            }
            preventBinding = false;
        },
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
            e.container.find("label[for=QuestionId]").parent("div .k-edit-label").hide();
            e.container.find("label[for=QuestionId]").parent().next("div .k-edit-field").hide();
            e.container.find("label[for=CreatedOn]").parent("div .k-edit-label").hide();
            e.container.find("label[for=CreatedOn]").parent().next("div .k-edit-field").hide();
            $('#IsCriterion').on('click',
                function() {
                    if ($(this).is(':checked')) {
                        $("#cbIndicators").data("kendoComboBox").enable(true);
                    } else {
                        $("#cbIndicators").data("kendoComboBox").value(null);
                        $("#cbIndicators").data("kendoComboBox").enable(false);
                    }
                });
            //up
            if (!e.model.isNew()) {
                $(".k-window-title").text("✔️ Обновить / Update");
                $(".k-window .k-grid-update").html("<span class=\"fa fa-check-circle\"></span> Обновить / Update");
                if ($('#IsCriterion').is(':checked')) {
                    $("#cbIndicators").data("kendoComboBox").enable(true);
                }
            }
            //add
            else {
                $(".k-window-title").text("✔️ Добавить / Add ");
                $(".k-window .k-grid-update").html("<span class=\"fa fa-plus\"></span> Добавить / Add");
                e.container.find("label[for=CreatedBy]").parent("div .k-edit-label").hide();
                e.container.find("label[for=CreatedBy]").parent().next("div .k-edit-field").hide();
                var dataAll = this.dataSource.data();
                var lastNum = 0;
                for (var i = 0; i < dataAll.length; i++) {
                    var thisNum = dataAll[i].Group;
                    if (thisNum - lastNum > 1) {
                        break;
                    }
                    lastNum = thisNum;
                }
                var nextNum = lastNum + 1;
                e.model.set("Group", nextNum);
           }
        },
        cancel: function (e) {
            e.preventDefault();
            $("#gridQuestions").data("kendoGrid").dataSource.read();
            $("#gridQuestions").data("kendoGrid").refresh();
        },
        dataBound: onDataBoundQuestions,
        selectable: "row",
        scrollable: true,
        filterable: false,
        columns: createColumns(),
        editable: {
            mode: "popup",
            window: {
                animation: false,
                width: "60%",
                maxHeight: 900,
                open: function () {
                    this.center();
                }
            }
        }
    });

    $("#gridQuestions").kendoTooltip({
        filter: ".k-grid-edit",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "🔨Изменить / Edit";
        }
    });
    $("#gridQuestions").kendoTooltip({
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
    windowHeight = windowHeight * 0.55;
    $("#gridQuestions").height(windowHeight);

    function createColumnsAnswers() {
        var cols = [
            {
                field: "AnswerRus",
                title: "Ответ (рус.) / Answer (rus.)",
                editor: textareaEditorRusAndEng,
                width: 60,
                attributes: { style: 'vertical-align: middle; text-align:left;;font-size: 11pt;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;;font-size: 11pt;'
                },
                sortable: false
            },
            {
                field: "AnswerEng",
                title: "Ответ (англ.) / Answer (eng.)",
                editor: textareaEditorEn,
                width: 60,
                attributes: { style: 'vertical-align: middle; text-align:left;;font-size: 11pt;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;;font-size: 11pt;'
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
                    style: 'vertical-align: top;text-align: center;;font-size: 11pt;'
                },
                filterable: false
            },
            {
                field: "IsValid",
                title: "Верно / Valid",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsValid ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 20,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false
            }
        ];
        if (window.permissions.Edit && window.IsActiveSurvey) {
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
                width: 25,
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
       // var typeId = this.dataItem(e.masterRow).QuestionTypeId;
        //var fix = this.dataItem(e.masterRow).QuestionType.IsFixedAnswer;

        var id = this.dataItem(e.masterRow).QuestionId;
        $('<div id="child-grid-' + id + '"/>').appendTo(e.detailCell).kendoGrid({
            dataSource: {
                transport: {
                    read: function(options) {
                        kendo.ui.progress($('.myclassselector'), true);
                        var modelsFilter = [];
                        modelsFilter.push({ Field: "IsActive", Value: true });
                         modelsFilter.push({ Field: "QuestionId", Value: id });
                        $.ajax({
                            url: "/answer/active",
                            data: JSON.stringify({ filterModels: modelsFilter }),
                            dataType: "json",
                            type: "POST",
                            traditional: true,
                            cache: false,
                            contentType: "application/json; charset=utf-8",
                            success: function(result) {
                                kendo.ui.progress($('.myclassselector'), false);
                                if (result && result.success && result.success === true) {
                                    options.success(result);
                                } else {
                                    window.popupNotification.show(result.message, "error");
                                }
                            },
                            error: function(response) {
                                window.popupNotification.show(
                                    "Ошибка получения ответов / Error get answers",
                                    "error");
                            }
                        });
                    },
                    create: function(e) {
                        if (e && e.data && id) {
                            e.data.QuestionId = id;
                            var createDate = e.data;
                            if (createDate) {
                                kendo.ui.progress($('.myclassselector'), true);
                                $.ajax({
                                    url: "/answer/add",
                                    dataType: "json",
                                    type: "POST",
                                    traditional: true,
                                    contentType: "application/json; charset=utf-8",
                                    data: JSON.stringify(createDate),
                                    success: function(response) {
                                        if (response && response.success) {
                                            e.success();
                                            window.popupNotification.show(response.message, "success");
                                        } else {
                                            kendo.ui.progress($('.myclassselector'), false);
                                            window.popupNotification.show(response.message, "error");
                                            e.error();
                                        }
                                    },
                                    error: function(response) {
                                        kendo.ui.progress($('.myclassselector'), false);
                                        window.popupNotification.show(response.responseText, "error");
                                        e.error();
                                    }
                                });
                            } else {
                                window.popupNotification.show("Укажите данные для создания / No data to create",
                                    "error");
                            }
                        }
                    },
                    update: function(e) {
                        if (e && e.data && id) {
                            e.data.QuestionId = id;
                            var updateDate = e.data;
                            if (updateDate) {
                                kendo.ui.progress($('.myclassselector'), true);
                                $.ajax({
                                    url: "/answer/add",
                                    dataType: "json",
                                    type: "POST",
                                    traditional: true,
                                    contentType: "application/json; charset=utf-8",
                                    data: JSON.stringify(updateDate),
                                    success: function(response) {
                                        kendo.ui.progress($('.myclassselector'), false);
                                        if (response && response.success) {
                                            e.success();
                                            window.popupNotification.show(response.message, "success");
                                        } else {
                                            window.popupNotification.show(response.message, "error");
                                            e.error();
                                        }
                                    },
                                    error: function(response) {
                                        kendo.ui.progress($('.myclassselector'), false);
                                        window.popupNotification.show(response.responseText, "error");
                                        e.error();
                                    }
                                });
                            } else {
                                kendo.ui.progress($('.myclassselector'), false);
                                window.popupNotification.show("Укажите данные для обновления / No data to edit",
                                    "error");
                            }
                        }
                    },
                    destroy: function(e) {
                        if (e && e.data) {
                            kendo.ui.progress($('.myclassselector'), true);
                            var deleteDate = e.data;
                            if (deleteDate && deleteDate.AnswerId) {
                                $.ajax({
                                    url: "/answer/remove",
                                    dataType: "json",
                                    type: "POST",
                                    contentType: "application/json; charset=utf-8",
                                    data: JSON.stringify({ id: deleteDate.AnswerId }),
                                    success: function(response) {
                                        kendo.ui.progress($('.myclassselector'), false);
                                        if (response && response.success) {
                                            e.success();
                                            window.popupNotification.show(response.message, "success");
                                        } else {
                                            window.popupNotification.show(response.message, "error");
                                            e.error();
                                        }
                                    },
                                    error: function(response) {
                                        kendo.ui.progress($('.myclassselector'), false);
                                        window.popupNotification.show(response.responseText, "error");
                                        e.error();
                                    }
                                });
                            } else {
                                kendo.ui.progress($('.myclassselector'), false);
                                window.popupNotification.show("Данные не удалены / Delete not completed", "error");
                            }
                        } else {
                            window.popupNotification.show("Ошибка при удалении / Delete error", "error");
                            e.error();
                        }
                    },
                    parameterMap: function(data, type) {
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
                requestEnd: function(e) {
                    if (e.type === "create" || e.type === "destroy" || e.type === "update") {
                        $("#child-grid-" + id).data("kendoGrid").dataSource.read();
                        $("#child-grid-" + id).data("kendoGrid").refresh();
                    }
                },
                pageSize: 10,
                schema: {
                    total: "data.length",
                    data: "data",
                    model: {
                        id: "AnswerId",
                        fields: {
                            AnswerId: {
                                type: "string",
                                editable: false,
                                validation: { required: true, min: 1, max: 38 }
                            },
                            QuestionId: {
                                type: "string",
                                editable: false,
                                validation: { required: true, min: 3, max: 200 }
                            },
                            AnswerRus: {
                                type: "string",
                                editable: true,
                                validation: { required: true, min: 3, max: 200 }
                            },
                            AnswerEng: {
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
                            IsValid: {
                                type: "boolean",
                                editable: true,
                                defaultValue: false
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
            cancel: function (e) {
                e.preventDefault();
                $("#child-grid-" + id).data("kendoGrid").dataSource.read();
                $("#child-grid-" + id).data("kendoGrid").refresh();
            },
            scrollable: false,
            sortable: true,
            pageable: false,
            editable: "inline",
            selectable: "row",
            dataBound: onDataBound,
            toolbar: createToolbar(),
            columns: createColumnsAnswers()
        });

        function onDataBound() {
            this.expandRow(this.tbody.find("tr.k-master-row").first());
            $(".btn-success").removeClass("k-button");
            $(".btn-danger").removeClass("k-button");
            $(".btn-primary").removeClass("k-button");
            $(".btn-warning").removeClass("k-button");
            $(".btn-info").removeClass("k-button");
        }

        $("#child-grid-" + id).kendoTooltip({
            filter: ".k-grid-edit",
            position: 'bottom',
            showAfter: 1,
            widht: 45,
            offset: 10,
            content: function(e) {
                return "🔨Изменить / Edit";
            }
        });
        $("#child-grid-" + id).kendoTooltip({
            filter: ".k-grid-delete",
            position: 'bottom',
            showAfter: 1,
            widht: 45,
            offset: 10,
            content: function(e) {
                return "❌Удалить / Delete";
            }
        });
    }
});