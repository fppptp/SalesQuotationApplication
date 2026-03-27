(function () {
    'use strict';

    function esc(str) {
        var d = document.createElement('div');
        d.textContent = str || '';
        return d.innerHTML;
    }

    /* ------------------------------------------------------------------ */
    /*  SearchLookup                                                       */
    /*  data-api          = API url                                        */
    /*  data-take         = max results (default 20)                       */
    /*  data-debounce     = ms debounce (default 300)                      */
    /*  data-min-chars    = min chars before search (default 0)            */
    /*  data-columns      = JSON array of column defs                      */
    /*       [{"field":"code","header":"Code","css":"sl-col-code"},         */
    /*        {"field":"name","header":"Name"}]                             */
    /*  data-display-format = display template e.g. "{code} - {name}"      */
    /*  data-value-field    = which response field → hidden value           */
    /*  data-depends-on     = name of form field to watch                   */
    /*  data-depends-param  = query-param name sent to API                  */
    /*  data-depends-clear  = "true" to clear value when parent changes    */
    /* ------------------------------------------------------------------ */
    function SearchLookup(root) {
        this.root = root;
        this.hiddenInput = root.querySelector('.sl-value');
        this.displayInput = root.querySelector('.sl-display');
        this.clearBtn = root.querySelector('.sl-clear');
        this.dropdown = root.querySelector('.sl-dropdown');
        this.gridWrap = root.querySelector('.sl-grid-wrap');
        this.thead = root.querySelector('.sl-grid thead tr');
        this.tbody = root.querySelector('.sl-grid tbody');
        this.emptyEl = root.querySelector('.sl-empty');
        this.loadingEl = root.querySelector('.sl-loading');
        this.infoEl = root.querySelector('.sl-info');

        this.apiUrl = root.dataset.api || '';
        this.take = parseInt(root.dataset.take || '20');
        this.debounceMs = parseInt(root.dataset.debounce || '300');
        this.minChars = parseInt(root.dataset.minChars || '0');
        this.valueField = root.dataset.valueField || 'value';
        this.displayFormat = root.dataset.displayFormat || '';
        this.columns = root.dataset.columns ? JSON.parse(root.dataset.columns) : [];

        // fills: map from HTML form field name → JSON property name
        // e.g. {"CustomerName":"customerName"} means: set input[name="CustomerName"] = item.customerName
        this.fills = root.dataset.fills ? JSON.parse(root.dataset.fills) : {};

        // depends-on: watch another form field
        this.dependsOn = root.dataset.dependsOn || '';
        this.dependsParam = root.dataset.dependsParam || '';
        this.dependsClear = root.dataset.dependsClear !== 'false';

        this.items = [];
        this.activeIndex = -1;
        this.isOpen = false;
        this.lastText = this.displayInput.value;
        this.ac = null;
        this.timer = null;

        this._buildHeaders();
        this._bind();
        this._syncClear();
    }

    /* Build <thead> from data-columns config */
    SearchLookup.prototype._buildHeaders = function () {
        if (this.columns.length === 0 || !this.thead) return;
        this.thead.innerHTML = '';
        for (var i = 0; i < this.columns.length; i++) {
            var th = document.createElement('th');
            th.textContent = this.columns[i].header || this.columns[i].field;
            this.thead.appendChild(th);
        }
    };

    /* Format display text from template + item data */
    SearchLookup.prototype._formatDisplay = function (item) {
        if (!this.displayFormat) {
            // fallback: first two column values joined by " - "
            var parts = [];
            var cols = this.columns.length > 0 ? this.columns : [{ field: 'code' }, { field: 'name' }];
            for (var i = 0; i < Math.min(cols.length, 2); i++) {
                var v = item[cols[i].field];
                if (v) parts.push(v);
            }
            return parts.join(' - ');
        }
        return this.displayFormat.replace(/\{(\w+)\}/g, function (_, key) {
            return item[key] != null ? item[key] : '';
        });
    };

    SearchLookup.prototype._bind = function () {
        var self = this;
        this.displayInput.addEventListener('focus', function () { self._onFocus(); });
        this.displayInput.addEventListener('input', function () { self._onInput(); });
        this.displayInput.addEventListener('keydown', function (e) { self._onKey(e); });
        this.displayInput.addEventListener('blur', function () {
            setTimeout(function () {
                if (!self.root.contains(document.activeElement)) self._close();
            }, 200);
        });
        this.clearBtn.addEventListener('mousedown', function (e) { e.preventDefault(); });
        this.clearBtn.addEventListener('click', function () { self._clear(); });
        document.addEventListener('click', function (e) {
            if (!self.root.contains(e.target)) self._close();
        });

        // depends-on: watch parent field for changes
        if (this.dependsOn) {
            var form = this.root.closest('form') || document;
            var parentEl = form.querySelector('[name="' + this.dependsOn + '"]');
            if (parentEl) {
                parentEl.addEventListener('change', function () {
                    if (self.dependsClear) self._clear();
                });
            }
        }
    };

    SearchLookup.prototype._onFocus = function () {
        if (!this.isOpen) {
            // If depends-on field is required but empty, show hint
            if (this.dependsOn && this.dependsParam) {
                var form = this.root.closest('form') || document;
                var parentEl = form.querySelector('[name="' + this.dependsOn + '"]');
                if (parentEl && !parentEl.value) {
                    this._open();
                    this._showMsg('Please select ' + this.dependsOn + ' first');
                    return;
                }
            }
            var q = (this.displayInput.value === this.lastText) ? '' : this.displayInput.value;
            if (this.minChars > 0 && q.length < this.minChars) {
                this._open();
                this._showMsg('Type at least ' + this.minChars + ' characters');
                return;
            }
            this._search(q);
        }
    };

    SearchLookup.prototype._onInput = function () {
        var self = this;
        clearTimeout(this.timer);
        this.timer = setTimeout(function () {
            var q = self.displayInput.value;
            if (q.length >= self.minChars) {
                self._search(q);
            } else if (self.isOpen) {
                self._showMsg('Type at least ' + self.minChars + ' characters');
            }
        }, this.debounceMs);
    };

    SearchLookup.prototype._onKey = function (e) {
        if (!this.isOpen) {
            if (e.key === 'ArrowDown' || e.key === 'Enter') {
                e.preventDefault();
                this._search(this.displayInput.value);
            }
            return;
        }
        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                this._nav(1);
                break;
            case 'ArrowUp':
                e.preventDefault();
                this._nav(-1);
                break;
            case 'Enter':
                e.preventDefault();
                if (this.activeIndex >= 0 && this.activeIndex < this.items.length) {
                    this._select(this.items[this.activeIndex]);
                }
                break;
            case 'Escape':
                e.preventDefault();
                this.displayInput.value = this.lastText;
                this._close();
                break;
            case 'Tab':
                if (this.activeIndex >= 0 && this.activeIndex < this.items.length) {
                    this._select(this.items[this.activeIndex]);
                } else {
                    this.displayInput.value = this.lastText;
                    this._close();
                }
                break;
        }
    };

    SearchLookup.prototype._nav = function (dir) {
        var rows = this.tbody.querySelectorAll('tr');
        if (rows.length === 0) return;
        this.activeIndex = Math.max(0, Math.min(rows.length - 1, this.activeIndex + dir));
        for (var i = 0; i < rows.length; i++) {
            rows[i].classList.toggle('sl-active', i === this.activeIndex);
        }
        rows[this.activeIndex].scrollIntoView({ block: 'nearest' });
    };

    SearchLookup.prototype._search = function (q) {
        if (this.ac) this.ac.abort();
        this.ac = new AbortController();
        var self = this;

        this._showLoading();
        this._open();

        var url = this.apiUrl + (this.apiUrl.indexOf('?') >= 0 ? '&' : '?')
            + 'q=' + encodeURIComponent(q || '')
            + '&take=' + this.take;

        // append depends-on value
        if (this.dependsOn && this.dependsParam) {
            var form = this.root.closest('form') || document;
            var parentEl = form.querySelector('[name="' + this.dependsOn + '"]');
            var pv = parentEl ? parentEl.value : '';
            url += '&' + encodeURIComponent(this.dependsParam) + '=' + encodeURIComponent(pv);
        }

        fetch(url, { signal: this.ac.signal })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                self.items = data;
                self._render(data);
            })
            .catch(function (ex) {
                if (ex.name !== 'AbortError') {
                    self._showMsg('Error loading results');
                }
            });
    };

    SearchLookup.prototype._render = function (items) {
        this.activeIndex = -1;
        this.tbody.innerHTML = '';

        if (items.length === 0) {
            this._showMsg('No results found');
            return;
        }

        this.emptyEl.style.display = 'none';
        this.loadingEl.style.display = 'none';
        this.gridWrap.style.display = '';

        var cols = this.columns.length > 0
            ? this.columns
            : [{ field: 'code', css: 'sl-col-code' }, { field: 'name' }];

        var self = this;
        for (var i = 0; i < items.length; i++) {
            (function (item, idx) {
                var tr = document.createElement('tr');
                var html = '';
                for (var c = 0; c < cols.length; c++) {
                    var cls = cols[c].css ? ' class="' + cols[c].css + '"' : '';
                    html += '<td' + cls + '>' + esc(item[cols[c].field]) + '</td>';
                }
                tr.innerHTML = html;
                tr.addEventListener('click', function () { self._select(item); });
                tr.addEventListener('mouseenter', function () {
                    self.activeIndex = idx;
                    var all = self.tbody.querySelectorAll('tr');
                    for (var j = 0; j < all.length; j++) all[j].classList.toggle('sl-active', j === idx);
                });
                self.tbody.appendChild(tr);
            })(items[i], i);
        }

        if (this.infoEl) {
            this.infoEl.textContent = items.length >= this.take
                ? 'Showing first ' + this.take + ' — type to narrow down'
                : items.length + ' result(s)';
            this.infoEl.style.display = '';
        }
    };

    SearchLookup.prototype._select = function (item) {
        this.hiddenInput.value = item[this.valueField] || item.value || '';
        this.displayInput.value = this._formatDisplay(item);
        this.lastText = this.displayInput.value;
        this._close();
        this._syncClear();

        // Auto-fill related form fields using mapping from data-fills attribute
        // fills = { "FormFieldName": "jsonPropertyName" }
        var form = this.root.closest('form') || document;
        for (var formField in this.fills) {
            var jsonProp = this.fills[formField];
            var el = form.querySelector('[name="' + formField + '"]');
            if (el) {
                el.value = item[jsonProp] != null ? item[jsonProp] : '';
                el.dispatchEvent(new Event('input', { bubbles: true }));
            }
        }

        this.hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
    };

    SearchLookup.prototype._clear = function () {
        this.hiddenInput.value = '';
        this.displayInput.value = '';
        this.lastText = '';
        this._close();
        this._syncClear();

        // Clear related form fields defined in data-fills
        var form = this.root.closest('form') || document;
        for (var formField in this.fills) {
            var el = form.querySelector('[name="' + formField + '"]');
            if (el) {
                el.value = '';
                el.dispatchEvent(new Event('input', { bubbles: true }));
            }
        }

        this.hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
    };

    SearchLookup.prototype._open = function () {
        this.dropdown.style.display = '';
        this.isOpen = true;
    };

    SearchLookup.prototype._close = function () {
        this.dropdown.style.display = 'none';
        this.isOpen = false;
        this.activeIndex = -1;
    };

    SearchLookup.prototype._showLoading = function () {
        this.tbody.innerHTML = '';
        this.gridWrap.style.display = 'none';
        this.emptyEl.style.display = 'none';
        this.loadingEl.style.display = '';
        if (this.infoEl) this.infoEl.style.display = 'none';
    };

    SearchLookup.prototype._showMsg = function (msg) {
        this.tbody.innerHTML = '';
        this.gridWrap.style.display = 'none';
        this.loadingEl.style.display = 'none';
        this.emptyEl.textContent = msg;
        this.emptyEl.style.display = '';
        if (this.infoEl) this.infoEl.style.display = 'none';
    };

    SearchLookup.prototype._syncClear = function () {
        this.clearBtn.classList.toggle('sl-visible', !!this.hiddenInput.value);
    };

    // Auto-init
    function initAll(container) {
        var els = (container || document).querySelectorAll('.sl-root:not([data-sl-init])');
        for (var i = 0; i < els.length; i++) {
            els[i].setAttribute('data-sl-init', '1');
            new SearchLookup(els[i]);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { initAll(); });
    } else {
        initAll();
    }

    // Expose for dynamic content
    window.SearchLookup = { initAll: initAll };
})();
