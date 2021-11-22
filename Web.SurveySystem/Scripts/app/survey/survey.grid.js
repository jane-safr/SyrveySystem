'use strict';
function closeWinModalEdT() {
    try {
        var kendoWindow = $("#winModEditT").data("kendoWindow").close();
    }
    catch (err) { window.popupNotification.show(err.message, "error"); }
}
$(document).ready(function () {
    var dsTypes = new kendo.data.DataSource({
        serverFiltering: true,
        transport: {
            read: function (options) {
                $.ajax({
                    url: "/surveytype/all",
                    dataType: "json",
                    type: "GET",
                    async: false,
                    cache: false,
                    //data: { searchtxt: $("#cbSurveyTypes").data("kendoComboBox").input.val() },
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
    function comboBox(container, options) {
        $('<input id="cbSurveyTypes" style="width: 100%; max-width: 100%" name="' + options.field + '"/>')
            .appendTo(container)
            .kendoComboBox({
                dataSource: dsTypes,
                dataValueField: "SurveyTypeId",
                dataTextField: "FullName",
                filter: "contains",
                minLength: 3,
                placeholder: "Выберите тип / Select type",
                autoBind: true
            });
    }

    function createColumns() {
        var cols = [
            {
                field: "SurveyId",
                title: "...",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: "#=IsAnonymous ? '👁' : '👤' #",
                width: 8,
                headerAttributes: {
                    style: 'vertical-align: middle;text-align: center;'
                },
                filterable: false
            },
            {
                field: "SurveyCode",
                title: "ИД<br/>Id",
                template: "<a href='/survey/edit/#=SurveyCode#'>#=SurveyCode#</a>",
                width: 20,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: middle;text-align: center;'
                },
                sortable: false
            },
            {
                field: "SurveyTypeId",
                title: "Тип<br/>Type",
                template: '<span>#=kendo.toString(SurveyType.FullName, "")#</span>',
                width: 50,
                editable: false,
                sortable: false,
                editor: comboBox,
                headerAttributes: { style: 'vertical-align: top;text-align: center;' },
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                filterable: false
            },
            {
                field: "NameRus",
                title: "Наименование (рус.)<br/>Name Rus",
                editor: textareaEditorRusAndEng,
                width: 40,
                headerAttributes: {
                    style: 'vertical-align: middle;text-align: center;'
                },
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                sortable: false
            },
            {
                field: "NameEng",
                title: "Наименование (англ.)<br/>Name Eng",
                editor: textareaEditorEn,
                width: 40,
                headerAttributes: {
                    style: 'vertical-align: middle;text-align: center;'
                },
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                sortable: false
            },
            {
                field: "PurposeRus",
                title: "Цель<br/>Purpose",
                editor: textareaEditorRusAndEng,
                width: 40,
                headerAttributes: {
                    style: 'vertical-align: middle;text-align: center;'
                },
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                sortable: false,
                hidden: true
            },
            {
                field: "PurposeEng",
                title: "Цель<br/>Purpose",
                editor: textareaEditorEn,
                width: 40,
                headerAttributes: {
                    style: 'vertical-align: middle;text-align: center;'
                },
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                sortable: false,
                hidden: true
            },
            {
                field: "TimeEstimateMin",
                title: "Время<br/>Estimate Time (min)",
                template: '#= kendo.toString(TimeEstimateMin, "n0")#',
                format: "{0:n0}",
                width: 15,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: middle;text-align: center;'
                },
                sortable: false
            },
            {
                field: "IsRandomQuestions",
                title: "Случ.вопросы<br/>Random Questions",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsActive ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 20,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                sortable: false,
                hidden: true
            },
            {
                field: "IsAnonymous",
                title: "Анонимный<br/>IsAnonymous",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsAnonymous ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 20,
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
                field: "CreatedBy",
                title: "Создал<br/>Created by",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                width: 20,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                hidden: true,
                sortable: false
            }
        ];
        if (window.permissions.Edit) {
            var comm = [];
            comm.push({
                name: "invite",
                click: testInvite,
                text: " ",
                className: "btn btn-info btn-sm mr-1",
                title: "Пригласить / Invite",
                iconClass: "fa fa-envelope-open"
            });
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
                title: "Действия</br>Actions"
            });
        }
        return cols;
    }
    
    function onChangeMultiSelectUsers() {
        var multiSelectItems = $('#multiSelectUsers').data('kendoMultiSelect');
        if (multiSelectItems) {
            var count = multiSelectItems.value().length;
            if (count > 0) {
                $("#lbCountUser").css("color", "green");
            }
            else {
                $("#lbCountUser").css("color", "red");
            }
            $("#countUser").html(count);
        }
    }; 
    function testInvite(e) {
        e.preventDefault();
        var dataItemApp = this.dataItem($(e.currentTarget).closest("tr"));
        if (window.permissions.Add && dataItemApp && dataItemApp.SurveyId) {
            $("#winModEditT").kendoWindow({
                position: { top: "10%", left: "10%" },
                title: false,
                width: "80%",
                modal: true,
                content: {
                    template: $("#winModalSendInvitation").html()
                }
            }).data("kendoWindow").open();

            var lblNum = $("#lblNumber");
            if (lblNum && dataItemApp.SurveyCode) {
                lblNum.text(dataItemApp.SurveyCode);
            }
            
            $("#dpTestEnd").kendoDateTimePicker({
                value: new Date(new Date().setHours(18, 0, 0, 0)),//, //new Date(new Date().getTime() + (86400000 * 2)),
                format: "dd.MM.yyyy HH:mm",
                timeFormat: "hh:mm",
                disableDates: ["sa", "su"],
                max: new Date(2025, 0, 1),
                min: new Date(),
                dateInput: true,
                weekNumber: true,
                firstDay: 1
            });

            var datetimepicker = $("#dpTestEnd").data("kendoDateTimePicker");
            datetimepicker._dateInput.setOptions({
                messages: {
                    "year": "yyyy",
                    "month": "mm",
                    "day": "dd",
                    "hour": "hh",
                    "minute": "mm"
                }
            });
            
            $("#multiSelectUsers").kendoMultiSelect({
                //placeholder: "Выберите сотрудников / Select employees",
                dataTextField: "Name",
                dataValueField: "Id",
                maxSelectedItems: 100,
                autoBind: true,
                animation: {
                    close: {
                        effects: "fadeOut zoom:out",
                        duration: 300
                    },
                    open: {
                        effects: "fadeIn zoom:in",
                        duration: 300
                    }
                },
                change: onChangeMultiSelectUsers,
                dataSource: {
                    serverFiltering: true,
                    transport: {
                        read: function (options) {
                            $.ajax({
                                url: "/users/find",
                                dataType: "json",
                                type: "POST",
                                //data: JSON.stringify({ searchtxt: options.data.searchtxt ? options.data.searchtxt : "" }),
                                data: JSON.stringify({ searchtxt: $("#multiSelectUsers").data("kendoMultiSelect").input.val() }),
                                cache: true,
                                contentType: "application/json; charset=utf-8",
                                success: function (result) {
                                    if (result && result.success && result.success === true) {
                                        if (result.data && result.data.length < 1) {
                                            window.popupNotification.show("Сотрудник не найден. Уточните параметры.", "error");
                                        }
                                        options.success(result);
                                        kendo.ui.progress($('.myclassselector'), false);
                                    } else {
                                        window.popupNotification.show("Сотрудник не найден. Уточните параметры.", "error");
                                        options.error();
                                        kendo.ui.progress($('.myclassselector'), false);
                                    }
                                },
                                error: function (response) {
                                    alert(response);
                                }
                            });
                        }
                    },
                    schema: {
                        data: "data"
                    }
                }
            });

            $("#sendInvitationBtn").click(function (e) {
                if (confirm('Отправить приглашение / Send invitations?')) {
                    if (e) {
                       // var dataItemApp = this.dataItem($(e.currentTarget).closest("tr"));
                        if (!dataItemApp && !dataItemApp.SurveyId) {
                            window.popupNotification.show("Выберите тест / Select test", "error");
                            e.preventDefault();
                            return false;
                        }
                        var dateClose = $("#dpTestEnd").data("kendoDateTimePicker");
                        if (dateClose) {
                           
                            var dateCloseTest = dateClose._value;
                            if (dateCloseTest) {
                                var multiSelect = $("#multiSelectUsers").data("kendoMultiSelect");
                                if (multiSelect && multiSelect.dataItems() && multiSelect.dataItems().length > 0) {
                                   var multiSelectUser = multiSelect.dataItems();
                                    if (multiSelectUser && multiSelectUser.length > 0) {
                                        var saveModel = {
                                            SurveyId: dataItemApp.SurveyId,
                                            DateEnd: dateCloseTest,
                                            Users: multiSelectUser
                                        };
                                        $.ajax({
                                            url: "/invitation/addmany",
                                            dataType: "json",
                                            type: "POST",
                                            traditional: true,
                                            contentType: "application/json; charset=utf-8",
                                            data: JSON.stringify({ model: saveModel }),
                                            success: function (response) {
                                               
                                                if (response && response.success) {
                                                    closeWinModalEdT();
                                                    window.popupNotification.show(response.message, "success");
                                                } else {
                                                    window.popupNotification.show(response.message, "error");
                                                }
                                            },
                                            error: function (response) {
                                               
                                                window.popupNotification.show(response.responseText, "error");
                                            }
                                        });
                                    }
                                } else {
                                    window.popupNotification.show("Данные пользователей недействительны / Invalid users", "error");
                                }
                            } else {
                                window.popupNotification.show("Дата недействительна / Date end invalid", "error");
                            }
                        }
                        else {
                            window.popupNotification.show("Дата не найдена / Date end not found", "error");
                        }
                    }
                }
            });
        } else {
                window.popupNotification.show("Код теста не определен / Survey Code is undefined", "error");
        }
    };

    function createToolbar() {
        var toolbar = [];
        if (window.permissions.Add) {
            toolbar.push({
                name: "create",
                text: " Добавить / Add",
                className: "btn btn-success",
                iconClass: "fa fa-plus mt-1 mr-1"
            });
        }
        return toolbar;
    }

    function onDataBoundSurveys() {
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
            if (item && !item.IsActive) {
                row.addClass("table-danger");
            }
        });
    }
    
    $("#gridSurveys").kendoGrid({
        toolbar: createToolbar(),
      /*  excel: {
            allPages: true,
            filename: "surveys.xslx",
            filterable: true
        },*/
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
                    var modelsFilter = [];
                    if (options.data.searchtxt)
                        modelsFilter.push({ Field: "searchtxt", Value: options.data.searchtxt });
                    $.ajax({
                        url: "/survey/active",
                        dataType: "json",
                        type: "POST",
                        traditional: true,
                        cache: false,
                        data: JSON.stringify({ filterModels: modelsFilter }),
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
                            window.popupNotification.show("Ошибка получения тестов / Error get surveys", "error");
                        }
                    });
                },
                create: function (e) {
                    if (e && e.data) {
                        var createDate = e.data;
                       
                        if (createDate) {
                                $.ajax({
                                    url: "/survey/add",
                                    dataType: "json",
                                    type: "POST",
                                    traditional: true,
                                    contentType: "application/json; charset=utf-8",
                                    data: JSON.stringify(createDate),
                                    success: function (response) {
                                        if (response && response.success) {
                                            e.success();
                                            window.popupNotification.show(response.message, "success");
                                        } else {
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
                        } else {
                            window.popupNotification.show("Укажите данные для создания / No data to create", "error");
                        }
                },
                update: function (e) {
                    if (e && e.data) {
                        var updateDate = e.data;
                        if (updateDate) {
                                kendo.ui.progress($('.myclassselector'), true);
                                $.ajax({
                                    url: "/survey/edit",
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
                                        } else {
                                            window.popupNotification.show(response.message, "error");
                                        }
                                    },
                                    error: function (response) {
                                        kendo.ui.progress($('.myclassselector'), false);
                                        window.popupNotification.show(response.responseText, "error");
                                    }
                                });
                            }
                        } else {
                            kendo.ui.progress($('.myclassselector'), false);
                            window.popupNotification.show("Укажите данные для обновления / No data to edit", "error");
                        }
               
                },
                destroy: function (e) {
                    if (e && e.data) {
                        kendo.ui.progress($('.myclassselector'), true);
                        var deleteDate = e.data;
                        if (deleteDate && deleteDate.SurveyId) {
                            $.ajax({
                                url: "/survey/remove",
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify({ id: deleteDate.SurveyId }),
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
                                    $("#gridSurveys").data("kendoGrid").dataSource.read();
                                    $("#gridSurveys").data("kendoGrid").refresh();
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
                    $("#gridSurveys").data("kendoGrid").dataSource.read();
                    $("#gridSurveys").data("kendoGrid").refresh();
                }
            },
            batch: false,
            schema: {
                total: "data.length",
                data: "data",
                model: {
                    id: "SurveyId",
                    fields:
                    {
                        SurveyId: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 1, max: 38 }
                        },
                        SurveyCode: {
                            type: "number",
                            format: "0:#",
                            validation: { required: false, min: 1 },
                            defaultValue: 0,
                            editable: false
                        },
                        NameRus: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 3, max: 4000 }
                        },
                        NameEng: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 3, max: 4000 }
                        },
                        PurposeRus: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 3, max: 4000 }
                        },
                        PurposeEng: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 3, max: 4000 }
                        },
                        TimeEstimateMin: {
                            type: "number",
                            validation: { required: true, min: 1 },
                            defaultValue: 10
                        },
                        IsRandomQuestions: {
                            type: "boolean",
                            editable: true,
                            defaultValue: true
                        },
                        IsAnonymous: {
                            type: "boolean",
                            editable: true,
                            defaultValue: true
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
                        SurveyTypeId: {
                            type: "string",
                            editable: true,
                            validation: { required: true }
                        },
                        SurveyType:
                        {
                            NameRus: {
                                type: "string",
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
            e.container.find("label[for=SurveyId]").parent("div .k-edit-label").hide();
            e.container.find("label[for=SurveyId]").parent().next("div .k-edit-field").hide();
            e.container.find("label[for=SurveyCode]").parent("div .k-edit-label").hide();
            e.container.find("label[for=SurveyCode]").parent().next("div .k-edit-field").hide();
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
            $("#gridSurveys").data("kendoGrid").dataSource.read();
            $("#gridSurveys").data("kendoGrid").refresh();
        },
        dataBound: onDataBoundSurveys,
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
    var timeout = null;
    $("#txtSearchSurvey").keyup(function () {
        var value = $(this).val();
        var grid = $("#gridSurveys").data("kendoGrid");
        if (grid) {
            if (value && value.length >= 1) {
                // Clear the timeout if it has already been set.
                clearTimeout(timeout);
                // Make a new timeout set to go off in 800ms
                timeout = setTimeout(function () {
                    grid.dataSource.read({ searchtxt: value });
                    grid.refresh();
                }, 300);
                $("#clearSearchSurvey").show();
            }
            else if (!value) {
                grid.dataSource.read();
                grid.refresh();
            }
        }
    });

    $("#clearSearchSurvey").click(function (e) {
        e.preventDefault();
        $("#txtSearchSurvey").val(null);
        var grid = $("#gridSurveys").data("kendoGrid");
        if (grid) {
            grid.dataSource.read();
            grid.refresh();
        }
        $("#clearSearchSurvey").hide();
    });
    $("#gridSurveys").kendoTooltip({
        filter: ".k-grid-edit",
        position: 'bottom',
        showAfter: 1,
        offset: 10,
        content: function (e) {
            return "🔨Изменить / Edit";
        }
    });
    $("#gridSurveys").kendoTooltip({
        filter: ".k-grid-delete",
        position: 'bottom',
        showAfter: 1,
        offset: 10,
        content: function (e) {
            return "❌Удалить / Delete";
        }
    });
    $("#gridSurveys").kendoTooltip({
        filter: ".k-grid-invite",
        position: 'bottom',
        showAfter: 1,
        offset: 10,
        content: function (e) {
            return "📧Пригласить / Invite";
        }
    });

    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.74;
    $("#gridSurveys").height(windowHeight);
});

function clearSearch() {
    var grid = $("#gridSurveys").data("kendoGrid");
    if (grid) {
        $("#dateStart").data("kendoDatePicker").value(null);
        $("#dateEnd").data("kendoDatePicker").value(null);
        $("#txtSearchSurvey").val(null);
        $("#clearSearchSurvey").hide();
        grid.dataSource.read();
        grid.refresh();
    }
}