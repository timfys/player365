params = new Proxy(new URLSearchParams(window.location.search), {
    get: (searchParams, prop) => searchParams.get(prop),
});

let querySessionId = params.sId !== null ? `sId=${params.sId}` : "";

document.addEventListener("readystatechange", function () {


    try {
        document.querySelector("#bank_submit").addEventListener("click", async function (e) {
            e.stopImmediatePropagation();

            let gotAnError = false;

            let selectedForm;

            let selectedForms = Array.from(document.querySelectorAll(".modal_group")).filter(x => x.style.display !== "none");

            selectedForms.forEach(x => {
                Array.from(x.querySelectorAll("input")).filter(y => y.type === "number").forEach(z => {
                    if (!z.hasAttribute("disabled"))
                        selectedForm = x;

                    if (!z.hasAttribute("disabled") && z.value === "") {
                        z.classList.add("validation_param_error_border");
                        gotAnError = true;
                    } else
                        z.classList.remove("validation_param_error_border");
                })
            })

            if (gotAnError)
                return;

            let inputs = Array.from(selectedForm.querySelectorAll(".withdraw_input_field"));

            let dataObj = {CurrencyIso: selectedForm.attributes.currencyiso.value.toUpperCase()};
            let jsonDataObj = "";

            inputs.forEach(x => {
                dataObj[x.attributes.wsName.value] = x.tagName === "DIV" ? x.attributes.preselected_value.value : x.value;
            })
            dataObj["PaymentId"] = "9";

            jsonDataObj = JSON.stringify(dataObj);

            let resp = await fetch("/Payment/Withdraw/Create", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    'X-Fetch-Indicator': 'true'
                },
                body: jsonDataObj
            })

            if (resp.ok) {
                location.reload();
            } else {
                let validationField = document.querySelector("#withdraw_validation_error");
                validationField.textContent = await resp.text();
            }
        })
    } catch (e) {

    }
    try {
        document.querySelector("#withdraw_by_cheque").addEventListener("click", async function (e) {
            e.stopImmediatePropagation();

            let dataObj = {
                PaymentId: "3",
                CurrencyIso: "ILS",
            }

            let resp = await fetch("/Payment/Withdraw/Create", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    'X-Fetch-Indicator': 'true'
                },
                body: JSON.stringify(dataObj)
            })

            if (resp.ok) {
                window.location.reload();
            } else {
                document.querySelector("#withdraw_method_validation").textContent = await resp.text();
            }
        })
    } catch (e) {

    }
    try {

        document.querySelector("#balance_withdraw").addEventListener("click", async function (e) {

            e.stopImmediatePropagation();
            let minAmountNode = document.querySelector("#amount_input");

            let amount = minAmountNode.value;
            let minAmount = minAmountNode.attributes.minAmount.value;

            if (amount < minAmount) {
                minAmountNode.parentElement.classList.add("validation_param_error_border");
                return;
            } else {
                minAmountNode.parentElement.classList.remove("validation_param_error_border");
            }

            DisplayLoadingScreen();
            let withdrawId = e.target.attributes.withdraw_id.value;
            let currencyIso = e.target.attributes.currencyIso.value;

            let resp = await fetch("/Payment/Withdraw", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "CurrencyIso": currencyIso,
                    'X-Fetch-Indicator': 'true'
                },
                body: JSON.stringify({PurchasePaymentId: withdrawId, Amount: amount})
            })

            CloseLoadingScreen();
            let respText = await resp.text();
            if (respText.toLowerCase() === "ok") {
                window.location.href = resp.headers.get("redirect");
            } else {
                document.querySelector("#withdraw_validation").innerHTML = respText;
            }
        })
    } catch (e) {

    }

    try {


        document.querySelectorAll(".pagination_item").forEach(x => {
            x.addEventListener("click", async function (e) {
                e.stopImmediatePropagation();
                let pageIndex = parseInt(document.querySelector(".pagination_inner").attributes.pageIndex.value);

                if (e.currentTarget.classList.contains("nextButton")) {
                    pageIndex += 1;
                } else if (e.currentTarget.classList.contains("prevButton")) {
                    pageIndex -= 1;
                }

                let resp = await fetch(`/RenderTransactions?p=${pageIndex}`, {
                    headers: {
                        'X-Fetch-Indicator': 'true'
                    }
                });

                if (resp.ok) {

                    let lastPage = resp.headers.get("IsLastPage").toLowerCase() === "true";
                    let prevButton = document.querySelector(".prevButton");
                    let nextButton = document.querySelector(".nextButton");

                    if (pageIndex > 0) {
                        prevButton.classList.remove("disabled_pagination");
                    } else {
                        prevButton.classList.add("disabled_pagination");
                    }

                    if (!lastPage) {
                        nextButton.classList.remove("disabled_pagination");
                    } else {
                        nextButton.classList.add("disabled_pagination");
                    }

                    document.querySelector(".pagination_inner").attributes.pageIndex.value = pageIndex;
                    let table = await resp.text();
                    document.querySelector(".transactions_table").innerHTML = table;
                }
            })
        })
    } catch (e) {

    }

    try {
        document.querySelector("#change-phone-but").addEventListener("click", function () {
            let input1 = document.querySelector("#number_input1");
            let input2 = document.querySelector("#number_input2");
            let input3 = document.querySelector("#number_input3");

            input1.removeAttribute("required");
            input2.removeAttribute("required");
            input3.removeAttribute("required");

            setTimeout(function () {
                input1.setAttribute("required", "required");
                input2.setAttribute("required", "required");
                input3.setAttribute("required", "required");
            }, 1000)
        })
        document.querySelector(".send_again").addEventListener("click", function () {
            let input1 = document.querySelector("#number_input1");
            let input2 = document.querySelector("#number_input2");
            let input3 = document.querySelector("#number_input3");

            input1.removeAttribute("required");
            input2.removeAttribute("required");
            input3.removeAttribute("required");

            setTimeout(function () {
                input1.setAttribute("required", "required");
                input2.setAttribute("required", "required");
                input3.setAttribute("required", "required");
            }, 1000)
        })

    } catch (e) {

    }

    let formIsHidden = 0;
    let cardNumberEnc = "";
    let cardDateEnc = "";
    let payerNameEnc = "";
    try {

        let cards = Array.from(document.querySelectorAll(".debit_credit_card"));

        if (cards.length > 0) {
            let cardParams = cards[0].attributes.cardNumber.value.split(",,,");
            cardNumberEnc = cardParams[0];
            cardDateEnc = cardParams[1];
            payerNameEnc = cardParams[2];
            formIsHidden = 1;
        }

        let lotteryPriceEnc = "";

        try {
            lotteryPriceEnc = document.querySelector("#sum").getAttribute("lp");
        } catch (e) {
        }
        try {

            let cardButtons = document.querySelectorAll(".radio_button");

            for (let i = 0; i < cardButtons.length; i++) {
                cardButtons[i].addEventListener("click", function () {
                    if (this.classList.contains("radio_button_active")) {
                        return;
                    }

                    if (cardButtons[i].getAttribute("openForm") === "true") {
                        document.querySelector("#card_number").value = "";
                        ResetActiveAttribute(this);
                        formIsHidden = 0;
                        cardNumberEnc = "";
                        cardDateEnc = "";
                        payerNameEnc = "";
                        ShowHideForm(false);
                    } else if (cardButtons[i].getAttribute("openForm") === "false") {
                        let cardParams = this.parentNode.querySelector(".debit_card_text").getAttribute("cardNumber").split(',,,');
                        cardNumberEnc = cardParams[0];
                        cardDateEnc = cardParams[1];
                        payerNameEnc = cardParams[2];
                        ResetActiveAttribute(this);
                        formIsHidden = 1;
                        ShowHideForm(true);
                    }
                })
            }

            function ResetActiveAttribute(elem) {
                for (let i = 0; i < cardButtons.length; i++)
                    cardButtons[i].classList.remove("radio_button_active");

                elem.classList.add("radio_button_active");
            }

            function ShowHideForm(hide) {
                let formParts = document.querySelectorAll(".form_part");
                if (hide) {
                    for (let i = 0; i < formParts.length; i++)
                        formParts[i].style.display = "none";
                } else {
                    for (let i = 0; i < formParts.length; i++)
                        formParts[i].style.display = "";
                }
            }
        } catch (e) {

        }

        document.querySelector("#submit").addEventListener("click", async function (e) {

            console.log(e);
            e.stopPropagation();
            e.stopImmediatePropagation();
            e.preventDefault();

            let name = document.querySelector("#name");
            let cardNumber = document.querySelector("#card_number");
            let month = document.querySelector("#month");
            let year = document.querySelector("#year");
            let cvv = document.querySelector("#cvv");
            let withdrawType = document.querySelector("#attribute_elem").getAttribute("withdrawType");

            let monthVal = parseInt(month.value) <= 9 ? `0${month.value}` : month.value;

            let formNotValid = false;

            if (BaseValidateValue(cvv) || cvv.value.length > 4 || cvv.value.length < 3) {
                if (cvv.value.length > 4 || cvv.value.length < 3) {
                    cvv.style.marginBottom = "15px";
                    cvv.classList.add("error");
                    document.querySelector("#cvv_error").style.display = "block";
                }

                formNotValid = true;
            } else {
                cvv.classList.remove("error");
                document.querySelector("#cvv_error").style.display = "none";
            }
            if (!formIsHidden) {

                if (BaseValidateSelect(month, "month")) {
                    formNotValid = true;
                }
                if (BaseValidateSelect(year, "year")) {
                    formNotValid = true;
                }
                if (BaseValidateValue(cardNumber) || cardNumber.value.length > 16) {
                    if (cardNumber.value.length > 16) {
                        document.querySelector("#card_number_error").style.display = "block";
                        document.querySelector("#valid-until-id").classList.add("mt-top30-50");
                        cardNumber.classList.add("error");
                    }

                    formNotValid = true;
                } else {
                    document.querySelector("#card_number_error").style.display = "none";
                }
                if (BaseValidateValue(name) || name.value.length > 50) {
                    if (name.value.length > 50) {
                        document.querySelector("#name_error").style.display = "block";
                        name.classList.add("error");
                    }

                    formNotValid = true;
                } else {
                    document.querySelector("#name_error").style.display = "none";
                }

            }

            if (formNotValid) {
                return;
            }

            DisplayLoader(this);
            DisplayLoadingScreen();

            let obj;

            if (formIsHidden) {
                obj =
                    {
                        LotteryId: parseInt(this.getAttribute("lottery")),
                        LotteryName: document.querySelector("#lottery-header").getAttribute("ln"),
                        DrawTime: document.querySelector("#draw-time").getAttribute("dw-t"),
                        Withdraw: parseInt(withdrawType),
                        Credentials:
                            {
                                CardHolder: "",
                                CardNumber: "",
                                ValidDate: `2088-11-01T00:00:00`,
                                Cvv: cvv.value,
                            }
                    }
            } else {
                obj =
                    {
                        LotteryId: parseInt(this.getAttribute("lottery")),
                        LotteryName: document.querySelector("#lottery-header").getAttribute("ln"),
                        DrawTime: document.querySelector("#draw-time").getAttribute("dw-t"),
                        Withdraw: parseInt(withdrawType),
                        Credentials:
                            {
                                CardHolder: name.value,
                                CardNumber: cardNumber.value.trim(),
                                ValidDate: `${year.value}-${monthVal}-01T00:00:00`,
                                Cvv: cvv.value
                            }
                    }
            }

            let objJson = JSON.stringify(obj);

            let requestMeta = {
                method: "POST",
                headers:
                    {
                        "Content-Type": "application/json",
                        "FormIsHidden": `${formIsHidden}`,
                        "CardNumEnc": cardNumberEnc,
                        "CardDateEnc": cardDateEnc,
                        "PayerNameEnc": payerNameEnc,
                        "LotteryPriceEnc": lotteryPriceEnc,
                        "Voucher": document.querySelector("#sum").getAttribute("ft"),
                        "DrawsPerWeek": document.querySelector("#sum").getAttribute("drawsMonth"),
                        'X-Fetch-Indicator': 'true'
                    },
                body: objJson
            };

            if (48 === parseInt(this.getAttribute("lottery"))) {
                requestMeta.headers.PrizesMultiplayer = this.getAttribute("lMultiplayer");
            }

            let resp = await fetch("/Payment/Lottery", requestMeta);

            let respText = await resp.text();

            CloseLoader(this);

            let url = resp.headers.get("Redirect");
            if (resp.ok) {
                if (resp.headers.has("redirect_url_payhora")) {
                    window.location.href = resp.headers.get("redirect_url_payhora");
                    setTimeout(function () {
                        CloseLoadingScreen()
                    }, 5000);
                    return;
                }
                setTimeout(function () {
                    CloseLoadingScreen()
                }, 5000);
                window.location.href = url;
            } else {
                if (url !== null) {
                    window.location.href = url;
                }

                let mainErrorElem = document.querySelector("#main-error");
                mainErrorElem.style.display = "block";
                mainErrorElem.textContent = respText;
                CloseLoadingScreen();
            }

        })
       } catch (e) {

    }
    try {
        document.querySelector("#submit_crypto_startajob_deposit").addEventListener("click", async function (e) {

            let currency_price = document.querySelector("#currency_price");
            if (!currency_price) {
                return;
            }

            const hasAdvancedValidation = typeof CheckIfValidAmount === "function";
            const isAmountValid = hasAdvancedValidation
                ? CheckIfValidAmount({ requireValue: true, showErrors: true })
                : !BaseValidateValue(currency_price);

            if (!isAmountValid) {
                if (!hasAdvancedValidation) {
                    document.querySelector("#currency_price_error").style.display = "block";
                }
                currency_price.focus();
                return;
            }

            if (!hasAdvancedValidation) {
                document.querySelector("#currency_price_error").style.display = "none";
            }

            DisplayLoader(this);
            DisplayLoadingScreen();
            e.stopImmediatePropagation();


            let paymentAmountUsd = document.querySelector("#currency_price").value;
            let countryIso = document.querySelector("#currency_select").value;

            // Include bonus param if set (e.g., for welcome bonus)
            let bonusParam = window.depositBonusParam || "";
            let bonusQuery = bonusParam ? `&bonus=${encodeURIComponent(bonusParam)}` : "";

            let resp = await fetch(`/Payment/Plisio?a=${paymentAmountUsd}&t=2&c=usd${bonusQuery}`,
                {
                    method: "GET",
                    headers: {
                        "IsUsd": document.querySelector("#currency_select").value === "usd" ? "1" : "0",
                        "RedirectUrl": params.r,
                        'X-Fetch-Indicator': 'true'
                    }
                });

            CloseLoader(this);
            CloseLoadingScreen();
            if (resp.ok) {
                window.top.location.href = await resp.text();
            } else {
                let respText = await resp.text();
                let mainErrorElem = document.querySelector("#main-error-crypto-startAJob");
                mainErrorElem.style.display = "block";
                mainErrorElem.textContent = respText;
            }
            
        })
    } catch (e) {

    }
    // try {
    //     document.querySelector("#login").addEventListener("click", function () {
    //         let email = document.querySelector("#email");

    //         let password = document.querySelector("#password");

    //         let formNotValid = false;

    //         if (BaseValidateValue(password)) {
    //             formNotValid = true;
    //         }
    //         if (BaseValidateValue(email)) {
    //             formNotValid = true;
    //         }

    //         if (formNotValid) {
    //             return;
    //         }

    //         DisplayLoader();
    //     })
    // } catch (e) {

    // }
    // try {
    //     document.querySelector("#create_account_btn").addEventListener("click", function (e) {

    //         let phoneNumber = document.querySelector("#phone_number");

    //         let name = document.querySelector("#name");

    //         let email = document.querySelector("#email");

    //         let password = document.querySelector("#password");

    //         let password2 = document.querySelector("#re-enter_password");

    //         let formIsValid = true;

    //         if (BaseValidateValue(password2)) {
    //             formIsValid = false;
    //         }
    //         if (BaseValidateValue(password)) {
    //             formIsValid = false;
    //         }
    //         if (BaseValidateValue(email)) {
    //             formIsValid = false;
    //         }
    //         if (BaseValidateValue(name)) {
    //             formIsValid = false;
    //         }
    //         if (BaseValidateValue(phoneNumber)) {
    //             formIsValid = false;
    //             document.querySelector(".country_code").classList.add("error");
    //         } else {
    //             document.querySelector(".country_code").classList.remove("error");
    //         }
    //         if (!email.value.match(
    //             /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
    //         )) {
    //             formIsValid = false;
    //             email.classList.add("error");
    //             document.querySelector("#invalid_email_error").style.display = "block";
    //             document.querySelector("#invalid_email_error").focus();
    //         } else {
    //             document.querySelector("#invalid_email_error").style.display = "none";
    //             email.classList.remove("error");
    //         }
    //         if (password.value.length < 6 || password.value.length > 20) {
    //             formIsValid = false;
    //             document.querySelector("#password_valid_values_text").classList.add("validation_param_error");
    //             document.querySelector("#password_valid_values_text").focus();
    //         } else {
    //             document.querySelector("#password_valid_values_text").classList.remove("validation_param_error");
    //         }
    //         if (password2.value !== password.value) {
    //             formIsValid = false;
    //             password.classList.add("error");
    //             password2.classList.add("error");
    //             document.querySelector("#password2_error").style.display = "block";
    //             document.querySelector("#password2_error").focus();
    //         } else {

    //             document.querySelector("#password2_error").style.display = "none";
    //         }

    //         if (!formIsValid) {
    //             e.preventDefault();
    //             return;
    //         }

    //         DisplayLoader(this);
    //     })
    // } catch (e) {

    // }

    try {
        document.querySelector("#submit-deposit").addEventListener("click", async function (e) {
            e.stopImmediatePropagation();

            let name = document.querySelector("#name");
            let cardNumber = document.querySelector("#card_number");
            let month = document.querySelector("#month");
            let year = document.querySelector("#year");
            let cvv = document.querySelector("#cvv");
            let currency_price = document.querySelector("#currency_price");

            if (currency_price.value < 10)
                currency_price.value = 10;

            let monthVal = parseInt(month.value) <= 9 ? `0${month.value}` : month.value;

            let formNotValid = false;

            if (BaseValidateValue(currency_price)) {
                document.querySelector("#currency_price_error").style.display = "block";
                formNotValid = true;
            } else {
                document.querySelector("#currency_price_error").style.display = "none";
            }
            if (currency_price.value.split(".")[0].length > 5) {
                document.querySelector("#currency_price_error1").style.display = "block";
                formNotValid = true;
            } else {
                document.querySelector("#currency_price_error1").style.display = "none";
            }

            if (BaseValidateValue(cvv) || cvv.value.length > 4 || cvv.value.length < 3) {
                if (cvv.value.length > 4 || cvv.value.length < 3) {
                    cvv.style.marginBottom = "15px";
                    cvv.classList.add("error");
                    document.querySelector("#cvv_error").style.display = "block";
                }

                formNotValid = true;
            } else {
                cvv.classList.remove("error");
                document.querySelector("#cvv_error").style.display = "none";
            }
            if (!formIsHidden) {

                if (BaseValidateSelect(month, "month")) {
                    formNotValid = true;
                }
                if (BaseValidateSelect(year, "year")) {
                    formNotValid = true;
                }
                if (BaseValidateValue(cardNumber) || cardNumber.value.length > 16) {
                    if (cardNumber.value.length > 16) {
                        document.querySelector("#card_number_error").style.display = "block";
                        document.querySelector("#valid-until-id").classList.add("mt-top30-50");
                        cardNumber.classList.add("error");
                    }


                    formNotValid = true;
                } else {
                    document.querySelector("#card_number_error").style.display = "none";
                }
                if (BaseValidateValue(name) || name.value.length > 50) {
                    if (name.value.length > 50) {
                        document.querySelector("#name_error").style.display = "block";
                        name.classList.add("error");
                    }

                    formNotValid = true;
                } else {
                    document.querySelector("#name_error").style.display = "none";
                }

            }

            let termsCheckbox = document.querySelector("#terms_checkbox");

            if (!termsCheckbox.checked) {
                document.querySelector(".terms_checkbox_wrap_payment_reg").classList.add("validation_param_error");
                return;
            } else {
                document.querySelector(".terms_checkbox_wrap_payment_reg").classList.remove("validation_param_error");
            }

            if (formNotValid) {
                return;
            }

            DisplayLoader(this);
            DisplayLoadingScreen();

            let obj;

            if (formIsHidden) {
                obj =
                    {
                        PaymentSum: parseFloat(currency_price.value),
                        Credentials:
                            {
                                CardHolder: "",
                                CardNumber: "",
                                ValidDate: `2088-11-01T00:00:00`,
                                Cvv: cvv.value
                            }
                    }
            } else {
                obj =
                    {
                        PaymentSum: parseFloat(currency_price.value),
                        Credentials:
                            {
                                CardHolder: name.value,
                                CardNumber: cardNumber.value.trim(),
                                ValidDate: `${year.value}-${monthVal}-01T00:00:00`,
                                Cvv: cvv.value
                            }
                    }
            }


            let objJson = JSON.stringify(obj);

            let resp = await fetch("/Payment/AddSum",
                {
                    method: "POST",
                    headers:
                        {
                            "UseCurrencyConvert": document.querySelector("#currency_select").value !== "en" ? "true" : "false",
                            "Content-Type": "application/json",
                            "FormIsHidden": `${formIsHidden}`,
                            "CardNumEnc": cardNumberEnc,
                            "CardDateEnc": cardDateEnc,
                            "PayerNameEnc": payerNameEnc,
                            'X-Fetch-Indicator': 'true'
                        },
                    body: objJson
                });

            let respText = await resp.text();
            CloseLoader(this);

            if (resp.ok) {

                let objUrlParams = new URLSearchParams(window.location.search);

                let rUrl = objUrlParams.get("r");

                if (rUrl !== undefined && rUrl !== null && rUrl !== "") {
                    window.location.href = rUrl;
                    return;
                }

                if (resp.headers.has("redirect_url_payhora")) {
                    window.location.href = resp.headers.get("redirect_url_payhora");

                    setTimeout(function () {
                        CloseLoadingScreen()
                    }, 5000);
                    return;
                }

                window.location.href = resp.headers.get("redirect");
                setTimeout(function () {
                    CloseLoadingScreen()
                }, 5000);
            } else {

                let url = resp.headers.get("Redirect");
                if (url !== null) {
                    window.location.href = url;
                }

                let mainErrorElem = document.querySelector("#main-error");
                mainErrorElem.style.display = "block";
                mainErrorElem.textContent = respText;
                CloseLoadingScreen();
            }

        })
    } catch (e) {

    }


    function BaseValidateValue(elem) {
        if (elem.value === "") {
            elem.classList.add("error");
            elem.focus();
            return true;
        } else {
            elem.classList.remove("error");
            return false;
        }
    }

    function BaseValidateSelect(elem, incorrectBaseValue) {
        if (elem.value === incorrectBaseValue) {
            elem.classList.add("error");
            elem.focus();
            return true;
        } else {
            elem.classList.remove("error");
            return false;
        }
    }

    $('#forgot-pass, #change-password').click(async function (e) {
        e.stopImmediatePropagation();

        let objUrlParams = new URLSearchParams(window.location.search);
        let tokenQuery = objUrlParams.get('t') ? objUrlParams.get('t') : "null";

        let newPasswordEl = document.getElementById('newpassword');
        //let newPassword = newPasswordEl ? newPasswordEl.value : "";

        let newPassword = null;
        if (newPasswordEl) {
            DisplayLoader(this);

            newPasswordEl.style.borderColor = "#e0e0e0";

            newPassword = newPasswordEl.value;

            newPasswordEl.disabled = true;
            document.getElementById('change-password').disabled = true;

            if ((newPassword === "") || (newPassword.length < 4)) {
                newPasswordEl.disabled = false;
                document.getElementById('change-password').disabled = false;

                newPasswordEl.style.borderColor = "red";
                CloseLoader(this);
                return;
            }
        } else {
            newPassword = "";
        }

        let userNameEl = document.getElementsByClassName('phone_for_forgot')[0];
        let userName = userNameEl ? userNameEl.outerText.split(" ")[1] : objUrlParams.get('u');
        let countryCodeEl = document.getElementsByClassName('phone_for_forgot')[0];
        let countryPrefixCookie = document.cookie.split("CountryPrefix=")[1];
        let countryCode = "";
        if (countryCodeEl) {
            countryCode = countryCodeEl.outerText.split(" ")[0].replace(/[^a-zа-яё0-9\s]/gi, '');
        } else if (countryPrefixCookie !== undefined) {
            countryCode = countryPrefixCookie.split(";")[0];
        }

        if (!navigator.cookieEnabled) {
            countryCode = GetString("CountryPrefix")
        }

        if (userNameEl) {
            if (!userName) {
                userNameEl.classList.add('error');
                return;
            } else
                userNameEl.classList.remove('error');
        }

        let params = {
            phone: userName,
        }

        if (newPassword !== "" && tokenQuery !== "") {
            params.password = newPassword;
        } else {
            params.password = "null";
        }

        if ((tokenQuery !== "") && (userName !== "") && (newPassword !== "")) {
            params.token = tokenQuery;
        } else {
            params.token = "null";
        }

        let formData = new FormData();
        formData.append("phone", params.phone);
        formData.append("password", params.password);
        formData.append("token", params.token);
        let resp = await fetch(`/SignIn/ForgotPassword`,
            {
                method: "POST",
                headers:
                    {
                        "CountryPrefix": params.token === "null" ? countryCode : "",
                        'X-Fetch-Indicator': 'true'
                    },
                body: formData
            })

        document.querySelector("#form-valid").style.display = "block";

        let respBody = await resp.json();
        if (resp.ok) {
            document.querySelector("#form-valid").style.color = "green";
            document.querySelector("#form-valid").textContent = `${respBody.resultMessage}`;

            if ((params.token !== "null") && (params.token !== '')) {
                let timer = setTimeout(function () {

                    let redirectUrl = resp.headers.has("Redirect") ? resp.headers.get("Redirect") : "/"

                    window.location.href = redirectUrl;

                }, 3000);
            }
        } else {
            if (newPasswordEl) {
                newPasswordEl.disabled = false;
                document.getElementById('change-password').disabled = false;
                CloseLoader(this);
            }

            document.querySelector("#form-valid").style.color = "red";
            document.querySelector("#form-valid").textContent = `${respBody.resultMessage}`
        }
    });

    try {
        document.querySelector("#InviteBtn").addEventListener("click", function (e) {

            e.stopImmediatePropagation();

            let data = intlTelInput.getInstance(document.querySelector('#phone_number0')).getSelectedCountryData();
            let phoneNumber = document.querySelector("#phone_number0");
            let countryIso = data.iso2;

            if(!phoneNumber.value) {
                phoneNumber.classList.add("error");
                return;
            }else{
                phoneNumber.classList.remove("error");
            }

            let xhr = new XMLHttpRequest();

            xhr.open("POST", "/umbraco/surface/Promotions/FormHandler", false);

            xhr.setRequestHeader("Content-Type", "application/json");
            xhr.setRequestHeader('X-Fetch-Indicator', 'true');

            let body = JSON.stringify(
                {
                    CountryIso: countryIso,
                    Phone: phoneNumber.value/*,
                    MessageId : messageId.value*/
                })

            xhr.send(body)

            let respObj = JSON.parse(xhr.responseText);

            let responseElem = document.querySelector("#responseElem");
            responseElem.style.display = "block";

            if (xhr.status === 200) {
                responseElem.classList.remove("red");
                responseElem.classList.add("green");
                if (isWindowsPc(window.navigator.userAgent)) {
                    navigator.clipboard.writeText(respObj.message + respObj.inviteUrl);

                    navigator.share({
                        title: "Invite",
                        text: respObj.message,
                        url: respObj.inviteUrl
                    })
                } else {

                    navigator.share({
                        title: "Invite",
                        text: respObj.message,
                        url: respObj.inviteUrl
                    })
                }
            } else {
                responseElem.classList.remove("green");
                responseElem.classList.add("red");
            }
            responseElem.textContent = respObj.resultMessage;


        })
    } catch (e) {

    }
})
function isWindowsPc(userAgent){
    return userAgent.includes("Windows NT");
}

window.addEventListener("load", (event) => {
    let newPasswordEl = document.getElementById('newpassword');

    if (newPasswordEl) {
        newPasswordEl.click();
        newPasswordEl.focus();
        newPasswordEl.dispatchEvent(new Event('touchstart'));
    }
});

