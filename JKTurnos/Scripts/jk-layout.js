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
