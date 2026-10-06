// كود JavaScript للوظائف التفاعلية
document.addEventListener('DOMContentLoaded', function () {
    if (window.__cinemaLayoutScriptInitialized) {
        return;
    }

    // تبديل وضع الليل والنهار
    const themeToggle = document.getElementById('themeToggle');
    const body = document.body;

    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            const html = document.documentElement;
            body.classList.toggle('light-mode');
            html.classList.toggle('light-mode', body.classList.contains('light-mode'));

            // حفظ التفضيل في localStorage
            if (body.classList.contains('light-mode')) {
                localStorage.setItem('theme', 'light');
            } else {
                localStorage.setItem('theme', 'dark');
            }
        });
    }

    // تحميل التفضيل المحفوظ
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'light') {
        body.classList.add('light-mode');
        document.documentElement.classList.add('light-mode');
    } else {
        body.classList.remove('light-mode');
        document.documentElement.classList.remove('light-mode');
    }

    // شريط التمرير للعروض الخاصة
    const sliderContainer = document.getElementById('sliderContainer');
    const slides = document.querySelectorAll('.slide');
    const dots = document.querySelectorAll('.slider-dot');
    const prevBtn = document.getElementById('prevSlide');
    const nextBtn = document.getElementById('nextSlide');
    let currentSlide = 0;
    let slideInterval;

    function showSlide(n) {
        if (n >= slides.length) currentSlide = 0;
        if (n < 0) currentSlide = slides.length - 1;

        sliderContainer.style.transform = `translateX(-${currentSlide * 100}%)`;

        dots.forEach(dot => dot.classList.remove('active'));
        dots[currentSlide].classList.add('active');
    }

    function nextSlide() {
        currentSlide++;
        showSlide(currentSlide);
    }

    function prevSlide() {
        currentSlide--;
        showSlide(currentSlide);
    }

    // الأزرار والأحداث
    nextBtn.addEventListener('click', nextSlide);
    prevBtn.addEventListener('click', prevSlide);

    dots.forEach((dot, index) => {
        dot.addEventListener('click', () => {
            currentSlide = index;
            showSlide(currentSlide);
            resetInterval();
        });
    });

    // التبديل التلقائي للعروض
    function startInterval() {
        slideInterval = setInterval(nextSlide, 5000);
    }

    function resetInterval() {
        clearInterval(slideInterval);
        startInterval();
    }

    startInterval();

    // إيقاف التبديل التلقائي عند التمرير فوق الشريط
    const slider = document.querySelector('.offers-slider');
    slider.addEventListener('mouseenter', () => {
        clearInterval(slideInterval);
    });

    slider.addEventListener('mouseleave', () => {
        startInterval();
    });

    // البحث في الشريط العلوي
    const searchInputs = document.querySelectorAll('.search-box input');
    const searchButtons = document.querySelectorAll('.search-box button');

    searchButtons.forEach((button, index) => {
        button.addEventListener('click', function () {
            if (searchInputs[index].value.trim() !== '') {
                alert(`Searching for: ${searchInputs[index].value}`);
                // هنا يمكن إضافة منطق البحث الحقيقي
            }
        });
    });

    searchInputs.forEach((input, index) => {
        input.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                searchButtons[index].click();
            }
        });
    });

    // القائمة المتنقلة للهواتف
    const hamburger = document.getElementById('hamburger');
    const mobileMenu = document.getElementById('mobileMenu');
    const mobileSearch = document.querySelector('.mobile-search');

    hamburger.addEventListener('click', function () {
        mobileMenu.classList.toggle('active');

        // إظهار/إخفاء شريط البحث في القائمة المتنقلة للهواتف الصغيرة
        if (window.innerWidth <= 576) {
            mobileSearch.style.display = mobileMenu.classList.contains('active') ? 'flex' : 'none';
        }

        // تغيير الأيقونة
        if (mobileMenu.classList.contains('active')) {
            hamburger.innerHTML = '<i class="fas fa-times"></i>';
        } else {
            hamburger.innerHTML = '<i class="fas fa-bars"></i>';
        }
    });

    // إغلاق القائمة عند النقر على رابط
    const mobileLinks = document.querySelectorAll('.mobile-nav-links a');
    mobileLinks.forEach(link => {
        link.addEventListener('click', () => {
            mobileMenu.classList.remove('active');
            hamburger.innerHTML = '<i class="fas fa-bars"></i>';

            if (window.innerWidth <= 576) {
                mobileSearch.style.display = 'none';
            }
        });
    });

    // تأثير التمرير السلس للروابط
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();

            const targetId = this.getAttribute('href');
            if (targetId === '#') return;

            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                window.scrollTo({
                    top: targetElement.offsetTop - 70,
                    behavior: 'smooth'
                });
            }
        });
    });

    // تأثير الظهور التدريجي للعناصر
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
            }
        });
    }, observerOptions);

    // مراقبة العناصر لإضافة تأثير الظهور
    document.querySelectorAll('.movie-card, .category-card, .user-card').forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'opacity 0.5s ease, transform 0.5s ease';

        observer.observe(el);

        // إضافة صنف مرئي عند الظهور
        el.classList.add('fade-in');
    });

    // تعريف الصنف المرئي
    const style = document.createElement('style');
    style.textContent = `
                .fade-in.visible {
                    opacity: 1 !important;
                    transform: translateY(0) !important;
                }
            `;
    document.head.appendChild(style);
});
