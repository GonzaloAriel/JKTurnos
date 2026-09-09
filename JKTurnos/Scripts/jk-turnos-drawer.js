/*
   JKTurnos - Drawer de detalle del turno (compartido).
   Abre el mismo _TurnoDetailDrawer que la Agenda/dashboard/pagos (via
   GetDetailDrawer) desde:
     - el autocomplete global del topbar (drawer global en _Layout)
     - la grilla de Turnos y la pagina de resultados de Busqueda (drawer local)
   Replica el manejo de pagos del drawer (D-003 / D-030).

   API global: window.openTurnDrawer(id) -> abre el drawer global del layout.
   El drawer local (si esta presente en la pagina) responde a [data-ver-turno].
*/

(function () {
    'use strict';

    function makeDrawer(id) {
        var drawer = document.getElementById(id);
        var backdrop = document.getElementById(id + '-backdrop');
        var drawerBody = document.getElementById(id + '-body');
        var closeBtn = document.getElementById(id + '-close');
        if (!drawer || !backdrop || !drawerBody || !closeBtn) return null;

        var detailUrl = drawer.getAttribute('data-turno-detail-url');
        var registrarPagoUrl = drawer.getAttribute('data-registrar-pago-url');
        var toastId = drawer.getAttribute('data-toast-id');
        var currentTurnoId = null;

        function showToast(message, type) {
            var container = document.getElementById(toastId);
            if (!container) return;
            var toast = document.createElement('div');
            toast.className = 'jk-toast jk-toast-' + type;
            var icon = document.createElement('i');
            icon.className = 'bi bi-' + (type === 'success' ? 'check-circle' : 'exclamation-circle');
            toast.appendChild(icon);
            toast.appendChild(document.createTextNode(' ' + message));
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

        function loadDrawer(turnoId) {
            if (!turnoId || !detailUrl) return;

            currentTurnoId = turnoId;
            drawerBody.innerHTML = '<div class="jk-drawer-loading">Cargando detalle&hellip;</div>';
            drawer.classList.add('open');
            backdrop.classList.add('open');
            drawer.setAttribute('aria-hidden', 'false');
            document.body.classList.add('jk-drawer-open');

            fetch(detailUrl + '?id=' + encodeURIComponent(turnoId))
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

        if (id.indexOf('-local') === -1) {
            // El drawer global se abre SOLO via window.openTurnDrawer (autocomplete).
            // No responde a [data-ver-turno] para no pisar dashboard/agenda/pagos.
            closeBtn.addEventListener('click', closeTurnoDetail);
            backdrop.addEventListener('click', closeTurnoDetail);
            document.addEventListener('keydown', function (e) {
                if ((e.key === 'Escape' || e.keyCode === 27) && drawer.classList.contains('open')) {
                    closeTurnoDetail();
                }
            });
            return {
                open: loadDrawer,
                close: closeTurnoDetail
            };
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

        return {
            open: loadDrawer,
            close: closeTurnoDetail
        };
    }

    // Drawer global del layout (autocomplete del topbar)
    var globalDrawer = makeDrawer('drawer-turno-global');
    window.openTurnDrawer = function (id) {
        if (globalDrawer && globalDrawer.open) {
            globalDrawer.open(id);
        }
    };

    // Drawer local de la pagina (grilla de Turnos / Busqueda)
    makeDrawer('drawer-turno-local');
})();
