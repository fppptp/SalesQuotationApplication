(function () {
    'use strict';

    function toNumber(value) {
        const n = parseFloat(value);
        return isNaN(n) ? 0 : n;
    }

    function fmt(value) {
        return value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // ── Generic initialiser ──────────────────────────────────────────────────
    // prefix  : CSS class prefix  ('rc' for rate cards, 'rt' for templates)
    // listKey : model list prefix ('Lines')
    function initEditor(prefix, listKey) {
        const body     = document.querySelector(`.${prefix}-rate-body`);
        const addBtn   = document.querySelector(`.${prefix}-add-row`);
        const tpl      = document.getElementById(`${prefix}-row-template`);

        if (!body || !addBtn || !tpl) return;

        body.querySelectorAll(`.${prefix}-rate-row`).forEach(row => {
            bindRow(row);
            updateRow(row);
        });
        updateSummary();

        addBtn.addEventListener('click', () => {
            const index = body.querySelectorAll(`.${prefix}-rate-row`).length;
            const html  = tpl.innerHTML
                .replaceAll('__index__', index.toString())
                .replaceAll('__sort__',  (index + 1).toString());

            const wrapper = document.createElement('tbody');
            wrapper.innerHTML = html.trim();
            const row = wrapper.firstElementChild;
            if (!row) return;

            body.appendChild(row);
            bindRow(row);
            updateSummary();
        });

        function bindRow(row) {
            row.querySelectorAll(`.${prefix}-buy-rate, .${prefix}-sell-rate`).forEach(input => {
                input.addEventListener('input', () => {
                    updateRow(row);
                    updateSummary();
                });
            });

            const removeBtn = row.querySelector(`.${prefix}-remove-row`);
            if (removeBtn) {
                removeBtn.addEventListener('click', () => {
                    const rows = body.querySelectorAll(`.${prefix}-rate-row`);
                    if (rows.length <= 1) {
                        row.querySelectorAll('input').forEach(x => {
                            if (x.type !== 'hidden') x.value = '';
                        });
                        updateRow(row);
                        updateSummary();
                        return;
                    }
                    row.remove();
                    reindex();
                    updateSummary();
                });
            }
        }

        function updateRow(row) {
            const buy  = toNumber(row.querySelector(`.${prefix}-buy-rate`)?.value);
            const sell = toNumber(row.querySelector(`.${prefix}-sell-rate`)?.value);
            const margin    = sell - buy;
            const marginPct = buy === 0 ? 0 : (margin / buy * 100);

            const mEl   = row.querySelector(`.${prefix}-margin-display`);
            const mPctEl = row.querySelector(`.${prefix}-margin-pct`);
            if (mEl)    mEl.textContent    = fmt(margin);
            if (mPctEl) mPctEl.textContent = marginPct.toFixed(1) + '%';
        }

        function updateSummary() {
            let totalBuy = 0, totalSell = 0;
            body.querySelectorAll(`.${prefix}-rate-row`).forEach(row => {
                totalBuy  += toNumber(row.querySelector(`.${prefix}-buy-rate`)?.value);
                totalSell += toNumber(row.querySelector(`.${prefix}-sell-rate`)?.value);
            });
            const totalMargin = totalSell - totalBuy;
            setText(`.${prefix}-summary-buy`,    totalBuy);
            setText(`.${prefix}-summary-sell`,   totalSell);
            setText(`.${prefix}-summary-margin`, totalMargin);
        }

        function reindex() {
            body.querySelectorAll(`.${prefix}-rate-row`).forEach((row, i) => {
                row.querySelectorAll('input[name], select[name]').forEach(el => {
                    el.name = el.name.replace(new RegExp(`${listKey}\\[\\d+\\]`, 'g'), `${listKey}[${i}]`);
                    if (el.id) el.id = el.id.replace(new RegExp(`${listKey}_\\d+__`, 'g'), `${listKey}_${i}__`);
                });
                const so = row.querySelector(`.${prefix}-sort-order`);
                if (so) so.value = i + 1;
            });
        }

        function setText(selector, value) {
            const el = document.querySelector(selector);
            if (el) el.textContent = fmt(value);
        }
    }

    document.addEventListener('DOMContentLoaded', () => {
        initEditor('rc', 'Lines');   // Rate card lines
        initEditor('rt', 'Lines');   // Rate template lines
    });
})();
