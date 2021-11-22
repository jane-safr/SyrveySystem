'use strict';
function closeWinModalEdT() {
    try {
        var kendoWindow = $("#winModEditT").data("kendoWindow").close();
    }
    catch (err) { window.popupNotification.show(err.message, "error"); }
}
$(document).ready(function () {
    function viewFile(e) {
        if (e) {
            e.preventDefault();
            var isRus = true; // rus default
            var dataItemFile = this.dataItem($(e.currentTarget).closest("tr"));
            if (dataItemFile && dataItemFile.InstructionId) {
                if (e.data.commandName === "DownloadFRus") {
                    isRus = true;
                }
                else if (e.data.commandName === "DownloadFEng") {
                    isRus = false;
                } else {
                    window.popupNotification.show("Выберите инструкцию / Select instruction", "error");
                    e.preventDefault();
                    return false;
                }
                kendo.ui.progress($('.myclassselector'), true);
                var url = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + "/instructions/download/" + dataItemFile.InstructionId + "/" + isRus;
                window.open(url, '_blank');
                kendo.ui.progress($('.myclassselector'), false);
            } else {
                window.popupNotification.show("Инструкция не выбрана / Instruction not selected", "error");
            }
        }
    }

    function deleteFile(e) {
        if (window.permissions.View) {
            window.popupNotification.show("Удаление невозможно / Delete error", "error");
            return false;
        }
        if (e) {
            e.preventDefault();
            var isRus = true; // rus default
            if (confirm("Удалить документ? / Delete this item?")) {
                var dataItemFile = this.dataItem($(e.currentTarget).closest("tr"));
                if (e.data.commandName === "DeleteFEng") {
                    isRus = false;
                }
                else if (e.data.commandName === "DeleteFRus") {
                    isRus = true;
                }
                else {
                    window.popupNotification.show("Выберите инструкцию / Select instruction", "error");
                    e.preventDefault();
                    return false;
                }
                if (dataItemFile && dataItemFile.InstructionId && IsValidGuid(dataItemFile.InstructionId)) {
                    kendo.ui.progress($('.myclassselector'), true);
                    $.ajax({
                        url: "/instructions/fileremove",
                        dataType: "json",
                        type: "POST",
                        data: JSON.stringify({ instructionId: dataItemFile.InstructionId, isRus: isRus }),
                        traditional: true,
                        cache: false,
                        contentType: "application/json; charset=utf-8",
                        success: function (result) {
                            kendo.ui.progress($('.myclassselector'), false);
                            if (result && result.success && result.success === true) {
                                //Grid Document Read
                                $("#gridInstructions").data("kendoGrid").dataSource.read();
                                $("#gridInstructions").data("kendoGrid").refresh();
                                window.popupNotification.show(result.message, "success");
                            } else {
                                window.popupNotification.show(result.message, "error");
                            }
                        },
                        error: function (response) {
                            window.popupNotification.show("Ошибка при запросе / Error request", "error");
                        }
                    });
                }
                else {
                    window.popupNotification.show("Инструкция не выбрана / Instruction not selected", "error");
                }
            }
        }
    }

    function uploadFile(e) {
        if (e) {
            e.preventDefault();
            var isRus = true; // rus default
            var dataItemFile = this.dataItem($(e.currentTarget).closest("tr"));
            if (e.data.commandName === "UploadFEng") {
                isRus = false;
            }
            else if (e.data.commandName === "UploadFRus") {
                isRus = true;
            } else {
                window.popupNotification.show("Выберите инструкцию  / Select instruction", "error");
                e.preventDefault();
                return false;
            }
            if (dataItemFile && dataItemFile.InstructionId) {
                window.InstructionId = dataItemFile.InstructionId;
                $("#winModEditT").kendoWindow({
                    title: false,
                    width: 600,
                    modal: true,
                    content: {
                        template: $("#templateWinModalUploadFile").html()
                    }
                }).data("kendoWindow").open().center();

                $("#files").kendoUpload({
                    async: {
                        saveUrl: "/instructions/fileupload",
                        autoUpload: true,
                        multiple: false,
                        concurrent: true,
                        batch: false
                    },
                    multiple: false,
                    localization: {
                        select: "📂 Выбрать файл / Select file"
                    },
                    validation: {
                        maxFileSize: 4194304,
                        allowedExtensions: [".pdf", ".doc", ".docx"]
                    },
                    success: function (e) {
                        if (e) {
                            if (e.response && e.response.success) {
                                window.popupNotification.show(e.response.message, "success");
                                closeWinModalEdT();
                                $("#gridInstructions").data("kendoGrid").dataSource.read();
                                $("#gridInstructions").data("kendoGrid").refresh();
                            }
                            else {
                                if (e.response.errorText) {
                                    window.popupNotification.show(e.response.errorText, "error");
                                    $(".k-upload-files.k-reset").find("li").remove();
                                    $("#gridInstructions").data("kendoGrid").dataSource.read();
                                    $("#gridInstructions").data("kendoGrid").refresh();
                                }
                            }
                            e.preventDefault();
                        }
                    },
                    error: function (e) {
                        if (e) {
                            kendo.ui.progress($('.myclassselector'), false);
                            if (e.XMLHttpRequest.responseText) {
                                window.popupNotification.show(e.XMLHttpRequest.responseText, "error");
                            }
                            $(".k-upload-files.k-reset").find("li").remove();
                            e.preventDefault();
                        }
                    },
                    upload: function (e) {
                        if (e) {
                            if (!window.InstructionId) {
                                window.popupNotification.show("Выберите инструкцию / Select instruction", "error");
                                e.preventDefault();
                                return false;
                            }
                            var instructionId = window.InstructionId;
                            e.data = {
                                InstructionId: instructionId,
                                isRus: isRus
                            };
                        }
                    },
                    remove: function (e) {
                        window.popupNotification.show("Ошибка при удалении / Error remove file", "error");
                        e.preventDefault();
                    }
                });
                $(".k-upload-button").removeClass("k-button").addClass("btn btn-primary");
            }
            else {
                window.popupNotification.show("Инструкция не выбрана / Instruction not selected", "error");
            }
        }
    }

    function createColumns() {
        var colsFull = [];
        colsFull.push({
            field: "NameRus",
            title: "Наименование (рус.)<br/>Instruction Name (rus)",
            attributes: { style: 'vertical-align: middle; text-align:left;' },
            width: 80,
            editor: textareaEditorRusAndEng,
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            },
            filterable: false
        });
        colsFull.push({
            field: "NameEng",
            title: "Наименование (англ.)<br/>Instruction Name (eng)",
            editor: textareaEditorEn,
            width: 80,
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            },
            filterable: false
        });
        colsFull.push({
            command: [
                {
                    name: "UploadFRus",
                    text: ' Загрузить / Upload ',
                    className: "btn btn-primary btn-block  btn-sm  mr-1",
                    click: uploadFile,
                    iconClass: "fa fa-upload"
                },
                {
                    name: "DownloadFRus",
                    text: ' Скачать / Download ',
                    className: "btn btn-success  mr-1 btn-sm ",
                    click: viewFile,
                    iconClass: "fa fa-download"
                },
                {
                    name: "DeleteFRus",
                    text: ' ',
                    className: "btn btn-danger  mr-1 btn-sm ",
                    click: deleteFile,
                    iconClass: "fa fa-trash"
                }
            ],
            width: 47,
            title: "Файл (рус.)<br/>File (rus)",
            attributes: { style: 'vertical-align: middle; text-align:left;' },
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            }
        });
        colsFull.push({
            command: [
                {
                    name: "UploadFEng",
                    text: ' Загрузить / Upload',
                    className: "btn btn-primary btn-block  btn-sm  mr-1",
                    click: uploadFile,
                    iconClass: "fa fa-upload"
                },
                {
                    name: "DownloadFEng",
                    text: ' Скачать / Download',
                    className: "btn btn-success  mr-1 btn-sm ",
                    click: viewFile,
                    iconClass: "fa fa-download"
                },
                {
                    name: "DeleteFEng",
                    text: ' ',
                    className: "btn btn-danger  mr-1 btn-sm ",
                    click: deleteFile,
                    iconClass: "fa fa-trash"
                }
            ],
            width: 47,
            title: "Файл (англ.)<br/>File (eng)",
            attributes: { style: 'vertical-align: middle; text-align:left;' },
            headerAttributes: {
                style: 'vertical-align: top;text-align: center;'
            }
        });
        if (window.permissions.Edit) {
            colsFull.push({
                field: "IsActive",
                title: "Актив.<br/>Active",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsActive ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 20,
                headerAttributes: {
                    style: 'vertical-align:top;text-align: center;'
                },
                filterable: false
            });
            colsFull.push({
                field: "IsAdmin",
                title: "Админ.<br/>Admin",
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                template: '<input type="checkbox" #=IsAdmin ? "checked=checked" : "" # disabled="disabled"></input>',
                width: 20,
                headerAttributes: {
                    style: 'vertical-align:top;text-align: center;'
                },
                filterable: false
            });
            colsFull.push({
                field: "CreatedOn",
                title: "Создан<br/>Created On",
                width: 30,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                format: "{0: dd-MM-yyyy}",
                headerAttributes: { style: 'vertical-align: top;text-align: center;' },
                filterable: false
            });
            var comm = [];
            comm.push({
                name: "edit",
                text: " ",
                className: "btn btn-warning btn-sm mr-1",
                iconClass: "fa fa-wrench"
            });
            comm.push({
                name: "deleteR",
                click: markInstrInactive,
                text: " ",
                className: "btn btn-danger btn-sm mr-1",
                iconClass: "fa fa-times"
            });
            colsFull.push({
                command: comm,
                width: 25,
                attributes: { style: 'vertical-align: middle; text-align:center;' },
                headerAttributes: {
                    style: 'vertical-align: top;text-align: center;'
                },
                title: "Действия<br/>Actions"
            });
        }
        return colsFull;
    }

    function markInstrInactive(e) {
        if (window.permissions.View) {
            window.popupNotification.show("Удаление невозможно / Delete is not available", "error");
            return false;
        }
        if (e) {
            e.preventDefault();
            if (confirm("Удалить (сделать неактивной)? / Delete (mark instruction as inactive)?")) {
                var dataItemFile = this.dataItem($(e.currentTarget).closest("tr"));
                if (dataItemFile &&
                    dataItemFile.InstructionId &&
                    IsValidGuid(dataItemFile.InstructionId)) {
                    kendo.ui.progress($('.myclassselector'), true);
                    $.ajax({
                        url: "/instructions/remove",
                        dataType: "json",
                        type: "POST",
                        data: JSON.stringify({ instructionId: dataItemFile.InstructionId }),
                        traditional: true,
                        cache: false,
                        contentType: "application/json; charset=utf-8",
                        success: function (result) {
                            kendo.ui.progress($('.myclassselector'), false);
                            if (result && result.success && result.success === true) {
                                $("#gridInstructions").data("kendoGrid").dataSource.read();
                                $("#gridInstructions").data("kendoGrid").refresh();
                                window.popupNotification.show(result.message, "success");
                            } else {
                                window.popupNotification.show(result.message, "error");
                            }
                        },
                        error: function (response) {
                            window.popupNotification.show("Ошибка при запросе / Error request", "error");
                        }
                    });
                }
                else {
                    window.popupNotification.show("Удаление невозможно / Delete error", "error");
                    return false;
                }
            }
        }
    }

    function onDataBoundInstructions() {
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
            //fru
            if (IsValidGuid(item.UploadFileRusId) && window.permissions.View) {
                $(this).find('.k-grid-DownloadFRus').show();
                $(this).find('.k-grid-DeleteFRus').hide();
                $(this).find('.k-grid-UploadFRus').hide();
            }
            else if (!IsValidGuid(item.UploadFileRusId) && window.permissions.View) {
                $(this).find('.k-grid-DownloadFRus').hide();
                $(this).find('.k-grid-DeleteFRus').hide();
                $(this).find('.k-grid-UploadFRus').hide();
            }
            //a
            else if (IsValidGuid(item.UploadFileRusId) && window.permissions.Delete) {
                $(this).find('.k-grid-DownloadFRus').show();
                $(this).find('.k-grid-DeleteFRus').show();
                $(this).find('.k-grid-UploadFRus').hide();
            }
            else if (!IsValidGuid(item.UploadFileRusId) && window.permissions.Delete) {
                $(this).find('.k-grid-DownloadFRus').hide();
                $(this).find('.k-grid-DeleteFRus').hide();
                $(this).find('.k-grid-UploadFRus').show();
            }
            //engFile
            if (IsValidGuid(item.UploadFileEngId) && window.permissions.View) {
                $(this).find('.k-grid-DownloadFEng').show();
                $(this).find('.k-grid-DeleteFEng').hide();
                $(this).find('.k-grid-UploadFEng').hide();
            }
            else if (!IsValidGuid(item.UploadFileEngId) && window.permissions.View) {
                $(this).find('.k-grid-DownloadFEng').hide();
                $(this).find('.k-grid-DeleteFEng').hide();
                $(this).find('.k-grid-UploadFEng').hide();
            }
            else if (IsValidGuid(item.UploadFileEngId) && window.permissions.Delete) {
                $(this).find('.k-grid-DownloadFEng').show();
                $(this).find('.k-grid-DeleteFEng').show();
                $(this).find('.k-grid-UploadFEng').hide();
            }
            else if (!IsValidGuid(item.UploadFileEngId) && window.permissions.Delete) {
                $(this).find('.k-grid-DownloadFEng').hide();
                $(this).find('.k-grid-DeleteFEng').hide();
                $(this).find('.k-grid-UploadFEng').show();
            }

            if (item.IsActive) {
                row.addClass("table-success");
                $(this).find('.k-grid-deleteR').show();
            } else {
                row.addClass("table-danger");
                $(this).find('.k-grid-deleteR').show();
                $(this).find('.k-grid-UploadFRus').hide();
                $(this).find('.k-grid-UploadFEng').hide();
                $(this).find('.k-grid-DeleteFRus').hide();
                $(this).find('.k-grid-DeleteFEng').hide();
            }
        });
    }

    function createToolbar() {
        var toolbar = [];
        if (window.permissions.Add) {
            toolbar.push({
                name: "create",
                text: " Добавить / Add",
                className: "btn btn-success mr-1",
                iconClass: "fa fa-plus"
            });
        }
        return toolbar;
    }

    $("#gridInstructions").kendoGrid({
        toolbar: createToolbar(),
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
            serverSorting: false,
            serverFiltering: false,
            transport:
            {
                read: function (options) {
                    kendo.ui.progress($('.myclassselector'), true);
                    $.ajax({
                        url: "/instructions/all",
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
                create: function (e) {
                    if (e && e.data) {
                        var createDate = e.data;
                        if (createDate) {
                            $.ajax({
                                url: "/instructions/add",
                                dataType: "json",
                                type: "POST",
                                traditional: true,
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify(createDate),
                                success: function (response) {
                                    if (response && response.success) {
                                        e.success();
                                        window.popupNotification.show(response.message, "success");
                                        $("#gridInstructions").data("kendoGrid").dataSource.read();
                                    }
                                    else {
                                        window.popupNotification.show(response.message, "error");
                                        e.error();
                                    }
                                },
                                error: function (response) {
                                    window.popupNotification.show(response.responseText, "error");
                                    e.error();
                                }
                            });
                        }
                        else {
                            window.popupNotification.show("Error: Укажите данные для Создания", "error");
                        }
                    }
                },
                update: function (e) {
                    if (e && e.data) {
                        kendo.ui.progress($('.myclassselector'), true);
                        var updateDate = e.data;
                        if (updateDate) {
                            $.ajax({
                                url: "/instructions/edit",
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
                                    window.popupNotification.show("Ошибка при получении данных / Error getting data", "error");
                                }
                            });
                        }
                        else {
                            kendo.ui.progress($('.myclassselector'), false);
                            window.popupNotification.show("Укажите данные для обновления", "error");
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
            requestEnd: function (e) {
                if (e.type === "create" || e.type === "destroy" || e.type === "update") {
                    // Update the Yardage list
                    $("#gridInstructions").data("kendoGrid").dataSource.read();
                    $("#gridInstructions").data("kendoGrid").refresh();
                }
            },
            batch: false,
            schema: {
                total: "data.length",
                data: "data",
                model: {
                    id: "InstructionId",
                    fields:
                    {
                        InstructionId: {
                            type: "string",
                            editable: false,
                            defaultValue: ""
                        },
                        NameRus: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 2, max: 1000 }
                        },
                        NameEng: {
                            type: "string",
                            editable: true,
                            validation: { required: true, min: 2, max: 1000 }
                        },
                        UploadFileRusId: {
                            type: "string",
                            editable: false,
                            defaultValue: ""
                        },
                        UploadFileEngId: {
                            type: "string",
                            editable: false,
                            defaultValue: ""
                        },
                        IsAdmin: {
                            type: "boolean",
                            editable: true,
                            defaultValue: true
                        },
                        IsActive: {
                            type: "boolean",
                            editable: true,
                            defaultValue: true
                        },
                        Code: {
                            type: "int",
                            editable: false
                        },
                        CreatedOn: {
                            type: "date",
                            editable: false
                        },
                        CreatedBy: {
                            type: "string",
                            editable: false
                        }
                    }
                }
            }
        },
        dataBound: onDataBoundInstructions,
        selectable: "row",
        scrollable: true,
        filterable: false,
        columns: createColumns(),
        editable: {
            mode: "popup",
            window: {
                animation: false,
                width: 700,
                maxHeight: 950,
                open: function () {
                    this.center();
                }
            }
        },
        edit: function (e) {
            $(".k-grid-update").removeClass("k-button k-primary").addClass("btn btn-success btn-sm mr-1");
            $(".k-grid-cancel").removeClass("k-button").addClass("btn btn-danger btn-sm mr-1");
            e.preventDefault();
            $(".k-grid-update").removeClass("k-button k-primary").addClass("btn btn-success btn-sm mr-1");
            $(".k-grid-cancel").removeClass("k-button").addClass("btn btn-danger btn-sm mr-1");
            window.popupNotification.show("Загрузка.../ Loading...", "success");
            $(".k-window .k-grid-cancel").html("<span class=\"fa fa-times-circle\"></span> Отменить / Cancel");
            e.container.find("label[for=CreatedOn]").parent("div .k-edit-label").hide();
            e.container.find("label[for=CreatedOn]").parent().next("div .k-edit-field").hide();
            e.container.find("label[for=CreatedBy]").parent("div .k-edit-label").hide();
            e.container.find("label[for=CreatedBy]").parent().next("div .k-edit-field").hide();
            e.container.find("label[for=UploadFileRusId]").parent("div .k-edit-label").hide();
            e.container.find("label[for=UploadFileRusId]").parent().next("div .k-edit-field").hide();
            e.container.find("label[for=UploadFileEngId]").parent("div .k-edit-label").hide();
            e.container.find("label[for=UploadFileEngId]").parent().next("div .k-edit-field").hide();
            e.container.find('.k-checkbox').attr('title', '');
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
            $("#gridInstructions").data("kendoGrid").dataSource.read();
            $("#gridInstructions").data("kendoGrid").refresh();
        }
    });
    var windowHeight = $(window).height();
    windowHeight = windowHeight * 0.6;
    $("#gridInstructions .k-grid-content").height(windowHeight);

    //k-grid-Download
    $("#gridInstructions").kendoTooltip({
        filter: ".k-grid-DownloadFRus",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "📥Скачать файл / Download File";
        }
    });
    $("#gridInstructions").kendoTooltip({
        filter: ".k-grid-DownloadFEng",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "📥Скачать файл / Download File";
        }
    });
    // k-grid-DeleteT
    $("#gridInstructions").kendoTooltip({
        filter: ".k-grid-DeleteFRus",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "❌Удалить файл / Delete File";
        }
    });
    $("#gridInstructions").kendoTooltip({
        filter: ".k-grid-DeleteFEng",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "❌Удалить файл / Delete File";
        }
    });
    $("#gridInstructions").kendoTooltip({
        filter: ".k-grid-deleteR",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "❌Удалить / Delete";
        }
    });
    $("#gridInstructions").kendoTooltip({
        filter: ".k-grid-edit",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "🔨Изменить / Edit";
        }
    });
    $("#gridInstructions").kendoTooltip({
        filter: ".k-grid-UploadFRus",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "📥Загрузить / Upload";
        }
    });
    $("#gridInstructions").kendoTooltip({
        filter: ".k-grid-UploadFEng",
        position: 'bottom',
        showAfter: 1,
        widht: 45,
        offset: 10,
        content: function (e) {
            return "📥Загрузить / Upload";
        }
    });
});