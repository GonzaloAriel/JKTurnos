/*
   JKTurnos - Agenda Grid JavaScript
   Scroll sync and basic interactions for the calendar grid.
*/

(function () {
    'use strict';

    var gridWrap = document.querySelector('.jk-agenda-grid-wrap');
    if (!gridWrap) return;

    var grid = gridWrap.querySelector('.jk-agenda-grid');

    /*
     * Keyboard navigation: Arrow keys scroll the grid.
     * Enter/Space on a cell is reserved for future Time Block creation.
     */
    gridWrap.addEventListener('keydown', function (e) {
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

    gridWrap.addEventListener('click', function (e) {
        var cell = e.target.closest('.jk-agenda-cell');
        if (cell) {
            cell.focus();
        }
    });

})();
