/*document.addEventListener("readystatechange", function () {

    let buttons = document.querySelectorAll(".button_link");

    for (let i = 0; i < buttons.length; i++) {
        buttons[i].addEventListener("click", function (e) {

            e.stopPropagation();

            DisplayLoader(this.parentNode);
            
            if(this.parentNode.form != null){
                setTimeout(CloseLoader.bind(null, this.parentNode), 1000)
            }
        })
    }
})*/

function DisplayLoader(elem) {
/*    let childElems = elem.children;

    for (let i = 0; i < childElems.length; i++)
        if (childElems[i].classList.contains("loader"))
            return;

    for (let i = 0; i < childElems.length; i++)
        if (!childElems[i].classList.contains("loader"))
            childElems[i].style.display = "none";

    let loaderImg = document.createElement("img");

    loaderImg.setAttribute("src", "/assets/images/Double Ring-1s-200px.svg")

    loaderImg.style.height = `${elem.offsetHeight}px`;

    loaderImg.classList.add("loader");

    elem.appendChild(loaderImg);*/
    
}

function CloseLoader(elem) {
/*    let childElems = elem.children;

    for (let i = 0; i < childElems.length; i++) {
        if (childElems[i].classList.contains("loader")) {
            childElems[i].remove();
        }else{
            childElems[i].style.display = "";
        }
    }*/
}

function DisplayLoadingScreen(){
    document.querySelector("#loading-screen").style.display = "block";
    document.querySelector("html").style.pointerEvents = "none";
}
function CloseLoadingScreen(){
    document.querySelector("#loading-screen").style.display = "none";
    document.querySelector("html").style.pointerEvents = "initial";
}

/*
let loader =
    {
        DisplayLoader : DisplayLoader,
        CloseLoader : CloseLoader
    }
    
export {loader};*/
