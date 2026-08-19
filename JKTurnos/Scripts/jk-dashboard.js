/*
   JKTurnos - Dashboard JavaScript
   Drawer de detalle del turno desde las cards "Proximos turnos" y "Sin confirmar"
   (data-ver-turno). Abre el mismo _TurnoDetailDrawer que la Agenda (via
   GetDetailDrawer) SIN abandonar la seccion Dashboard. Replica el manejo de
   pagos del drawer (D-003 / D-030).
*/

(function () {
    'use strict';

    var drawer = document.getElementById('drawer-dashboard-turno');
    var backdrop = document.getElementById('drawer-dashboard-turno-backdrop');
    var drawerBody = document.getElementById('drawer-dashboard-turno-body');
    var closeBtn = document.getElementById('drawer-dashboard-turno-close');
    if (!drawer || !backdrop || !drawerBody || !closeBtn) return;

    var detailUrl = drawer.getAttribute('data-turno-detail-url');
    var registrarPagoUrl = drawer.getAttribute('data-registrar-pago-url');
    var currentTurnoId = null;

    function showToast(message, type) {
        var container = document.getElementById('jk-toast-dashboard');
        if (!container) return;
        var toast = document.createElement('div');
        toast.className = 'jk-toast jk-toast-' + type;
        toast.innerHTML = '<i class="bi bi-' + (type === 'success' ? 'check-circle' : 'exclamation-circle')
            + '"></i> ' + message;
        container.appendChild(toast);
        setTimeout(function () { toast.remove(); }, 4000);
    }

    function openPagoForm(tipo) {
        var form = drawer.querySelector('.jk-pago-form');
        if (!form) return;

        var select = form.querySelector('select[name="TipoPago"]');
        if (select) select.value = tipo || 'PagoParcial';

        var errEl = form.querySelector('.jk-pago-error');
        if (errEl) { errEl.hidden = true; errEl.textContent = ''; }

        form.hidden = false;
        var monto = form.querySelector('input[name="Monto"]');
        if (monto) monto.focus();
    }

    function loadDrawer(id) {
        if (!id || !detailUrl) return;

        currentTurnoId = id;
        drawerBody.innerHTML = '<div class="jk-drawer-loading">Cargando detalle&hellip;</div>';
        drawer.classList.add('open');
        backdrop.classList.add('open');
        drawer.setAttribute('aria-hidden', 'false');
        document.body.classList.add('jk-drawer-open');

        fetch(detailUrl + '?id=' + encodeURIComponent(id))
            .then(function (res) {
                if (!res.ok) throw new Error('HTTP ' + res.status);
                return res.text();
            })
            .then(function (html) {
                drawerBody.innerHTML = html;
                closeBtn.focus();
            })
            .catch(function () {
                closeTurnoDetail();
                showToast('No se pudo cargar el detalle del turno', 'error');
            });
    }

    function refreshTurnoDetail() {
        if (!currentTurnoId || !detailUrl) return;

        fetch(detailUrl + '?id=' + encodeURIComponent(currentTurnoId))
            .then(function (res) {
                if (!res.ok) throw new Error('HTTP ' + res.status);
                return res.text();
            })
            .then(function (html) {
                drawerBody.innerHTML = html;
                closeBtn.focus();
            })
            .catch(function () {
                showToast('No se pudo actualizar el detalle del turno', 'error');
            });
    }

    function closeTurnoDetail() {
        drawer.classList.remove('open');
        backdrop.classList.remove('open');
        drawer.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('jk-drawer-open');
        drawerBody.innerHTML = '';
        currentTurnoId = null;
    }

    document.addEventListener('click', function (e) {
        var verBtn = e.target.closest('[data-ver-turno]');
        if (verBtn) {
            e.preventDefault();
            loadDrawer(verBtn.getAttribute('data-ver-turno'));
            return;
        }

        if (!drawer.classList.contains('open')) return;

        var accionBtn = e.target.closest('[data-pago-accion]');
        if (accionBtn) {
            e.preventDefault();
            var tipo = accionBtn.getAttribute('data-pago-accion') === 'Senia' ? 'Senia' : 'PagoParcial';
            openPagoForm(tipo);
            return;
        }

        var cancelarBtn = e.target.closest('[data-pago-cancelar]');
        if (cancelarBtn) {
            e.preventDefault();
            var cancelForm = cancelarBtn.closest('.jk-pago-form');
            if (cancelForm) cancelForm.hidden = true;
        }
    });

    drawer.addEventListener('submit', function (e) {
        var form = e.target.closest('.jk-pago-form');
        if (!form) return;

        e.preventDefault();

        var errEl = form.querySelector('.jk-pago-error');
        if (errEl) { errEl.hidden = true; errEl.textContent = ''; }

        var montoEl = form.querySelector('input[name="Monto"]');
        var monto = parseFloat(montoEl ? montoEl.value : 0);
        if (!monto || monto <= 0) {
            if (errEl) { errEl.textContent = 'El monto debe ser mayor a 0'; errEl.hidden = false; }
            return;
        }

        fetch(registrarPagoUrl, {
            method: 'POST',
            body: new FormData(form)
        })
        .then(function (res) { return res.json(); })
        .then(function (data) {
            if (data.success) {
                showToast('Pago registrado', 'success');
                refreshTurnoDetail();
            } else {
                var msg = (data.errors && data.errors[''])
                    ? data.errors['']
                    : 'No se pudo registrar el pago';
                if (errEl) { errEl.textContent = msg; errEl.hidden = false; }
            }
        })
        .catch(function () {
            if (errEl) { errEl.textContent = 'No se pudo registrar el pago'; errEl.hidden = false; }
        });
    });

    closeBtn.addEventListener('click', closeTurnoDetail);
    backdrop.addEventListener('click', closeTurnoDetail);

    document.addEventListener('keydown', function (e) {
        if ((e.key === 'Escape' || e.keyCode === 27) && drawer.classList.contains('open')) {
            closeTurnoDetail();
        }
    });
})();