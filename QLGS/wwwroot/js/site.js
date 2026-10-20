// ===== Tiện ích dùng chung =====
(function () {
    'use strict';

    // Token chống CSRF cho các request AJAX
    window.csrfToken = function () {
        var m = document.querySelector('meta[name="csrf-token"]');
        return m ? m.content : '';
    };

    // Gọi API (PUT/DELETE...) kèm token & JSON
    window.apiFetch = function (url, method) {
        return fetch(url, {
            method: method || 'GET',
            headers: { 'RequestVerificationToken': window.csrfToken(), 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' },
            credentials: 'same-origin'
        }).then(function (r) {
            return r.json().catch(function () { return {}; }).then(function (data) {
                if (!r.ok) throw new Error(data.message || 'Có lỗi xảy ra (' + r.status + ')');
                return data;
            });
        });
    };

    window.escapeHtml = function (s) {
        return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    };

    // Toast nhỏ góc phải
    window.showToast = function (message, type) {
        var box = document.getElementById('toastBox');
        if (!box) {
            box = document.createElement('div');
            box.id = 'toastBox';
            box.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            box.style.zIndex = 2000;
            document.body.appendChild(box);
        }
        var el = document.createElement('div');
        el.className = 'toast align-items-center text-bg-' + (type || 'dark') + ' border-0';
        el.innerHTML = '<div class="d-flex"><div class="toast-body">' + window.escapeHtml(message) +
            '</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>';
        box.appendChild(el);
        var t = new bootstrap.Toast(el, { delay: 3500 });
        el.addEventListener('hidden.bs.toast', function () { el.remove(); });
        t.show();
    };

    // Tự đóng thông báo flash sau 6 giây
    document.querySelectorAll('.flash-alert').forEach(function (a) {
        setTimeout(function () { try { bootstrap.Alert.getOrCreateInstance(a).close(); } catch (e) { } }, 6000);
    });

    // Xác nhận trước khi thực hiện thao tác nguy hiểm: <form data-confirm="...">
    document.addEventListener('submit', function (e) {
        var msg = e.target.getAttribute && e.target.getAttribute('data-confirm');
        if (msg && !window.confirm(msg)) e.preventDefault();
    });

    // Chống bấm đúp nút submit
    document.addEventListener('submit', function (e) {
        if (e.defaultPrevented) return;
        var btn = e.target.querySelector && e.target.querySelector('button[type=submit]:not([data-no-lock])');
        if (btn) setTimeout(function () { btn.disabled = true; }, 0);
    });

    // Mở khóa lại các nút submit khi trang được khôi phục từ bộ nhớ đệm (nút Back của trình duyệt)
    window.addEventListener('pageshow', function (e) {
        if (!e.persisted) return;
        document.querySelectorAll('button[type=submit][disabled]:not([data-no-lock])').forEach(function (b) { b.disabled = false; });
    });

    // Adapter jQuery Unobtrusive Validation cho [FutureDate] (client-side)
    if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive) {
        jQuery.validator.addMethod('futuredate', function (value, element, hours) {
            if (!value) return true;
            return new Date(value).getTime() > Date.now() + (parseFloat(hours) || 0) * 3600000;
        });
        jQuery.validator.unobtrusive.adapters.add('futuredate', ['hours'], function (options) {
            options.rules['futuredate'] = options.params.hours;
            options.messages['futuredate'] = options.message;
        });
    }
})();
