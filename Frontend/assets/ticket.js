import {
    gatewayUrl,
    isExists,
    isUserAdmin,
    isUserAgent,
    isUserLogin,
    isUserOwnerSupportTicket,
    logout,
    priorityCodeToElement,
    setJwtTokenBearer,
    statusCodeToElement,
    userId,

} from "./utils.js"

$(function () {
    if (!isUserLogin()) {
        window.location.href = "login.html"
    }

    if (!isExists(localStorage.currentSupportTicketId)) {
        window.location.href = "index.html"
    }

    $("#logout").click(function () {
        logout()
    })

    $("#CloseTicketClick").click(function () {
        $.ajax({
            url: `${gatewayUrl()}/supporttickets/${localStorage.currentSupportTicketId}/status/close`,
            type: "POST",
            contentType: "application/json;charset=utf-8",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr)
            },
            error: function (request) {
                switch (request.status) {
                    case 401:
                        logout()
                        break
                }
            }
        }).then(function () {
            $("#CloseSupportTicketModal").modal('hide')
            showSupportTicket()
        })
    })

    $("#HideCloseSupportTicketModal").click(function () {
        $("#CloseSupportTicketModal").modal('hide')
    })

    $("#UpdateClick").click(function () {
        let descriptionElement = $("#NewDescription")
        let messageElement = $("#UpdateSupportTicketMessage")
        messageElement.html("");
        $.ajax({
            url: `${gatewayUrl()}/supporttickets`,
            type: "PUT",
            contentType: "application/json;charset=utf-8",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr)
            },
            error: function (request) {
                switch (request.status) {
                    case 400:
                        messageElement.html("Support ticket have suggested solution")
                        break
                    case 401:
                        logout()
                        break
                }
            },
            data: JSON.stringify({
                id: localStorage.currentSupportTicketId, description: descriptionElement.val()
            })
        }).then(function () {
            descriptionElement.val("")
            $("#UpdateSupportTicketModal").modal('hide')
            showSupportTicket()
        })
    })

    $("#HideUpdateSupportTicketModal").click(function () {
        $("#UpdateSupportTicketModal").modal('hide')
    })


    $("#UpdateMessageClick").click(function () {
        let descriptionElement = $("#NewContent")
        let messageElement = $("#UpdateMessageMessage")
        messageElement.html("")
        $.ajax({
            url: `${gatewayUrl()}/supporttickets/messages`,
            type: "PUT",
            contentType: "application/json;charset=utf-8",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr)
            },
            error: function (request) {
                switch (request.status) {
                    case 400:
                        messageElement.html("Support ticket have suggested solution")
                        break
                    case 401:
                        logout()
                        break
                }
            },
            data: JSON.stringify({
                id: localStorage.messageId, content: descriptionElement.val()
            })
        }).then(function () {
            descriptionElement.val("")
            $("#UpdateMessageModal").modal('hide')
            showSupportTicket()
        })
    })
    $("#DeleteMessageClick").click(function () {
        let messageElement = $("#UpdateMessageMessage")
        messageElement.html("")
        $.ajax({
            url: `${gatewayUrl()}/supporttickets/messages/${localStorage.messageId}`,
            type: "DELETE",
            contentType: "application/json;charset=utf-8",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr)
            },
            error: function (request) {
                switch (request.status) {
                    case 400:
                        messageElement.html("Support ticket have suggested solution")
                        break
                    case 401:
                        logout()
                        break
                }
            },
        }).then(function () {
            $("#NewContent").val("")
            $("#UpdateMessageModal").modal('hide')
            showSupportTicket()
        })
    })


    $("#HideUpdateMessageModal").click(function () {
        $("#UpdateMessageModal").modal('hide')
    })

    $("#SuggestMessageClick").click(function () {
        let descriptionElement = $("#NewContent")
        let messageElement = $("#SuggestMessageMessage")
        messageElement.empty()
        $.ajax({
            url: `${gatewayUrl()}/supporttickets/status/solution/suggest`,
            type: "POST",
            contentType: "application/json;charset=utf-8",
            beforeSend: function (xhr) {
                setJwtTokenBearer(xhr)
            },
            error: function (request) {
                switch (request.status) {
                    case 400:
                        messageElement.html("Support ticket have suggested solution")
                        break
                    case 401:
                        logout()
                        break
                }
            },
            data: JSON.stringify({
                messageId: localStorage.messageId,
            })
        }).then(function () {
            descriptionElement.val("")
            $("#SuggestMessageModal").modal('hide')
            showSupportTicket()
        })
    })

    $("#HideSuggestMessageModal").click(function () {
        $("#SuggestMessageModal").modal('hide')
    })

    showSupportTicket();
})

function showSupportTicket() {
    let detailsElement = $("#Details")
    detailsElement.empty();

    $.ajax({
        url: `${gatewayUrl()}/supporttickets/${localStorage.currentSupportTicketId}`,
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
    }).then(function (ticket) {

        showDetailsElement(ticket)

        showMessagesElement(ticket)

        showActionsElement(ticket)
    });
}

function showDetailsElement(supportTicket) {
    let detailsElement = $("#Details");
    detailsElement.empty();

    detailsElement.append(`<p>Description: ${supportTicket.description} </p> ` + `<div class="d-inline"> ` + `<p class="d-inline">Status: </p> ` + `${statusCodeToElement(supportTicket.status)} ` + `<p class="d-inline">Priority:</p> ` + `${priorityCodeToElement(supportTicket.priority)}</div>`)

    let userElement = $(`<p class="text-primary"><u>${supportTicket.user.firstName} ${supportTicket.user.lastName}</u></p>`)
    userElement.click(function () {
        localStorage.currentUserId = supportTicket.user.id;
        window.location.href = "user.html"
    })
    let agentElement;
    if (isExists(supportTicket.agent)) {
        agentElement = $(`<p class="text-primary"><u>${supportTicket.agent.firstName} ${supportTicket.agent.lastName}</u></p>`)
        agentElement.click(function () {
            localStorage.currentUserId = supportTicket.agent.id;
            window.location.href = "user.html"
        })
    } else {
        agentElement = $(`<p>Search...</p>`)
    }
    detailsElement.append(agentElement)

    if (!isUserOwnerSupportTicket(supportTicket)) {
        detailsElement.append(userElement)
    }

}

function showMessagesElement(supportTicket) {
    let messagesElement = $("#Messages");
    messagesElement.empty();


    for (let message of supportTicket.messages) {
        let messageElement = $(`<div class="border-3 rounded w-25 mt-3 shadow"></div>`)

        let userElement = $(`<p class="text-primary"><u>${message.user.firstName}</u></p>`)
        userElement.click(function () {
            localStorage.currentUserId = message.user.id;
            window.location.href = "user.html"
        })
        messageElement.append(userElement);
        messageElement.append(`<p>${message.content}</p>${imagesToElements(message.images)}`)
        messageElement.append(`<p class="text-secondary">${new Date(message.dateTime).toDateString()}</p>`)
        if (message.user.id === userId()) {
            messageElement.addClass("border border-primary")
            if (supportTicket.status === 0) {
                let updateButton = $("<button class='btn btn-primary'>Update</button>");
                updateButton.click(function () {
                    localStorage.messageId = message.id;
                    $("#UpdateMessageModal").modal("show");
                })
                messageElement.append(updateButton);
            }

        } else {
            messageElement.addClass("ms-auto border border-secondary")
        }

        let solutionElement = null;
        for (let solution of supportTicket.solutions) {
            if (!(message.id === solution.messageId)) {
                continue;
            }
            if (isUserAgent()) {
                switch (solution.status) {
                    case 0:
                        solutionElement = $("<p>Your accept this message as solution</p>")
                        break;
                    case 1:
                        solutionElement = $("<p class='text-success'>This message was accepted as a solution</p>")
                        break;
                    case 2:
                        solutionElement = $("<p class='text-danger'>This message was rejected as a solution\n</p>")
                        break;
                }
                continue
            }
            if (solution.status !== 0) {
                continue;
            }
            solutionElement = $("<div></div>")
            let acceptButton = $(`<button class="btn btn-success">Accept</button>`)
            acceptButton.click(function () {
                $.ajax({
                    url: `${gatewayUrl()}/supporttickets/${localStorage.currentSupportTicketId}/status/solutions/accept`,
                    type: "POST",
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
                }).then(function () {
                    showSupportTicket()
                })
            })
            solutionElement.append(acceptButton)
            let rejectButton = $(`<button class="btn btn-danger">Reject</button>`)
            rejectButton.click(function () {
                $.ajax({
                    url: `${gatewayUrl()}/supporttickets/${localStorage.currentSupportTicketId}/status/solutions/reject`,
                    type: "POST",
                    beforeSend: function (xhr) {
                        setJwtTokenBearer(xhr);
                    },
                }).then(function () {
                    showSupportTicket()
                })
            })
            solutionElement.append(rejectButton)
        }

        if (!isExists(solutionElement) && isUserAgent() && message.user.id === userId() && supportTicket.status === 0) {
            let suggestButton = $("<button class='btn btn-primary'>Suggest</button>");
            suggestButton.click(function () {
                localStorage.messageId = message.id;
                $("#SuggestMessageModal").modal("show");
            })
            solutionElement = suggestButton;
        }
        messageElement.append(solutionElement)
        messagesElement.append(messageElement)
    }
}

function showActionsElement(supportTicket) {

    let actionElement = $("#Actions")
    actionElement.empty()
    let imagesStorage = new Set()
    switch (supportTicket.status) {
        case 0:
            if(isUserAdmin()){
                return;
            }
            let addButtonElement = $(`</div> <button class="btn btn-primary">Add</button>`)
            addButtonElement.click(function () {
                let contentElement = $("#Content")
                let id = localStorage.currentSupportTicketId
                $.ajax({
                    url: `${gatewayUrl()}/supporttickets/messages`,
                    type: "POST",
                    contentType: "application/json;charset=utf-8",
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
                    data: JSON.stringify({
                        supportTicketId: id, content: contentElement.val(), images: Array.from(imagesStorage)
                    })
                }).then(function () {
                    contentElement.val("");
                    $("#ImagesPreview").empty();
                    showSupportTicket();
                })
            })
            let actionsElement = $("<div> " + "<label for=\"Content\">Text</label> " + "<input class=\"form-control\" type=\"text\" id=\"Content\"/>" + "<label for=\"Images\">images</label> " + "<div id=\"ImagesPreview\"></div>")
            let imagesElement = $(`<input class=\"form-control\" type=\"file\" multiple id=\"Images\">`)
            imagesElement.on('change', function () {
                let reader = new FileReader();
                for (let i = 0; i < this.files.length; i++) {
                    reader.onloadend = function () {
                        imagesStorage.add({"base64Content": reader.result.replace(/^data:image\/[a-z]+;base64,/, "")});
                        $("#ImagesPreview").append(`<img class = "img-fluid" src="${reader.result}" alt="">`)
                    }
                    reader.readAsDataURL(this.files[i]);
                }
            })
            actionsElement.append(imagesElement)
            actionsElement.append(addButtonElement);
            actionElement.append(actionsElement)
            if (isUserOwnerSupportTicket(supportTicket)) {
                let updateButton = $("<button class='btn btn-primary'>Update</button>")
                updateButton.click(function () {
                    $("#UpdateSupportTicketModal").modal("show")
                })
                actionElement.append(updateButton)
            }
            if (isUserAgent()) {
                let closeButton = $(`<button type="button" class="btn btn-primary">Close this ticket</button>`)
                closeButton.click(function () {
                    $("#CloseSupportTicketModal").modal('show')
                })
                actionElement.append(closeButton)
            }
            break;
        case 1:
            actionElement.append("<p class='text-success'>This ticket has been solved</p>")
            break;
        default:
            actionElement.append("<p class='text-danger'>This ticket has been close</p>")
            if (!isUserOwnerSupportTicket(supportTicket)) {
                return
            }
            let reopenButton = $(`<button type="button" class="btn btn-primary">Reopen</button>`)
            reopenButton.click(function () {
                $.ajax({
                    url: `${gatewayUrl()}/supporttickets/${localStorage.currentSupportTicketId}/status/reopen`,
                    type: "POST",
                    contentType: "application/json;charset=utf-8",
                    beforeSend: function (xhr) {
                        setJwtTokenBearer(xhr)
                    },
                    error: function (request) {
                        switch (request.status) {
                            case 401:
                                logout()
                                break
                        }
                    }
                }).then(function () {
                    showSupportTicket()
                })
            })
            actionElement.append(reopenButton)
    }
}

function imagesToElements(images) {
    if (!isExists(images)) {
        return "";
    }
    let result = ``;
    for (let image of images) {
        result += `<img class="img-fluid" src="data:image/jpeg;base64,${image.base64Content}" alt="">`
    }
    return result;
}
