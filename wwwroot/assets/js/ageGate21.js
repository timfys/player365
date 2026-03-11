(function () {
    var COOKIE_NAME = 'age_gate_21';

    function getCookie(name) {
        var value = '; ' + document.cookie;
        var parts = value.split('; ' + name + '=');
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    function setCookie(name, value, days) {
        var expires = '';
        if (typeof days === 'number') {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = '; expires=' + date.toUTCString();
        }
        document.cookie = name + '=' + value + expires + '; path=/; SameSite=Lax';
    }

    function lockScroll() {
        try { document.body.style.overflow = 'hidden'; } catch (e) { }
    }

    function unlockScroll() {
        try { document.body.style.overflow = ''; } catch (e) { }
    }

    function hideGate() {
        var gate = document.getElementById('age-gate-21');
        if (gate) {
            gate.style.display = 'none';
            gate.setAttribute('aria-hidden', 'true');
        }
        unlockScroll();
    }

    function showGate() {
        var gate = document.getElementById('age-gate-21');
        if (gate) {
            gate.style.display = 'flex';
            gate.setAttribute('aria-hidden', 'false');
        }
        lockScroll();
    }

    function init() {
        var verified = getCookie(COOKIE_NAME) === '1';
        if (verified) {
            hideGate();
            return;
        }

        showGate();

        var accept = document.getElementById('age-gate-accept');
        var decline = document.getElementById('age-gate-decline');
        var msg = document.getElementById('age-gate-message');

        if (accept) {
            accept.addEventListener('click', function () {
                setCookie(COOKIE_NAME, '1', 365);
                hideGate();
            });
        }

        if (decline) {
            decline.addEventListener('click', function () {
                try {
                    window.location.href = 'https://www.google.com/';
                } catch (e) {
                    window.location = 'https://www.google.com/';
                }
            });
        }
    }

    // Show only after the page has fully loaded (no flash during initial render)
    if (document.readyState === 'complete') {
        init();
    } else {
        window.addEventListener('load', init);
    }
})();