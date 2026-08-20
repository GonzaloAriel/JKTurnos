/* 
   JKTurnos - Layout Scripts
   Vanilla JS. Sidebar: Desktop collapse, Tablet expand, Mobile drawer.
   Dependencies: None (jQuery not required).
*/

(function () {
    'use strict';

    var sidebar = document.getElementById('sidebar');
    var overlay = document.getElementById('sidebar-overlay');
    var hamburger = document.getElementById('mobile-menu-toggle');
    var collapseBtn = document.getElementById('sidebar-collapse-toggle');
    var appContainer = document.getElementById('app-container');
    var body = document.body;
    var BREAKPOINT_TABLET = 768;
    var BREAKPOINT_DESKTOP = 1200;
    var STORAGE_KEY = 'jk-sidebar-collapsed';

    function getWidth() {
        return window.innerWidth || document.documentElement.clientWidth;
    }

    function isMobile() {
        return getWidth() < BREAKPOINT_TABLET;
    }

    function isTablet() {
        return getWidth() >= BREAKPOINT_TABLET && getWidth() < BREAKPOINT_DESKTOP;
    }

    function isDesktop() {
        return getWidth() >= BREAKPOINT_DESKTOP;
    }

    function initSidebar() {
        if (!sidebar) return;

        if (isMobile()) {
            sidebar.classList.remove('is-collapsed');
            sidebar.classList.remove('is-open');
            if (overlay) overlay.classList.remove('visible');
            body.classList.remove('drawer-open');
            if (hamburger) hamburger.setAttribute('aria-expanded', 'false');
        } else if (isTablet()) {
            sidebar.classList.remove('is-open');
            if (overlay) overlay.classList.remove('visible');
            body.classList.remove('drawer-open');
            sidebar.classList.add('is-collapsed');
            if (hamburger) hamburger.setAttribute('aria-expanded', 'false');
        } else if (isDesktop()) {
            sidebar.classList.remove('is-open');
            if (overlay) overlay.classList.remove('visible');
            body.classList.remove('drawer-open');
            var saved = localStorage.getItem(STORAGE_KEY);
            if (saved === 'true') {
                sidebar.classList.add('is-collapsed');
            } else {
                sidebar.classList.remove('is-collapsed');
            }
            updateCollapseBtn();
        }
    }

    function updateCollapseBtn() {
        if (!collapseBtn) return;
        var isCollapsed = sidebar.classList.contains('is-collapsed');
        collapseBtn.setAttribute('aria-expanded', isCollapsed ? 'false' : 'true');
        collapseBtn.setAttribute('aria-label', isCollapsed ? 'Expandir sidebar' : 'Colapsar sidebar');
        collapseBtn.title = isCollapsed ? 'Expandir sidebar' : 'Colapsar sidebar';
    }

    function toggleDesktopCollapse() {
        if (!sidebar || !isDesktop()) return;
        var isCollapsed = sidebar.classList.toggle('is-collapsed');
        localStorage.setItem(STORAGE_KEY, isCollapsed ? 'true' : 'false');
        updateCollapseBtn();
    }

    function openMobileDrawer() {
        if (!sidebar || !overlay) return;
        sidebar.classList.add('is-open');
        overlay.classList.add('visible');
        body.classList.add('drawer-open');
        if (hamburger) {
            hamburger.setAttribute('aria-expanded', 'true');
            hamburger.setAttribute('aria-label', 'Cerrar menu');
        }
        sidebar.setAttribute('aria-hidden', 'false');
        focusFirstItem();
    }

    function closeMobileDrawer() {
        if (!sidebar || !overlay) return;
        sidebar.classList.remove('is-open');
        overlay.classList.remove('visible');
        body.classList.remove('drawer-open');
        if (hamburger) {
            hamburger.setAttribute('aria-expanded', 'false');
            hamburger.setAttribute('aria-label', 'Abrir menu');
        }
        sidebar.setAttribute('aria-hidden', 'true');
        if (hamburger) hamburger.focus();
    }

    function toggleTabletSidebar() {
        if (!sidebar || !isTablet()) return;
        var isCollapsed = sidebar.classList.toggle('is-collapsed');
        if (hamburger) {
            hamburger.setAttribute('aria-expanded', isCollapsed ? 'false' : 'true');
            hamburger.setAttribute('aria-label', isCollapsed ? 'Abrir menu' : 'Cerrar menu');
        }
        if (!isCollapsed) {
            focusFirstItem();
        }
    }

    function focusFirstItem() {
        if (!sidebar) return;
        var firstItem = sidebar.querySelector('[data-sidebar-item]');
        if (firstItem) {
            firstItem.setAttribute('tabindex', '0');
            firstItem.focus();
        }
    }

    function handleHamburgerClick(e) {
        e.preventDefault();
        if (isMobile()) {
            if (sidebar.classList.contains('is-open')) {
                closeMobileDrawer();
            } else {
                openMobileDrawer();
            }
        } else if (isTablet()) {
            toggleTabletSidebar();
        }
    }

    function handleOverlayClick(e) {
        e.preventDefault();
        if (isMobile() && sidebar.classList.contains('is-open')) {
            closeMobileDrawer();
        }
    }

    function handleSidebarItemClick(e) {
        if (isMobile() && sidebar.classList.contains('is-open')) {
            var target = e.currentTarget;
            var href = target.getAttribute('href');
            if (href && href !== '#') {
                closeMobileDrawer();
            }
        }
    }

    function handleEscapeKey(e) {
        if (e.key === 'Escape' || e.keyCode === 27) {
            if (isMobile() && sidebar.classList.contains('is-open')) {
                closeMobileDrawer();
                e.preventDefault();
            }
        }
    }

    function handleCollapseBtnClick(e) {
        e.preventDefault();
        if (isDesktop()) {
            toggleDesktopCollapse();
        }
    }

    function handleResize() {
        initSidebar();
        if (isMobile()) {
            sidebar.setAttribute('aria-hidden', 'true');
        } else {
            sidebar.setAttribute('aria-hidden', 'false');
        }
    }

    function init() {
        if (!sidebar) return;

        initSidebar();

        if (isMobile()) {
            sidebar.setAttribute('aria-hidden', 'true');
        } else {
            sidebar.setAttribute('aria-hidden', 'false');
        }

        if (hamburger) {
            hamburger.addEventListener('click', handleHamburgerClick);
        }

        if (overlay) {
            overlay.addEventListener('click', handleOverlayClick);
        }

        if (collapseBtn) {
            collapseBtn.addEventListener('click', handleCollapseBtnClick);
        }

        var items = sidebar.querySelectorAll('[data-sidebar-item]');
        for (var i = 0; i < items.length; i++) {
            items[i].addEventListener('click', handleSidebarItemClick);
        }

        document.addEventListener('keydown', handleEscapeKey);

        var resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(handleResize, 150);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();

/*
 * D-033 Fase 2: recordatorio por email desde el drawer del turno.
 * Handler global (jk-layout.js se carga en todas las paginas) con delegacion
 * en document, asi funciona en Agenda, Pagos y Dashboard sin duplicar logica.
 * El boton ([data-enviar-email]) vive dentro de _TurnoDetailDrawer.
 */
(function () {
    'use strict';

    function showToast(message, type) {
        var container = document.querySelector('.jk-toast-container');
        if (!container) return;
        var toast = document.createElement('div');
        toast.className = 'jk-toast jk-toast-' + type;
        toast.innerHTML = '<i class="bi bi-' + (type === 'success' ? 'check-circle' : 'exclamation-circle')
            + '"></i> ' + message;
        container.appendChild(toast);
        setTimeout(function () { toast.remove(); }, 4000);
    }

    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-enviar-email]');
        if (!btn || btn.disabled) return;

        e.preventDefault();

        var turnoId = btn.getAttribute('data-enviar-email');
        var url = btn.getAttribute('data-enviar-email-url');
        if (!turnoId || !url) return;

        var drawer = btn.closest('.jk-drawer');
        var tokenEl = drawer
            ? drawer.querySelector('input[name="__RequestVerificationToken"]')
            : null;
        var token = tokenEl ? tokenEl.value : '';

        btn.disabled = true;
        var originalLabel = btn.innerHTML;
        btn.innerHTML = '<i class="bi bi-envelope"></i> Enviando&hellip;';

        var data = new FormData();
        data.append('__RequestVerificationToken', token);
        data.append('turnoId', turnoId);

        function resetBtn() {
            btn.disabled = false;
            btn.innerHTML = originalLabel;
        }

        fetch(url, {
            method: 'POST',
            body: data
        })
        .then(function (res) { return res.json(); })
        .then(function (json) {
            if (json && json.success) {
                showToast(json.message || 'Recordatorio enviado por email', 'success');
            } else {
                showToast(json && json.message ? json.message : 'No se pudo enviar el correo', 'error');
            }
            resetBtn();
        })
        .catch(function () {
            showToast('No se pudo enviar el correo', 'error');
            resetBtn();
        });
    });
})();

/*
 * Notificaciones del sistema (feed in-app):
 * - Toggle del dropdown de la campanita.
 * - "Marcar todas leidas" (POST ajax + antiforgery).
 * - Cierre con click afuera o Escape.
 */
(function () {
    'use strict';

    var container = document.getElementById('topbar-notif');
    if (!container) return;

    var toggle = document.getElementById('topbar-notif-toggle');
    var panel = document.getElementById('topbar-notif-panel');
    var markReadBtn = document.getElementById('topbar-notif-markread');

    function showToast(message, type) {
        var toastContainer = document.querySelector('.jk-toast-container');
        if (!toastContainer) return;
        var toast = document.createElement('div');
        toast.className = 'jk-toast jk-toast-' + type;
        toast.innerHTML = '<i class="bi bi-' + (type === 'success' ? 'check-circle' : 'exclamation-circle')
            + '"></i> ' + message;
        toastContainer.appendChild(toast);
        setTimeout(function () { toast.remove(); }, 4000);
    }

    function setOpen(open) {
        panel.setAttribute('aria-hidden', open ? 'false' : 'true');
        toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
    }

    function isOpen() {
        return panel.getAttribute('aria-hidden') === 'false';
    }

    toggle.addEventListener('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        setOpen(!isOpen());
    });

    document.addEventListener('click', function (e) {
        if (isOpen() && !container.contains(e.target)) {
            setOpen(false);
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' || e.keyCode === 27) {
            if (isOpen()) setOpen(false);
        }
    });

    function getToken() {
        var tokenEl = container.querySelector('input[name="__RequestVerificationToken"]');
        return tokenEl ? tokenEl.value : '';
    }

    function clearReminderUI() {
        var badge = document.getElementById('topbar-notif-badge');
        if (badge) badge.remove();

        var recSec = document.getElementById('topbar-recordatorios-section');
        if (recSec) recSec.remove();

        if (markReadBtn) markReadBtn.disabled = true;

        var list = panel.querySelector('.jk-notif-list');
        if (list && !list.querySelector('.jk-notif-empty')) {
            var empty = document.createElement('div');
            empty.className = 'jk-notif-empty';
            empty.textContent = 'No hay recordatorios pendientes';
            list.appendChild(empty);
        }
    }

    function decrementBadge() {
        var badge = document.getElementById('topbar-notif-badge');
        if (!badge) return;
        var n = parseInt(badge.textContent, 10) - 1;
        if (n > 0) {
            badge.textContent = n;
        } else {
            badge.remove();
        }
    }

    // Descartar un recordatorio (boton "x"): se quita de a uno y baja el
    // contador de la campanita. Al quedar vacio, se muestra el estado vacio
    // y se deshabilita "Marcar todas leidas".
    panel.addEventListener('click', function (e) {
        var btn = e.target.closest ? e.target.closest('[data-descartar-recordatorio]') : null;
        if (!btn) return;

        e.preventDefault();
        e.stopPropagation();

        var url = btn.getAttribute('data-descartar-url');
        var clave = btn.getAttribute('data-clave');
        if (!url || !clave) return;

        var data = new FormData();
        data.append('__RequestVerificationToken', getToken());
        data.append('clave', clave);

        fetch(url, {
            method: 'POST',
            body: data
        })
        .then(function (res) { return res.json(); })
        .then(function (json) {
            if (json && json.success) {
                var item = btn.closest('.jk-recordatorio');
                if (item) item.remove();
                decrementBadge();
                if (!panel.querySelector('.jk-recordatorio')) {
                    clearReminderUI();
                }
                showToast('Recordatorio descartado', 'success');
            } else {
                showToast('No se pudo descartar el recordatorio', 'error');
            }
        })
        .catch(function () {
            showToast('No se pudo descartar el recordatorio', 'error');
        });
    });

    if (markReadBtn) {
        markReadBtn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            var url = markReadBtn.getAttribute('data-marcar-notif-url');
            if (!url) return;

            var data = new FormData();
            data.append('__RequestVerificationToken', getToken());

            fetch(url, {
                method: 'POST',
                body: data
            })
            .then(function (res) { return res.json(); })
            .then(function (json) {
                if (json && json.success) {
                    clearReminderUI();
                    showToast('Todas las notificaciones marcadas como leidas', 'success');
                } else {
                    showToast('No se pudo actualizar el estado', 'error');
                }
            })
            .catch(function () {
                showToast('No se pudo actualizar el estado', 'error');
            });
        });
    }
})();
