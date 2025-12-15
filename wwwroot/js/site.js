/// <reference path="../lib/jquery/dist/jquery.js" />
// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//-------all functions Common----------
function FormReset(frm) {
    $("#" + frm)[0].reset();
}
function SaveExpense() {
    $("#newExpenseForm").validate();

    if ($("#newExpenseForm").valid() === true) {

        var FormData = $("#newExpenseForm").serialize();
        $.post({
            url: "/ExpenseInfoes/Create",
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            data: FormData,
            success: function (result) {
                if (result.flag == 'y') {
                    FormReset('newExpenseForm');
                    toastr.success(result.msg);
                }
                else {
                    toastr.error(result.msg);
                }
            },
            error: function () {
                toastr.error(result.msg);
            }
        }
        );
    }
}

function UpdateExpense() {
    $("#frmUpdate").validate();

    if ($("#frmUpdate").valid() === true) {
        var FormData = $("#frmUpdate").serialize();
        $.post({
            url: "/Account/Edit",
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            data: FormData,
            success: function (result) {
                if (result.flag == 'y') {
                    toastr.success(result.msg);
                }
                else {
                    toastr.error(result.msg);
                }
            },
            error: function () {
                toastr.error(result.msg);
            }
        }
        );
    }
}