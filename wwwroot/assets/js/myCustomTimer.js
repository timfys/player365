let langIso = document.cookie.split("ino4pf3t3495fn9y48ynf455y6n=")[1].split(";")[0];

document.addEventListener('DOMContentLoaded', function () {
    let time = Array.from(document.querySelectorAll(".hours_timer_js"));

    let time1 = document.querySelectorAll(".hours_timer_js-1");

    for (let i = 0; i < time1.length; i++) {
        time.push(time1[i])
    }

/*    InitLotteryTime();*/

    setInterval(Timer, 1000);

    function Timer() {
        for (let i = 0; i < time.length; i++) {

            let closeText = time[i].attributes.closeText.value;
            
            if(time[i].classList.contains('switch_color_offer')) {
                
                let e = document.querySelector(".red_text_offer");
                
                if(e.style.color === "red"){
                    e.style.color = "#BC0203";
                }else{
                    e.style.color = "red";
                }
            }
            
            if (time[i].textContent.indexOf(closeText) !== -1)
                continue;

            let lotteryTime = SplitTime(time[i]);

            let lotteryDays;

            let lotteryHours = parseInt(lotteryTime[0].split(' ')[1]);
            let lotteryMinutes = parseInt(lotteryTime[1]);
            let lotterySeconds = parseInt(lotteryTime[2]);
            if (lotteryTime[0].length > 2) {

                lotteryDays = lotteryTime[0].split(' ')[0];

                if (lotteryDays == "0d") {
                    lotteryDays = "";
                }
            } else {
                lotteryHours = parseInt(lotteryTime[0])
                lotteryDays = "";
            }


            if (lotteryHours <= 0 && lotteryMinutes <= 0 && lotterySeconds <= 0 && lotteryDays == "") {
                time[i].textContent = closeText;
                continue;
            }

            lotterySeconds -= 1;

            if (lotterySeconds == -1) {

                lotterySeconds = 59;
                lotteryMinutes -= 1;

                if (lotteryMinutes == -1) {

                    lotteryMinutes = 59;
                    lotteryHours -= 1;

                    if (lotteryHours == -1) {

                        lotteryHours = 23;
                        let lotteryDaysParsed = parseInt(lotteryDays[0]);

                        lotteryDaysParsed -= lotteryDaysParsed;

                        if (lotteryDaysParsed == -1) {

                            time.textContent = closeText;
                            continue;
                        } else if (lotteryDaysParsed > 0) {
                            lotteryDays = `${lotteryDaysParsed}d`;
                        } else {
                            lotteryDays = "";
                        }

                    }

                }
            }
            let lotteryHoursStr = lotteryHours <= 9 ? `0${lotteryHours}` : lotteryHours;
            let lotteryMinutesStr = lotteryMinutes <= 9 ? `0${lotteryMinutes}` : lotteryMinutes;
            let lotterySecondsStr = lotterySeconds <= 9 ? `0${lotterySeconds}` : lotterySeconds;

            if (!time[i].textContent.indexOf("Next Draw:") == -1 || time[i].classList.contains("hours_timer_js-1")) {
                time[i].textContent = `${lotteryDays} ${lotteryHoursStr}:${lotteryMinutesStr}:${lotterySecondsStr}`;

            } else {
                time[i].textContent = `Next Draw: ${lotteryDays} ${lotteryHoursStr}:${lotteryMinutesStr}:${lotterySecondsStr}`;
            }
        }
    }

    /*function InitLotteryTime() {

        for (let i = 0; i < lotteriesTime.length; i++) {

            let date = new Date(Date.now());

            let offsetHours = -date.getTimezoneOffset() / 60;

            let lotteryTime = SplitTime(lotteriesTime[i]);
            if (lotteryTime[0].indexOf("Closed") !== -1)
                continue;

            let lotteryHours = parseInt(lotteryTime[0].split(' ')[1]);
            let lotteryMinutes = parseInt(lotteryTime[1]);
            let lotterySeconds = parseInt(lotteryTime[2]);
            let lotteryDays;

            if (lotteryTime[0].length > 2) {

                lotteryDays = lotteryTime[0].split(' ')[0];

                if (lotteryDays == "0d") {
                    lotteryDays = "";
                }
            } else {
                lotteryHours = parseInt(lotteryTime[0])
                lotteryDays = "";
            }

            lotteryHours += offsetHours;

            if (lotteryHours > 24) {
                lotteryHours = lotteryHours - 24;
                if (lotteryDays !== "") {
                    lotteryDays = `${parseInt(lotteryDays.split("d")[0]) + 1}d`;
                } else {
                    lotteryDays = `1d`;
                }
            } else if (lotteryHours < 0) {
                if (lotteryDays !== "") {
                    lotteryHours = 24 + lotteryHours;

                    lotteryDays = `${parseInt(lotteryDays.split("d")[0]) - 1}d`;
                } else if (lotteryDays === "") {
                    lotteriesTime[i].textContent = "Closed";
                    continue;
                }
            }

            let lotteryHoursStr = lotteryHours <= 9 ? `0${lotteryHours}` : lotteryHours;
            let lotteryMinutesStr = lotteryMinutes <= 9 ? `0${lotteryMinutes}` : lotteryMinutes;
            let lotterySecondsStr = lotterySeconds <= 9 ? `0${lotterySeconds}` : lotterySeconds;


            if (!lotteriesTime[i].textContent.indexOf("Next Draw:") == -1 || lotteriesTime[i].classList.contains("hours_timer_js-1")) {
                lotteriesTime[i].textContent = `${lotteryDays} ${lotteryHoursStr}:${lotteryMinutesStr}:${lotterySecondsStr}`;

            } else {
                lotteriesTime[i].textContent = `Next Draw: ${lotteryDays} ${lotteryHoursStr}:${lotteryMinutesStr}:${lotterySecondsStr}`;
            }
        }

    }*/

    function SplitTime(lotteryObject) {
        let lotteryTime;

        if (lotteryObject.classList.contains("hours_timer_js-1")) {
            lotteryTime = lotteryObject.textContent.trim().split(':');
        } else {
            if (!lotteryObject.textContent.indexOf("Next Draw:") == -1) {
                lotteryTime = lotteryObject.textContent.trim().split(':');

            } else {
                lotteryTime = lotteryObject.textContent.trim().split("Next Draw:")[1].split(':');
                lotteryTime[0] = lotteryTime[0].trimStart();
            }
        }

        return lotteryTime;
    }
});


