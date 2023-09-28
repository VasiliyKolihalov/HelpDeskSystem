export function gatewayUrl(){
    return "http://localhost:5000/gateway"
}
export function login(token) {
    localStorage.token = token
    $.ajax({
        type: "GET",
        url: `${gatewayUrl()}/accounts/my`,
        contentType: "application/json;charset=utf-8",
        beforeSend: function (xhr) {
            setJwtTokenBearer(xhr)
        }
    }).then(function (data) {
        localStorage.userId = data.id
        localStorage.isEmailConfirm = data.isEmailConfirm;
        for (const role of data.roles) {
            if (role.id === "admin") {
                localStorage.userRole = "admin"
                continue
            }
            if (role.id === "agent") {
                localStorage.userRole = "agent"
            }
        }
        window.location.href = "index.html";
    })
}

export function logout() {
    localStorage.token = null
    localStorage.userId = null
    localStorage.userRole = null
    localStorage.isEmailConfirm = null
    window.location.href = "login.html"
}

export function isUserLogin() {
    return isExists(localStorage.token)
}

export function isUserAdmin(){
    return isUserHasRole("admin")
}

export function isUserAgent(){
    return isUserHasRole("agent")
}

export function isUserHasRole(role){
    return isExists(localStorage.userRole) && localStorage.userRole === role
}

export function isUserEmailIsConfirm(){
    return isExists(localStorage.isEmailConfirm) && localStorage.isEmailConfirm
}

export function userId(){
    return localStorage.userId;
}

export function isExists(object) {
    return object !== "undefined" && object !== "null" && object !== null && object !== undefined
}

export function isUserOwnerSupportTicket(supportTicket){
    return supportTicket.user.id === userId()
}

export function setJwtTokenBearer(xhr) {
    xhr.setRequestHeader("Authorization", "Bearer " + localStorage.token)
}

export function validateEmail(email) {
    let emailRegex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|.(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
    return email.match(emailRegex)
}

export function validatePassword(password) {
    let passwordRegex = /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9])(?!.*\s).{8,16}$/
    return password.match(passwordRegex)
}


export function statusCodeToElement(statusCode) {
    switch (statusCode) {
        case 0:
            return '<span class="text-warning ">Open</span> '
        case 1:
            return '<span class="text-primary ">Solved</span> '
        case 2:
            return '<span class="text-secondary ">Close</span> '
    }
}

export function priorityCodeToElement(priorityCode) {
    switch (priorityCode) {
        case 0:
            return '<span class="text-info ">Low</span> '
        case 1:
            return '<span class="text-warning ">Medium</span> '
        case 2:
            return '<span class="text-danger ">High</span> '
    }
}

