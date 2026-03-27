(function () {
    'use strict';

    var overlay, textEl;

    function ensure() {
        if (overlay) return;
        overlay = document.getElementById('loading-overlay');
        if (overlay) {
            textEl = overlay.querySelector('.lo-text');
        }
    }

    window.LoadingOverlay = {
        show: function (text) {
            ensure();
            if (!overlay) return;
            if (textEl) textEl.textContent = text || '';
            overlay.classList.add('lo-active');
        },
        hide: function () {
            ensure();
            if (!overlay) return;
            overlay.classList.remove('lo-active');
        },
        setText: function (text) {
            ensure();
            if (textEl) textEl.textContent = text || '';
        }
    };
})();
