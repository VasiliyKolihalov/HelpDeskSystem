import {gatewayUrl, isUserLogin, login, validateEmail, validatePassword} from "./utils.js"

$(function () {
    if (isUserLogin()) {
        window.location.href = "index.html"
    }

    $("#RegisterCLick").click(function () {
        let messageElement = $("#Message")
        messageElement.html("")

        let firstNameElement = $("#FirstName")
        let firstName = firstNameElement.val()
        if (firstName === "") {
            messageElement.html("FirstName is required")
            return
        }
        let lastNameElement = $("#LastName")
        let lastName = lastNameElement.val()
        if (lastName === "") {
            messageElement.html("LastName is required")
            return
        }
        let emailElement = $("#Email")
        let email = emailElement.val()
        if (!validateEmail(email)) {
            messageElement.html("Wrong email")
            return
        }
        let passwordElement = $("#Password")
        let password = passwordElement.val()
        if (!validatePassword(password)) {
            messageElement.html("Weak password")
            return
        }

        $.ajax({
            type: "POST",
            url: `${gatewayUrl()}/accounts/register`,
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify({
                firstName: firstName,
                lastName: lastName,
                email: email,
                password: password
            }),
            error: function (request) {
                firstNameElement.val("");
                lastNameElement.val("");
                emailElement.val("");
                passwordElement.val("");
                let errorMessage;
                switch (request.status) {
                    case 0:
                        errorMessage = "Fail connect to server. Try later"
                        break
                    case 400 :
                        errorMessage = "User with this email already exists"
                        break
                }
                messageElement.html(errorMessage)
            },
            success: function (token) {
                login(token)
            }
        });
    })
})
