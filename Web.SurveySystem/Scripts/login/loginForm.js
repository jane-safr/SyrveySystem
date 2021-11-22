"use strict";
function showValidate(input) {
    let thisAlert = $(input).parent();
    $(thisAlert).addClass('alert-validate');
}

function hideValidate(input) {
    let thisAlert = $(input).parent();
    $(thisAlert).removeClass('alert-validate');
}

function OnSuccess(result) {
    $("#errorMessage").addClass("d-none");
    if (result && result.success && result.redirectUrl)
    {
        window.location.href = result.redirectUrl;
    }
    else if (result && result.twofactor && result.message) {
        $("#divVCode").show();
        $("#verifyCode").focus();
        $("#btnSubmit").show();
        $.toast({
            heading: "<strong>" + result.message +"</strong>",
            showHideTransition: 'slide', // It can be plain, fade or slide
            bgColor: '#B7FFCC', // Background color for toast
            textColor: '#000000', // text color
            icon: 'info',
            allowToastClose: false, // Show the close button or not
            hideAfter: 15000, // `false` to make it sticky or time in miliseconds to hide after
            stack: 2, // `fakse` to show one stack at a time count showing the number of toasts that can be shown at once
            textAlign: 'left', // Alignment of text i.e. left, right, center
            position: 'top-right' // bottom-left or bottom-right or bottom-center or top-left or top-right or top-center or mid-center or an object representing the left, right, top, bottom values to position the toast on page
        });
    }
    else {
        $("#divVCode").hide();
        $("#verifyCode").val(null);
        $("#errorMessage").removeClass("d-none");
        $("#errorMessage").html(result.message);
        $("#btnSubmit").show();
        $("#providerLogin").show();
    }
    $('#Modal').modal('hide');
}
function OnFailure(data) {
    $('#Modal').modal('hide');
    $("#btnSubmit").show();
    $("#providerLogin").show();
    if (data && data.message)
        $("#errorMessage").html(data.message);
}
function onBegin() {
    $('#Modal').modal('show');
    $("#btnSubmit").hide();
    $("#providerLogin").hide();
}

$(document).ready(function ()
{
    function validate(input) {
        if ($(input).attr('type') === 'email' || $(input).attr('name') === 'email') {
            if ($(input).val().trim().match(/^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{1,5}|[0-9]{1,3})(\]?)$/) === null) {
                return false;
            }
        }
        else if ($(input).attr('type') === 'number') {
            if ($(input).val().trim().match(/^[0-9]{0,12}$/) === null) {
                return false;
            }
        }
        else {
            if ($(input).val().trim() === "") {
                return false;
            }
        }
        return true;
    }
    /*==========[ Focus input ]=============================*/
    $('.input100').each(function () {
        $(this).on('blur',
            function () {
                if ($(this).val().trim() !== "") {
                    $(this).addClass('has-val');
                } else {
                    $(this).removeClass('has-val');
                }
            });
    });
    /*===========[ Validate ]========================*/
    let input = $('.validate-input .input100');
    $('.validate-form').on('submit', function () {
        var check = true;
        for (let i = 0; i < input.length; i++) {
            if (validate(input[i]) === false) {
                showValidate(input[i]);
                check = false;
            }
        }
        return check;
    });

    $('.validate-form .input100').each(function () {
        $(this).focus(function () {
            hideValidate(this);
        });
    });


    /*=============[ Show pass ]================*/
    var showPass = 0;
    $('.btn-show-pass').on('click', function () {
        if (showPass === 0) {
            $("#password").attr('type', 'text');
            $(this).find('i').removeClass('zmdi-eye');
            $(this).find('i').addClass('zmdi-eye-off');
            showPass = 1;
        }
        else {
            $("#password").attr('type', 'password');
            $(this).find('i').addClass('zmdi-eye');
            $(this).find('i').removeClass('zmdi-eye-off');
            showPass = 0;
        }
    });
});