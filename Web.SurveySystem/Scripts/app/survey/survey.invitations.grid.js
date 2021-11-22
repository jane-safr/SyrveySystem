'use strict';
function closeWinModalEdT() {
    try {
        var kendoWindow = $("#winModEditT").data("kendoWindow").close();
    }
    catch (err) { window.popupNotification.show(err.message, "error"); }
}

// начать тест
function startTest(e) {
    if (e) {
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

//Повторная отправка приглашения
function resendEmailRow(e) {
    if (e) {
        e.preventDefault();
        if (confirm("Отправить приглашение повторно? / Resend invitation?")) {
            var dataItemFile = this.dataItem($(e.currentTarget).closest("tr"));
            if (dataItemFile && dataItemFile.InvitationId) {
                kendo.ui.progress($('.myclassselector'), true);
                $.ajax({
                    url: "/invitation/resendemail",
                    dataType: "json",
                    type: "POST",
                    data: JSON.stringify({ invitationId: dataItemFile.InvitationId }),
                    traditional: true,
                    cache: false,
                    contentType: "application/json; charset=utf-8",
                    success: function (result) {
                        kendo.ui.progress($('.myclassselector'), false);
                        if (result && result.success && result.success === true) {
                            $("#gridInvitations").data("kendoGrid").dataSource.read();
                            $("#gridInvitations").data("kendoGrid").refresh();
                            window.popupNotification.show(result.message, "success");
                        } else {
                            window.popupNotification.show(result.message, "error");
                        }
                    },
                    error: function (response) {
                        kendo.ui.progress($('.myclassselector'), false);
                        window.popupNotification.show("Ошибка при запросе / Error requesting ", "error");
                    }
                });
            }
            else {
                window.popupNotification.show("Отправка невозможна / Cannot resend invitation", "error");
                return false;
            }
        }
    }
}

$(document).ready(function () {
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

    function createToolbarAdd() {
        var toolbar = [];
        if (window.permissions.Add) {
            toolbar.push({
                name: "invite",
                template: "<button class='btn btn-success' id='btnInvite'> <i class='fa fa-envelope mt-1 mr-1'></i> Пригласить / Invite </button>"
            });
        }
        return toolbar;
    }
    
    function createInvitationColumns() {
        var cols = [
            {
                field: "CreatedOn",
                title: "Дата создания<br/>Created on",
                width: 20,
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                format: "{0: dd-MM-yyyy HH:mm}",
                headerAttributes: { style: 'vertical-align: top;text-align: center;font-size: 11pt;' },
                filterable: false
            },
            {
                field: "UserName",
                title: "Ф.И.О. <br/>Employee name",
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                width: 35,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                filterable: false
            },
            {
                field: "UserEmail",
                title: "Эл. почта <br/>Email",
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                width: 25,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                filterable: false
            },
            {
                field: "DateEnd",
                title: "Дата окончания<br/>Date end",
                width: 20,
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                format: "{0: dd-MM-yyyy HH:mm}",
                headerAttributes: { style: 'vertical-align: top;text-align: center;font-size: 11pt;' },
                filterable: false
            },
            {
                field: "IsAccepted",
                title: "Начат<br/>Accepted",
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                template: '<input type="checkbox" #=IsAccepted ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 20,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false
            },
            {
                field: "IsFinished",
                title: "Завершен<br/>Finished",
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                template: '<input type="checkbox" #=IsFinished ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 20,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false
            },
            {
                field: "Percent",
                title: "%",
                template: "#=kendo.format('{0:p0}', Percent/100)#",
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                width: 15,
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                filterable: false
            },
        ];
        if (window.permissions.Edit) {
            var comm = [];
            comm.push({
                name: "answer",
                text: " ",
                className: "btn btn-success btn-sm mr-1",
                title: "Пройти тест / Answer",
                iconClass: "fa fa-arrow-circle-right",
                click: startTest
            });
            comm.push({
                name: "resend",
                text: " ",
                className: "btn btn-info btn-sm mr-1",
                title: "Отправить / Send",
                iconClass: "fa fa-envelope",
                click: resendEmailRow
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
                width: 20,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                title: "Действия</br>Actions"
            });
        }
        return cols;
    }

    function onDataBoundInvit() {
        $(".btn-success").removeClass("k-button");
        $(".btn-danger").removeClass("k-button");
        $(".btn-primary").removeClass("k-button");
        $(".btn-warning").removeClass("k-button");
        $(".btn-info").removeClass("k-button");
        var items = this._data;
        var tableRows = $(this.table).find("tr");
        tableRows.each(function(index) {
            var row = $(this);
            var item = items[index];
            if (item && !item.IsFinished && (item.DateEnd < new Date().setHours(0, 0, 0, 0)) && item.IsAccepted) {
                row.addClass("table-danger");
                $(this).find('.k-grid-answer').hide();
                $(this).find('.k-grid-resend').hide();
                $(this).find('.k-grid-delete').hide();
            }
            else if (item && !item.IsFinished && (item.DateEnd < new Date().setHours(0, 0, 0, 0))) {
                row.addClass("table-danger");
                $(this).find('.k-grid-answer').hide();
                $(this).find('.k-grid-resend').hide();
            }
            else if (item.IsFinished) {
                row.addClass("row_success");
                $(this).find('.k-grid-answer').hide();
                $(this).find('.k-grid-delete').hide();
                $(this).find('.k-grid-resend').hide();
            }
            else if (!item.IsFinished && item.IsAccepted) {
                row.addClass("row_warning");
                $(this).find('.k-grid-delete').hide();
            }
            if (!item.Survey.IsAnonymous && window.CurrentUserId!=item.UserId) {
                $(this).find('.k-grid-answer').hide();
            }
        });
    }

    $("#gridInvitations").kendoGrid({
        autoBind: true,
        batch: false,
        sortable: false,
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
                read: function(options) {
                    if (window.SurveyCode) {
                        kendo.ui.progress($('.myclassselector'), true);
                        $.ajax({
                            url: "/invitation/bytest",
                            dataType: "json",
                            type: "GET",
                            data: { codeSurvey: window.SurveyCode },
                            traditional: true,
                            cache: false,
                            contentType: "application/json; charset=utf-8",
                            success: function(result) {
                                kendo.ui.progress($('.myclassselector'), false);
                                if (result && result.success && result.success === true) {
                                    $("#countInvitations").text(result.data.length);
                                    options.success(result);
                                } else {
                                    window.popupNotification.show(result.message, "error");
                                }
                            },
                            error: function(response) {
                                kendo.ui.progress($('.myclassselector'), false);
                                window.popupNotification.show("Невозможно получить приглашения / Error get invitations",
                                    "error");
                            }
                        });
                    }
                },
                destroy: function (e) {
                    debugger;
                    if (e && e.data) {
                        kendo.ui.progress($('.myclassselector'), true);
                        var deleteDate = e.data;
                            if (deleteDate && deleteDate.InvitationId) {
                                $.ajax({
                                    url: "/invitation/remove",
                                    dataType: "json",
                                    type: "POST",
                                    contentType: "application/json; charset=utf-8",
                                    data: JSON.stringify({ id: deleteDate.InvitationId }),
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
                                        $("#gridInvitations").data("kendoGrid").dataSource.read();
                                        $("#gridInvitation").data("kendoGrid").refresh();
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
                    // Update the Yardage list
                    $("#gridInvitations").data("kendoGrid").dataSource.read();
                    $("#gridInvitations").data("kendoGrid").refresh();
                }
            },
            batch: false,
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
                            validation: { required: true, min: 1, max: 120 }
                        },
                        UserId: {
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
                        EmailText: {
                            type: "string",
                            editable: true
                        },
                        IsAccepted: {
                            type: "boolean",
                            editable: false,
                            defaultValue: true
                        },
                        IsFinished: {
                            type: "boolean",
                            editable: false,
                            defaultValue: true
                        },
                        DateEnd: {
                            type: "date",
                            editable: false
                        }
                    }
                }
            }
        },
        edit: function(e) {
            $(".k-grid-update").removeClass("k-button k-primary").addClass("btn btn-success btn-sm mr-1");
            $(".k-grid-cancel").removeClass("k-button").addClass("btn btn-danger btn-sm mr-1");
        },
        cancel: function (e) {
            debugger;
            e.preventDefault();
            $("#gridInvitations").data("kendoGrid").dataSource.read();
            $("#gridInvitations").data("kendoGrid").refresh();
        },
        dataBound: onDataBoundInvit,
        toolbar: createToolbarAdd(),
        selectable: "row",
        scrollable: true,
        filterable: false,
        columns: createInvitationColumns(),
        editable: "inline"
    });

    //$("#gridInvitations").find(".k-dropdown-wrap:last").width(35);
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.55;
    $("#gridInvitations").height(windowHeight);


    $("#gridInvitations").kendoTooltip({
        filter: ".k-grid-answer",
        position: 'bottom',
        showAfter: 1,
        offset: 10,
        content: function(e) {
            return "📝Пройти тест / Answer test";
        }
    });
    $("#gridInvitations").kendoTooltip({
        filter: ".k-grid-resend",
        position: 'bottom',
        showAfter: 1,
        offset: 10,
        content: function (e) {
            return "📧Отправить повторно / Resend invitation";
        }
    });
    $("#gridInvitations").kendoTooltip({
        filter: ".k-grid-delete",
        position: 'bottom',
        showAfter: 1,
        offset: 10,
        content: function(e) {
            return "❌Удалить приглашение / Delete invitation";
        }
    });
   

    $("#btnInvite").click(function (e) {
        e.preventDefault();
        if (window.permissions.Add && window.SurveyId) {
            $("#winModEditT").kendoWindow({
                position: { top: "10%", left: "10%"},
                title: false,
                width: "80%",
                modal: true,
                content: {
                    template: $("#winModalSendInvitation").html()
                }
            }).data("kendoWindow").open();


            var lblNum = $("#lblNumber");
            if (lblNum && window.SurveyCode) {
                lblNum.text(window.SurveyCode);
            }

            $("#dpTestEnd").kendoDateTimePicker({
                value: new Date(new Date().setHours(18, 0, 0, 0)),//, //new Date(new Date().getTime() + (86400000 * 2)),
                format: "dd.MM.yyyy HH:mm",
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
                        read: function(options) {
                            $.ajax({
                                url: "/users/find",
                                dataType: "json",
                                type: "POST",
                                //data: JSON.stringify({ searchtxt: options.data.searchtxt ? options.data.searchtxt : "" }),
                                data: JSON.stringify({
                                    searchtxt: $("#multiSelectUsers").data("kendoMultiSelect").input.val()
                                }),
                                cache: true,
                                contentType: "application/json; charset=utf-8",
                                success: function(result) {
                                    if (result && result.success && result.success === true) {
                                        if (result.data && result.data.length < 1) {
                                            window.popupNotification.show("Сотрудник не найден. Уточните параметры.",
                                                "error");
                                        }
                                        options.success(result);
                                        kendo.ui.progress($('.myclassselector'), false);
                                    } else {
                                        window.popupNotification.show("Сотрудник не найден. Уточните параметры.",
                                            "error");
                                        options.error();
                                        kendo.ui.progress($('.myclassselector'), false);
                                    }
                                },
                                error: function(response) {
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

            $("#sendInvitationBtn").click(function(e) {
                if (confirm('Отправить приглашение / Send invitations?')) {
                    if (e) {
                        if (!window.SurveyId || !window.SurveyCode ) {
                            window.popupNotification.show("Выберите тест / Select test", "error");
                            e.preventDefault();
                            return false;
                        }
                        var dateClose = $("#dpTestEnd").data("kendoDateTimePicker");
                        if (dateClose) {

                            var dateCloseTest = dateClose._value;
                            if (dateCloseTest) {
                                debugger;
                                var multiSelect = $("#multiSelectUsers").data("kendoMultiSelect");
                                if (multiSelect && multiSelect.dataItems() && multiSelect.dataItems().length > 0) {
                                    var multiSelectUser = multiSelect.dataItems();
                                    if (multiSelectUser && multiSelectUser.length > 0) {
                                        var saveModel = {
                                            SurveyId: window.SurveyId,
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
                                            success: function(response) {
                                                if (response && response.success) {
                                                    closeWinModalEdT();
                                                    window.TabActive = 2;
                                                    $("#gridInvitations").data("kendoGrid").dataSource.read();
                                                    $("#gridInvitations").data("kendoGrid").refresh();
                                                    window.popupNotification.show(response.message, "success");
                                                } else {
                                                    window.popupNotification.show(response.message, "error");
                                                }
                                            },
                                            error: function(response) {
                                               window.popupNotification.show(response.responseText, "error");
                                            }
                                        });
                                    }
                                } else {
                                    window.popupNotification.show(
                                        "Данные пользователей недействительны / Invalid users",
                                        "error");
                                }
                            } else {
                                window.popupNotification.show("Дата недействительна / Date end invalid", "error");
                            }
                        } else {
                            window.popupNotification.show("Дата не найдена / Date end not found", "error");
                        }
                    }
                }
            });
        }
    });
});