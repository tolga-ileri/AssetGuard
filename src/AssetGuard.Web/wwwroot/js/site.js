document.addEventListener('DOMContentLoaded', function () {
    initTheme();
    initSidebar();
    initConfirmModal();
});

function initTheme() {
    const root = document.documentElement;
    const toggle = document.getElementById('themeToggle');
    const icon = document.getElementById('themeIcon');
    const stored = localStorage.getItem('assetguard-theme');

    if (stored === 'dark' || stored === 'light') {
        root.setAttribute('data-bs-theme', stored);
    }

    function updateIcon() {
        if (!icon) return;
        const isDark = root.getAttribute('data-bs-theme') === 'dark';
        icon.className = isDark ? 'bi bi-sun' : 'bi bi-moon-stars';
    }

    updateIcon();

    toggle?.addEventListener('click', function () {
        const isDark = root.getAttribute('data-bs-theme') === 'dark';
        const next = isDark ? 'light' : 'dark';
        root.setAttribute('data-bs-theme', next);
        localStorage.setItem('assetguard-theme', next);
        updateIcon();
    });
}

function initSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const toggleBtn = document.querySelector('[data-sidebar-toggle]');

    function closeSidebar() {
        sidebar?.classList.remove('show');
        overlay?.classList.remove('show');
    }

    toggleBtn?.addEventListener('click', function () {
        if (sidebar?.classList.contains('show')) closeSidebar();
        else {
            sidebar?.classList.add('show');
            overlay?.classList.add('show');
        }
    });

    overlay?.addEventListener('click', closeSidebar);

    document.querySelectorAll('.sidebar-nav .nav-link').forEach(function (link) {
        link.addEventListener('click', function () {
            if (window.innerWidth < 992) closeSidebar();
        });
    });
}

function initConfirmModal() {
    const modal = document.getElementById('confirmModal');
    if (!modal) return;

    const bsModal = new bootstrap.Modal(modal);
    let pendingForm = null;

    modal.addEventListener('hidden.bs.modal', function () {
        pendingForm = null;
        const btn = document.getElementById('confirmModalBtn');
        if (btn) {
            btn.className = 'btn btn-danger';
            btn.textContent = 'Confirm';
            btn.onclick = null;
        }
    });

    document.querySelectorAll('[data-confirm]').forEach(function (el) {
        if (el.tagName === 'A') {
            el.addEventListener('click', function (e) {
                e.preventDefault();
                document.getElementById('confirmModalTitle').textContent = el.dataset.confirmTitle || 'Confirm Action';
                document.getElementById('confirmModalBody').textContent = el.dataset.confirm || 'Are you sure?';
                const btn = document.getElementById('confirmModalBtn');
                btn.className = el.dataset.confirmClass || 'btn btn-danger';
                btn.textContent = el.dataset.confirmBtn || 'Confirm';
                pendingForm = null;
                btn.onclick = function () { window.location.href = el.href; };
                bsModal.show();
            });
        }
    });

    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            pendingForm = form;
            document.getElementById('confirmModalTitle').textContent = form.dataset.confirmTitle || 'Confirm Action';
            document.getElementById('confirmModalBody').textContent = form.dataset.confirm || 'Are you sure?';
            const btn = document.getElementById('confirmModalBtn');
            btn.className = form.dataset.confirmClass || 'btn btn-danger';
            btn.textContent = form.dataset.confirmBtn || 'Confirm';
            btn.onclick = function () {
                if (pendingForm) pendingForm.submit();
            };
            bsModal.show();
        });
    });
}

// Page loading indicator
document.addEventListener('click', function (e) {
    const link = e.target.closest('a[href]:not([target="_blank"]):not([download])');
    if (link && link.hostname === window.location.hostname && !link.getAttribute('href')?.startsWith('#')) {
        document.body.classList.add('page-loading');
    }
});

window.addEventListener('pageshow', function () {
    document.body.classList.remove('page-loading');
});
