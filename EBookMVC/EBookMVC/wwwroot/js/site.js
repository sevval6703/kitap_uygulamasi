// Global variables
let isLoading = false;

// Document ready
$(document).ready(function () {
    initializeComponents();
    updateCartCount();
});

// Initialize page components
function initializeComponents() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Add loading states to forms
    $('form').on('submit', function () {
        showLoading();
    });

    // Add smooth scrolling to anchor links
    $('a[href*="#"]').on('click', function (e) {
        var target = $(this.hash);
        if (target.length) {
            e.preventDefault();
            $('html, body').animate({
                scrollTop: target.offset().top - 100
            }, 800);
        }
    });
}

// Loading functions
function showLoading() {
    if (!isLoading) {
        isLoading = true;
        $('body').append('<div id="loading-overlay" class="position-fixed w-100 h-100 d-flex justify-content-center align-items-center" style="top: 0; left: 0; background: rgba(0,0,0,0.5); z-index: 9999;"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>');
    }
}

function hideLoading() {
    isLoading = false;
    $('#loading-overlay').remove();
}

// Cart functions
function updateCartCount() {
    $.get('/Cart/GetCartCount', function (data) {
        $('#cart-count').text(data.count);
        if (data.count > 0) {
            $('#cart-count').show();
        } else {
            $('#cart-count').hide();
        }
    }).fail(function () {
        console.log('Sepet sayısı güncellenemedi');
    });
}

function addToCart(bookId, quantity = 1) {
    if (isLoading) return;

    showLoading();

    $.post('/Books/AddToCart', {
        bookId: bookId,
        quantity: quantity
    }, function (response) {
        hideLoading();
        if (response.success) {
            updateCartCount();
            showToast('success', response.message);
        } else {
            showToast('error', response.message);
        }
    }).fail(function () {
        hideLoading();
        showToast('error', 'Sepete eklenirken bir hata oluştu.');
    });
}

function removeFromCart(bookId) {
    if (confirm('Bu ürünü sepetten çıkarmak istediğinizden emin misiniz?')) {
        showLoading();

        $.post('/Cart/RemoveItem', {
            bookId: bookId
        }, function (response) {
            hideLoading();
            if (response.success) {
                location.reload();
            } else {
                showToast('error', 'Ürün silinirken bir hata oluştu.');
            }
        }).fail(function () {
            hideLoading();
            showToast('error', 'Ürün silinirken bir hata oluştu.');
        });
    }
}

function updateQuantity(bookId, quantity) {
    if (quantity < 1) {
        removeFromCart(bookId);
        return;
    }

    $.post('/Cart/UpdateQuantity', {
        bookId: bookId,
        quantity: quantity
    }, function (response) {
        if (response.success) {
            location.reload();
        }
    });
}

// Favorite functions
function addToFavorites(bookId) {
    if (isLoading) return;

    showLoading();

    $.post('/Books/AddToFavorites', {
        bookId: bookId
    }, function (response) {
        hideLoading();
        if (response.success) {
            showToast('success', response.message);
            // Update favorite button state
            updateFavoriteButton(bookId, true);
        } else {
            showToast('error', response.message);
        }
    }).fail(function () {
        hideLoading();
        showToast('error', 'Favorilere eklenirken bir hata oluştu.');
    });
}

function updateFavoriteButton(bookId, isFavorite) {
    const btn = $(`.favorite-btn[data-book-id="${bookId}"]`);
    if (isFavorite) {
        btn.removeClass('btn-outline-danger').addClass('btn-danger');
        btn.find('i').removeClass('bi-heart').addClass('bi-heart-fill');
    } else {
        btn.removeClass('btn-danger').addClass('btn-outline-danger');
        btn.find('i').removeClass('bi-heart-fill').addClass('bi-heart');
    }
}

// Toast notification function
function showToast(type, message, duration = 3000) {
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const icon = type === 'success' ? 'bi-check-circle' : 'bi-exclamation-triangle';

    const toast = `
        <div class="alert ${alertClass} alert-dismissible fade show position-fixed animate__animated animate__fadeInRight" 
             style="top: 20px; right: 20px; z-index: 1060; min-width: 300px;" role="alert">
            <i class="bi ${icon} me-2"></i>${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>`;

    $('body').append(toast);

    // Auto remove after duration
    setTimeout(function () {
        $('.alert').addClass('animate__fadeOutRight');
        setTimeout(function () {
            $('.alert').alert('close');
        }, 500);
    }, duration);
}

// Search function
function performSearch() {
    const searchTerm = $('#search-input').val();
    if (searchTerm.trim() !== '') {
        window.location.href = `/Books?searchTerm=${encodeURIComponent(searchTerm)}`;
    }
}

// Filter functions
function filterByCategory(categoryId) {
    const currentUrl = new URL(window.location);
    if (categoryId) {
        currentUrl.searchParams.set('categoryId', categoryId);
    } else {
        currentUrl.searchParams.delete('categoryId');
    }
    window.location.href = currentUrl.toString();
}

function sortBooks(sortBy) {
    const currentUrl = new URL(window.location);
    currentUrl.searchParams.set('sortBy', sortBy);
    window.location.href = currentUrl.toString();
}

// Image handling
function handleImageError(img) {
    img.src = '/images/no-image.png';
    img.alt = 'Resim bulunamadı';
}

// Form validation
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return false;
    }
    return true;
}

// Admin functions
function confirmDelete(message = 'Bu öğeyi silmek istediğinizden emin misiniz?') {
    return confirm(message);
}

// Price formatting
function formatPrice(price) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(price);
}

// Date formatting
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('tr-TR');
}

// Utility functions
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Back to top button
$(window).scroll(function () {
    if ($(this).scrollTop() > 100) {
        $('#back-to-top').fadeIn();
    } else {
        $('#back-to-top').fadeOut();
    }
});

// Add back to top button to page
$(document).ready(function () {
    $('body').append('<button id="back-to-top" class="btn btn-primary position-fixed" style="bottom: 20px; right: 20px; display: none; z-index: 1000; border-radius: 50%; width: 50px; height: 50px;"><i class="bi bi-arrow-up"></i></button>');

    $('#back-to-top').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 800);
    });
});