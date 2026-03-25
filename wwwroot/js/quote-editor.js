(function () {

    // ── Charge-code lookup / autocomplete ────────────────────────────────────
    let ccDropdown = null;

    function getCcDropdown() {
        if (!ccDropdown) {
            ccDropdown = document.createElement("ul");
            ccDropdown.className = "list-group shadow";
            ccDropdown.style.cssText =
                "display:none;position:absolute;" +
                "max-height:260px;overflow-y:auto;" +
                "min-width:340px;z-index:9999;border-radius:.375rem;";
            document.body.appendChild(ccDropdown);
        }
        return ccDropdown;
    }

    function hideCcDropdown() { getCcDropdown().style.display = "none"; }

    function applyChargeCode(row, item) {
        const code     = row.querySelector(".js-charge-code");
        const name     = row.querySelector(".js-charge-name");
        const cat      = row.querySelector(".js-charge-category");
        const basis    = row.querySelector(".js-charge-basis");
        const costInp  = row.querySelector(".js-unit-cost");
        const priceInp = row.querySelector(".js-unit-price");
        if (code)     code.value     = item.code;
        if (name)     name.value     = item.name;
        if (cat)      cat.value      = item.category;
        if (basis)    basis.value    = item.defaultBasis;
        if (costInp  && item.defaultBuyRate  > 0) costInp.value  = item.defaultBuyRate;
        if (priceInp && item.defaultSellRate > 0) priceInp.value = item.defaultSellRate;
        if (costInp || priceInp) { updateRow(row, "sell"); updateSummary(); }
        hideCcDropdown();
        setTimeout(() => { const n = row.querySelector(".js-charge-name"); if (n) n.focus(); }, 10);
    }

    function setDdActive(li, on) {
        li.style.background = on ? "#eef2ff" : "";
        li.style.color      = on ? "#1744d1" : "";
        if (on) li.setAttribute("data-cc-active", "1"); else li.removeAttribute("data-cc-active");
    }

    function moveCcSelection(dir) {
        const d = getCcDropdown();
        if (d.style.display === "none") return false;
        const items = [...d.querySelectorAll("li.list-group-item-action")];
        if (!items.length) return false;
        let idx = items.findIndex(li => li.hasAttribute("data-cc-active"));
        if (idx >= 0) setDdActive(items[idx], false);
        idx = (idx + dir + items.length) % items.length;
        setDdActive(items[idx], true);
        items[idx].scrollIntoView({ block: "nearest" });
        return true;
    }

    function showCcDropdown(input, row, items) {
        const d = getCcDropdown();
        const rect = input.getBoundingClientRect();
        d.style.top   = (rect.bottom + window.scrollY) + "px";
        d.style.left  = (rect.left   + window.scrollX) + "px";
        d.style.width = Math.max(rect.width + 40, 340) + "px";
        d.innerHTML   = "";

        if (items.length === 0) {
            const li = document.createElement("li");
            li.className = "list-group-item px-3 py-2 small text-muted fst-italic";
            li.textContent = "No matching charge codes";
            d.appendChild(li);
            d.style.display = "block";
            return;
        }

        items.forEach(item => {
            const li = document.createElement("li");
            li.className =
                "list-group-item list-group-item-action d-flex align-items-center gap-2 px-2 py-1 small";
            li.style.cursor = "pointer";
            li.innerHTML =
                `<span class="font-monospace fw-bold" style="min-width:56px;color:#1744d1">${item.code}</span>` +
                `<span class="flex-grow-1 text-truncate">${item.name}</span>` +
                `<span class="badge rounded-pill text-bg-light border" ` +
                `style="font-size:.68rem;white-space:nowrap">${item.category}</span>`;
            li.addEventListener("mousedown", e => { e.preventDefault(); applyChargeCode(row, item); });
            li.addEventListener("mouseover",  () => {
                d.querySelectorAll("li[data-cc-active]").forEach(el => setDdActive(el, false));
                setDdActive(li, true);
            });
            d.appendChild(li);
        });
        d.style.display = "block";
    }

    function initCcAutocomplete(row) {
        const input = row.querySelector(".js-charge-code");
        if (!input) return;
        input.setAttribute("autocomplete", "off");

        function search() {
            if (!window.chargeCodes) { hideCcDropdown(); return; }
            const q = input.value.toLowerCase().trim();
            let hits;
            if (q) {
                hits = window.chargeCodes
                    .filter(c =>
                        c.code.toLowerCase().startsWith(q) ||
                        c.code.toLowerCase().includes(q)   ||
                        c.name.toLowerCase().includes(q))
                    .sort((a, b) => {
                        const aS = a.code.toLowerCase().startsWith(q) ? 0 : 1;
                        const bS = b.code.toLowerCase().startsWith(q) ? 0 : 1;
                        return aS - bS || a.code.localeCompare(b.code);
                    });
            } else {
                hits = window.chargeCodes;   // show all on empty focus
            }
            showCcDropdown(input, row, hits.slice(0, 15));
        }

        // ▾ lookup button opens the full list
        const lookupBtn = input.closest(".input-group")?.querySelector(".js-cc-lookup");
        if (lookupBtn) {
            lookupBtn.addEventListener("mousedown", e => {
                e.preventDefault();
                input.focus();
                search();
            });
        }

        input.addEventListener("input",  search);
        input.addEventListener("focus",  search);
        input.addEventListener("blur",   () => setTimeout(hideCcDropdown, 200));
        input.addEventListener("keydown", e => {
            const d    = getCcDropdown();
            const open = d.style.display !== "none";
            if (e.key === "ArrowDown") {
                e.preventDefault(); if (!open) search(); else moveCcSelection(1);
            } else if (e.key === "ArrowUp") {
                e.preventDefault(); if (open) moveCcSelection(-1);
            } else if ((e.key === "Enter" || e.key === "Tab") && open) {
                const active = d.querySelector("li[data-cc-active]");
                if (active) { e.preventDefault(); active.dispatchEvent(new MouseEvent("mousedown", { bubbles: true })); }
            } else if (e.key === "Escape" && open) {
                e.preventDefault(); hideCcDropdown();
            }
        });
    }

    // ── Number helpers ───────────────────────────────────────────────────────
    function toNumber(value) {
        const num = parseFloat(value);
        return isNaN(num) ? 0 : num;
    }

    function fmt(value) {
        return value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function fmtPct(value) {
        return fmt(value) + " %";
    }

    // source: 'markup' | 'sell' | 'cost' | 'qty'
    function updateRow(row, source) {
        const qty         = toNumber(row.querySelector(".js-qty")?.value);
        const costInput   = row.querySelector(".js-unit-cost");
        const priceInput  = row.querySelector(".js-unit-price");
        const markupInput = row.querySelector(".js-markup-pct");

        let unitCost  = toNumber(costInput?.value);
        let unitPrice = toNumber(priceInput?.value);
        let markup    = toNumber(markupInput?.value);

        if (source === 'markup') {
            // Markup % typed → recalculate sell price
            unitPrice = unitCost * (1 + markup / 100);
            if (priceInput) priceInput.value = unitPrice.toFixed(4);
        } else if (source === 'sell') {
            // Sell price typed → recalculate markup %
            markup = unitCost === 0 ? 0 : (unitPrice - unitCost) / unitCost * 100;
            if (markupInput) markupInput.value = markup.toFixed(2);
        } else {
            // Cost or qty changed → keep sell price, recalculate markup %
            markup = unitCost === 0 ? 0 : (unitPrice - unitCost) / unitCost * 100;
            if (markupInput) markupInput.value = markup.toFixed(2);
        }

        const costAmount   = qty * unitCost;
        const sellAmount   = qty * unitPrice;
        const profitAmount = sellAmount - costAmount;

        const set = (cls, text) => {
            const el = row.querySelector(cls);
            if (el) el.textContent = text;
        };

        set(".js-cost-amount",   fmt(costAmount));
        set(".js-sell-amount",   fmt(sellAmount));
        set(".js-profit-amount", fmt(profitAmount));
    }

    function updateSummary() {
        const rows = document.querySelectorAll(".js-quote-row");
        let subtotal  = 0;
        let costTotal = 0;

        rows.forEach(row => {
            const qty      = toNumber(row.querySelector(".js-qty")?.value);
            const unitCost = toNumber(row.querySelector(".js-unit-cost")?.value);
            const unitPrice = toNumber(row.querySelector(".js-unit-price")?.value);
            subtotal  += qty * unitPrice;
            costTotal += qty * unitCost;
        });

        const profit      = subtotal - costTotal;
        const markup      = costTotal === 0 ? 0 : profit / costTotal * 100;
        const gp          = subtotal  === 0 ? 0 : profit / subtotal  * 100;
        const discount    = toNumber(document.querySelector(".js-discount")?.value);
        const includeVat  = document.querySelector(".js-include-vat")?.checked ?? false;
        const vatRate     = toNumber(document.querySelector(".js-vat-rate")?.value);
        const netBeforeVat = Math.max(subtotal - discount, 0);
        const vat         = includeVat ? (netBeforeVat * vatRate / 100) : 0;
        const grand       = netBeforeVat + vat;

        const set = (selector, text) => {
            const el = document.querySelector(selector);
            if (el) el.textContent = text;
        };

        set(".js-summary-subtotal", fmt(subtotal));
        set(".js-summary-cost",     fmt(costTotal));
        set(".js-summary-markup",   fmtPct(markup));
        set(".js-summary-profit",   fmt(profit));
        set(".js-summary-gp",       fmtPct(gp));
        set(".js-summary-vat",      fmt(vat));
        set(".js-summary-grand",    fmt(grand));
    }

    function reindexRows() {
        document.querySelectorAll(".js-quote-row").forEach((row, index) => {
            row.querySelectorAll("input[name], select[name], textarea[name]").forEach(el => {
                el.name = el.name.replace(/Items\[\d+\]/g, `Items[${index}]`);
                if (el.id) el.id = el.id.replace(/Items_\d+__/g, `Items_${index}__`);
            });
            const sort = row.querySelector(".js-sort-order");
            if (sort) sort.value = index + 1;
        });
    }

    function bindRowEvents(row) {
        const on = (cls, source) => {
            const el = row.querySelector(cls);
            if (el) el.addEventListener("input", () => { updateRow(row, source); updateSummary(); });
        };

        on(".js-qty",        "qty");
        on(".js-unit-cost",  "cost");
        on(".js-unit-price", "sell");
        on(".js-markup-pct", "markup");

        initCcAutocomplete(row);

        const removeBtn = row.querySelector(".js-remove-row");
        if (removeBtn) {
            removeBtn.addEventListener("click", () => {
                const rows = document.querySelectorAll(".js-quote-row");
                if (rows.length <= 1) {
                    row.querySelectorAll("input").forEach(x => { if (x.type !== "hidden") x.value = ""; });
                    row.querySelector(".js-cost-amount").textContent   = "0.00";
                    row.querySelector(".js-sell-amount").textContent   = "0.00";
                    row.querySelector(".js-profit-amount").textContent = "0.00";
                    const mu = row.querySelector(".js-markup-pct");
                    if (mu) mu.value = "0.00";
                    updateSummary();
                    return;
                }
                row.remove();
                reindexRows();
                updateSummary();
            });
        }

        updateRow(row, "qty");
    }

    // ── Customer-code lookup (filtered by selected Company) ──────────────────
    let custDropdown = null;

    function getCustDropdown() {
        if (!custDropdown) {
            custDropdown = document.createElement("ul");
            custDropdown.className = "list-group shadow";
            custDropdown.style.cssText =
                "display:none;position:absolute;" +
                "max-height:280px;overflow-y:auto;" +
                "min-width:420px;z-index:9999;border-radius:.375rem;";
            document.body.appendChild(custDropdown);
        }
        return custDropdown;
    }

    function hideCustDropdown() { getCustDropdown().style.display = "none"; }

    function setCustActive(li, on) {
        li.style.background = on ? "#eef2ff" : "";
        li.style.color      = on ? "#1744d1" : "";
        if (on) li.setAttribute("data-cust-active", "1");
        else    li.removeAttribute("data-cust-active");
    }

    function moveCustSelection(dir) {
        const d = getCustDropdown();
        if (d.style.display === "none") return;
        const items = [...d.querySelectorAll("li.list-group-item-action")];
        if (!items.length) return;
        let idx = items.findIndex(li => li.hasAttribute("data-cust-active"));
        if (idx >= 0) setCustActive(items[idx], false);
        idx = (idx + dir + items.length) % items.length;
        setCustActive(items[idx], true);
        items[idx].scrollIntoView({ block: "nearest" });
    }

    function showCustDropdown(input, items) {
        const d    = getCustDropdown();
        const rect = input.getBoundingClientRect();
        d.style.top   = (rect.bottom + window.scrollY) + "px";
        d.style.left  = (rect.left   + window.scrollX) + "px";
        d.style.width = Math.max(rect.width, 420) + "px";
        d.innerHTML   = "";

        if (!items.length) {
            const li = document.createElement("li");
            li.className = "list-group-item px-3 py-2 small text-muted fst-italic";
            li.textContent = "No matching customers";
            d.appendChild(li);
            d.style.display = "block";
            return;
        }

        items.forEach(c => {
            const li = document.createElement("li");
            li.className =
                "list-group-item list-group-item-action d-flex align-items-center gap-2 px-2 py-1 small";
            li.style.cursor = "pointer";
            li.innerHTML =
                `<span class="font-monospace fw-bold" style="min-width:76px;color:#1744d1">${c.customerCode}</span>` +
                `<span class="flex-grow-1 text-truncate">${c.customerName}</span>`;
            li.addEventListener("mousedown", e => {
                e.preventDefault();
                const set = (id, val) => { const el = document.getElementById(id); if (el) el.value = val; };
                set("CustomerCode",     c.customerCode);
                set("CustomerName",     c.customerName);
                set("CustomerTaxId",    c.taxId);
                set("CustomerTaxBranch", c.taxBranch);
                set("CustomerAddress",  c.address);
                set("ContactPerson",    c.contactPerson);
                hideCustDropdown();
                input.focus();
            });
            li.addEventListener("mouseover", () => {
                d.querySelectorAll("li[data-cust-active]").forEach(el => setCustActive(el, false));
                setCustActive(li, true);
            });
            d.appendChild(li);
        });
        d.style.display = "block";
    }

    function initCustomerCodeLookup() {
        const input = document.getElementById("CustomerCode");
        if (!input) return;
        input.setAttribute("autocomplete", "off");

        function search() {
            const customers = window.companyCustomers;
            if (!customers?.length) { hideCustDropdown(); return; }
            const q = input.value.toLowerCase().trim();
            const hits = q
                ? customers
                    .filter(c =>
                        c.customerCode.toLowerCase().startsWith(q) ||
                        c.customerCode.toLowerCase().includes(q)   ||
                        c.customerName.toLowerCase().includes(q))
                    .sort((a, b) => {
                        const aS = a.customerCode.toLowerCase().startsWith(q) ? 0 : 1;
                        const bS = b.customerCode.toLowerCase().startsWith(q) ? 0 : 1;
                        return aS - bS || a.customerCode.localeCompare(b.customerCode);
                    })
                : customers;
            showCustDropdown(input, hits.slice(0, 20));
        }

        input.addEventListener("input",  search);
        input.addEventListener("focus",  search);
        input.addEventListener("blur",   () => setTimeout(hideCustDropdown, 200));
        input.addEventListener("keydown", e => {
            const d    = getCustDropdown();
            const open = d.style.display !== "none";
            if (e.key === "ArrowDown") {
                e.preventDefault(); if (!open) search(); else moveCustSelection(1);
            } else if (e.key === "ArrowUp") {
                e.preventDefault(); if (open) moveCustSelection(-1);
            } else if ((e.key === "Enter" || e.key === "Tab") && open) {
                const active = d.querySelector("li[data-cust-active]");
                if (active) { e.preventDefault(); active.dispatchEvent(new MouseEvent("mousedown", { bubbles: true })); }
            } else if (e.key === "Escape" && open) {
                e.preventDefault(); hideCustDropdown();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", () => {
        // Pre-load charge codes for the lookup dropdown
        fetch("/chargecodes/all")
            .then(r => r.json())
            .then(data => {
                window.chargeCodes = data;
                // If a code input already has focus, open the dropdown now
                const focused = document.activeElement;
                if (focused && focused.classList.contains("js-charge-code")) {
                    focused.dispatchEvent(new Event("focus"));
                }
            })
            .catch(() => { window.chargeCodes = []; });

        document.querySelectorAll(".js-quote-row").forEach(bindRowEvents);

        document.querySelectorAll(".js-discount, .js-vat-rate, .js-include-vat").forEach(el => {
            el.addEventListener("input",  updateSummary);
            el.addEventListener("change", updateSummary);
        });

        const addBtn = document.querySelector(".js-add-row");
        if (addBtn) {
            addBtn.addEventListener("click", () => {
                const body     = document.querySelector(".js-quote-body");
                const template = document.querySelector("#quote-row-template");
                if (!body || !template) return;

                const index = body.querySelectorAll(".js-quote-row").length;
                const html  = template.innerHTML
                    .replaceAll("__index__", index.toString())
                    .replaceAll("__sort__",  (index + 1).toString());

                const wrapper = document.createElement("tbody");
                wrapper.innerHTML = html.trim();
                const row = wrapper.firstElementChild;
                if (!row) return;

                body.appendChild(row);
                bindRowEvents(row);
                updateSummary();
            });
        }

        updateSummary();

        // ── Company → Customer Code lookup ───────────────────────────────────
        initCustomerCodeLookup();

        const companySelect = document.getElementById("CompanyCode");
        if (companySelect) {
            const loadCustomers = async (company) => {
                window.companyCustomers = [];
                if (!company) return;
                try {
                    const res = await fetch(
                        `/quotations/customersbycompany?company=${encodeURIComponent(company)}`);
                    if (res.ok) window.companyCustomers = await res.json();
                } catch { window.companyCustomers = []; }
            };

            companySelect.addEventListener("change", function () {
                ["CustomerCode", "CustomerName", "CustomerTaxId",
                 "CustomerTaxBranch", "CustomerAddress", "ContactPerson"]
                    .forEach(id => { const el = document.getElementById(id); if (el) el.value = ""; });
                hideCustDropdown();
                loadCustomers(this.value);
            });

            // Pre-load on edit (company already set on page load).
            if (companySelect.value) loadCustomers(companySelect.value);
        }
    });
})();
