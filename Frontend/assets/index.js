import {
    gatewayUrl,
    isUserAdmin,
    isUserAgent, isUserEmailIsConfirm,
    isUserLogin, logout, priorityCodeToElement,
    setJwtTokenBearer, statusCodeToElement,
} from "./utils.js"

$(function () {
    if (!isUserLogin()) {
        window.location.href = "login.html"
    }
    $("#logout").click(function () {
        logout()
    })

    showSupportTicketsListAndActions();
})

function showSupportTicketsListAndActions() {

    if (isUserAdmin()) {
        showAllSupportTicketsList();
        return;
    }

    let actions = $("#Actions")

    if (isUserAgent()) {
        let freeButton = $("<button class='btn btn-primary w-25'>Free</button>")
        let allMySupportTicketsButton = $("<button class='btn btn-primary w-25'>All my</button>")
        let myOpenButton = $("<button class='btn btn-primary w-25'>My open</button>")
        myOpenButton.attr("disabled", "disabled")
        freeButton.click(function () {
            freeButton.attr("disabled", "disabled")
            myOpenButton.removeAttr("disabled")
            allMySupportTicketsButton.removeAttr("disabled")
            showFreeSupportTicketList()
        })
        myOpenButton.click(function () {
            myOpenButton.attr("disabled", "disabled")
            freeButton.removeAttr("disabled")
            allMySupportTicketsButton.removeAttr("disabled")
            showMyOpenSupportTicketsList()
        })
        allMySupportTicketsButton.click(function () {
            myOpenButton.removeAttr("disabled")
            allMySupportTicketsButton.attr("disabled", "disabled")
            freeButton.removeAttr("disabled")
            showAllMySupportTickets();
        })
        actions.append(myOpenButton)
        actions.append(freeButton)
        actions.append(allMySupportTicketsButton)

        showMyOpenSupportTicketsList()

        return;
    }

    if (!isUserEmailIsConfirm()) {
        actions.append("<p class='text-secondary'>You need to confirm your email for creation support tickets</p>")
        return;
    }
    let allTicketsButton = $("<button class='btn btn-primary w-25'>All my</button>")
    let myOpenTicketsButton = $("<button class='btn btn-primary w-50'>My open</button>")
    myOpenTicketsButton.attr("disabled", "disabled")
    myOpenTicketsButton.click(function () {
        myOpenTicketsButton.attr("disabled", "disabled")
        allTicketsButton.removeAttr("disabled")
        showMyOpenSupportTicketsList()
    })
    allTicketsButton.click(function () {
        myOpenTicketsButton.removeAttr("disabled")
        allTicketsButton.attr("disabled", "disabled")
        showAllMySupportTickets();
    })
    actions.append(myOpenTicketsButton)
    actions.append(allTicketsButton)
    let addNewActionElement = $(`<div class="mt-2"></div>`)
    addNewActionElement.append(`<textarea placeholder="Description" class="form-control w-75" type="text" id="Description" />`);
    let newButton = $("<button class=\"btn btn-primary mt-4 mb-4 w-25 h-25\" id=\"addButton\" type=\"button\">New</button>")
    newButton.click(function () {
        let descriptionElement = $("#Description");
        $.ajax({
            type: "POST",
            url: `${gatewayUrl()}/supporttickets`,
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify({
                description: descriptionElement.val()
            }),
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
            success: function () {
                showMyOpenSupportTicketsList();
                descriptionElement.val("")
            }
        });
    })
    addNewActionElement.append(newButton)
    actions.append(addNewActionElement)
    showMyOpenSupportTicketsList()
}


function showMyOpenSupportTicketsList(pageNumber = 1) {
    let contentElement = $("#Content")
    contentElement.empty()
    $.ajax({
        url: `${gatewayUrl()}/supporttickets/my/page?PageNumber=${pageNumber}&PageSize=5&Status=0`,
        type: "GET",
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
    }).then(function (pageData) {

        showPage(pageData, function (pageNumber) {
            showMyOpenSupportTicketsList(pageNumber)
        });

        for (let supportTicket of pageData.supportTickets) {
            let ticketElement = supportTicketToElement(supportTicket)

            ticketElement.click(function () {
                localStorage.currentSupportTicketId = supportTicket.id;
                window.location.href = "ticket.html";
            })
            contentElement.append(ticketElement)
        }
    });
}

function showAllMySupportTickets(pageNumber = 1) {
    let contentElement = $("#Content")
    contentElement.empty()

    $.ajax({
        url: `${gatewayUrl()}/supporttickets/my/page?PageNumber=${pageNumber}&PageSize=5`,
        type: "GET",
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
    }).then(function (pageData) {

        showPage(pageData, function (pageNumber) {
            showAllMySupportTickets(pageNumber)
        });

        for (let supportTicket of pageData.supportTickets) {
            let ticketElement = supportTicketToElement(supportTicket)

            ticketElement.click(function () {
                localStorage.currentSupportTicketId = supportTicket.id;
                window.location.href = "ticket.html";
            })
            contentElement.append(ticketElement)
        }
    })
}

function showAllSupportTicketsList(pageNumber = 1) {
    let contentElement = $("#Content")
    contentElement.empty()

    $.ajax({
        url: `${gatewayUrl()}/supporttickets/page?PageNumber=${pageNumber}&PageSize=5`,
        type: "GET",
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
    }).then(function (pageData) {

        showPage(pageData, function (pageNumber) {
            showAllSupportTicketsList(pageNumber)
        });

        for (let supportTicket of pageData.supportTickets) {
            let ticketElement = supportTicketToElement(supportTicket)

            ticketElement.click(function () {
                localStorage.currentSupportTicketId = supportTicket.id;
                window.location.href = "ticket.html";
            })
            contentElement.append(ticketElement)
        }
    })
}


function showFreeSupportTicketList(pageNumber = 1) {
    let contentElement = $("#Content")
    contentElement.empty()
    $.ajax({
        url: `${gatewayUrl()}/supporttickets/free/page?PageNumber=${pageNumber}&PageSize=5`,
        type: "GET",
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
    }).then(function (pageData) {
        showPage(pageData, function (pageNumber) {
            showMyOpenSupportTicketsList(pageNumber)
        });

        for (let supportTicket of pageData.supportTickets) {
            let ticketElement = supportTicketToElement(supportTicket);
            let appointButton = $("<button class='btn btn-primary'>Appoint</button>")
            appointButton.click(function () {
                $.ajax({
                    url: `${gatewayUrl()}/supporttickets/${supportTicket.id}/agents/appoint`,
                    type: "POST",
                    beforeSend: function (xhr) {
                        setJwtTokenBearer(xhr)
                    },
                }).then(function () {
                    localStorage.currentSupportTicketId = supportTicket.id;
                    window.location.href = "ticket.html";
                })
            })
            ticketElement.append(appointButton);
            contentElement.append(ticketElement)
        }
    });
}


function showPage(pageData, pageFunction) {
    let pageElement = $("#Page");
    pageElement.empty()

    if (pageData.totalPages <= 1) {
        return
    }

    let previousButton
    if (pageData.hasPreviousPage) {
        previousButton = $(`<button class='btn btn-primary'> < </button>`)
        previousButton.click(function () {
            pageFunction(pageData.pageNumber - 1)
        })
    } else {
        previousButton = $(`<button class='btn btn-secondary disabled'> < </button>`)
    }
    pageElement.append(previousButton)

    for (let i = 1; i <= pageData.totalPages; i++) {
        let buttonElement
        if (i === pageData.pageNumber) {
            buttonElement = $(`<button class='btn btn-secondary' disabled>${i}</button>`)
        } else {
            buttonElement = $(`<button class='btn btn-primary'>${i}</button>`)
        }
        buttonElement.click(function () {
            pageFunction(i)
        });
        pageElement.append(buttonElement)
    }

    let nextButton;
    if (pageData.hasNextPage) {
        nextButton = $(`<button class='btn btn-primary' > > </button>`)
        nextButton.click(function () {
            pageFunction(pageData.pageNumber + 1)
        })
    } else {
        nextButton = $(`<button class='btn btn-secondary disabled' > > </button>`)
    }
    pageElement.append(nextButton)
}

function supportTicketToElement(supportTicket) {
    return $(
        "<div class='border border-primary border-5 rounded w-75 mb-3 shadow p-2' " +
        `<p>Description: ${supportTicket.description} </p> ` +
        `<p>Status: ${statusCodeToElement(supportTicket.status)}</p> ` +
        `<p>Priority: ${priorityCodeToElement(supportTicket.priority)}</p> ` +
        "</div>")
}



