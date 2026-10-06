const postersContainer = document.getElementById('moviePosters');
const posterCount = 15;

function createPoster() {
    const poster = document.createElement('div');
    poster.classList.add('poster');

    // Random position
    const xPos = Math.random() * 100;
    const yPos = Math.random() * 100;
    poster.style.left = `${xPos}%`;
    poster.style.top = `${yPos}%`;

    // Random size
    const scale = 0.7 + Math.random() * 0.6;
    poster.style.transform = `scale(${scale}) rotateX(${Math.random() * 10 - 5}deg) rotateY(${Math.random() * 10 - 5}deg)`;

    // Random animation
    const duration = 15 + Math.random() * 20;
    const delay = Math.random() * 5;
    poster.style.animation = `float ${duration}s ease-in-out ${delay}s infinite alternate`;

    postersContainer.appendChild(poster);

    // Add animation style
    const style = document.createElement('style');
    const startX = Math.random() * 5 - 2.5;
    const startY = Math.random() * 5 - 2.5;
    const endX = Math.random() * 5 - 2.5;
    const endY = Math.random() * 5 - 2.5;

    style.innerHTML = `
                @keyframes float {
                    0% {
                        transform: scale(${scale}) translate(${startX}px, ${startY}px) rotateX(${Math.random() * 10 - 5}deg) rotateY(${Math.random() * 10 - 5}deg);
                        opacity: ${0.1 + Math.random() * 0.2};
                    }
                    100% {
                        transform: scale(${scale}) translate(${endX}px, ${endY}px) rotateX(${Math.random() * 10 - 5}deg) rotateY(${Math.random() * 10 - 5}deg);
                        opacity: ${0.1 + Math.random() * 0.2};
                    }
                }
            `;
    document.head.appendChild(style);
}

for (let i = 0; i < posterCount; i++) {
    createPoster();
}

// Simulate projector flicker effect
function simulateFlicker() {
    const theater = document.querySelector('.theater');

    const flicker = () => {
        if (Math.random() > 0.985) {
            theater.style.opacity = Math.random() * 0.15 + 0.85;
            setTimeout(() => {
                theater.style.opacity = 1;
            }, 50 + Math.random() * 50);
        }
        requestAnimationFrame(flicker);
    };

    flicker();
}

simulateFlicker();