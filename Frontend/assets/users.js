import {
    gatewayUrl,
    isUserLogin, logout,
    setJwtTokenBearer
} from "./utils.js"

$(function () {
    if (!isUserLogin()) {
        window.location.href = "login.html"
    }

    $("#logout").click(function () {
        logout()
    })

    showUsersList()
})

function showUsersList() {
    let contentElement = $("#Content")
    $.ajax({
        url: `${gatewayUrl()}/users`,
        type: "GET",
        beforeSend: function (xhr) {
            setJwtTokenBearer(xhr)
        },
    }).then(function (response) {
        for (let user of response) {
            let userElement = $(`<div class='border border-primary border-5 rounded w-25 mb-3 shadow p-2'> <p>${user.firstName} ${user.lastName}</p>  </div>`)
            userElement.click(function (){
                localStorage.currentUserId = user.id
                window.location.href = "user.html"
            })
            contentElement.append(userElement)
        }
    })
}
