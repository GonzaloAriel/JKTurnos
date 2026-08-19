/*
   JKTurnos - Agenda Grid JavaScript
   Scroll sync, Time Block interaction and Turno Detail Drawer (V1 read-only).
*/

(function () {
    'use strict';

    var agenda = document.querySelector('.jk-agenda');
    if (!agenda) return;

    var gridWrap = agenda.querySelector('.jk-agenda-grid-wrap');
    var detailUrl = agenda.getAttribute('data-turno-detail-url');

    /*
     * Turno Detail Drawer (V1)
     * Click / Enter / Space on a Time Block loads _TurnoDetailDrawer into the
     * drawer container via fetch and opens it. V2/V3 (state actions, economics)
     * are registered in Docs/16 as future work. Open/close works for both the
     * desktop grid blocks and the mobile accordion cards (_AgendaMobile).
     */
    var drawerId = 'drawer-turno-detalle';
    var drawer = document.getElementById(drawerId);
    var backdrop = document.getElementById(drawerId + '-backdrop');
    var drawerBody = document.getElementById(drawerId + '-body');
    var closeBtn = document.getElementById(drawerId + '-close');

    if (drawer && backdrop && drawerBody) {
        var lastFocusedBlock = null;
        var currentTurnoId = null;
        var registrarPagoUrl = drawer.getAttribute('data-registrar-pago-url');

        function showToast(message, type) {
            var container = document.getElementById('jk-toast-agenda');
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

        function openTurnoDetail(block, focusPagos) {
            var id = block.getAttribute('data-turno-id');
            if (!id || !detailUrl) return;

            lastFocusedBlock = block;
            currentTurnoId = id;
            drawerBody.innerHTML = '<div class="jk-drawer-loading">Cargando detalle&hellip;</div>';
            drawer.classList.add('open');
            backdrop.classList.add('open');
            drawer.setAttribute('aria-hidden', 'false');

            fetch(detailUrl + '?id=' + encodeURIComponent(id))
                .then(function (res) {
                    if (!res.ok) throw new Error('HTTP ' + res.status);
                    return res.text();
                })
                .then(function (html) {
                    drawerBody.innerHTML = html;
                    closeBtn.focus();
                    if (focusPagos) openPagoForm('PagoParcial');
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
            drawerBody.innerHTML = '';
            currentTurnoId = null;
            if (lastFocusedBlock) {
                lastFocusedBlock.focus();
                lastFocusedBlock = null;
            }
        }

        agenda.addEventListener('click', function (e) {
            var pagoBtn = e.target.closest('.jk-agenda-time-block-pago');
            if (pagoBtn) {
                e.preventDefault();
                e.stopPropagation();
                var pagoBlock = pagoBtn.closest('.jk-agenda-time-block, .jk-agenda-week-block');
                if (pagoBlock) openTurnoDetail(pagoBlock, true);
                return;
            }
            var block = e.target.closest('.jk-agenda-time-block, .jk-agenda-week-block');
            if (block) {
                openTurnoDetail(block);
                return;
            }
            var cell = e.target.closest('.jk-agenda-cell');
            if (cell) {
                cell.focus();
            }
        });

        if (gridWrap) {
            gridWrap.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
                    var pagoBtn = e.target.closest('.jk-agenda-time-block-pago');
                    if (pagoBtn) {
                        e.preventDefault();
                        var pagoBlock = pagoBtn.closest('.jk-agenda-time-block');
                        if (pagoBlock) openTurnoDetail(pagoBlock, true);
                        return;
                    }
                    var block = e.target.closest('.jk-agenda-time-block');
                    if (block) {
                        e.preventDefault();
                        openTurnoDetail(block);
                        return;
                    }
                }

                var step = 80;
                switch (e.key) {
                    case 'ArrowLeft':
                        gridWrap.scrollLeft -= step;
                        e.preventDefault();
                        break;
                    case 'ArrowRight':
                        gridWrap.scrollLeft += step;
                        e.preventDefault();
                        break;
                    case 'ArrowUp':
                        gridWrap.scrollTop -= step;
                        e.preventDefault();
                        break;
                    case 'ArrowDown':
                        gridWrap.scrollTop += step;
                        e.preventDefault();
                        break;
                }
            });

            /*
             * Focus trap for grid cells: Tab moves between cells,
             * Shift+Tab moves backwards.
             */
            var cells = gridWrap.querySelectorAll('.jk-agenda-cell');
            cells.forEach(function (cell) {
                cell.setAttribute('tabindex', '-1');
            });
        }

        /*
         * Weekly grid keyboard: Enter/Space opens the turno detail drawer
         * from a week block (same behavior as the day grid time blocks).
         */
        var weekWrap = agenda.querySelector('.jk-agenda-week');
        if (weekWrap) {
            weekWrap.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
                    var pagoBtn = e.target.closest('.jk-agenda-time-block-pago');
                    if (pagoBtn) {
                        e.preventDefault();
                        var pagoBlock = pagoBtn.closest('.jk-agenda-week-block');
                        if (pagoBlock) openTurnoDetail(pagoBlock, true);
                        return;
                    }
                    var block = e.target.closest('.jk-agenda-week-block');
                    if (block) {
                        e.preventDefault();
                        openTurnoDetail(block);
                    }
                }
            });
        }

        closeBtn.addEventListener('click', closeTurnoDetail);
        backdrop.addEventListener('click', closeTurnoDetail);

        /*
         * Auto-open del drawer al llegar con openTurnoId (link "Ver turno"
         * desde el listado de Pagos). El bloque debe estar visible en la vista.
         */
        var openTurnoId = agenda.getAttribute('data-open-turno-id');
        if (openTurnoId) {
            var target = agenda.querySelector('[data-turno-id="' + openTurnoId + '"]');
            if (target) {
                openTurnoDetail(target);
            } else {
                showToast('El turno no se encuentra en esta vista', 'info');
            }
        }

        /*
         * V3 payments: "Registrar senia"/"Registrar pago" open the inline form,
         * submit via fetch (anti-forgery) and refresh the drawer (D-028/D-016).
         */
        drawer.addEventListener('click', function (e) {
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

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && drawer.classList.contains('open')) {
                closeTurnoDetail();
            }
        });
    }

    /*
     * Recursos filter dropdown -> oculta/muestra las tarjetas del acordeon
     * (_AgendaMobile) segun el checkbox y filtra la grilla desktop (_AgendaGrid,
     * _AgendaGridSemanal): las columnas del recurso desmarcado se colapsan y sus
     * Time Blocks desaparecen.
     */
    var filterList = document.querySelector('.jk-agenda-filter-list');
    if (filterList) {
        filterList.addEventListener('change', function (e) {
            var input = e.target.closest('input[data-recurso-id]');
            if (!input) return;

            var id = input.getAttribute('data-recurso-id');
            var cards = document.querySelectorAll('.jk-agenda-mobile-resource[data-recurso-id="' + id + '"]');
            cards.forEach(function (card) {
                card.hidden = !input.checked;
            });

            applyDesktopResourceFilter();
        });
    }

    function applyDesktopResourceFilter() {
        var inputs = Array.prototype.slice.call(document.querySelectorAll('.jk-agenda-filter-list input[data-recurso-id]'));
        var checked = inputs
            .filter(function (i) { return i.checked; })
            .map(function (i) { return i.getAttribute('data-recurso-id'); });

        var count = document.querySelector('.jk-agenda-filter-count');
        if (count) { count.textContent = checked.length; }

        var weekBlocks = document.querySelectorAll('.jk-agenda-week-block');
        weekBlocks.forEach(function (b) {
            b.classList.toggle('is-filtered', checked.indexOf(b.getAttribute('data-recurso-id')) === -1);
        });

        var grid = document.querySelector('.jk-agenda-grid');
        if (!grid) { return; }

        var headers = Array.prototype.slice.call(grid.querySelectorAll('.jk-agenda-resource-header'));
        var visible = headers
            .map(function (h) { return h.getAttribute('data-recurso-id'); })
            .filter(function (rid) { return checked.indexOf(rid) !== -1; });

        grid.style.setProperty('--jk-resource-count', visible.length);

        headers.forEach(function (h) {
            var rid = h.getAttribute('data-recurso-id');
            var idx = visible.indexOf(rid);
            if (idx === -1) {
                h.classList.add('is-filtered');
            } else {
                h.classList.remove('is-filtered');
                h.style.gridColumn = idx + 2;
            }
        });

        var cells = grid.querySelectorAll('.jk-agenda-cell');
        cells.forEach(function (c) {
            var rid = c.getAttribute('data-recurso-id');
            var idx = visible.indexOf(rid);
            if (idx === -1) {
                c.classList.add('is-filtered');
            } else {
                c.classList.remove('is-filtered');
                c.style.gridColumn = idx + 2;
            }
        });

        var blocks = grid.querySelectorAll('.jk-agenda-time-block');
        blocks.forEach(function (b) {
            var rid = b.getAttribute('data-recurso-id');
            var idx = visible.indexOf(rid);
            if (idx === -1) {
                b.classList.add('is-filtered');
            } else {
                b.classList.remove('is-filtered');
                b.style.gridColumn = idx + 2;
            }
        });
    }

})();
