

// Quality Buttons
const qualityBtns = document.querySelectorAll('.quality-btn');
qualityBtns.forEach(btn => {
    btn.addEventListener('click', function () {
        qualityBtns.forEach(b => b.classList.remove('active'));
        this.classList.add('active');
        const quality = this.dataset.quality;
        showNotification(`Quality changed to ${quality}`);
    });
});

// Server Buttons
const serverBtns = document.querySelectorAll('.server-btn');
serverBtns.forEach(btn => {
    btn.addEventListener('click', function () {
        serverBtns.forEach(b => b.classList.remove('active'));
        this.classList.add('active');
        const server = this.dataset.server;
        showNotification(`Loading from server ${server}`);
    });
});

// Download Button
document.querySelector('.download-btn').addEventListener('click', function () {
    showNotification('Preparing download link...');
    setTimeout(() => {
        showNotification('Download link is ready! 🎉');
    }, 2000);
});

// Trailer Button
document.querySelector('.trailer-btn').addEventListener('click', function () {
    showNotification('Loading trailer...');
});

// Favorite Button
const favoriteBtn = document.querySelector('.favorite-btn');
let isFavorite = false;
favoriteBtn.addEventListener('click', function () {
    isFavorite = !isFavorite;
    if (isFavorite) {
        this.innerHTML = '<i class="fas fa-heart"></i> Added to Favorites';
        this.style.background = 'var(--primary)';
        this.style.color = 'var(--light)';
        showNotification('Movie added to favorites ❤️');
    } else {
        this.innerHTML = '<i class="far fa-heart"></i> Add to Favorites';
        this.style.background = 'rgba(231, 76, 60, 0.1)';
        this.style.color = 'var(--primary)';
        showNotification('Movie removed from favorites');
    }
});

// Share Buttons
document.querySelectorAll('.share-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        const platform = this.className.split(' ')[1];
        showNotification(`Sharing on ${platform}`);
    });
});

// Keywords
document.querySelectorAll('.keyword').forEach(keyword => {
    keyword.addEventListener('click', function () {
        const keywordText = this.textContent;
        showNotification(`Searching for ${keywordText} movies...`);
    });
});

// Notification Function
function showNotification(message) {
    const notification = document.createElement('div');
    notification.style.cssText = `
                position: fixed;
                top: 100px;
                right: 20px;
                background: linear-gradient(45deg, var(--primary), var(--primary-dark));
                color: var(--light);
                padding: 1rem 1.5rem;
                border-radius: 15px;
                box-shadow: 0 15px 35px rgba(0, 0, 0, 0.3);
                z-index: 9999;
                animation: slideInRight 0.5s ease-out;
                font-weight: 600;
                backdrop-filter: blur(10px);
                border: 1px solid rgba(255, 255, 255, 0.1);
            `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.5s ease-out';
        setTimeout(() => {
            notification.remove();
        }, 500);
    }, 3000);
}

// Add animation keyframes
const style = document.createElement('style');
style.textContent = `
            @keyframes slideInRight {
                from {
                    transform: translateX(400px);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }

            @keyframes slideOutRight {
                from {
                    transform: translateX(0);
                    opacity: 1;
                }
                to {
                    transform: translateX(400px);
                    opacity: 0;
                }
            }
        `;
document.head.appendChild(style);

// Video Player Enhancement
const video = document.querySelector('video');
if (video) {
    video.addEventListener('play', () => {
        console.log('Playback started');
    });

    video.addEventListener('ended', () => {
        showNotification('Movie ended! We hope you enjoyed it 🎬');
    });
}

// Back button enhancement
document.querySelector('.back-btn').addEventListener('click', (e) => {
    e.preventDefault();
    showNotification('Returning to home page...');
    setTimeout(() => {
        history.back();
    }, 500);
});

// Auto-hide header on scroll
let lastScroll = 0;
window.addEventListener('scroll', () => {
    const currentScroll = window.pageYOffset;
    const header = document.querySelector('header');

    if (currentScroll > lastScroll && currentScroll > 100) {
        header.style.transform = 'translateY(-100%)';
    } else {
        header.style.transform = 'translateY(0)';
    }

    lastScroll = currentScroll;
});

// Parallax effect for hero section
window.addEventListener('scroll', () => {
    const scrolled = window.pageYOffset;
    const hero = document.querySelector('.movie-backdrop');
    if (hero) {
        hero.style.transform = `scale(1.1) translateY(${scrolled * 0.5}px)`;
    }
});