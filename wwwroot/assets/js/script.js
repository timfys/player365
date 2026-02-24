/*Menu nav toggle*/
$("#nav_toggle").on("click", function (event) {
    event.stopImmediatePropagation();
    event.preventDefault();

    $(this).toggleClass("active");
    let navMenu = document.querySelector(".header_container_popup");

    if (navMenu.classList.contains("active_mobile_menu")) {
        navMenu.classList.remove("active_mobile_menu");
    } else {
        navMenu.classList.add("active_mobile_menu");
    }

});

let CookieManager = {};
CookieManager.setCookie = function (cookieName, cookieValue, expirationDays) {
    let d = new Date();
    d.setTime(d.getTime() + (expirationDays * 24 * 60 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = cookieName + "=" + cookieValue + ";" + expires + ";path=/";
}

document.querySelectorAll(".accordion_item").forEach(x => x.addEventListener("click", function () {

    if (x.classList.contains("active")) {
        x.classList.remove("active");
        x.querySelector(".accordion_title").classList.remove("active");
    } else {
        x.classList.add("active");
        x.querySelector(".accordion_title").classList.add("active");
    }

}))


/*Collapse*/
$("[data-collapse]").on("click", function (event) {
    event.preventDefault();

    var $this = $(this),
        blockId = $(this).data('collapse');

    $this.toggleClass("active");
})

/* select */
$('.select').on("click", function (event) {
    event.preventDefault();

    $('.select_body').toggleClass("active");

})

/* password */
function myFunction() {
    var x = document.getElementById("password");
    if (x.type === "password") {
        x.type = "text";
    } else {
        x.type = "password";
    }
}

/* pey method */
$('#first_choice').on("click", function (event) {
    event.preventDefault();

    $(this).addClass("active");
    $('#first_form').css('display', 'flex');
    $('#second_choise').removeClass("active");
    /*$('#third_choice').removeClass("active");*/
    $('#fourth_choice').removeClass("active");
    $('#second_form').css('display', 'none');
    $('#third_form').css('display', 'none');
    $('#fifth_choice').removeClass('active');
    document.querySelector("#main-error").style.display = "block";
    document.querySelector("#main-error-crypto-startAJob").style.display = "none";
})
$('#second_choise').on("click", function (event) {
    event.preventDefault();

    $(this).addClass("active");
    $('#second_form').css('display', 'flex');
    $('#first_choice').removeClass("active");
    /*$('#third_choice').removeClass("active");*/
    $('#fourth_choice').removeClass("active");
    $('#first_form').css('display', 'none');
    $('#third_form').css('display', 'none');
    document.querySelector("#main-error").style.display = "none";
    document.querySelector("#main-error-crypto-startAJob").style.display = "none";

})
/*$("#third_choice").on("click", function () {
    this.classList.add("active");
    document.querySelector("#first_choice").classList.remove("active");
    //document.querySelector("#second_choise").classList.remove("active");
    document.querySelector("#fourth_choice").classList.remove("active");
    document.querySelector("#first_form").style.display = "none";
    //document.querySelector("#second_form").style.display = "none";
    document.querySelector("#third_form").style.display = "block";
    document.querySelector("#main-error-crypto-startAJob").style.display = "block";
    document.querySelector("#main-error").style.display = "none";
})*/
$("#fourth_choice").on("click", function () {
    this.classList.add("active");
    document.querySelector("#first_choice").classList.remove("active");
    //document.querySelector("#second_choise").classList.remove("active");
    /*document.querySelector("#third_choice").classList.remove("active");*/
    document.querySelector("#first_form").style.display = "none";
    //document.querySelector("#second_form").style.display = "none";
    document.querySelector("#third_form").style.display = "block";
    document.querySelector("#main-error-crypto-startAJob").style.display = "block";
    document.querySelector("#main-error").style.display = "none";
})
$("#fifth_choice").on("click", function () {
    this.classList.add("active");
    document.querySelector("#first_choice").classList.remove("active");
    //document.querySelector("#second_choise").classList.remove("active");
    /*document.querySelector("#third_choice").classList.remove("active");*/
    document.querySelector("#first_form").style.display = "none";
    //document.querySelector("#second_form").style.display = "none";
    document.querySelector("#third_form").style.display = "block";
    document.querySelector("#main-error-crypto-startAJob").style.display = "block";
    document.querySelector("#main-error").style.display = "none";
})
/* withdraw */
$('#withdraw').on("click", function (event) {
    event.preventDefault();

    $(this).addClass("active");
    $('#withdraw_form').css('display', 'block');
    $('#transactions').removeClass("active");
    $('#transactions_form').css('display', 'none');

})
$('#transactions').on("click", function (event) {
    event.preventDefault();

    $(this).addClass("active");
    $('#transactions_form').css('display', 'block');
    $('#withdraw').removeClass("active");
    $('#withdraw_form').css('display', 'none');

})

/* personal_information */
$('#personal_information').on("click", function (event) {
    event.preventDefault();

    $(this).addClass("active");
    $('#address_information').removeClass("active");
    $('#login_information').removeClass("active");
    $('#card_information').removeClass("active");

    $('#personal_information_form').css('display', 'block');
    $('#address_information_form').css('display', 'none');
    $('#login_information_form').css('display', 'none');
    $('#card_information_empty').css('display', 'none');
    $('#card_information_form').css('display', 'none');
    $('#card_information_filled').css('display', 'none');

})

$("#back_button_from_personal_info").on("click", function (event) {
    event.stopImmediatePropagation();
    let currentCulture = document.cookie.split("current_culture=")[1];
    let langIso = '';
    if (currentCulture !== undefined) {
        langIso = currentCulture.split(";")[0];
    }

    if (!navigator.cookieEnabled) {
        langIso = GetString("current_culture");
    }

    if (langIso !== '') {
        window.location.href = `account/${langIso}`;
    } else {
        window.location.href = 'account/';
    }
});
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".prevent_float").forEach(x => x.addEventListener("click", function (e) {
        e.stopImmediatePropagation()
    }))
})

const startStatus = $("#card_information_text_phone_number").val();
$("#phone_number0").keyup(function () {
    $("#phone_number_verified").hide();
    $("#phone_number_not_verified").hide();

    var initialStatus = $("#card_information_text_phone_number").val();
    var initialValue = $("#initial_phone_number").val();
    var currentValue = $(this).val();

    if ((startStatus === "True") && (initialValue === currentValue)) {
        $("#phone_number_verified").show();
        document.querySelector("#phone_number_not_verified").style.pointerEvents = "initial";
        $("#card_information_text_phone_number").val("True");
    } else {
        $("#phone_number_not_verified").show();
        document.querySelector("#phone_number_not_verified").style.pointerEvents = "none";
        $("#card_information_text_phone_number").val("False");
    }
});


const startStatusLogin = $("#card_information_text_phone_number_login").val();
$("#phone_number1").keyup(function () {
    $("#phone_number_verified_login").hide();
    $("#phone_number_not_verified_login").hide();

    var initialStatusLogin = $("#card_information_text_phone_number_login").val();
    var initialValue = $("#initial_phone_number_login").val();
    var currentValue = $(this).val();

    if ((startStatusLogin === "True") && (initialValue === currentValue)) {
        $("#phone_number_verified_login").show();
        document.querySelector("#phone_number_not_verified_login").style.pointerEvents = "initial";
        $("#card_information_text_phone_number_login").val("True");
    } else {
        $("#phone_number_not_verified_login").show();
        document.querySelector("#phone_number_not_verified_login").style.pointerEvents = "none";
        $("#card_information_text_phone_number_login").val("False");
    }
});

$('#card_information').on("click", function (event) {
    event.preventDefault();

    $(this).addClass("active");
    $('#address_information').removeClass("active");
    $('#login_information').removeClass("active");
    $('#personal_information').removeClass("active");

    $('#personal_information_form').css('display', 'none');
    $('#address_information_form').css('display', 'none');
    $('#login_information_form').css('display', 'none');
    $('#card_information_empty').css('display', 'flex');
    $('#card_information_form').css('display', 'none');
    $('#card_information_filled').css('display', 'none');

})
$('.add_card_link').on("click", function (event) {
    event.preventDefault();

    $('#personal_information_form').css('display', 'none');
    $('#address_information_form').css('display', 'none');
    $('#login_information_form').css('display', 'none');
    $('#card_information_empty').css('display', 'none');
    $('#card_information_form').css('display', 'block');
    $('#card_information_filled').css('display', 'none');

})
$('#add_card').on("click", function (event) {
    event.preventDefault();

    $('#personal_information_form').css('display', 'none');
    $('#address_information_form').css('display', 'none');
    $('#login_information_form').css('display', 'none');
    $('#card_information_empty').css('display', 'none');
    $('#card_information_form').css('display', 'none');
    $('#card_information_filled').css('display', 'block');

})

/* error */
$(".personality_error").hover(function () {

        $('.error_text').css('opacity', '1');
    }, function () {
        $('.error_text').css('opacity', '0');
    }
);

/*mobil width 375px*/
function mobilFunction(x) {
    if (x.matches) { // Если медиа запрос совпадает

        /* pey method */
        $('#first_choice').on("click", function (event) {
            event.preventDefault();

            //$(this).addClass("active");
            $('.discount').css('display', 'none');
            $('.paymethod_mobil_title').css('display', 'none');
            $('.payment_method_choice').css('display', 'none');

            $('.payment_back').css('display', 'flex');
            $('#first_form').css('display', 'flex');
            $('#second_choise').removeClass("active");
            $('#second_form').css('display', 'none');
        })

        $('#second_choise').on("click", function (event) {
            event.preventDefault();

            //$(this).addClass("active");
            $('.discount').css('display', 'none');
            $('.paymethod_mobil_title').css('display', 'none');
            $('.payment_method_choice').css('display', 'none');

            $('.payment_back').css('display', 'flex');
            $('#second_form').css('display', 'flex');
            $('#first_choice').removeClass("active");
            $('#first_form').css('display', 'none');
        })

        /* payment_back button */
        $('.payment_back').on("click", function (event) {
            event.preventDefault();

            $('.discount').css('display', 'flex');
            $('.paymethod_mobil_title').css('display', 'flex');
            $('.payment_method_choice').css('display', 'block');
            $('.payment_back').css('display', 'none');
            //$('#first_choice').removeClass("active") ;
            $('#first_form').css('display', 'none');
            //$('#second_choise').removeClass("active") ;
            $('#second_form').css('display', 'none');

            $('.personal_information_title').css('display', 'none');
            $('.personal_help_link').css('display', 'none');
            $('.personal_information_left').css('display', 'block');

            $('#personal_information_form').css('display', 'none');
            $('#address_information_form').css('display', 'none');
            $('#login_information_form').css('display', 'none');
            $('#card_information_empty').css('display', 'none');
            $('#card_information_form').css('display', 'none');
            $('#card_information_filled').css('display', 'none');


            $('#withdraw_form').css('display', 'none');
            $('#transactions_form').css('display', 'none');
        })

        /* withdraw */
        $('#withdraw').on("click", function (event) {
            event.preventDefault();

            $('.payment_back').css('display', 'block');
            $('.personal_help_link').css('display', 'block');
            $('.personal_information_left').css('display', 'none');

            $('#withdraw_form').css('display', 'block');
            $('#transactions').removeClass("active");
            $('#transactions_form').css('display', 'none');
            window.location.href = "#withdraw";

        })
        $('#transactions').on("click", function (event) {
            event.preventDefault();

            $('.payment_back').css('display', 'block');
            $('.personal_help_link').css('display', 'block');
            $('.personal_information_left').css('display', 'none');

            $('#transactions_form').css('display', 'block');
            $('#withdraw').removeClass("active");
            $('#withdraw_form').css('display', 'none');
            window.location.href = "#transactions";

        })

        $('#card_information').on("click", function (event) {
            event.preventDefault();

            $('.payment_back').css('display', 'block');
            $('.personal_information_title').css('display', 'none');
            $('.personal_help_link').css('display', 'block');
            $('.personal_information_left').css('display', 'none');

            $('#personal_information_form').css('display', 'none');
            $('#address_information_form').css('display', 'none');
            $('#login_information_form').css('display', 'none');
            $('#card_information_empty').css('display', 'block');
            $('#card_information_form').css('display', 'none');
            $('#card_information_filled').css('display', 'none');

        })
        $('.add_card_link').on("click", function (event) {
            event.preventDefault();

            $('.payment_back').css('display', 'block');
            $('.personal_information_title').css('display', 'none');
            $('.add_card_title').css('display', 'block');
            $('.personal_help_link').css('display', 'block');
            $('.personal_information_left').css('display', 'none');

            $('#personal_information_form').css('display', 'none');
            $('#address_information_form').css('display', 'none');
            $('#login_information_form').css('display', 'none');
            $('#card_information_empty').css('display', 'none');
            $('#card_information_form').css('display', 'block');
            $('#card_information_filled').css('display', 'none');

        })
        $('#add_card').on("click", function (event) {
            event.preventDefault();

            $('.payment_back').css('display', 'block');
            $('.personal_information_title').css('display', 'none');
            $('.personal_help_link').css('display', 'block');
            $('.personal_information_left').css('display', 'none');

            $('#personal_information_form').css('display', 'none');
            $('#address_information_form').css('display', 'none');
            $('#login_information_form').css('display', 'none');
            $('#card_information_empty').css('display', 'none');
            $('#card_information_form').css('display', 'none');
            $('#card_information_filled').css('display', 'block');

        })

    } else {
        $('.payment_back').css('display', 'none');
    }
}

var x = window.matchMedia("(max-width: 275px)")
mobilFunction(x) // Вызов функции прослушивателя во время выполнения
x.addListener(mobilFunction) // Присоединить функцию прослушивателя при изменении состояния


/*$(".bank_verify").click(function () {
    $("#verify_bank").addClass('active');
    $("body").addClass('lock');

    if ($("#verify_submit").click(function () {
        $(this).addClass("send");
        var n, t, i;
        if (document.getElementById("identityCard").files[0] == undefined) return $("#identityCard").parent("div").children(".invalid-tooltip").show(), !1;

        if ($("#identityCard").parent("div").children(".invalid-tooltip").hide(), n = new FormData, t = document.getElementById("identityCard").files[0], n.append("identityCard", t), $("#ContentPlaceHolder1_divOther").index() != -1) {
            if (document.getElementById("bankDetails").files[0] == undefined)
                return $("#bankDetails").parent("div").children(".invalid-tooltip").show(), !1;
            $("#bankDetails").parent("div").children(".invalid-tooltip").hide();
            i = document.getElementById("bankDetails").files[0];
        }
        $('#verify_bank').removeClass("active");
        $("body").removeClass('lock');
        if ($("#verify_submit").hasClass('send')) {
            $(".bank_verify").parent().parent().addClass('checking');
            $(".bank_verify").addClass('bank_checking').text('Again verify?');
            $(".bank_verify").removeClass('bank_verify');
            $(".status_notverified").addClass('status_checking');
            $('.icon').attr('viewBox', '0 0 512 512');
            $('.status_notverified .icon path').attr('d', 'M464 256A208 208 0 1 1 48 256a208 208 0 1 1 416 0zM0 256a256 256 0 1 0 512 0A256 256 0 1 0 0 256zM232 120V256c0 8 4 15.5 10.7 20l96 64c11 7.4 25.9 4.4 33.3-6.7s4.4-25.9-6.7-33.3L280 243.2V120c0-13.3-10.7-24-24-24s-24 10.7-24 24z');
            $(".status_notverified b").text('Waiting for verification');
            $(".status_notverified").removeClass('status_notverified');
            $("#verify_submit").removeClass("send");
        }
        ;
    })) ;
});*/
/*End Verify bank*/
/*Start Drop Down Language*/
$(".dropdown dt").mouseover(function () {
    $(this).parent().addClass('hover');
});
$(".dropdown dt").mouseout(function () {
    $(this).parent().removeClass('hover');
});
$(".dropdown dt").click(function () {
    $(this).next().children().toggle();
    $(this).parent().toggleClass('active');


});


try {
    document.querySelector(".dropdown_menu_data").addEventListener("click", function (e) {
        e.stopImmediatePropagation();

        let dropDownMenu = this.querySelector(".dropdown_menu_select");

        if (dropDownMenu.classList.contains("active")) {
            this.parentElement.classList.remove("active");
            dropDownMenu.classList.remove("active");
            dropDownMenu.style.display = "none";
        } else {
            this.parentElement.classList.add("active");
            dropDownMenu.classList.add("active");
            dropDownMenu.style.display = "block";
        }
    })
    document.querySelectorAll(".dropdown_menu_option").forEach(x => {
        x.addEventListener("click", function (e) {
            e.stopImmediatePropagation();
            let dropDownMenu = this.parentElement.parentElement.parentElement;
            dropDownMenu.attributes.preselected_value.value = this.textContent;
            dropDownMenu.classList.remove("active");
            dropDownMenu.querySelector(".dropdown_menu_text").textContent = this.textContent;
            dropDownMenu.querySelector(".dropdown_menu_select").classList.remove("active");
            dropDownMenu.querySelector(".dropdown_menu_select").style.display = "none";
        })
    })
} catch (e) {
}

/*End Drop Down Language*/
/*Start Again Verify bank*/
/*$(".bank_checking").click(function () {
    $("#verify_bank").addClass('active');
    $("body").addClass('lock');

    if ($("#verify_submit").click(function () {
        $(this).addClass("send");
        var n, t, i;
        if (document.getElementById("identityCard").files[0] == undefined) return $("#identityCard").parent("div").children(".invalid-tooltip").show(), !1;

        if ($("#identityCard").parent("div").children(".invalid-tooltip").hide(), n = new FormData, t = document.getElementById("identityCard").files[0], n.append("identityCard", t), $("#ContentPlaceHolder1_divOther").index() != -1) {
            if (document.getElementById("bankDetails").files[0] == undefined)
                return $("#bankDetails").parent("div").children(".invalid-tooltip").show(), !1;
            $("#bankDetails").parent("div").children(".invalid-tooltip").hide();
            i = document.getElementById("bankDetails").files[0];
        }
        $('#verify_bank').removeClass("active");
        $("body").removeClass('lock');
        if ($("#verify_submit").hasClass('send')) {
            $(".bank_checking").parent().parent().addClass('verified');
            $(".bank_checking").addClass('bank_withdraw').text('Withdraw');
            $(".bank_checking").removeClass('bank_checking');
            $(".status_checking").addClass('status_verified');
            $('.status_checking .icon path').attr('d', 'M470.6 105.4c12.5 12.5 12.5 32.8 0 45.3l-256 256c-12.5 12.5-32.8 12.5-45.3 0l-128-128c-12.5-12.5-12.5-32.8 0-45.3s32.8-12.5 45.3 0L192 338.7 425.4 105.4c12.5-12.5 32.8-12.5 45.3 0z');
            $(".status_checking b").text('Verified');
            $(".status_checking").removeClass('status_checking');
            $("#verify_submit").removeClass("send");
        }
        ;
    })) ;
});*/
/*End Again Verify bank*/


$("#withdraw_online").click(function () {
    $("#add_bank").addClass('active');
    $("body").addClass('lock');
    document.querySelector("body").style.overflow = "hidden";
});

$(".close_btn").click(function () {
    $("#add_bank").removeClass('active');
    $("#verify_bank").removeClass('active');
    $("body").removeClass('lock');
    document.querySelector("body").style.overflow = "initial";
});

$(".modal_cancel").click(function () {
    $("#add_bank").removeClass('active');
    $("#verify_bank").removeClass('active');
    $("body").removeClass('lock');
    document.querySelector("body").style.overflow = "initial";
});

function AccountInfo() {
    $("#choice_bank").prop("checked", !0);
    $("#choice_iban").prop("checked", !1);
    $("#iban").prop("disabled", !0);
    $("#bank_code").prop("disabled", !1);
    $("#branch_number").prop("disabled", !1);
    $("#account_number").prop("disabled", !1)
}

document.querySelectorAll(".modal_radio").forEach(x => {
    x.addEventListener("click", (e) => {
        IBANFun(e)
    })
})

function IBANFun(e) {
    Array.from(e.target.parentElement.parentElement.querySelectorAll(".modal_group")).filter(x => x.attributes.currencyIso.value === e.target.attributes.currencyIso.value).forEach(x => {

        let inputElems = Array.from(x.querySelectorAll("input"));

        if (inputElems.find(y => y.type === "radio" && y.checked)) {
            inputElems.forEach(z => {
                z.removeAttribute("disabled");
                z.classList.remove("validation_param_error_border");
            });
        } else
            inputElems.forEach(z => {

                if (z.type !== "radio")
                    z.setAttribute("disabled", "");
            });

    })

}


/*Start UploadDocument*/
function UploadDocument(n) {
    $("#" + n).parent("div").children(".invalid-tooltip").hide();
    var t = document.getElementById(n).files[0].name, i = t.substr(t.lastIndexOf(".") + 1);
    if ($.inArray(i, ["jpeg", "gif", "png", "pdf"]) == -1) return $("#" + n).parent("div").children(".invalid-tooltip").show(), !1;
    document.getElementById("identityCard").files[0] ? ($("#identityCardSpan").html(document.getElementById("identityCard").files[0].name), $("#identityCardDiv").addClass("alert-success")) : ($("#identityCardSpan").html("Upload identity card"), $("#identityCardDiv").removeClass("alert-success"));
    document.getElementById("bankDetails").files[0] ? ($("#bankDetailsSpan").html(document.getElementById("bankDetails").files[0].name), $("#bankDetailsDiv").addClass("alert-success")) : ($("#bankDetailsSpan").html("Upload bank details"), $("#bankDetailsDiv").removeClass("alert-success"))
}



$(".close_btn").click(function () {
    $("#add_bank").removeClass('active');
    $("#verify_bank").removeClass('active');
    $("body").removeClass('lock');
});

async function AddAndRemoveTopWinnerSlide(type, max) {
    const currentLang = document.documentElement.lang || 'en';
    const resp = await fetch(`/TopWinners/Get?t=${type}&lg=${currentLang}`, {
        method: "GET",
        headers: {
            "X-Fetch-Indicator": "true"
        }
    });

    const html = await resp.text();
    
    if(!html)
        return;
    
    const carousel = document.querySelector("#winners-carousel-lasted");

    if (!carousel) 
        return;

    const slidesWrapper = carousel.shadowRoot.querySelector('.carousel__slides');
    const currentSlides = carousel.querySelectorAll('sl-carousel-item');

    if (currentSlides.length >= max) {
        currentSlides[currentSlides.length - 1].style.visibility = "hidden";
        currentSlides[currentSlides.length - 1].style.opacity = "0";
        currentSlides[currentSlides.length - 1].style.transition = "1s ease-in-out";
    }

    const newSlide = document.createElement("sl-carousel-item");
    newSlide.innerHTML = html;
    newSlide.classList.add("new-slide");

    carousel.prepend(newSlide);

    if(carousel.childElementCount > parseInt(carousel.getAttribute("slides-per-page"))) {

        let removeCount = carousel.childElementCount - parseInt(carousel.getAttribute("slides-per-page"));

        for(let i = 1; i < removeCount; i++) {
            carousel.children[carousel.childElementCount - 1 - i].remove();
        }
    }
    
    carousel.shadowRoot.querySelector("[part='base']").style.height = `${52 * carousel.childElementCount}px`;

    slidesWrapper.style.transition = 'none';
    slidesWrapper.style.transform = `translateY(-52px)`;

    requestAnimationFrame(() => {
        slidesWrapper.style.transition = 'transform 1s ease-in-out';
        slidesWrapper.style.transform = `translateY(0)`;

        slidesWrapper.addEventListener('transitionend', () => {
            slidesWrapper.style.transition = "";
            slidesWrapper.style.transform = '';
            if (currentSlides.length >= max) {
                currentSlides[currentSlides.length - 1].remove();
            }
        }, { once: true });
    });
    
}
async function UpdateCarousel(type, max, rebindEvent){

    if(window.CarouselTimeout){
        clearInterval(window.CarouselTimeout);
    }
    
    if(rebindEvent){
        
        
        let carousel =  document.querySelector("#winners-carousel-lasted") ?? document.querySelector("#winners-carousel");
        
        if(carousel.classList.contains(`carousel-count-${max}`))
            return;
        
        carousel.setAttribute("slides-per-page", max);
        
        if(carousel.classList.contains("carousel-count-10")){
            carousel.classList.replace("carousel-count-10", `carousel-count-${max}`);
        }

        if(carousel.classList.contains("carousel-count-25")){
            carousel.classList.replace("carousel-count-25", `carousel-count-${max}`);
        }

        if(carousel.classList.contains("carousel-count-50")){
            carousel.classList.replace("carousel-count-50", `carousel-count-${max}`);
        }
        
        window.CarouselTimeout = setInterval(function() { AddAndRemoveTopWinnerSlide( type, max); },3000);
    }else{
        var lang = document.documentElement.lang || 'en';
        let resp = await fetch(`/TopWinners/GetList?t=${type}&c=${max}&lg=${lang}`, {
            method : "GET",
            headers : {
                "X-Fetch-Indicator" : "true"
            }
        });

        let html = await resp.text();

        let carouselWrap = document.querySelector(".winners-carousel-body-wrap");

        carouselWrap.innerHTML = html;

        window.CarouselTimeout = setInterval(function() { AddAndRemoveTopWinnerSlide( type, max); },3000);
    }
    
}

function observe(parent, elem, callback){
    let observer = new IntersectionObserver(
        (entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    callback()
                }
            });
        },
        {
            root: parent,
            threshold: 0.1
        }
    );

    observer.observe(elem);
    
    return observer;
}