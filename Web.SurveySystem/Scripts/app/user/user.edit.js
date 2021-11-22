"use strict";
$(document).ready(function () {
    if (window.UserId) {
        var rolesEdit = $("#selRoles").kendoMultiSelect({
            placeholder: "Выберите роль / Select role",
            dataTextField: "Name",
            dataValueField: "Id",
            maxSelectedItems: 3,
            autoBind: true,
            dataSource: {
                transport: {
                    read: function (options) {
                        $.ajax({
                            url: "/roles/all",
                            dataType: "json",
                            type: "GET",
                            cache: false,
                            contentType: "application/json; charset=utf-8",
                            success: function (result) {
                                if (result && result.data) {
                                    options.success(result);
                                } else {
                                    window.popupNotification.show("Роль не найдена", "error");
                                }
                            },
                            error: function (response) {
                                window.popupNotification.show("Роль не найдена", "error");
                            }
                        });
                    }
                },
                schema: {
                    data: "data"
                }
            }
        }).data("kendoMultiSelect");

        kendo.ui.progress($('.myclassselector'), true);
        $.ajax({
            url: "/users/user",
            dataType: "json",
            data: { id: window.UserId },
            type: "GET",
            traditional: true,
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (result) {
                if (result && result.success) {
                    //Employee
                    var employee = $("#employeeName");
                    if (employee && result.data.Name) {
                        employee.val(result.data.Name);
                    }
                    //Email
                    var email = $("#employeeEmail");
                    if (email && result.data.Email) {
                        email.val(result.data.Email);
                    }
                    if (result.data && result.data.Roles && result.data.Roles.length > 0) {
                        var msR = $("#selRoles").data("kendoMultiSelect");
                        if (msR) {
                            msR.value(result.data.Roles);
                            if (window.permissions.View) {
                                msR.enable(false);
                            }
                        }
                    }
                }
                kendo.ui.progress($('.myclassselector'), false);
            },
            error: function (response) {
                kendo.ui.progress($('.myclassselector'), false);
                window.popupNotification.show("Ошибка в запросе / Error get data", "error");
            }
        });

        $("#saveUserBt").click(function (e) {
            e.preventDefault();
            $("#winModUserEdit").kendoWindow({
                title: false,
                width: 510,
                modal: true
            }).data("kendoWindow").open().center().content("<div style='color: #0370ce;font-weight: bold;text-align:center;'>Идет загрузка ... / Loading ...</div>");
            var emp = $("#employeeName");
            if (emp && emp.val()) {
                // debugger;
                var email = $("#employeeEmail");
                if (email && email.val()) {
                    var roles = [];
                    if (rolesEdit && rolesEdit.value()) {
                        $.each(rolesEdit.value(), function (index, value) {
                            roles.push({ Id: value });
                        });
                        if (roles.length < 1) {
                            window.popupNotification.show("Выберите роль / Select role", "error");
                            $("#winModUserEdit").data("kendoWindow").close();
                            e.preventDefault();
                            return false;
                        }
                    }
                    if (window.UserId) {
                        var request =
                        {
                            Id: window.UserId,
                            Name: emp.val(),
                            Email: email.val(),
                            Roles: roles
                        };
                        $.ajax({
                            url: "/users/save",
                            dataType: "json",
                            type: "POST",
                            traditional: true,
                            contentType: "application/json; charset=utf-8",
                            data: JSON.stringify({ model: request }),
                            success: function (response) {
                                if (response && response.success) {
                                    $("#winModUserEdit").data("kendoWindow").close();
                                    window.popupNotification.show(response.message, "success");
                                    window.location.href = "/users/index";
                                }
                                //если ошибка
                                else {
                                    $("#winModUserEdit").data("kendoWindow").content("<div class='col-xs-12' style='color: red;font-weight: bold;text-align:center;'>" + response.message + " <br/><small style='color:blue;'>Окно закроется автоматически, через 3 сек.</small></div> ");
                                    setTimeout(function () { $("#winModUserEdit").data("kendoWindow").close(); }, 3000);
                                }
                            },
                            error: function (response) {
                                $("#winModUserEdit").data("kendoWindow").content("<div class='col-xs-12' style='color: red;font-weight: bold;text-align:center;'>" + response.message + " <br/><small style='color:blue;'>Окно закроется автоматически, через 3 сек.</small></div> ");
                                setTimeout(function () { $("#winModUserEdit").data("kendoWindow").close(); }, 3000);
                            }
                        });
                    } else {
                        $("#winModUserEdit").data("kendoWindow").close();
                        window.popupNotification.show("UserId invalid", "error");
                    }

                } else {
                    $("#winModUserEdit").data("kendoWindow").close();
                    window.popupNotification.show("Введите Email / Enter Email", "error");
                }
            } else {
                $("#winModUserEdit").data("kendoWindow").close();
                window.popupNotification.show("Введите данные сотрудника / Enter employee details", "error");
            }
        });
    } else {
        $("#divCard").hide();
    }
});