'use strict';
$(function () {
    kendo.ui.Window.fn._keydown = function (originalFn) {
        var KEY_ESC = 27;
        return function (e) {
            if (e.which !== KEY_ESC) {
                originalFn.call(this, e);
            }
        };
    }(kendo.ui.Window.fn._keydown);
}); 
$(document).ready(function () {
    kendo.culture().calendar.firstDay = 1;
    window.popupNotification = $("#popupNotificationEdit").kendoNotification(
        {
            allowHideAfter: 2000,
            position: $(window).width() < 700
                ?
                {
                    pinned: true,
                    top: 5
                }
                :
                {
                    pinned: true,
                    top: 50,
                    right: 180
                }
        }
    ).data("kendoNotification");
});

function createGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}  

function IsValidGuid(value) {
    if (value) {
        var validGuid = /^(\{|\()?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}(\}|\))?$/;
        var emptyGuid = /^(\{|\()?0{8}-(0{4}-){3}0{12}(\}|\))?$/;
        return validGuid.test(value) && !emptyGuid.test(value);
    }
    return false;
}

function IsGuidEmpty(value) {
    return value === null // NULL value
            || value === undefined // undefined
            || value === 'undefined' // undefined
            || value.length === 0 // Array is empty
            || value === '00000000-0000-0000-0000-000000000000' // Guid empty
        ;
}

function getDateDDMMYYYY(e) {
    if (e) {

        var currentDate = new Date(e);
        // year
        var yyyy = '' + currentDate.getFullYear();
        // month
        var mm = ('0' + (currentDate.getMonth() + 1)); // prepend 0 // +1 is because Jan is 0
        mm = mm.substr(mm.length - 2); // take last 2 chars
        // day
        var dd = ('0' + currentDate.getDate()); // prepend 0
        dd = dd.substr(dd.length - 2); // take last 2 chars
        return yyyy + "." + mm + "." + dd;
    }
}

function d2(n) {
    if (n < 9) return "0" + n;
    return n;
}
function dateToString(data) {
    if (data) {
        var sDate = d2(data.getDate()) + "." + d2(parseInt(data.getMonth() + 1)) + "." + data.getFullYear() + " " + d2(data.getHours()) + ":" + d2(data.getMinutes());
        return sDate;
    }
}
function inputPhoneNotRequerd(container, options) {
    $('<input type="tel" class="form-control" style="width: 85%;" minlength="0" onkeyup="onlyNumbers" maxlength="35" data-bind="value: ' + options.field + '" placeholder="Телефон / Phone"></input>')
        .appendTo(container);
}
function inputPhoneRequerd(container, options) {
    $('<input type="tel" class="form-control" required  style="width: 85%;" minlength="1" onkeyup="onlyNumbers" maxlength="35" data-bind="value: ' + options.field + '" placeholder="Телефон / Phone"></input>')
        .appendTo(container);
}
function inputEmailNotRequerd(container, options) {
    $('<input type="tel" class="form-control" style="width: 85%;" minlength="1" onkeyup="Latin(this);" maxlength="40" data-bind="value: ' + options.field + '" placeholder="Email"></input>')
        .appendTo(container);
}

function inputtextEngFirst(container, options) {
    $('<input type="text" class="form-control" style="width: 85%;" minlength="1" onkeyup="LatinUpB(this);" maxlength="1" data-bind="value: ' + options.field + '" placeholder="Только латинские буквы / Latin letters only"></input>')
        .appendTo(container);
}

function inputTextRusNoRequired(container, options) {
    $('<input type="text" class="form-control" style="width: 95%;" onkeyup="RusAndEng(this);" maxlength="500" data-bind="value: ' + options.field + '" placeholder="Введите текст / Enter text"></input>')
        .appendTo(container);
}

function inputTextEngNoRequired(container, options) {
    $('<input type="text" class="form-control" style="width: 95%;" onkeyup="Latin(this);" maxlength="500" data-bind="value: ' + options.field + '" placeholder="Только латинские буквы / Latin letters only"></input>')
        .appendTo(container);
}

function inputtextNumber(container, options) {
    $('<input type="text" class="form-control" required style="width: 85%;" minlength="15" onkeyup="onlyNumbers(this);" maxlength="15" data-bind="value: ' + options.field + '" placeholder="Только цифры / Only numbers"></input>')
        .appendTo(container);
}

function inputtextNumberNoRequerd(container, options) {
    $('<input type="text" class="form-control" style="width: 95%;" minlength="25" onkeyup="onlyNumbers(this);" maxlength="15" data-bind="value: ' + options.field + '" placeholder="Только цифры / Only numbers"></input>')
        .appendTo(container);
}

function inputtextNoRequerdInn(container, options) {
    $('<input type="text" class="form-control"  style="width: 85%;" minlength="15" onkeyup="Latin(this);" maxlength="25" data-bind="value: ' + options.field + '" placeholder="Только цифры / Only numbers"></input>')
        .appendTo(container);
}

function textareaEditorEn(container, options) {
    $('<textarea class="form-control" style="width: 95%;" minlength="1" onkeyup="Latin(this);" maxlength="3050" data-bind="value: ' + options.field + '"  rows="2" placeholder="Только латинские буквы / Latin letters only"></textarea>')
        .appendTo(container);
}
function textareaEditorRus(container, options) {
    $('<textarea class="form-control" required style="width: 95%;" minlength="1" onkeyup="Rus(this);" maxlength="3050" data-bind="value: ' + options.field + '"   rows="2" placeholder="Введите текст / Enter text"></textarea>')
        .appendTo(container);
}

function textareaEditorRusAndEng(container, options) {
    $('<textarea class="form-control" style="width: 95%;" onkeyup="RusAndEng(this);" maxlength="3050" data-bind="value: ' + options.field + '"  rows="2" placeholder="Введите текст / Enter text"></textarea>')
        .appendTo(container);
}
function textareaEditorRusEng(container, options) {
    $('<textarea class="form-control"  style="width: 95%;" onkeyup="RusAndEng(this);" maxlength="3050" data-bind="value: ' + options.field + '"   rows="2" placeholder="Введите текст / Enter text"></textarea>')
        .appendTo(container);
}

function textareaEditorAll(container, options) {
    $('<textarea class="form-control editor" style="width: 100%;" maxlength="5000" data-bind="value: ' + options.field + '"  rows="5" placeholder="Введите текст / Enter text"></textarea>')
        .appendTo(container);
}

function keyUpInputNoEng(e) {
    if (e) {
        e.value = e.value.toUpperCase();
        Latin(e);
    }
}

function onlyNumbers(e) {
    if (/^[0-9&\-+ ]*?$/.test(e.value))
        e.defaultValue = e.value;
    else
        e.value = e.defaultValue;
}

function Rus(obj) {
    if (/^[а-я-А-Я0-9A-ZЁёğüşöçİĞÜŞÖÇıIØ ,.\-:"/()_’'№*«»&+@–><]*?$/.test(obj.value))
        obj.defaultValue = obj.value;
    else
        obj.value = obj.defaultValue;
}

function RusAndEng(obj) {
    if (/^[а-я-А-Я0-9A-Za-zЁёğüşöçİĞÜŞÖÇıIØ ,.\-:"/()_«»=’'№*&+@–?<>]*?$/.test(obj.value))
        obj.defaultValue = obj.value;
    else
        obj.value = obj.defaultValue;
}

function Latin(obj) {
    if (/^[a-zA-Z0-9ЁёğüşöçİĞÜŞÖÇıIØ ,.\-:"()/*«»’№'_&+@–?]*?$/.test(obj.value))
        obj.defaultValue = obj.value;
    else
        obj.value = obj.defaultValue;
}

function LatinUpB(obj) {
    if (/^[a-zA-ZЁёğüşöçİĞÜŞÖÇıIØ]*?$/.test(obj.value))
        obj.defaultValue = obj.value;
    else
        obj.value = obj.defaultValue;
}

function CodeFormLatinUpper(obj) {
    if (obj) {
        obj.value = obj.value.toUpperCase();
        if (/^[a-zA-Z0-9ЁёğüşöçİĞÜŞÖÇıIØ\-/–]*?$/.test(obj.value))
            obj.defaultValue = obj.value;
        else
            obj.value = obj.defaultValue;
    }
}

var TRange = null;
function searchText() {
    var str = $("#searchTextValue").val();
    if (str) {
        if (parseInt(navigator.appVersion) < 4) return;
        var strFound;
        if (window.find) {
            // CODE FOR BROWSERS THAT SUPPORT window.find
            strFound = self.find(str);
            if (strFound && self.getSelection && !self.getSelection().anchorNode) {
                strFound = self.find(str);
            }
            if (!strFound) {
                strFound = self.find(str, 0, 1);
                while (self.find(str, 0, 1)) continue;
            }
        } else if (navigator.appName.indexOf("Microsoft") !== -1) {
            // EXPLORER-SPECIFIC CODE        
            if (TRange !== null) {
                TRange.collapse(false);
                strFound = TRange.findText(str);
                if (strFound) TRange.select();
            }
            if (TRange === null || strFound === 0);
            {
                TRange = self.document.body.createTextRange();
                strFound = TRange.findText(str);
                if (strFound) TRange.select();
            }
        } else if (navigator.appName === "Opera") {
            alert("Opera browsers not supported, sorry...");
            return;
        }
        // ReSharper disable once UsageOfPossiblyUnassignedValue
        if (!strFound)
            alert("String '" + str + "' not found!");
        return;

    } else {
        alert("Укажите параметры поиска / Specify search parameters");
    }
}
