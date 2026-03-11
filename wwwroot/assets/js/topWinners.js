
window.WinnersCaruselSize = 10;
window.WinnersCaruselType = 0;

document.addEventListener("DOMContentLoaded", function (){
    document.querySelector("#dropdown-winners-option").addEventListener("sl-select", function (e){
        window.WinnersCaruselSize = e.detail.item.value;
        e.currentTarget.querySelector("#selected-option").innerHTML = `<div>${e.detail.item.value}</div>`;

        let menu = document.querySelector("#menu-wrap");

        menu.innerHTML = `${DropDownMenuPart(e.detail.item.value, "10")}
					${DropDownMenuPart(e.detail.item.value, "25")}
					${DropDownMenuPart(e.detail.item.value, "50")}`;

        UpdateCarousel(window.WinnersCaruselType, window.WinnersCaruselSize, true);
    })

    function DropDownMenuPart(current, next){
        if (current === next)
            return "";

        return `<sl-menu-item value="${next}">
											<div>
													${next}
											</div>
									</sl-menu-item>`
    }
})

function SetClickedWinnersType(selected){
    document.querySelector(".winners-carousel-options").querySelectorAll("div").forEach(z => {
        if(z === selected){
            z.classList.add("active-winners-carousel-option");
        }else{
            z.classList.remove("active-winners-carousel-option");
        }
    });

    window.WinnersCaruselType = selected.getAttribute("value");
    UpdateCarousel(window.WinnersCaruselType, window.WinnersCaruselSize);
}