'use strict';
function getQuestions(id) {
    if (!IsValidGuid(id))
        return "";
    if (questionsDs && questionsDs.data()) {
        for (var step = 0; step < questionsDs.view().length; step++) {
            if (questionsDs.view()[step].QuestionId === id) 
                return questionsDs.view()[step].QuestionRus.concat(' / ', questionsDs.view()[step].QuestionEng);
        }
    }
    return "";
}

function getQuestionGroup(id) {
    if (!IsValidGuid(id))
        return "";
    if (questionsDs && questionsDs.data()) {
        for (var step = 0; step < questionsDs.view().length; step++) {
            if (questionsDs.view()[step].QuestionId === id)
                return questionsDs.view()[step].Group;
        }
    }
    return "";
}

function getAnswers(idA) {
    if (!IsValidGuid(idA))
        return "";
    if (answersDs && answersDs.data()) {
        
        for (var step = 0; step < answersDs.view().length; step++) {
            if (answersDs.view()[step].AnswerId === idA) {
                creditSum += answersDs.view()[step].Credit;
                return answersDs.view()[step].Credit;
            }
        }
    }
    return "";
}

var creditSum = 0;
var questionsDs, answersDs;
$(document).ready(function () {
    
    if (window.InvitationId && window.SurveyCode) {
        testRefresh(window.InvitationId);

        questionsDs = new kendo.data.DataSource({
            serverFiltering: true,
            transport: {
                read: function (options) {
                    $.ajax({
                        url: "/question/bytest",
                        dataType: "json",
                        type: "GET",
                        async: false,
                        cache: true,
                        data: { codeSurvey: window.SurveyCode },
                        contentType: "application/json; charset=utf-8",
                        success: function (result) {
                            if (result && result.success) {
                                options.success(result);
                            }
                            else {
                                window.popupNotification.show("Вопросы не найдены / Questions not found", "error");
                                options.error();
                            }
                        },
                        error: function (response) {
                            window.popupNotification.show("Вопросы не найдены / Questions not found", "error");
                            options.error();
                        }
                    });
                }
            },
            schema: {
                data: "data"
            }
        });
        questionsDs.read();


        answersDs = new kendo.data.DataSource({
            serverFiltering: true,
            transport: {
                read: function (options) {
                    var modelsFilter = [];
                    modelsFilter.push({ Field: "SurveyId", Value: window.SurveyId });
                    $.ajax({
                        url: "/answer/active",
                        dataType: "json",
                        type: "POST",
                        async: false,
                        cache: true,
                        data: JSON.stringify({ filterModels: modelsFilter }),
                        contentType: "application/json; charset=utf-8",
                        success: function (result) {
                            if (result && result.success) {
                                options.success(result);
                            }
                            else {
                                window.popupNotification.show("Ответы не найдены / Answers not found", "error");
                                options.error();
                            }
                        },
                        error: function (response) {
                            window.popupNotification.show("Ответы не найдены / Answers not found", "error");
                            options.error();
                        }
                    });
                }
            },
            schema: {
                data: "data"
            }
        });
        answersDs.read();
    }

    function createColumns() {
        var cols = [
            {
                field: "QuestionId",
                title: "Номер<br/>#",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: "#= getQuestionGroup(QuestionId) #",
                format: "{0:n0}",
                width: "5%",
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                filterable: false
            },
            {
                field: "UserAnswerId",
                title: "Вопрос (рус.)<br/>Question (rus.)",
                attributes: { style: 'vertical-align: middle; text-align:left;font-size: 11pt;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false,
                hidden: true
            },
            {
                field: "QuestionId",
                title: "Вопрос <br/>Question",
                width: "40%",
                attributes: { style: 'vertical-align: middle; text-align:left;font-size: 11pt;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false,
               template: "#= getQuestions(QuestionId) #"
            },
            //{
            //    field: "AnswerId",
            //    title: "Ответ<br/>Answer",
            //    width: "25%",
            //    attributes: { style: 'vertical-align: middle; text-align:left;font-size: 11pt;' },
            //    headerAttributes: {
            //        style: 'vertical-align: top;text-align: center;font-size: 11pt;'
            //    },
            //    sortable: false
            //},
            {
                field: "UserAnswerText",
                title: "Ответ пользователя<br/>UserAnswerText",
                width: "25%",
                headerAttributes: { style: 'vertical-align: top;text-align: center;font-size: 11pt;' },
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                filterable: false
            },
            {
                field: "IsValid",
                title: "Верно<br/>Is Valid",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template:
                    '<input type="checkbox" #=IsValid ? "checked=checked" : "" # disabled="disabled"></input>',
                width: "10%",
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;font-size: 11pt;'
                },
                sortable: false
            },
            {
                field: "Credit",
                title: "Балл<br/>Credit",
                width: "8%",
                headerAttributes: { style: 'vertical-align: top;text-align: center;font-size: 11pt;' },
                attributes: { style: 'vertical-align: middle; text-align:center;font-size: 11pt;' },
                filterable: false,
                template: "#= getAnswers(AnswerId) #"
            }
        ];
        return cols;
    }

    $("#gridUserAnswers").kendoGrid({
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
                    modelsFilter.push({ Field: "InvitationId", Value: window.InvitationId });
                    $.ajax({
                        url: "/useranswer/active",
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
                                $('#lblSum').text(creditSum);
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
                    id: "UserAnswerId",
                    fields:
                    {
                        UserAnswerId: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 1, max: 38 }
                        },
                        QuestionId: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 1, max: 38 }
                        },
                        AnswerId: {
                            type: "string",
                            editable: false,
                            validation: { required: true, min: 1, max: 38 }
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

    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.6;
    $("#gridUserAnswers").height(windowHeight);

});
function testRefresh(codeId) {
    if (codeId) {
        var headerTemplate = kendo.template($("#headerStartSurvey").html());
        kendo.ui.progress($('.myclassselector'), true);
        $.ajax({
            url: "/invitation/invitation",
            dataType: "json",
            data: { id: codeId },
            type: "GET",
            traditional: true,
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function(result) {
                if (result && result.success && result.success === true) {
                 var res = headerTemplate(result.data);
                    $("#contentStartUser").html(res);
                }
                kendo.ui.progress($('.myclassselector'), false);
            },
            error: function(response) {
                kendo.ui.progress($('.myclassselector'), false);
                window.popupNotification.show("Ошибка в запросе / Error get data", "error");
            }
        });
    } else {
        window.popupNotification.show("Не указан индентификатор теста / Invalid test ID", "error");
    }
}