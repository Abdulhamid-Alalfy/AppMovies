document.addEventListener('DOMContentLoaded', function () {
    // تشغيل الفيديو
    const video = document.getElementById('moviePlayer');
    const playButton = document.getElementById('playButton');

    if (video && playButton) {
        playButton.addEventListener('click', function () {
            if (video.paused) {
                video.play();
                playButton.style.display = 'none';
            } else {
                video.pause();
            }
        });

        video.addEventListener('play', function () {
            playButton.style.display = 'none';
        });

        video.addEventListener('pause', function () {
            playButton.style.display = 'flex';
        });

        video.addEventListener('ended', function () {
            playButton.style.display = 'flex';
        });
    }

    const elements = document.querySelectorAll('.info-item');
    elements.forEach((element, index) => {
        element.style.opacity = '0';
        element.style.animation = `fadeIn 0.5s ease forwards ${0.2 + index * 0.1}s`;
    });
});
