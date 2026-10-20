// ===== Trang chủ: tìm kiếm gia sư qua Web API (AJAX + phân trang) =====
(function () {
    'use strict';
    var cfg = window.TUTOR_SEARCH || {};
    var state = { q: '', maMon: '', maKhoi: '', khuVuc: '', mucGia: '', trinhDo: '', daXacMinh: false, sortBy: 'rating_desc', page: 1, pageSize: 6 };
    var $ = function (id) { return document.getElementById(id); };
    var container = $('tutorsContainer'), pager = $('pager'), summary = $('resultSummary'), chips = $('activeChips');
    var reqId = 0, debounceTimer;

    var fmtMoney = function (n) { return new Intl.NumberFormat('vi-VN').format(n) + 'đ'; };

    function stars(score, count) {
        if (!count) return '<span class="badge bg-success-subtle text-success-emphasis">Gia sư mới</span>';
        var full = Math.round(score), html = '';
        for (var i = 1; i <= 5; i++) html += '<i class="fa-solid fa-star' + (i > full ? ' off' : '') + '"></i>';
        return '<span class="stars small">' + html + '</span> <strong class="small">' + Number(score).toFixed(1) + '</strong> <span class="text-muted small">(' + count + ')</span>';
    }

    function avatar(t) {
        if (t.avatar) return '<img src="' + escapeHtml(t.avatar) + '" class="avatar" style="width:62px;height:62px" alt="' + escapeHtml(t.hoTen) + '">';
        return '<span class="avatar" style="width:62px;height:62px;font-size:1.25rem;background:' + t.avatarColor + '">' + escapeHtml(t.initials) + '</span>';
    }

    function card(t, i) {
        var subjects = (t.monHocs || []).slice(0, 3).map(function (s) { return '<span class="badge-soft">' + escapeHtml(s) + '</span>'; }).join('');
        if ((t.monHocs || []).length > 3) subjects += '<span class="badge-soft">+' + (t.monHocs.length - 3) + '</span>';
        var mode = t.hinhThucDay === 'Cả hai' ? 'Offline & Online' : t.hinhThucDay;
        return '<div class="col-md-6 col-xl-4 fade-up" style="animation-delay:' + (i * 60) + 'ms"><div class="card-soft p-hover tutor-card">' +
            (t.soLopDangDay > 0 ? '<span class="ribbon"><i class="fa-solid fa-fire me-1"></i>' + t.soLopDangDay + ' lớp đang dạy</span>' : '') +
            '<div class="d-flex align-items-center gap-3 mb-3 mt-1">' + avatar(t) +
            '<div class="min-w-0"><a class="t-name" href="' + cfg.detailsUrl + '/' + t.maGiaSu + '">' + escapeHtml(t.hoTen) + '</a>' +
            '<div class="small text-muted">' + escapeHtml(t.trinhDo) + (t.truongDaoTao ? ' • ' + escapeHtml(t.truongDaoTao) : '') + '</div>' +
            (t.daXacMinh ? '<span class="badge-verified mt-1"><i class="fa-solid fa-circle-check me-1"></i>Đã xác minh</span>' : '') + '</div></div>' +
            '<div class="mb-2">' + subjects + '</div>' +
            '<div class="small text-secondary mb-3"><i class="fa-solid fa-location-dot text-danger me-2"></i>' + escapeHtml(t.khuVucDay) + ' <span class="fw-semibold">(' + mode + ')</span></div>' +
            (t.thanhTichNoiBat ? '<div class="achieve-box mb-3 flex-grow-1"><i class="fa-solid fa-trophy text-warning me-2"></i>' + escapeHtml(t.thanhTichNoiBat) + '</div>' : '<div class="flex-grow-1"></div>') +
            '<div class="d-flex justify-content-between align-items-end pt-3 border-top mt-auto"><div>' +
            '<div class="price">' + fmtMoney(t.hocPhiTheoGio) + '<span class="small text-muted fw-normal">/giờ</span></div>' +
            '<div>' + stars(t.diemDanhGiaTB, t.tongSoDanhGia) + '</div></div>' +
            '<a href="' + cfg.detailsUrl + '/' + t.maGiaSu + '" class="btn btn-primary btn-sm rounded-pill px-3">Xem hồ sơ</a></div></div></div>';
    }

    function skeletons() {
        var h = '';
        for (var i = 0; i < state.pageSize; i++) h += '<div class="col-md-6 col-xl-4"><div class="skeleton" style="height:300px"></div></div>';
        container.innerHTML = h;
    }

    function empty() {
        container.innerHTML = '<div class="col-12"><div class="card-soft empty-state"><i class="fa-regular fa-face-frown d-block"></i>' +
            '<h5 class="fw-bold text-secondary">Không tìm thấy gia sư phù hợp</h5><p class="small mb-3">Hãy thử bỏ bớt tiêu chí hoặc mở rộng mức học phí.</p>' +
            '<button class="btn btn-outline-primary rounded-pill" id="btnResetEmpty">Xóa tất cả bộ lọc</button></div></div>';
        var b = $('btnResetEmpty'); if (b) b.onclick = resetAll;
    }

    function renderPager(total, current) {
        if (total <= 1) { pager.innerHTML = ''; return; }
        var h = '<nav><ul class="pagination justify-content-center mb-0">';
        var add = function (p, label, dis, act) {
            h += '<li class="page-item' + (dis ? ' disabled' : '') + (act ? ' active' : '') + '"><a class="page-link" href="#" data-page="' + p + '">' + label + '</a></li>';
        };
        add(current - 1, '&laquo;', current === 1);
        for (var p = 1; p <= total; p++) {
            if (p === 1 || p === total || Math.abs(p - current) <= 1) add(p, p, false, p === current);
            else if (Math.abs(p - current) === 2) h += '<li class="page-item disabled"><span class="page-link">…</span></li>';
        }
        add(current + 1, '&raquo;', current === total);
        pager.innerHTML = h + '</ul></nav>';
    }

    function label(sel, val) { var o = $(sel); if (!o) return val; var opt = o.querySelector('option[value="' + val + '"]'); return opt ? opt.textContent.trim() : val; }

    function renderChips() {
        var list = [];
        if (state.q) list.push(['q', 'Từ khóa: ' + state.q]);
        if (state.maMon) list.push(['maMon', label('filterMon', state.maMon)]);
        if (state.maKhoi) list.push(['maKhoi', label('filterKhoi', state.maKhoi)]);
        if (state.khuVuc) list.push(['khuVuc', label('filterKhuVuc', state.khuVuc)]);
        if (state.mucGia) list.push(['mucGia', label('filterMucGia', state.mucGia)]);
        if (state.trinhDo) list.push(['trinhDo', state.trinhDo]);
        if (state.daXacMinh) list.push(['daXacMinh', 'Chỉ gia sư đã xác minh']);
        chips.innerHTML = list.map(function (c) {
            return '<span class="filter-chip">' + escapeHtml(c[1]) + '<button type="button" data-clear="' + c[0] + '" aria-label="Bỏ lọc">✕</button></span>';
        }).join(' ');
    }

    function syncControls() {
        $('searchBox').value = state.q; $('filterMon').value = state.maMon; $('filterKhoi').value = state.maKhoi;
        $('filterKhuVuc').value = state.khuVuc; $('filterMucGia').value = state.mucGia; $('filterTrinhDo').value = state.trinhDo;
        $('onlyVerified').checked = state.daXacMinh; $('sortBy').value = state.sortBy;
        document.querySelectorAll('.subject-chip').forEach(function (b) { b.classList.toggle('active', (b.dataset.mon || '') === String(state.maMon)); });
    }

    function readControls() {
        state.q = $('searchBox').value.trim(); state.maMon = $('filterMon').value; state.maKhoi = $('filterKhoi').value;
        state.khuVuc = $('filterKhuVuc').value; state.mucGia = $('filterMucGia').value; state.trinhDo = $('filterTrinhDo').value;
        state.daXacMinh = $('onlyVerified').checked; state.sortBy = $('sortBy').value;
    }

    function load(scroll) {
        var id = ++reqId;
        skeletons(); renderChips();
        var p = new URLSearchParams();
        Object.keys(state).forEach(function (k) { if (state[k] !== '' && state[k] !== false) p.set(k, state[k]); });
        history.replaceState(null, '', location.pathname + (p.toString() ? '?' + p.toString() : ''));

        fetch(cfg.apiUrl + '?' + p.toString(), { headers: { 'Accept': 'application/json' } })
            .then(function (r) { if (!r.ok) throw new Error('HTTP ' + r.status); return r.json(); })
            .then(function (data) {
                if (id !== reqId) return;
                summary.innerHTML = 'Tìm thấy <strong>' + data.totalItems + '</strong> gia sư' + (data.totalItems ? ' • Trang ' + data.currentPage + '/' + data.totalPages : '');
                if (!data.items.length) { empty(); pager.innerHTML = ''; return; }
                container.innerHTML = data.items.map(card).join('');
                renderPager(data.totalPages, data.currentPage);
                if (scroll) $('resultsAnchor').scrollIntoView({ behavior: 'smooth', block: 'start' });
            })
            .catch(function () {
                if (id !== reqId) return;
                container.innerHTML = '<div class="col-12"><div class="alert alert-danger">Không tải được danh sách gia sư. Vui lòng kiểm tra kết nối CSDL và thử lại.</div></div>';
            });
    }

    function apply(resetPage) { readControls(); if (resetPage !== false) state.page = 1; syncControls(); load(false); }
    function resetAll() {
        state = { q: '', maMon: '', maKhoi: '', khuVuc: '', mucGia: '', trinhDo: '', daXacMinh: false, sortBy: 'rating_desc', page: 1, pageSize: 6 };
        syncControls(); load(false);
    }

    // ----- gắn sự kiện -----
    ['filterMon', 'filterKhoi', 'filterKhuVuc', 'filterMucGia', 'filterTrinhDo', 'sortBy', 'onlyVerified'].forEach(function (id) {
        $(id).addEventListener('change', function () { apply(); });
    });
    $('btnSearch').addEventListener('click', function () { apply(); $('resultsAnchor').scrollIntoView({ behavior: 'smooth' }); });
    $('btnReset').addEventListener('click', resetAll);
    $('searchBox').addEventListener('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); apply(); $('resultsAnchor').scrollIntoView({ behavior: 'smooth' }); } });
    $('searchBox').addEventListener('input', function () {
        clearTimeout(debounceTimer);
        var term = this.value.trim();
        debounceTimer = setTimeout(function () {
            apply();
            if (term.length >= 2) {
                fetch(cfg.suggestUrl + '?term=' + encodeURIComponent(term)).then(function (r) { return r.json(); }).then(function (list) {
                    $('suggestList').innerHTML = list.map(function (x) { return '<option value="' + escapeHtml(x.name) + '"></option>'; }).join('');
                }).catch(function () { });
            }
        }, 350);
    });
    pager.addEventListener('click', function (e) {
        var a = e.target.closest('[data-page]'); if (!a) return; e.preventDefault();
        var li = a.parentElement; if (li.classList.contains('disabled') || li.classList.contains('active')) return;
        state.page = parseInt(a.dataset.page, 10); load(true);
    });
    chips.addEventListener('click', function (e) {
        var b = e.target.closest('[data-clear]'); if (!b) return;
        var k = b.dataset.clear; state[k] = (k === 'daXacMinh') ? false : ''; state.page = 1; syncControls(); load(false);
    });
    document.addEventListener('click', function (e) {
        var chip = e.target.closest('.subject-chip'); if (!chip) return;
        state.maMon = chip.dataset.mon || ''; state.page = 1; syncControls(); load(false);
    });
    var rb = $('btnResetHeader'); if (rb) rb.addEventListener('click', resetAll);

    // ----- khởi tạo từ query string -----
    var qs = new URLSearchParams(location.search);
    Object.keys(state).forEach(function (k) {
        if (!qs.has(k)) return;
        state[k] = (k === 'daXacMinh') ? qs.get(k) === 'true' : (k === 'page' || k === 'pageSize') ? (parseInt(qs.get(k), 10) || state[k]) : qs.get(k);
    });
    syncControls(); load(false);
})();
