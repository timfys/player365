document.addEventListener("DOMContentLoaded", async () => {
		const iframe = document.getElementById("game-frame");
		const errorOverlay = document.getElementById("error_overlay");
		const gameClosedOverlay = document.getElementById("game_closed_overlay");
		const scriptErrors = document.getElementById("script_errors");
		const loader = document.getElementById("game_loading");
	const apiUrl = `/play/${window.gameConfig.integratorId}/${window.gameConfig.studioId}/${window.gameConfig.gameCode}`;
		if (!iframe || !errorOverlay) {
			return;
		}

		const showLoader = () => {
			if (loader) {
				loader.style.display = "flex";
			}
		};

		const showGameClosedState = () => {
			iframe.style.display = "none";
			errorOverlay.style.display = "none";
			if (gameClosedOverlay) gameClosedOverlay.style.display = "flex";
			hideLoader();
		};

		const showErrorState = (message) => {
			iframe.style.display = "none";
			if (gameClosedOverlay) gameClosedOverlay.style.display = "none";
			errorOverlay.style.display = "flex";
			if (scriptErrors) {
				scriptErrors.innerText = message ?? "";
				scriptErrors.style.display = message ? "block" : "none";
			}
			hideLoader();
		};

		const hideLoader = () => {
			if (loader) {
				loader.style.display = "none";
			}
		};

		const focusIframe = () => {
			if (!iframe) {
				return;
			}
			requestAnimationFrame(() => {
				try {
					iframe.focus({ preventScroll: true });
					iframe.contentWindow?.focus();
				} catch {
					// Ignore focus errors from cross-origin iframes
				}
			});
		};

		iframe.addEventListener("load", () => {
			hideLoader();
			iframe.style.display = "flex";
			focusIframe();
		});

		showLoader();

		// Hide overlays and script errors by default
		errorOverlay.style.display = "none";
		if (gameClosedOverlay) gameClosedOverlay.style.display = "none";
		if (scriptErrors) {
			scriptErrors.style.display = "none";
			scriptErrors.innerText = "";
		}

		try {
			const res = await fetch(apiUrl);
			if (!res.ok) {
				let serviceErrorMessage = "";
				try {
					const errorPayload = await res.clone().json();
					serviceErrorMessage = typeof errorPayload === "string" ? errorPayload : JSON.stringify(errorPayload);
				} catch {
					try {
						serviceErrorMessage = await res.text();
					} catch {
						serviceErrorMessage = `HTTP ${res.status}`;
					}
				}
				showErrorState(serviceErrorMessage);
				return;
			}
			const data = await res.json();

			if (data && data.redirectUrl) {
				// Redirect found → show iframe, hide error overlay and errors box
				async function getFinalUrl(url) {
					try {
						const response = await fetch(`/get-final-url?url=${encodeURIComponent(url)}`);
						const data = await response.json();
						return data.finalUrl;
					} catch (error) {
						console.error('Ошибка при получении финального URL:', error);
						return url; // Возвращаем исходный URL в случае ошибки
					}
				}

				// Использование:
				//getFinalUrl(data.redirectUrl).then(finalUrl => {
				//		iframe.src=finalUrl;
				//});
				iframe.src=data.redirectUrl.replace('http://', 'https://');
				iframe.style.display = "none";
				errorOverlay.style.display = "none";
				if (gameClosedOverlay) gameClosedOverlay.style.display = "none";
				if (scriptErrors) {
					scriptErrors.style.display = "none";
					scriptErrors.innerText = "";
				}
			} else if (data && (data.errorCode === "game_closed" || data.ErrorCode === "game_closed")) {
				// Game is temporarily closed
				showGameClosedState();
			} else {
				// No redirect → show error overlay, hide iframe and show any message
				showErrorState(JSON.stringify(data));
			}
		} catch (err) {
			console.error("Error resolving game URL:", err);
			// On error → show error overlay, hide iframe, and show error details
			const errMessage = (err && err.message) ? err.message : String(err);
			showErrorState(errMessage);
		}
	});
(() => {
	// DOM Elements
	const gameContainer = document.getElementById('game-container');
	const btn = document.getElementById('fsBtn'); // Bottom toggle button
	const closeBtn = document.getElementById('fsCloseBtn'); // Top Close X
	const fsTopToolbar = document.getElementById('fs-top-toolbar');
	const bottomToolbar = document.getElementById('game-toolbar');
	const fsIconEnter = document.getElementById('fsIconEnter');
	const fsIconExit = document.getElementById('fsIconExit');

	if (!gameContainer || !btn) return;

	// --- Config ---
	const canUseFullscreenApi = Boolean(document.fullscreenEnabled && gameContainer.requestFullscreen);
	const isIosDevice = /iPad|iPhone|iPod/.test(navigator.userAgent);

	let fallbackFsActive = false;
	let lastOrientation = window.matchMedia("(orientation: landscape)").matches ? 'landscape' : 'portrait';

	// --- UI State Helper ---
	const updateUiState = (isFs) => {
		// Toggle Icons on bottom bar
		if (fsIconEnter) fsIconEnter.classList.toggle('hidden', isFs);
		if (fsIconExit) fsIconExit.classList.toggle('hidden', !isFs);

		// Toggle visibility of Top Toolbar
		if (fsTopToolbar) fsTopToolbar.classList.toggle('hidden', !isFs);

		// Hide Bottom Toolbar if in fullscreen (optional, cleaner look)
		if (bottomToolbar) {
			if (isFs) bottomToolbar.classList.add('mobile-fullscreen-hidden');
			else bottomToolbar.classList.remove('mobile-fullscreen-hidden');
		}

		// Add class to container for CSS adjustments
		gameContainer.classList.toggle('is-fullscreen', isFs);
	};

	// --- CSS Fallback (iOS / Android Force) ---
	const enterFallbackFullscreen = () => {
		if (fallbackFsActive) return;
		fallbackFsActive = true;

		gameContainer.classList.add('mobile-fullscreen');
		document.body.classList.add('mobile-fullscreen-lock');

		updateUiState(true);

		// Hack to prompt iOS UI to settle
		setTimeout(() => window.scrollTo(0, 1), 100);
	};

	const exitFallbackFullscreen = () => {
		if (!fallbackFsActive) return;
		fallbackFsActive = false;

		gameContainer.classList.remove('mobile-fullscreen');
		document.body.classList.remove('mobile-fullscreen-lock');

		updateUiState(false);
	};

	// --- Logic: Enter ---
	const requestEnterFs = async () => {
		if (!canUseFullscreenApi || isIosDevice) {
			enterFallbackFullscreen();
			return;
		}
		try {
			// Request on CONTAINER so overlay is visible
			await gameContainer.requestFullscreen({ navigationUI: 'hide' });
		} catch (e) {
			console.warn("Native FS failed, using fallback");
			enterFallbackFullscreen();
		}
	};

	// --- Logic: Exit ---
	const requestExitFs = async () => {
		if (document.fullscreenElement) {
			try { await document.exitFullscreen(); } catch (e) { }
		}
		exitFallbackFullscreen();
	};

	// --- User Actions ---

	// 1. Toggle Button (Bottom)
	btn.addEventListener('click', () => {
		btn.blur();
		const isNativeFs = document.fullscreenElement === gameContainer;
		if (isNativeFs || fallbackFsActive) requestExitFs();
		else requestEnterFs();
	});

	// 2. Close Button (Top Toolbar)
	if (closeBtn) {
		closeBtn.addEventListener('click', (e) => {
			e.stopPropagation(); // Prevent bubbling issues
			requestExitFs();
		});
	}

	// --- Orientation Logic (Stable) ---
	const checkOrientation = () => {
		const currentOrientation = window.matchMedia("(orientation: landscape)").matches ? 'landscape' : 'portrait';

		// Only act if orientation CHANGED
		if (currentOrientation === lastOrientation) return;

		lastOrientation = currentOrientation;

		if (currentOrientation === 'landscape') {
			requestEnterFs();
		} else {
			requestExitFs();
		}
	};

	// --- Listeners ---

	// Native Fullscreen Change
	document.addEventListener('fullscreenchange', () => {
		const isFs = document.fullscreenElement === gameContainer;
		updateUiState(isFs);
		if (!isFs) exitFallbackFullscreen(); // Ensure Sync
	});

	// Orientation / Resize
	window.addEventListener('resize', () => {
		clearTimeout(window.resizeTimer);
		window.resizeTimer = setTimeout(checkOrientation, 100);
	});

	if (screen.orientation) {
		screen.orientation.addEventListener("change", checkOrientation);
	} else {
		window.addEventListener("orientationchange", checkOrientation);
	}

})();

// Attempt to enable background video audio; browsers may require a user gesture
document.addEventListener("DOMContentLoaded", () => {
	const targetVolume = 0.2; // 20%
	const videos = document.querySelectorAll(".game-frame__bg-video");

	const tryPlay = (video) => {
		try {
			video.play();
		} catch {
			// Autoplay with sound might be blocked until user interaction
		}
	};

	const enableAudioOnGesture = (video) => {
		const handler = () => {
			video.muted = false;
			video.volume = targetVolume;
			tryPlay(video);
			document.removeEventListener("click", handler);
			document.removeEventListener("touchend", handler);
		};
		document.addEventListener("click", handler, { once: true });
		document.addEventListener("touchend", handler, { once: true });
	};

	videos.forEach((video) => {
		if (!video) return;
		video.volume = targetVolume;
		if (!video.muted) {
			tryPlay(video);
			enableAudioOnGesture(video);
		}
	});
});

const phoneInput = document.getElementById("phoneInput");
const serverIso = window.gameConfig.initialCountryIso;

let phoneNumber = "";
let isPhoneComplete = false;
let country = null; // ожидаем объект { name, iso2, dialCode } | null
let iso = serverIso;

document.addEventListener("DOMContentLoaded", function () {
	const localIso = localStorage.getItem("Iso") ?? "";
	if (!iso || iso === "empty") {
		iso = localIso && localIso !== "empty" ? localIso : "us";
	}

	iso = (iso || "").toLowerCase();

	if (phoneInput && iso)
		phoneInput.country = iso;

	// Hide phone input label on small viewports (Tailwind sm breakpoint)
	const smQuery = window.matchMedia("(max-width: 639px)");
	const applyPhoneLabelVisibility = (evt) => {
		const isSmall = evt?.matches ?? smQuery.matches;
		if (!phoneInput) return;
		phoneInput.showLabel = !isSmall;
	};
	applyPhoneLabelVisibility();
	if (smQuery.addEventListener) smQuery.addEventListener("change", applyPhoneLabelVisibility);
	else if (smQuery.addListener) smQuery.addListener(applyPhoneLabelVisibility);

	// Focus logic (handles shadow DOM inner input if present)
	function focusPhoneElement() {
		if (!phoneInput) return;
		// Direct focus on custom element
		phoneInput.focus();
		// Try focusing internal input inside shadow root (if component uses one)
		const innerInput = phoneInput.shadowRoot?.querySelector('input, input[type="tel"]');
		if (innerInput) innerInput.focus();
	}

	// Immediate attempt
	focusPhoneElement();
	// Retry shortly in case custom element upgrades later
	setTimeout(focusPhoneElement, 150);
	setTimeout(focusPhoneElement, 500);
});

// --- helpers ---------------------------------------------------
const getDetail = (e) => Array.isArray(e.detail) ? e.detail[0] : e.detail;
const digitsOnly = (s) => (s || "").replace(/\D+/g, "");
const hasLib = !!(window.libphonenumber && window.libphonenumber.parsePhoneNumber);

const getIso2 = (c) => (c && typeof c === "object" ? c.iso2 : (typeof c === "string" ? c : ""));
const getDial = (c) => (c && typeof c === "object" ? c.dialCode : "");

// построить username-версию: <countryCode><NSN без ведущего 0>
function buildUsernameFromE164(e164, dialCode) {
	const d0 = digitsOnly(e164);
	if (!d0) return "";
	const d = d0.startsWith("00") ? d0.slice(2) : d0; // 00380... -> 380...
	const cc = (dialCode || "");
	let rest = d.startsWith(cc) ? d.slice(cc.length) : d;
	if (rest.startsWith("0")) rest = rest.slice(1);  // срезаем один ведущий "национальный ноль"
	return cc + rest;
}

// привести ввод к E.164: если без "+", приклеим код страны
function toE164(raw, ctry) {
	const cleaned = raw.trim();
	if (!cleaned) return "";
	if (cleaned.startsWith("+")) {
		return "+" + digitsOnly(cleaned);
	}
	const cc = getDial(ctry);
	const local = digitsOnly(cleaned);
	return cc ? `+${cc}${local}` : "";
}

// основная нормализация и валидация
function normalizePhoneSafe(raw, ctry) {
	const candidate = toE164(raw, ctry); // может быть пустым, если нет кода страны
	if (!candidate) return { valid: false, reason: "NO_COUNTRY_CODE" };

	if (hasLib) {
		try {
			const p = window.libphonenumber.parsePhoneNumber(candidate);
			if (!p || !p.isValid()) {
				return { valid: false, reason: "INVALID_LIB" };
			}
			return {
				valid: true,
				e164: p.format("E.164"),
				username: `${p.countryCallingCode}${p.nationalNumber}`,
				region: p.country || getIso2(ctry)?.toUpperCase() || ""
			};
		} catch {
			return { valid: false, reason: "PARSE_ERROR" };
		}
	}

	// fallback без библиотеки (не можем на 100% гарантировать валидность)
	const cc = getDial(ctry);
	if (!cc) return { valid: false, reason: "NO_COUNTRY_CODE" };
	const e164 = candidate;
	const username = buildUsernameFromE164(e164, cc);
	return { valid: true, e164, username, region: getIso2(ctry)?.toUpperCase() || "" };
}

// --- CE events -------------------------------------------------
phoneInput.addEventListener("update:modelValue", (e) => {
	const val = (getDetail(e) || "").toString().replace(/\s+/g, "");
	phoneNumber = val;
	phoneInput.setAttribute("model-value", phoneNumber);
});

phoneInput.addEventListener("update:country", (e) => {
	country = getDetail(e) || null; // { name, iso2, dialCode } | null
});

document.addEventListener("keypress", function (e) {
	if (e.key === "Enter") {
		e.preventDefault();
		document.querySelector("#submit_login_1").click();
	}
});

// --- Submit ----------------------------------------------------
document.querySelector("#submit_login_1").addEventListener("click", async function () {
	const btn = this;
	const formErrorElem = document.querySelector("#form-valid");

	// базовые проверки до лоадера (чтобы не дёргать UI лишний раз)
	if (!phoneNumber && !country) {
		formErrorElem.style.display = "block";
		formErrorElem.textContent = window.gameConfig.specifyPhoneNumber;
		return;
	}
	if (!country) {
		formErrorElem.style.display = "block";
		formErrorElem.textContent = window.gameConfig.specifyCountryCode;
		return;
	}

	// нормализация + строгая проверка валидности
	const normalized = normalizePhoneSafe(phoneNumber, country);
	if (!normalized.valid) {
		formErrorElem.style.display = "block";
		// показываем «100% invalid» только когда библиотека точно сказала "invalid":
		const msg =
			(normalized.reason === "INVALID_LIB" || normalized.reason === "PARSE_ERROR")
				? "Invalid phone number"
				: window.gameConfig.specifyPhoneNumber;
		formErrorElem.textContent = msg;
		return; // стоп, не отправляем
	}

	// (опционально) можно также требовать isPhoneComplete, если хочешь UX-жёсткость:
	// if (!isPhoneComplete) { ... return; }

	formErrorElem.textContent = "";
	formErrorElem.style.display = "none";

	btn.style.pointerEvents = "none";
	DisplayLoadingScreen();

	const aId = parseInt(window.gameConfig.getAffiliateId);

	// Сохраняем обе версии
	localStorage.setItem("PhoneE164", normalized.e164 || "");       // "+380636422844"
	localStorage.setItem("PhoneUsername", normalized.username || ""); // "380636422844"
	localStorage.setItem("PhonePrefix", getDial(country) || "");      // "380"
	localStorage.setItem("Mobile", normalized.username || "");        // совместимость
	localStorage.setItem("Iso", (getIso2(country) || normalized.region || "").toLowerCase());
	localStorage.setItem("AffiliateId", Number.isFinite(aId) ? aId : -1);

	const body = {
		phone: normalized.e164,                               // E.164
		affiliateEntityId: Number.isFinite(aId) ? aId : -1,
		iso: getIso2(country) || normalized.region || "",
		prefix: getDial(country) || ""
	};

	try {
		const resp = await fetch(`/api/Auth/phone?r=${window.gameConfig.getReturnUrl}`, {
			method: "POST",
			headers: {
				"Content-Type": "application/json",
				"X-Fetch-Indicator": "true",
			},
			body: JSON.stringify(body),
		});

		if (resp.ok) {
			const data = await resp.json();
			let redirectUrl;
			if (data.verify === true) {
				redirectUrl = data.redirectUrl;
			} else {
				const baseUrl = window.location.origin;

				const redirectUrl = `${baseUrl}/sign-in?page=1&min=false`;
				//currentUrl.searchParams.set("page", "1");
				//currentUrl.searchParams.set("min", "false");
				//redirectUrl = currentUrl.toString();
				window.top.location.href = redirectUrl;

			}
		} else {
			const data = await resp.json().catch(() => ({}));
			formErrorElem.style.display = "block";
			if (data.code === "INVALID_PHONE_NUMBER") {
				formErrorElem.textContent = data.detail || "Invalid phone number";
			} else {
				formErrorElem.textContent = data.detail || "Unknown error";
			}
		}
	} catch (err) {
		formErrorElem.style.display = "block";
		formErrorElem.textContent = "Network error";
	} finally {
		btn.style.pointerEvents = "initial";
		CloseLoader(btn);
		CloseLoadingScreen();
	}
});

window.addEventListener('pageshow', function (event) {
	if (event.persisted) {
		CloseLoadingScreen();
	}
});
document.addEventListener("DOMContentLoaded", function (e) {
	try {
		CloseLoadingScreen();
		CloseLoader(document.querySelector("#submit-deposit1"));
	} catch (e) {

	}
})

// Pass bonus param to global scope for FormScript.js to use
window.depositBonusParam = window.gameConfig.bonusParam;

function RemoveLastChar(str) {
	let tempStr = "";

	for (let i = 0; i < str.length - 1; i++) {
		tempStr += str[i];
	}
	return tempStr;
}

let oldVal = window.gameConfig.amount;
let oldCurrency = "USD";
const USD_MIN_DEPOSIT = 10;
const userExchangeRate = 1;
const minDepositErrorTemplate = window.gameConfig.errorTemplate;
const amountRequiredMessage = window.gameConfig.enterAmount;
const userCurrencySymbol = window.gameConfig._symbol;
const totalPriceElem = document.querySelector("#total_price_display");
let depositHintMessages = [];

function getMinDepositSum(currencyCode) {
	const userExchangeRate = 1;
	const USD_MIN_DEPOSIT = 10;
	return currencyCode === "USD" ? USD_MIN_DEPOSIT : USD_MIN_DEPOSIT * userExchangeRate;
}

function formatMinSum(value) {
	return Number(value).toFixed(2).replace(/\.00$/, "");
}

function showValidationError(message, element) {
	if (!element) {
		return;
	}
	element.textContent = message;
	element.style.display = "block";
}

function hideValidationError(element) {
	if (!element) {
		return;
	}
	element.textContent = "";
	element.style.display = "none";
}

function toggleDepositHintHighlight(isActive) {
	let depositHintMessages = [];
	if (!depositHintMessages.length) {
		depositHintMessages = Array.from(document.querySelectorAll(".js-deposit-hint"));
	}
	depositHintMessages.forEach(function (elem) {
		if (!elem) {
			return;
		}
		elem.classList.toggle("deposit-hint--active", !!isActive);
	});
}

function updateTotalPriceDisplay() {
	const totalPriceElem = document.querySelector("#total_price_display");
	if (!totalPriceElem) {
		return;
	}
	const userCurrencySymbol = window.gameConfig._symbol;

	const priceInput = document.querySelector("#currency_price");
	const currencySelect = document.querySelector("#currency_select");
	const currencyCode = ((currencySelect && currencySelect.value) || "USD").toUpperCase();
	const sym = currencyCode === "USD" ? "$" : userCurrencySymbol;
	const rawVal = priceInput && priceInput.value ? priceInput.value.trim() : "0";
	const numericVal = Number(rawVal);
	const formatted = Number.isFinite(numericVal) ? numericVal.toFixed(2) : "0.00";
	totalPriceElem.textContent = `Total Price: ${sym}${formatted}`;
}
document.addEventListener("DOMContentLoaded", function (e) {
	const currencySelect = document.querySelector("#currency_select");
	if (currencySelect) {
		currencySelect.addEventListener("change", function () {
			const priceInput = document.querySelector("#currency_price");
			const hasValue = priceInput && priceInput.value.trim().length > 0;
			CheckIfValidAmount({ showErrors: hasValue });
			updateTotalPriceDisplay();
		});
	}

	const currencyPrice = document.querySelector("#currency_price");
	if (!currencyPrice) {
		return;
	}

	// Set default amount to 50 on load
	if (!currencyPrice.value || Number(currencyPrice.value) === 0) {
		currencyPrice.value = "50";
	}
	updateTotalPriceDisplay();

	currencyPrice.focus();
	currencyPrice.oninput = function (e) {

		if (e.data === ".") {
			return;
		}

		let maxLength = 5;
		const regex = /^\d+(\.\d{0,2})?$/;
		let value = e.currentTarget.value;

		if (!regex.test(value)) {
			e.currentTarget.value = RemoveLastChar(value);
		}

		let splitedFloat = value.split(".");
		splitedFloat[1] = splitedFloat[1] === undefined ? "" : `.${splitedFloat[1]}`;

		value = splitedFloat[0];

		if (value.length > maxLength) {

			while (value.length > 5) {
				value = RemoveLastChar(value);
			}

			e.currentTarget.value = `${value}${splitedFloat[1]}`;
		} else if (parseFloat(value) < 20) {

		} else {

		}

		CheckIfValidAmount({ showErrors: false });
		updateTotalPriceDisplay();
	};

	currencyPrice.addEventListener("focusout", function () {
		const hasValue = currencyPrice.value.trim().length > 0;
		CheckIfValidAmount({ showErrors: hasValue });
		updateTotalPriceDisplay();
	});
});

function CheckIfValidAmount(options = {}) {
	const { requireValue = false, showErrors = true } = options;
	const currency = document.querySelector("#currency_select");
	const currencyPrice = document.querySelector("#currency_price");
	const errorElem = document.querySelector("#currency-error");

	if (!currency || !currencyPrice || !errorElem) {
		return false;
	}

	const shouldShowMessage = showErrors || requireValue;
	toggleDepositHintHighlight(false);

	const currencyCode = (currency.value || "USD").toUpperCase();
	const minSum = getMinDepositSum(currencyCode);
	const sym = currencyCode === "USD" ? "$" : userCurrencySymbol;
	const amountRaw = currencyPrice.value.trim();
	const amountRequiredMessage = window.gameConfig.enterAmount;

	if (!amountRaw.length) {
		if (requireValue) {
			showValidationError(amountRequiredMessage, errorElem);
			currencyPrice.classList.add("input-error");
		} else {
			hideValidationError(errorElem);
		}
		currencyPrice.classList.add("input-error");
		return false;
	}

	const amount = Number(amountRaw);

	if (Number.isNaN(amount)) {
		if (shouldShowMessage) {
			showValidationError(amountRequiredMessage, errorElem);
		} else {
			hideValidationError(errorElem);
		}
		currencyPrice.classList.add("input-error");
		return false;
	}

	if (amount < minSum) {
		const formattedMin = `${sym}${formatMinSum(minSum)}`;
		if (shouldShowMessage) {
			toggleDepositHintHighlight(true);
		} else {
			toggleDepositHintHighlight(false);
		}
		hideValidationError(errorElem);
		currencyPrice.classList.add("input-error");
		return false;
	}

	hideValidationError(errorElem);
	toggleDepositHintHighlight(false);
	currencyPrice.classList.remove("input-error");
	let oldVal = window.gameConfig.amount;
	let oldCurrency = "USD";

	if (oldVal !== currencyPrice.value || oldCurrency !== currencyCode) {
		oldVal = currencyPrice.value;
		oldCurrency = currencyCode;
	}

	return true;
}


function SetBonusOption(voucherId, amount) {
	document.querySelector("#currency_select").value = window.gameConfig.currency;
	document.querySelector("#currency_select").setAttribute("voucherId", voucherId);
	document.querySelector("#currency_price").value = `${amount}`.replaceAll(",", ".");
	CheckIfValidAmount({ showErrors: true });
}

function DisplayStripe() {
	document.querySelector(".cryptoMethod").style.display = "none";
	document.querySelector(".stripeMethod").style.display = "initial";
}

function DisplayCrypto() {
	document.querySelector(".stripeMethod").style.display = "none";
	document.querySelector(".cryptoMethod").style.display = "flex";
}

function RedirectToPayment() {
	if (!CheckIfValidAmount({ requireValue: true, showErrors: true })) {
		const priceInput = document.querySelector("#currency_price");
		if (priceInput) {
			priceInput.focus();
		}
		return;
	}

	let currency = document.querySelector("#currency_select");
	let currencyPrice = document.querySelector("#currency_price");

	DisplayLoadingScreen();

	// Build return URL with encrypted bonus payload (bp parameter)
	// Payload contains: entityId:;:username:;:password:;:businessId:;:lid:;:bonusType (encrypted)
	let bonusPayload = window.gameConfig.bonusPayload;
	let returnPath = window.gameConfig.successfull;
	if (bonusPayload) {
		returnPath += `?bp=${encodeURIComponent(bonusPayload)}`;
	}
	let ouParam = `&ou=${encodeURIComponent(returnPath)}`;

	window.top.location.href = window.gameConfig.topLocation.replace("_currencyPrice", currencyPrice.value).replace("_ouParam", ouParam);
}

