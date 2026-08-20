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

    /*
     * Quick Create Turno Drawer (D-017)
     * Click en una celda libre de la grilla, en un slot libre mobile o en el
     * cuerpo de un dia semanal, abre un drawer con Cliente/Servicio/Recurso y
     * la fecha+hora (+recurso) de la celda prellenados. El envio es ajax
     * (patron QuickCreate D-013); al crear, recarga la agenda de la misma
     * fecha/vista y abre el detalle del turno nuevo.
     */
    var qcDrawer = document.getElementById('drawer-turno-quick');
    var qcBackdrop = document.getElementById('drawer-turno-quick-backdrop');
    var qcClose = document.getElementById('drawer-turno-quick-close');
    var qcForm = document.getElementById('form-turno-quick');
    var agendaFecha = agenda.getAttribute('data-agenda-fecha');
    var agendaVista = agenda.getAttribute('data-agenda-vista');
    var agendaBaseUrl = agenda.getAttribute('data-agenda-base-url');
    var quickCreateUrl = agenda.getAttribute('data-quick-create-url');
    var recursosUrl = agenda.getAttribute('data-recursos-url');
    var recursosTodosUrl = agenda.getAttribute('data-recursos-todos-url');

    function qcToast(message, type) {
        var container = document.getElementById('jk-toast-agenda');
        if (!container) return;
        var toast = document.createElement('div');
        toast.className = 'jk-toast jk-toast-' + type;
        toast.innerHTML = '<i class="bi bi-' + (type === 'success' ? 'check-circle' : 'exclamation-circle')
            + '"></i> ' + message;
        container.appendChild(toast);
        setTimeout(function () { toast.remove(); }, 4000);
    }

    if (qcDrawer && qcBackdrop && qcForm) {
        function setQcValue(id, value) {
            var el = document.getElementById(id);
            if (el) el.value = value || '';
        }

        function openQuickCreate(hora, recursoId, fecha) {
            setQcValue('qk-FechaHoraInicio', (fecha || agendaFecha) + 'T' + (hora || '09:00'));
            setQcValue('qk-RecursoId', recursoId);
            clearQcErrors();
            qcDrawer.classList.add('open');
            qcBackdrop.classList.add('open');
            qcDrawer.setAttribute('aria-hidden', 'false');
            document.body.classList.add('jk-drawer-open');
            var first = qcForm.querySelector('select, input');
            if (first) setTimeout(function () { first.focus(); }, 150);
        }

        function closeQuickCreate() {
            qcDrawer.classList.remove('open');
            qcBackdrop.classList.remove('open');
            qcDrawer.setAttribute('aria-hidden', 'true');
            document.body.classList.remove('jk-drawer-open');
        }

        function clearQcErrors() {
            qcForm.querySelectorAll('.jk-field-error').forEach(function (el) {
                el.hidden = true;
                el.textContent = '';
            });
            qcForm.querySelectorAll('.jk-input.error').forEach(function (el) {
                el.classList.remove('error');
            });
        }

        function showQcFieldError(field, message) {
            var input = qcForm.querySelector('[name="' + field + '"]');
            var error = qcForm.querySelector('[data-error-for="' + field + '"]');
            if (input) input.classList.add('error');
            if (error) {
                error.textContent = message;
                error.hidden = false;
            }
        }

        agenda.addEventListener('click', function (e) {
            var weekBlock = e.target.closest('.jk-agenda-week-block');
            if (weekBlock) return;

            var cell = e.target.closest('.jk-agenda-cell');
            if (cell) {
                openQuickCreate(cell.getAttribute('data-hour'), cell.getAttribute('data-recurso-id'));
                return;
            }

            var freeSlot = e.target.closest('.jk-agenda-mobile-free-slot');
            if (freeSlot) {
                openQuickCreate(freeSlot.getAttribute('data-hora'), freeSlot.getAttribute('data-recurso-id'));
                return;
            }

            var weekBody = e.target.closest('.jk-agenda-week-day-body');
            if (weekBody) {
                openQuickCreate('09:00', '', weekBody.getAttribute('data-agenda-fecha'));
            }
        });

        if (gridWrap) {
            gridWrap.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
                    var cell = e.target.closest('.jk-agenda-cell');
                    if (cell) {
                        e.preventDefault();
                        openQuickCreate(cell.getAttribute('data-hour'), cell.getAttribute('data-recurso-id'));
                    }
                }
            });
        }

        qcClose.addEventListener('click', closeQuickCreate);
        qcBackdrop.addEventListener('click', closeQuickCreate);
        qcForm.querySelectorAll('[data-quick-cancel]').forEach(function (btn) {
            btn.addEventListener('click', closeQuickCreate);
        });

        document.addEventListener('keydown', function (e) {
            if ((e.key === 'Escape' || e.keyCode === 27) && qcDrawer.classList.contains('open')) {
                closeQuickCreate();
            }
        });

        // Cambio de servicio -> recarga los recursos habilitados (misma logica
        // que el alta completa, replicada para el formulario del drawer).
        var qcServicio = document.getElementById('qk-ServicioId');
        if (qcServicio && recursosUrl) {
            qcServicio.addEventListener('change', function () {
                var recursoDdl = document.getElementById('qk-RecursoId');
                if (!recursoDdl) return;

                recursoDdl.innerHTML = '<option value="">Cargando...</option>';

                var url = this.value
                    ? recursosUrl + '?servicioId=' + encodeURIComponent(this.value)
                    : recursosTodosUrl;

                fetch(url)
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        recursoDdl.innerHTML = '<option value="">Seleccione un recurso</option>';
                        data.forEach(function (r) {
                            var option = document.createElement('option');
                            option.value = r.Id;
                            option.textContent = r.Nombre;
                            recursoDdl.appendChild(option);
                        });
                    });
            });
        }

        qcForm.addEventListener('submit', function (e) {
            e.preventDefault();
            clearQcErrors();

            var submitBtn = qcForm.querySelector('[type="submit"]');
            submitBtn.disabled = true;

            fetch(quickCreateUrl, { method: 'POST', body: new FormData(qcForm) })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data && data.success) {
                        closeQuickCreate();
                        qcToast('Turno creado correctamente', 'success');
                        var url = agendaBaseUrl
                            + '?fecha=' + encodeURIComponent(agendaFecha)
                            + '&vista=' + encodeURIComponent(agendaVista || 'dia')
                            + '&openTurnoId=' + data.id;
                        window.location.href = url;
                    } else {
                        var errors = data && data.errors;
                        var shown = false;
                        if (errors) {
                            Object.keys(errors).forEach(function (key) {
                                if (key) {
                                    showQcFieldError(key, errors[key]);
                                    shown = true;
                                }
                            });
                        }
                        if (!shown) {
                            qcToast((errors && errors['']) ? errors[''] : 'No se pudo crear el turno', 'error');
                        }
                    }
                })
                .catch(function () {
                    qcToast('Error de conexion al crear el turno', 'error');
                })
                .then(function () { submitBtn.disabled = false; });
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
