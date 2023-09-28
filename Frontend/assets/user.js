import {
    gatewayUrl,
    isExists, isUserAdmin, isUserAgent,
    logout,
    setJwtTokenBearer
} from "./utils.js"

$(document).ready(function () {
    if (!isExists(localStorage.token)) {
        window.location.href = "login.html"
    }

    if (!isExists(localStorage.currentUserId)) {
        window.location.href = "user.html"
    }

    $("#logout").click(function () {
        logout()
    })

    $("#AddRoleClick").click(function () {
        let messageElement = $("#AddRoleModalMessage")

        let roleNameElement = $("#AddRoleName")
        let roleName = roleNameElement.val()
        if (roleName === "") {
            messageElement.html("Role name is required")
            return
        }

        $.ajax({
            url: `${gatewayUrl()}/accounts/${localStorage.currentUserId}/roles/${roleName}`,
            contentType: "application/json;charset=utf-8",
            type: "Post",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr)
            },
            error: function (request) {
                roleNameElement.val("")
                let errorMessage
                switch (request.status) {
                    case 0:
                        errorMessage = "Fail connect to server. Try later"
                        break
                    case 400 :
                        errorMessage = "User already has this role"
                        break
                    case 401:
                        logout()
                        break
                    case 404:
                        errorMessage = "Role not found"
                        break
                }
                messageElement.html(errorMessage)
            },
            success: function () {
                $("#AddRoleModal").modal("hide")
                showUser()
            }
        })
    })

    $("#HideAddRoleModal").click(function () {
        $("#AddRoleModal").modal('hide')
    })

    showUser();
})

function showUser() {
    let userDetailsElement = $("#Details");
    userDetailsElement.empty()
    $.ajax({
        url: `${gatewayUrl()}/users/${localStorage.currentUserId}`,
        type: "GET",
        beforeSend: function (xhr) {
            setJwtTokenBearer(xhr);
        },
    }).then(function (user) {
        userDetailsElement.append(
            `<p>First Name: ${user.firstName} </p> ` +
            `<p>Last Name: ${user.lastName}</p> ` +
            `<p class="">Email: ${user.email}</p> `)

        if (!isUserAdmin()) {
            return;
        }
        $.ajax({
            url: `${gatewayUrl()}/accounts/${localStorage.currentUserId}`,
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
            }
        }).then(function (account) {
            userDetailsElement.append("<p>Roles: </p>")
            for (const role of account.roles) {
                let removeButton = $(`<button type="button" class="btn btn-outline-primary">Delete</button>`)
                removeButton.click(function () {
                    $.ajax({
                        url: `${gatewayUrl()}/accounts/${localStorage.currentUserId}/roles/${role.id}`,
                        contentType: "application/json;charset=utf-8",
                        type: "DELETE",
                        beforeSend: function (xhr) {
                            setJwtTokenBearer(xhr)
                        },
                        success: function () {
                            $("#DeleteRoleModal").modal("hide")
                            showUser();
                        }
                    })
                })
                let roleElement = $(`<div class='border border-secondary border-3 rounded w-25 mb-3 shadow'><p>${role.id}</p></div>`)
                roleElement.append(removeButton)
                userDetailsElement.append(roleElement)
            }
            let addRoleButton = $(`<button type="button" class="btn btn-outline-primary w-50">Add role</button>`)
            addRoleButton.click(function () {
                $("#AddRoleModal").modal('show');
            })
            userDetailsElement.append(addRoleButton);
            let deleteButton = $(`<button type="button" class="btn btn-outline-danger w-50">Delete</button>`)
            deleteButton.click(function () {
                $.ajax({
                    url: `${gatewayUrl()}/accounts/${localStorage.currentUserId}`,
                    type: "DELETE",
                    beforeSend: function (xhr) {
                        setJwtTokenBearer(xhr);
                    }
                }).then(function () {
                    window.location.href = "users.html"
                })
            })
            userDetailsElement.append(deleteButton);
        });
    })
}


