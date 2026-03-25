(function () {
    function reindexRows() {
        document.querySelectorAll(".js-ccrate-row").forEach((row, i) => {
            row.querySelectorAll("input[name], select[name]").forEach(el => {
                el.name = el.name.replace(/Rates\[\d+\]/g, `Rates[${i}]`);
                if (el.id) el.id = el.id.replace(/Rates_\d+__/g, `Rates_${i}__`);
            });
            const sort = row.querySelector(".js-ccrate-sort");
            if (sort) sort.value = i + 1;
        });
    }

    function bindRow(row) {
        const removeBtn = row.querySelector(".js-ccrate-remove");
        if (removeBtn) {
            removeBtn.addEventListener("click", () => {
                row.remove();
                reindexRows();
            });
        }
    }

    document.addEventListener("DOMContentLoaded", () => {
        document.querySelectorAll(".js-ccrate-row").forEach(bindRow);

        const addBtn = document.querySelector(".js-ccrate-add");
        if (!addBtn) return;

        addBtn.addEventListener("click", () => {
            const body     = document.querySelector(".js-ccrate-body");
            const template = document.querySelector("#ccrate-row-template");
            if (!body || !template) return;

            const index = body.querySelectorAll(".js-ccrate-row").length;
            const html  = template.innerHTML
                .replaceAll("__index__", index.toString())
                .replaceAll("__sort__",  (index + 1).toString());

            const wrapper = document.createElement("tbody");
            wrapper.innerHTML = html.trim();
            const row = wrapper.firstElementChild;
            if (!row) return;

            body.appendChild(row);
            bindRow(row);
        });
    });
})();
