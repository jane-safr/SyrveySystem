'use strict';
var answersId = '';
var quaerId = '';
var userAnswId = window.UserAnswerId;
var quaerType = '';
var invitationId = window.InvitationId;
var code = window.InvitationCode;
var multipleQuest;
var multipleAnswersId = {}; // dict: key-value (quest-answer) - UserAnswerId - Answer

function getUserAnswerIndex(code) {
    userAnswId = '';
    var headertemplate = kendo.template($("#headerAswersUser").html());
    if (QuestionId) {
        window.location.href = '/useranswer/index?invitationId=' + code;
        }
};

function getServerData(QuestionId) {
    debugger;
    var headertemplate = kendo.template($("#headerAswersUser").html());
    if (QuestionId) {
        $.ajax({
            url: "/question/question",
            dataType: "json",
            data: { questionId: QuestionId},
            type: "GET",
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (result) {
                if (result && result.success) {
                   
                    if (!Array.isArray(result.data)) {
                        //TODO

                        quaerId = result.data.QuestionId;
                        quaerType = result.data.QuestionType;
                        var res = headertemplate(result.data);
                        $("#contentAnswersUser").html(res);
                    } else {
                     //  quaerId = result.data[0].QuestionId;
                        quaerType = result.data[0].QuestionType;
                        multipleQuest = result.data;
                        var res = headertemplate(multipleQuest);
                        $("#contentAnswersUser").html(res);
                        console.log(invitationId);

                    }
                } else {
                   window.popupNotification.show("Ошибка получения вопросов / Error get questions", "error");
                }
            },
            error: function (response) {
                window.popupNotification.show(response.message, "error");
            }
        });
    }
}

function onclickRadioAnswerMult(questId, andswerId) {
    debugger;
    if (multipleQuest && questId && IsValidGuid(questId) && andswerId && IsValidGuid(andswerId)) { // если есть список вопросов
        if (multipleAnswersId.hasOwnProperty(questId)) {
            multipleAnswersId[questId] = andswerId;
        }
        else multipleAnswersId[questId] = andswerId;
    }
    if (multipleQuest.length === Object.keys(multipleAnswersId).length) {
        $("#btnNext").prop('disabled', false);
        $("#btnEnd").prop('disabled', false);
    }
};

function onclickRadioAnswer(_aId) {
    if (_aId && IsValidGuid(_aId)) {// если один вопрос
        answersId = _aId;
        $("#btnNext").prop('disabled', false);
        $("#btnEnd").prop('disabled', false);
    }
};

function textAreaInput(e) {
    if (e && e.length > 2) {
        $("#btnNext").prop('disabled', false);
        $("#btnEnd").prop('disabled', false);
    }
    else {
        $("#btnNext").prop('disabled', true);
        $("#btnEnd").prop('disabled', true);
    }
}

function btnNextQuestion(nextQuestionId) {
    kendo.ui.progress($(".chart-loading"), true);
   
    //Проверяем существует ли след вопрос
   
        //disabled button next
        $("#btnNext").attr('disabled', true);
        var win = $("#windialog").data("kendoWindow");
       // var _testId = SurveyCode;
        var _quaerId = quaerId;
    var _quaerType = quaerType;
    var _userAnswId = userAnswId;
        var _answersId;
        var _answersText;
        
        //Если тип вопроса выбор
        if (!_quaerType.IsFixedAnswer) {
            _answersId = answersId;
        }
        //Если тип вопроса текст, т.е. пользователь будет писать ответ
        else if (quaerType === 2) {
        _answersText = $("#textAnswer").val();
        _answersId = answersId;
    }

    if (multipleAnswersId && _quaerType.IsFixedAnswer && invitationId) { // группа вопросов
            var dataVM = [];
            Object.keys(multipleAnswersId).forEach(function (key) {
                //console.log(key, multipleAnswersId[key]);
                debugger;
                var data = {
                    QuestionId: key,
                    //UserAnswerId: multipleAnswersId[key],
                    AnswerId: multipleAnswersId[key],
                    AnswerText: _answersText,
                    IsActive: true,
                    InvitationId: invitationId
                };
                dataVM.push(data);
            });
            if (dataVM) {
                $.ajax({
                    url: "/useranswer/addmany",
                    dataType: "json",
                    data: JSON.stringify(dataVM),
                    type: "POST",
                    cache: false,
                    contentType: "application/json; charset=utf-8",
                    success: function (result) {
                       // debugger;
                        if (result && result.success) {
                            //При успешнос сохранении переходим к след. вопросу.
                            getUserAnswerIndex(code);
                            if (nextQuestionId) {
                                getServerData(nextQuestionId);
                            }
                            kendo.ui.progress($(".chart-loading"), false);
                            answersId = 0;
                            nextQuestionId = '';
                        }
                        else {
                            if (result.message) {
                                window.popupNotification.show(result.message, "error");
                                kendo.ui.progress($(".chart-loading"), false);
                            }
                        }
                    },
                    error: function (response) {
                        window.popupNotification.show(response.message, "error");
                        kendo.ui.progress($(".chart-loading"), false);
                    }
                });
            }
        }
  
        //отправка на сервер
    
    if (invitationId && code && _userAnswId && _quaerId && _quaerType) {
            var data = {
                QuestionId: _quaerId,
                UserAnswerId: _userAnswId,
                AnswerId: _answersId,
                UserAnswerText: $("#taUserAnswer").val(),
                IsActive: true,
                InvitationId: invitationId
            };
           // win.content($("#winModalAnswerAdd").html());
           // win.center().open();
            $.ajax({
                url: "/useranswer/addanswer",
                dataType: "json",
                data: JSON.stringify(data),
                type: "POST",
                cache: false,
                contentType: "application/json; charset=utf-8",
                success: function (result) {
                   // debugger;
                    if (result && result.success) {
                            //При успешнос сохранении на сервера переходим к след. вопросу.
                        getUserAnswerIndex(code);
                        if (nextQuestionId) {
                            getServerData(nextQuestionId);
                        }
                        kendo.ui.progress($(".chart-loading"), false);
                        answersId = 0;
                        nextQuestionId = '';
                    }
                    else {
                            kendo.ui.progress($(".chart-loading"), false);
                            window.popupNotification.show(result.message, "error");
                    }
                },
                error: function (response) {
                    kendo.ui.progress($(".chart-loading"), false);
                    window.popupNotification.show(response.message);
                }
            });
        }
    if (!nextQuestionId) {
        $.ajax({
            url: "/invitation/finish",
            dataType: "json",
            data: { id: code },
            type: "GET",
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function(result) {
                if (result && result.success) {
                   // window.location.href = '/invitation/index';
                    kendo.ui.progress($(".chart-loading"), false);
                } else {
                    kendo.ui.progress($(".chart-loading"), false);
                    window.popupNotification.show(result.message, "error");
                }

            }
        });
    }
    ////default Error
    //else {

    //    kendo.ui.progress($(".chart-loading"), false);
    //    alert("Неверный тип вопроса  / Invalid question type");

    //    return false;
    //}
}

$(document).ready(function () {

    var popupNotification = $("#popupNotification").kendoNotification(
        {
            position: {
                pinned: true,
                top: 30,
                right: 30
            }
        }
    ).data("kendoNotification");
    $("#windialog").kendoWindow({
        minWidth: 100,
        modal: true,
        scrollable: false,
        animation: {
            open: {
                duration: 100
            },
            close: {
                effects: "fade:out"
            }
        },
        width: 193,
        height: 193,
        title: false,
        visible: false
    });
    
    if (QuestionId) {
        getServerData(QuestionId);
    }
    else {
        var headertemplateError = kendo.template($("#headerAswersUserError").html());
        $("#contentAnswersUser").html(headertemplateError);
    }
});