import {
    gatewayUrl,
    isUserAdmin,
    isUserLogin, logout, setJwtTokenBearer, validateEmail, validatePassword
} from "./utils.js" 

$(function () {
    if (!isUserLogin()) {
        window.location.href = "login.html" 
    }
    $("#LogoutClick").click(function () {
        logout()
    })

    $("#ShowChangeEmailModal").click(function () {
        $("#ChangeEmailModal").modal("show")
    })

    $("#ChangeEmailClick").click(function () {
        let messageElement = $("#ChangeEmailModalMessage") 
        messageElement.html("") 

        let emailElement = $("#NewEmail") 
        let email = emailElement.val() 
        if (!validateEmail(email)) {
            messageElement.html("Wrong email")
            return 
        }
        $.ajax({
            url: `${gatewayUrl()}/accounts/email/change`,
            contentType: "application/json;charset=utf-8",
            type: "PUT",
            data: JSON.stringify({
                newEmail: email
            }),
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr) 
            },
            success: function () {
                $("#ChangeEmailModal").modal("hide")
                showAccount()
            },
            error: function (request) {
                emailElement.val("") 
                let errorMessage 
                switch (request.status) {
                    case 0:
                        errorMessage = "Fail connect to server. Try later"
                        break
                    case 400 :
                        errorMessage = "User with this email already exists"
                        break
                }
                messageElement.html(errorMessage)
            }
        })
    })

    $("#HideChangeEmailModal").click(function () {
        $("#ChangeEmailModal").modal("hide")
    })

    $("#ShowConfirmEmailModal").click(function () {
        $.ajax({
            url: `${gatewayUrl()}/accounts/email/sendConfirmCode`,
            type: "POST",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr) 
            },
        });
        $("#ConfirmEmailModal").modal("show")
    })

    $("#ConfirmEmailClick").click(function () {
        let messageElement = $("#ConfirmEmailModalMessage");
        messageElement.html("");

        let codeElement = $("#ConfirmCode");
        let code = codeElement.val();
        if (code === "") {
            messageElement.html("Code is required")
            return;
        }

        $.ajax({
            url: `${gatewayUrl()}/accounts/email/confirm/${code}`,
            type: "PUT",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr);
            },
            error: function (request) {
                codeElement.val("");
                let errorMessage;
                switch (request.status) {
                    case 0:
                        errorMessage = "Fail connect to server. Try later"
                        break
                    case 400 :
                        errorMessage = "Wrong confirm code"
                        break
                }
                messageElement.html(errorMessage)
            },
            success: function () {
                $("#ConfirmEmailModal").modal("hide")
                showAccount();
            }
        })
    })

    $("#HideConfirmEmailModal").click(function () {
        $("#ConfirmEmailModal").modal("hide")
    })

    $("#ShowChangePasswordModal").click(function () {
        $("#ChangePasswordModal").modal("show")
    })

    $("#ChangePasswordClick").click(function () {
        let messageElement = $("#ChangePasswordModalMessage");
        messageElement.html("");

        let oldPasswordElement = $("#OldPassword");
        let oldPassword = oldPasswordElement.val();
        if (oldPassword === "") {
            messageElement.html("Old password is required")
            return;
        }

        let newPasswordElement = $("#NewPassword");
        let newPassword = newPasswordElement.val();
        if (!validatePassword(newPassword)) {
            messageElement.html("Weak password")
            return;
        }

        $.ajax({
            url: `${gatewayUrl()}/accounts/password/change`,
            contentType: "application/json;charset=utf-8",
            type: "PUT",
            data: JSON.stringify({
                currentPassword: oldPassword,
                newPassword: newPassword
            }),
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr);
            },
            error: function (request) {
                oldPasswordElement.val("");
                newPasswordElement.val("")
                let errorMessage;
                switch (request.status) {
                    case 0:
                        errorMessage = "Fail connect to server. Try later"
                        break
                    case 400 :
                        errorMessage = "Wrong new password"
                        break
                }
                messageElement.html(errorMessage)
            },
            success: function () {
                $("#ChangePasswordModal").modal("hide")
                showAccount();
            }
        })
    })

    $("#HideChangePasswordModal").click(function () {
        $("#ChangePasswordModal").modal("hide")
    })

    $("#ShowDeleteAccountModal").click(function () {
        $("#DeleteAccountModal").modal("show")
    })

    $("#DeleteAccountClick").click(function () {
        $.ajax({
            url: `${gatewayUrl()}/accounts`,
            contentType: "application/json;charset=utf-8",
            type: "DELETE",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr);
            },
        }).then(function () {
            logout();
        });
    })

    $("#HideDeleteAccountModal").click(function () {
        $("#DeleteAccountModal").modal("hide")
    })

    showAccount();
})

function showAccount() {
    if(isUserAdmin()){
        return;
    }
    let accountDetailsElement = $("#AccountDetails");
    accountDetailsElement.empty();

    let id;
    let isEmailConfirm;
    let email;
    $.ajax({
        url: `${gatewayUrl()}/accounts/my`,
        contentType: "application/json;charset=utf-8",
        type: "GET",
        beforeSend: function (xhr) {
            setJwtTokenBearer(xhr);
        },
        error: function (request) {
            switch (request.status) {
                case 401:
                    logout()
                    break
            }
        },
        success: function (account) {
            id = account.id;
            email = account.email;
            isEmailConfirm = account.isEmailConfirm;
        }
    }).then(function () {
        let firstName;
        let lastName;
        $.ajax({
            url: `${gatewayUrl()}/users/${id}`,
            contentType: "application/json;charset=utf-8",
            type: "GET",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr);
            },
            success: function (user) {
                firstName = user.firstName;
                lastName = user.lastName;
            }
        }).then(function () {
            accountDetailsElement.append(
                `<p>Firstname: ${firstName}</p> ` +
                `<p>LastName: ${lastName}</p> ` +
                `<p>Email: ${email}</p> ` +
                `<p>EmailConfirm: ${isEmailConfirm}</p> `);

            if(isEmailConfirm === true){
                $("#ShowConfirmEmailModal").prop('disabled', true);
            }
            else {
                $("#ShowConfirmEmailModal").prop('disabled', false);
            }
        })
    });
}

