let modalCountGlobal = 0;
function RenderModal(){
    ++modalCountGlobal
    let modalCount = modalCountGlobal;

    let modal = document.createElement("div");
    
    modal.style.height = '100%';
    modal.style.position = 'absolute';
    modal.style.top = '0';
    modal.style.left = '0';

    modal.innerHTML = `<div class="modal_window_background" id="modal_background_${modalCount}">
    <div class="modal_window_form_lottery" id="modal_${modalCount}">
        <button class="close_btn close_modal" id="close_modal_button_${modalCount}">×</button>
        <div id="modal_content_${modalCount}" class="content_modal"></div>
    </div>
</div>`;

    let modalObjControl = {
        ModalElem : modal,
        ModalId : modalCount
    }
    modalObjControl.Close = function (){

        document.querySelector("body").style.overflowY = "overlay";

        let modalBg = document.querySelector(`#modal_background_${modalCount}`);
        modalBg.style.opacity = 0;
        setTimeout(function () {modalBg.style.display = "none";}, 300);
    };

    modalObjControl.Open = function (){
        document.querySelector("body").style.overflowY = "hidden";
        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        modalBg.style.display = "flex";
        setTimeout(function(){modalBg.style.opacity = 1;}, 1);
    }

    modalObjControl.InsertData = function(data){
        document.querySelector(`#modal_content_${modalCount}`).innerHTML = data;
    }


    document.querySelector("html").append(modalObjControl.ModalElem);

    document.querySelector(`#modal_background_${modalCount}`).addEventListener("click", function (e) {

        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        if (e.target.classList.contains("modal_window_background")){
            modalBg.style.opacity = 0;
            document.querySelector("body").style.overflowY = "overlay";
            setTimeout(function () {modalBg.style.display = "none";}, 300)
        }
    })
    document.querySelector(`#close_modal_button_${modalCount}`).addEventListener("click", function (e) {
        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        modalBg.style.opacity = 0;
        document.querySelector("body").style.overflowY = "overlay";
        setTimeout(function () {modalBg.style.display = "none";}, 300)
    })

    return modalObjControl;
}
function RenderModalSmall(){
    ++modalCountGlobal
    let modalCount = modalCountGlobal;

    let modal = document.createElement("div");

    modal.style.height = '100%';
    modal.style.position = 'absolute';
    modal.style.top = '0';
    modal.style.left = '0';

    modal.innerHTML = `<div class="modal_window_background" id="modal_background_${modalCount}">
    <div class="modal_window_form" id="modal_${modalCount}">
        <button class="close_btn close_modal" id="close_modal_button_${modalCount}">×</button>
        <div id="modal_content_${modalCount}" class="content_modal"></div>
    </div>
</div>`;

    let modalObjControl = {
        ModalElem : modal,
        ModalId : modalCount
    }
    modalObjControl.Close = function (){

        document.querySelector("body").style.overflowY = "overlay";

        let modalBg = document.querySelector(`#modal_background_${modalCount}`);
        modalBg.style.opacity = 0;
        setTimeout(function () {modalBg.style.display = "none";}, 300);
    };

    modalObjControl.Open = function (){
        document.querySelector("body").style.overflowY = "hidden";
        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        modalBg.style.display = "flex";
        setTimeout(function(){modalBg.style.opacity = 1;}, 1);
    }

    modalObjControl.InsertData = function(data){
        document.querySelector(`#modal_content_${modalCount}`).innerHTML = data;
    }


    document.querySelector("html").append(modalObjControl.ModalElem);

    document.querySelector(`#modal_background_${modalCount}`).addEventListener("click", function (e) {

        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        if (e.target.classList.contains("modal_window_background")){
            modalBg.style.opacity = 0;
            document.querySelector("body").style.overflowY = "overlay";
            setTimeout(function () {modalBg.style.display = "none";}, 300)
        }
    })
    document.querySelector(`#close_modal_button_${modalCount}`).addEventListener("click", function (e) {
        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        modalBg.style.opacity = 0;
        document.querySelector("body").style.overflowY = "overlay";
        setTimeout(function () {modalBg.style.display = "none";}, 300)
    })

    return modalObjControl;
}
function RenderModal1(submitButtonText){
    ++modalCountGlobal
    let modalCount = modalCountGlobal;

    let modal1 = document.createElement("div");

    modal1.innerHTML = `<div class="modal_window_background" id="modal_background_${modalCount}">
    <div class="modal_window_form">
            <button class="close_btn" id="close_modal_button_${modalCount}">×</button>
        <div style="margin-top: 30px; width: 100%">
            <div id="modal_content_${modalCount}"></div>
            <div class="modal_button_wrap">
                <button class="delete_withdraw_button player1_modal_button" id="submit_button_${modalCount}">${submitButtonText}</button>
            </div>
        </div>
    </div>
</div>`

    let modalObjControl = {
        ModalElem : modal1,
        ModalId : modalCount
    }

    modalObjControl.Open = function (){
        document.querySelector("body").style.overflowY = "hidden";
        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        modalBg.style.display = "flex";
        setTimeout(function(){modalBg.style.opacity = 1;}, 1);
    }
    
    modalObjControl.InsertData = function(data){
        document.querySelector(`#modal_content_${modalCount}`).innerHTML = data;
    }

    modalObjControl.Close = function (){

        document.querySelector("body").style.overflowY = "overlay";

        let modalBg = document.querySelector(`#modal_background_${modalCount}`);
        modalBg.style.opacity = 0;
        setTimeout(function () {modalBg.style.display = "none";}, 300);
    };

    modalObjControl.SetDelegates = function (submitButtonDelegate){
        document.querySelector(`#submit_button_${modalCount}`).onclick = submitButtonDelegate;
    }

    document.querySelector("html").append(modalObjControl.ModalElem);

    document.querySelector(`#modal_background_${modalCount}`).addEventListener("click", function (e) {

        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        if (e.target.classList.contains("modal_window_background")){
            modalBg.style.opacity = 0;
            document.querySelector("body").style.overflowY = "overlay";
            setTimeout(function () {modalBg.style.display = "none";}, 300)
        }
    })
    document.querySelector(`#close_modal_button_${modalCount}`).addEventListener("click", function (e) {
        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        modalBg.style.opacity = 0;
        document.querySelector("body").style.overflowY = "overlay";
        setTimeout(function () {modalBg.style.display = "none";}, 300)
    })

    return modalObjControl;
}
function RenderModal2(cancelButtonText, submitButtonText){
    ++modalCountGlobal

    let modal2 = document.createElement("div");

    modal2.innerHTML = `<div class="modal_window_background" id="modal_background_${modalCountGlobal}">
    <div class="modal_window_form">
            <button class="close_btn" id="close_modal_button_${modalCountGlobal}">×</button>
        <div style="margin-top: 30px; width: 100%">
            <p class="withdraw_text withdraw_text_intendant" id="withdraw_method_delete_text_${modalCountGlobal}" style="text-align:center">
            </p>
            <div class="modal_button_wrap">
                <button class="cancel_delete_withdraw_button" id="cancel_button_${modalCountGlobal}">${cancelButtonText}</button>
                <button class="delete_withdraw_button" id="submit_button_${modalCountGlobal}">${submitButtonText}</button>
            </div>
        </div>
    </div>
</div>`

    let modalObjControl = {
        ModalElem : modal2,
        ModalId : modalCountGlobal
    }

    modalObjControl.Open = function (){
        document.querySelector("body").style.overflowY = "hidden";
        let modalBg = document.querySelector(`#modal_background_${modalCountGlobal}`);

        modalBg.style.display = "flex";
        setTimeout(function(){modalBg.style.opacity = 1;}, 1);
    }

    modalObjControl.Close = function (){

        document.querySelector("body").style.overflowY = "overlay";

        let modalBg = document.querySelector(`#modal_background_${modalCountGlobal}`);
        modalBg.style.opacity = 0;
        setTimeout(function () {modalBg.style.display = "none";}, 300);
    };

    modalObjControl.SetDelegates = function (cancelButtonDelegate, submitButtonDelegate){
        document.querySelector(`#submit_button_${modalCountGlobal}`).onclick = submitButtonDelegate;
        document.querySelector(`#cancel_button_${modalCountGlobal}`).onclick = cancelButtonDelegate;
    }

    document.querySelector("html").append(modalObjControl.ModalElem);

    document.querySelector(`#modal_background_${modalCountGlobal}`).addEventListener("click", function (e) {

        let modalBg = document.querySelector(`#modal_background_${modalCountGlobal}`);

        if (e.target.classList.contains("modal_window_background")){
            modalBg.style.opacity = 0;
            document.querySelector("body").style.overflowY = "overlay";
            setTimeout(function () {modalBg.style.display = "none";}, 300)
        }
    })
    document.querySelector(`#close_modal_button_${modalCountGlobal}`).addEventListener("click", function (e) {
        let modalBg = document.querySelector(`#modal_background_${modalCountGlobal}`);

        modalBg.style.opacity = 0;
        document.querySelector("body").style.overflowY = "overlay";
        setTimeout(function () {modalBg.style.display = "none";}, 300)
    })

    return modalObjControl;
}

function RenderModal3(secondSubmitButtonText, submitButtonText){
    ++modalCountGlobal
    let modalCount = modalCountGlobal;

    let modal3 = document.createElement("div");

    modal3.innerHTML = `<div class="modal_window_background" id="modal_background_${modalCount}">
    <div class="modal_window_form">
            <button class="close_btn" id="close_modal_button_${modalCount}">×</button>
        <div style="margin-top: 30px; width: 100%">
            <div id="modal_content_${modalCount}">
            </div>
            <div class="modal_button_wrap">
                <button class="cancel_delete_withdraw_button player1_modal_button_yellow" id="submit_second_button_${modalCount}">${secondSubmitButtonText}</button>
                <button class="delete_withdraw_button player1_modal_button" id="submit_button_${modalCount}">${submitButtonText}</button>
            </div>
        </div>
    </div>
</div>`

    let modalObjControl = {
        ModalElem : modal3,
        ModalId : modalCount
    }

    modalObjControl.Open = function (){
        document.querySelector("body").style.overflowY = "hidden";
        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        modalBg.style.display = "flex";
        setTimeout(function(){modalBg.style.opacity = 1;}, 1);
    }

    modalObjControl.Close = function (){

        document.querySelector("body").style.overflowY = "overlay";

        let modalBg = document.querySelector(`#modal_background_${modalCount}`);
        modalBg.style.opacity = 0;
        setTimeout(function () {modalBg.style.display = "none";}, 300);
    };
    modalObjControl.InsertData = function(data){
        document.querySelector(`#modal_content_${modalCount}`).innerHTML = data;
    }
    modalObjControl.SetDelegates = function (submitButtonDelegate, secondSubmitButtonDelegate){
        document.querySelector(`#submit_button_${modalCount}`).onclick = submitButtonDelegate;
        document.querySelector(`#submit_second_button_${modalCount}`).onclick = secondSubmitButtonDelegate;
    }

    document.querySelector("html").append(modalObjControl.ModalElem);

    document.querySelector(`#modal_background_${modalCountGlobal}`).addEventListener("click", function (e) {

        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        if (e.target.classList.contains("modal_window_background")){
            modalBg.style.opacity = 0;
            document.querySelector("body").style.overflowY = "overlay";
            setTimeout(function () {modalBg.style.display = "none";}, 300)
        }
    })
    document.querySelector(`#close_modal_button_${modalCount}`).addEventListener("click", function (e) {
        let modalBg = document.querySelector(`#modal_background_${modalCount}`);

        modalBg.style.opacity = 0;
        document.querySelector("body").style.overflowY = "overlay";
        setTimeout(function () {modalBg.style.display = "none";}, 300)
    })

    return modalObjControl;
}

