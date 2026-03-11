function toggleDisclaimer() {
    const details = document.querySelector('.disclaimer details');
    if (window.innerWidth >= 600) {
        details.setAttribute('open', '');   // На десктопе открыть
    } else {
        details.removeAttribute('open');    // На мобилке закрыть
    }
}

// Запускаем при загрузке
toggleDisclaimer();

// И при изменении размера экрана
window.addEventListener('resize', toggleDisclaimer);

const track = document.querySelector('.track');
const prevBtn = document.querySelector('.nav.prev');
const nextBtn = document.querySelector('.nav.next');

// Calculate how much to scroll (one slide width + gap)
function getScrollDistance() {
    const slideGap = parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--slide-gap'));

    // Convert clamp values to actual pixels
    const slideElement = document.querySelector('.slide');
    const actualSlideWidth = slideElement ? slideElement.offsetWidth : 200;
    const actualGap = slideGap || 16;

    return actualSlideWidth + actualGap;
}

// Smooth scroll function that temporarily enables smooth behavior
function smoothScrollBy(distance) {
    const originalBehavior = track.style.scrollBehavior;
    track.style.scrollBehavior = 'smooth';

    track.scrollBy({
        left: distance,
        behavior: 'smooth'
    });

    // Reset to auto after animation completes
    setTimeout(() => {
        track.style.scrollBehavior = originalBehavior || 'auto';
    }, 300);
}

// Previous button click handler
prevBtn.addEventListener('click', () => {
    const scrollDistance = getScrollDistance();
    smoothScrollBy(-scrollDistance);
});

// Next button click handler
nextBtn.addEventListener('click', () => {
    const scrollDistance = getScrollDistance();
    smoothScrollBy(scrollDistance);
});

// Optional: Hide/show buttons based on scroll position
function updateButtonVisibility() {
    const isAtStart = track.scrollLeft <= 10;
    const isAtEnd = track.scrollLeft >= track.scrollWidth - track.clientWidth - 10;

    prevBtn.disabled = isAtStart;
    nextBtn.disabled = isAtEnd;
}

// Update button visibility on scroll
track.addEventListener('scroll', updateButtonVisibility);

// Initial button state
updateButtonVisibility();