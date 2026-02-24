let isCookieEnabled = navigator.cookieEnabled;
let params = new Proxy(new URLSearchParams(window.location.search), {
    get: (searchParams, prop) => searchParams.get(prop),
});

function Redirect(path) {
    window.location.href = path;
}

document.addEventListener("DOMContentLoaded", async function () {
    
    let timeZone = null;
    try {
        timeZone = document.cookie.split("4iu5ny64596ogyn5u6n7gu5no=")[1].split(";")[0]
    } catch (e) {

    }

    let offset = new Date().getTimezoneOffset();

    if (timeZone === undefined || timeZone === null || -parseInt(timeZone) !== offset) {
        await fetch(`/Helper/TimeZone?t=${-offset}`,
            {
                method: "GET",
                headers: {
                    'X-Fetch-Indicator': 'true'
                }
            });
    }
    

})

async function SetString(key, value, compress, encrypt) {
    let resp = await fetch(`/Helper/Set?n=Lines`,
        {
            method: "POST",
            body: JSON.stringify
            (
                {
                    Name: key,
                    Value: value,
                    Compress: compress !== undefined && compress,
                    Encrypt: encrypt !== undefined && encrypt
                }
            ),
            headers:
                {
                    "Content-Type": "application/json",
                    'X-Fetch-Indicator': 'true'
                }
        });
    return resp;
}

async function GetString(key, compress, encrypt) {
    let resp = await fetch(`/Helper/Get?n=Lines`,
        {
            method: "POST",
            body: JSON.stringify
            (
                {
                    Name: key,
                    Value: "",
                    Compress: compress !== undefined,
                    Encrypt: encrypt !== undefined
                }
            ),
            headers:
                {
                    "Content-Type": "application/json",
                    'X-Fetch-Indicator': 'true'
                }
        });
    return await resp.text();
}
async function Delete(name) {
    let resp = await fetch(`/Helper/Delete?n=${name}`);
}
function GetCookie(name){
    let cookieSplitted = document.cookie.split(`${name}=`);
    if(cookieSplitted !== undefined && cookieSplitted.length === 2){
        return cookieSplitted[1].split(";")[0]
    }
    return null;
}
function CookieExist(name){
    return GetCookie(name) !== null;
}
