document.addEventListener("DOMContentLoaded", () => {
    const buyButtons = document.querySelectorAll("[data-confetti-buy]");
    const colors = ["#FFD700", "#FFCC33", "#FFB703", "#F7C948", "#E0AA3E", "#FFE39F"];
    document.querySelectorAll('a[href]').forEach(link => {
        link.href = link.href.toLowerCase();
    });
    const burstConfetti = (anchor) => {
        const rect = anchor.getBoundingClientRect();
        const originX = rect.left + rect.width / 2;
        const originY = rect.top + rect.height / 2;
        const pieces = 38;

        for (let i = 0; i < pieces; i++) {
            const piece = document.createElement("span");
            piece.className = "confetti-piece";
            piece.style.setProperty("--x", `${originX}px`);
            piece.style.setProperty("--y", `${originY}px`);
            const angle = Math.random() * Math.PI * 2;
            const distance = 120 + Math.random() * 180;
            piece.style.setProperty("--dx", `${Math.cos(angle) * distance}px`);
            piece.style.setProperty("--dy", `${Math.sin(angle) * distance}px`);
            piece.style.setProperty("--rot", `${(Math.random() - 0.5) * 720}deg`);
            piece.style.backgroundColor = colors[Math.floor(Math.random() * colors.length)];
            piece.style.animationDelay = `${Math.random() * 60}ms`;

            document.body.appendChild(piece);
            piece.addEventListener("animationend", () => piece.remove());
        }
    };

    buyButtons.forEach((btn) => {
        btn.addEventListener("click", (e) => {
            if (e.defaultPrevented || e.button !== 0) return;
            e.preventDefault();
            burstConfetti(btn);
            setTimeout(() => {
                window.location.href = btn.href;
            }, 420);
        });
    });
});

document.addEventListener("DOMContentLoaded", () => {
    const btn = document.getElementById("profile-menu-btn");
    const menu = document.getElementById("profile-dropdown");

    // Toggle on button click
    btn.addEventListener("click", (e) => {
        e.stopPropagation(); // не пробрасываем выше
        menu.classList.toggle("hidden");
    });

    // Close if click outside
    document.addEventListener("click", (e) => {
        if (!btn.contains(e.target) && !menu.contains(e.target)) {
            menu.classList.add("hidden");
        }
    });
});

document.addEventListener("DOMContentLoaded", () => {
    const btn = document.getElementById("profile-mobile-menu-btn");
    const menu = document.getElementById("profile-mobile-dropdown");

    // Toggle on button click
    btn.addEventListener("click", (e) => {
        e.stopPropagation();
        menu.classList.toggle("hidden");
    });

    // Close if click outside
    document.addEventListener("click", (e) => {
        if (!btn.contains(e.target) && !menu.contains(e.target)) {
            menu.classList.add("hidden");
        }
    });
});