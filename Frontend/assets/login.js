import {gatewayUrl, isUserLogin, login} from "./utils.js"

$(function () {
    if (isUserLogin()) {
        window.location.href = "index.html"
    }
    $("#LoginClick").click(function () {
        let messageElement = $("#Message")
        messageElement.html("")

        let emailElement = $("#Email")
        let email = emailElement.val()
        if (email === "") {
            messageElement.html("Email is required")
            return
        }
        let passwordElement = $("#Password")
        let password = passwordElement.val()
        if (password === "") {
            messageElement.html("Password is required")
            return
        }

        $.ajax({
            type: "POST",
            url: `${gatewayUrl()}/accounts/login`,
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify({
                email: email,
                password: password
            }),
            error: function (request) {
                emailElement.val("");
                passwordElement.val("");
                let errorMessage;
                switch (request.status) {
                    case 0:
                        errorMessage = "Fail connect to server. Try later"
                        break
                    case 400 :
                        errorMessage = "Wrong email or password"
                        break
                }
                messageElement.html(errorMessage)
            },
            success: function (token) {
                login(token)
            },
        });
    })
})
