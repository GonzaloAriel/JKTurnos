/*
   JKTurnos - Pagos (listado) JavaScript
   Autocomplete de turnos para el registro de pagos (D-030) + auto-remove de toasts.
*/

(function () {
    'use strict';

    document.querySelectorAll('.jk-toast').forEach(function (toast) {
        setTimeout(function () { toast.remove(); }, 4000);
    });

    var register = document.querySelector('.jk-pago-register');
    if (!register) return;

    var input = document.getElementById('turnoBusqueda');
    var hidden = document.getElementById('TurnoId');
    var list = document.getElementById('turnoResultados');
    var url = register.getAttribute('data-buscar-turnos-url');
    if (!input || !hidden || !list || !url) return;

    var timer = null;

    function hideResults() {
        list.hidden = true;
        list.innerHTML = '';
    }

    input.addEventListener('input', function () {
        hidden.value = '0';

        var termino = input.value.trim();
        if (termino.length < 2) {
            hideResults();
            return;
        }

        clearTimeout(timer);
        timer = setTimeout(function () {
            fetch(url + '?termino=' + encodeURIComponent(termino))
                .then(function (res) { return res.json(); })
                .then(function (items) {
                    list.innerHTML = '';

                    if (!items || items.length === 0) {
                        var empty = document.createElement('li');
                        empty.className = 'jk-autocomplete-empty';
                        empty.textContent = 'Sin resultados';
                        list.appendChild(empty);
                    } else {
                        items.forEach(function (item) {
                            var li = document.createElement('li');
                            li.className = 'jk-autocomplete-item';
                            li.textContent = item.label;
                            li.setAttribute('data-id', item.id);
                            li.setAttribute('role', 'option');
                            li.addEventListener('click', function () {
                                hidden.value = item.id;
                                input.value = item.label;
                                hideResults();
                            });
                            list.appendChild(li);
                        });
                    }

                    list.hidden = false;
                })
                .catch(function () { hideResults(); });
        }, 200);
    });

    document.addEventListener('click', function (e) {
        if (!list.hidden && !e.target.closest('.jk-turno-busqueda')) {
            hideResults();
        }
    });
})();

/*
 * Drawer de detalle del turno desde el listado de Pagos ("Ver turno").
 * Abre el mismo _TurnoDetailDrawer que la Agenda (via GetDetailDrawer) sin
 * abandonar la seccion Pagos. Replica el manejo de pagos del drawer.
 */
(function () {
    'use strict';

    var drawer = document.getElementById('drawer-pago-turno');
    var backdrop = document.getElementById('drawer-pago-turno-backdrop');
    var drawerBody = document.getElementById('drawer-pago-turno-body');
    var closeBtn = document.getElementById('drawer-pago-turno-close');
    if (!drawer || !backdrop || !drawerBody || !closeBtn) return;

    var detailUrl = drawer.getAttribute('data-turno-detail-url');
    var registrarPagoUrl = drawer.getAttribute('data-registrar-pago-url');
    var currentTurnoId = null;

    function showToast(message, type) {
        var container = document.getElementById('jk-toast-pagos');
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

    /*
     * Acciones contextuales de estado (P5/D-040) desde el Drawer de detalle
     * (seccion Pagos reutiliza _TurnoDetailDrawer). fetch POST + anti-forgery
     * (token dentro del drawer) + reload de la pagina para reflejar el estado.
     */
    function aplicarAccionEstado(btn) {
        var accionUrl = btn.getAttribute('data-url');
        var turnoId = btn.getAttribute('data-turno');
        var token = drawerBody.querySelector('input[name="__RequestVerificationToken"]');
        if (!accionUrl || !turnoId || !token) return;

        var body = '__RequestVerificationToken=' + encodeURIComponent(token.value) +
                   '&turnoId=' + encodeURIComponent(turnoId);

        btn.disabled = true;

        fetch(accionUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: body
        })
        .then(function (response) { return response.json(); })
        .then(function (data) {
            btn.disabled = false;
            if (data.success) {
                showToast('Estado actualizado', 'success');
                window.location.reload();
            } else {
                showToast(data.message || 'No se pudo realizar la accion', 'error');
            }
        })
        .catch(function () {
            btn.disabled = false;
            showToast('Error de conexion al realizar la accion', 'error');
        });
    }

    document.addEventListener('click', function (e) {
        var verBtn = e.target.closest('[data-ver-turno]');
        if (verBtn) {
            e.preventDefault();
            loadDrawer(verBtn.getAttribute('data-ver-turno'));
            return;
        }

        if (!drawer.classList.contains('open')) return;

        var estadoBtn = e.target.closest('[data-accion]');
        if (estadoBtn) {
            e.preventDefault();
            aplicarAccionEstado(estadoBtn);
            return;
        }

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