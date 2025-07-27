// Initialize AOS (Animate On Scroll)
AOS.init({
    duration: 800,
    easing: 'ease-in-out',
    once: true,
    offset: 100
});

// Navbar scroll effect
window.addEventListener('scroll', function () {
    const navbar = document.querySelector('.navbar');
    const navbarBrandImg = document.querySelector('.navbar-brand img');

    if (window.scrollY > 50) {
        navbar.classList.add('scrolled');
        if (navbarBrandImg) navbarBrandImg.style.height = '50px';
    } else {
        navbar.classList.remove('scrolled');
        if (navbarBrandImg) navbarBrandImg.style.height = '60px';
    }
});

// Go to top button functionality
const goToTopBtn = document.getElementById('goToTopBtn');
if (goToTopBtn) {
    window.addEventListener('scroll', function () {
        if (window.scrollY > 300) {
            goToTopBtn.classList.add('show', 'active');
        } else {
            goToTopBtn.classList.remove('show', 'active');
        }
    });

    goToTopBtn.addEventListener('click', function () {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
}

// Smooth scrolling for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const targetId = this.getAttribute('href');
        if (targetId !== '#') {
            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        }
    });
});

// Fade-in observer for sections
const fadeObserver = new IntersectionObserver((entries, observer) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('fade-in');
            fadeObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.1 });

document.querySelectorAll('section').forEach(section => {
    fadeObserver.observe(section);
});

// Initialize all functionality when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Features slider
    const featuresSliderElement = document.querySelector('.features-slider');
    if (featuresSliderElement) {
        new Swiper(featuresSliderElement, {
            loop: true,
            slidesPerView: 1,
            spaceBetween: 30,
            autoplay: {
                delay: 3000,
                disableOnInteraction: false,
                pauseOnMouseEnter: true
            },
            pagination: {
                el: '.swiper-pagination',
                clickable: true,
            },
            navigation: {
                nextEl: '.swiper-button-next',
                prevEl: '.swiper-button-prev',
            },
            breakpoints: {
                576: {
                    slidesPerView: 2,
                },
                768: {
                    slidesPerView: 2,
                },
                992: {
                    slidesPerView: 3,
                },
                1200: {
                    slidesPerView: 4,
                }
            },
            observer: true,
            observeParents: true,
        });
    }

    // Parent Reviews Slider
    const parentReviewsSliderElement = document.querySelector('.parent-reviews-slider');
    if (parentReviewsSliderElement) {
        new Swiper(parentReviewsSliderElement, {
            loop: true,
            slidesPerView: 1,
            spaceBetween: 30,
            autoplay: {
                delay: 4000,
                disableOnInteraction: false,
                pauseOnMouseEnter: true
            },
            pagination: {
                el: '.swiper-pagination',
                clickable: true,
            },
            navigation: {
                nextEl: '.parent-reviews-slider .swiper-button-next',
                prevEl: '.parent-reviews-slider .swiper-button-prev',
            },
            breakpoints: {
                768: {
                    slidesPerView: 2,
                },
                992: {
                    slidesPerView: 3,
                }
            },
            observer: true,
            observeParents: true,
        });
    }

    // Gallery modal functionality
    const galleryModal = document.getElementById('galleryModal');
    if (galleryModal) {
        galleryModal.addEventListener('show.bs.modal', function (event) {
            const triggerLink = event.relatedTarget;
            const imageSrc = triggerLink.getAttribute('data-img-src');
            const modalImage = document.getElementById('modalImage');
            if (modalImage) modalImage.src = imageSrc;
        });
    }

    // Main contact form submission
    const mainContactForm = document.getElementById('contactForm');
    if (mainContactForm) {
        const submitFormBtn = mainContactForm.querySelector('#submitForm');
        if (submitFormBtn) {
            submitFormBtn.addEventListener('click', function () {
                if (mainContactForm.checkValidity()) {
                    const formData = {
                        parentName: document.getElementById('parentName').value,
                        phone: document.getElementById('phone').value,
                        email: document.getElementById('email').value,
                        childName: document.getElementById('childName').value,
                        childAge: document.getElementById('childAge').value,
                        educationType: document.getElementById('educationType').value,
                        message: document.getElementById('message').value
                    };

                    console.log('Main form submitted:', formData);
                    alert('Your form has been submitted successfully!');
                    mainContactForm.reset();
                } else {
                    mainContactForm.reportValidity();
                }
            });
        }
    }

    // Modal contact form submission
    const modalContactForm = document.getElementById('contactFormModal');
    if (modalContactForm) {
        const submitFormModalBtn = document.getElementById('submitFormModal');
        if (submitFormModalBtn) {
            submitFormModalBtn.addEventListener('click', function () {
                if (modalContactForm.checkValidity()) {
                    const formDataModal = {
                        parentName: document.getElementById('parentNameModal').value,
                        phone: document.getElementById('phoneModal').value,
                        email: document.getElementById('emailModal').value,
                        childName: document.getElementById('childNameModal').value,
                        childAge: document.getElementById('childAgeModal').value,
                        educationType: document.getElementById('educationTypeModal').value,
                        message: document.getElementById('messageModal').value
                    };

                    console.log('Modal form submitted:', formDataModal);
                    alert('Your form has been submitted successfully!');

                    const modal = bootstrap.Modal.getInstance(document.getElementById('contactModal'));
                    if (modal) modal.hide();
                    modalContactForm.reset();
                } else {
                    modalContactForm.reportValidity();
                }
            });
        }
    }
});