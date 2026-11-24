(function () {
  'use strict';

  /* ---------- Flight details drawer (open / close / re-open) ---------- */
  var overlay = document.getElementById('drawer-overlay');
  var drawer = document.getElementById('flight-drawer');
  var drawerBody = document.getElementById('drawer-body');
  var closeBtn = document.getElementById('drawer-close');
  var lastFocus = null;

  function openDrawer(url, trigger) {
    lastFocus = trigger || document.activeElement;
    drawerBody.innerHTML = '<div class="skeleton" aria-label="Loading flight details"></div><p>Loading flight details…</p>';
    overlay.hidden = false;
    drawer.hidden = false;
    document.body.style.overflow = 'hidden';
    fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
      .then(function (r) { if (!r.ok) throw new Error('HTTP ' + r.status); return r.text(); })
      .then(function (html) {
        drawerBody.innerHTML = html;
        closeBtn.focus();
      })
      .catch(function () {
        drawerBody.innerHTML = '<p>Could not load the flight details. Please try again.</p>';
      });
  }
  function closeDrawer() {
    overlay.hidden = true;
    drawer.hidden = true;
    drawerBody.innerHTML = '';
    document.body.style.overflow = '';
    if (lastFocus && lastFocus.focus) lastFocus.focus();
  }
  document.addEventListener('click', function (e) {
    var t = e.target.closest('[data-flight-details]');
    if (t) { e.preventDefault(); openDrawer(t.getAttribute('href') || t.dataset.flightDetails, t); }
  });
  if (closeBtn) closeBtn.addEventListener('click', closeDrawer);
  if (overlay) overlay.addEventListener('click', closeDrawer);
  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && !drawer.hidden) closeDrawer();
  });

  /* ---------- Calendar: day list <-> month grid ---------- */
  var toggle = document.getElementById('calendar-toggle');
  var grid = document.getElementById('month-grid');
  var list = document.getElementById('day-list');
  if (toggle && grid && list) {
    toggle.addEventListener('click', function () {
      var showing = grid.classList.toggle('show');
      list.style.display = showing ? 'none' : '';
      toggle.textContent = showing ? 'Show day list' : 'Show month grid';
      toggle.setAttribute('aria-expanded', showing ? 'true' : 'false');
    });
  }

  /* ---------- Results sort (client-side, no re-price) ---------- */
  var sort = document.getElementById('sort-select');
  var results = document.getElementById('flight-results');
  if (sort && results) {
    sort.addEventListener('change', function () {
      var cards = Array.prototype.slice.call(results.querySelectorAll('.flight-card'));
      cards.sort(function (a, b) {
        var pa = parseInt(a.dataset.total || '0', 10), pb = parseInt(b.dataset.total || '0', 10);
        var da = parseFloat(a.dataset.hours || '0'), db = parseFloat(b.dataset.hours || '0');
        return sort.value === 'fastest' ? da - db : pa - pb;
      });
      cards.forEach(function (c, i) { c.style.animationDelay = (i * 0.04) + 's'; results.appendChild(c); });
    });
  }

  /* ---------- Compare 2–3 flights ---------- */
  var compareBar = document.getElementById('compare-bar');
  function refreshCompare() {
    var checked = Array.prototype.slice.call(document.querySelectorAll('.compare-check input:checked'));
    if (checked.length < 2) { compareBar.hidden = true; compareBar.innerHTML = ''; return; }
    compareBar.hidden = false;
    compareBar.innerHTML = '<span><strong>' + checked.length + ' flights selected</strong> — compare totals, duration and baggage.</span>' +
      '<button type="button" class="btn" id="compare-go">Compare now</button>' +
      '<button type="button" class="btn btn-outline" id="compare-clear">Clear</button>';
    document.getElementById('compare-clear').addEventListener('click', function () {
      document.querySelectorAll('.compare-check input:checked').forEach(function (c) { c.checked = false; });
      refreshCompare();
    });
    document.getElementById('compare-go').addEventListener('click', function () {
      var rows = checked.map(function (c) {
        var card = c.closest('.flight-card');
        return (card.querySelector('.airline strong') || {}).textContent + ' — ' +
          (card.querySelector('.price') || {}).textContent.trim().replace(/\s+/g, ' ');
      });
      alert('Compare (demo):\n\n' + rows.join('\n') + '\n\nCheapest is marked on the card. Full breakdowns are in each flight’s details drawer.');
    });
  }
  document.addEventListener('change', function (e) {
    if (e.target.matches('.compare-check input')) {
      var checked = document.querySelectorAll('.compare-check input:checked');
      if (checked.length > 3) { e.target.checked = false; alert('You can compare up to 3 flights.'); return; }
      refreshCompare();
    }
  });

  /* ---------- Ziyarat filter ---------- */
  document.querySelectorAll('.filter-btn').forEach(function (btn) {
    btn.addEventListener('click', function () {
      document.querySelectorAll('.filter-btn').forEach(function (b) { b.classList.remove('active'); });
      btn.classList.add('active');
      var f = btn.dataset.filter;
      document.querySelectorAll('.ziyarat-card').forEach(function (card) {
        card.style.display = (f === 'All' || card.dataset.city === f) ? '' : 'none';
      });
    });
  });

  /* ---------- Live streams: tap to play (no autoplay) ---------- */
  document.querySelectorAll('[data-live-channel]').forEach(function (btn) {
    btn.addEventListener('click', function () {
      var wrap = btn.closest('.live-thumb');
      var channel = btn.dataset.liveChannel;
      var frame = document.createElement('iframe');
      frame.className = 'live-frame';
      frame.src = 'https://www.youtube.com/embed/live_stream?channel=' + channel + '&autoplay=1';
      frame.title = btn.dataset.liveTitle || 'Live stream';
      frame.allow = 'accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture';
      frame.allowFullscreen = true;
      wrap.replaceWith(frame);
    });
  });

  /* ---------- Recent searches (local, on this device only) ---------- */
  try {
    var form = document.getElementById('search-form');
    var recentBox = document.getElementById('recent-searches');
    var key = 'carwane-recent-searches';
    function renderRecent() {
      if (!recentBox) return;
      var items = JSON.parse(localStorage.getItem(key) || '[]');
      if (!items.length) { recentBox.innerHTML = ''; return; }
      recentBox.innerHTML = '<strong>Recent searches:</strong> ' + items.map(function (s) {
        return '<button type="button" data-recent="' + s.replace(/"/g, '&quot;') + '">' + s + '</button>';
      }).join('');
      recentBox.querySelectorAll('[data-recent]').forEach(function (b) {
        b.addEventListener('click', function () {
          var parts = b.dataset.recent.split(' → ');
          if (parts.length === 2 && form) {
            var fromSel = form.querySelector('[name=from]'), toSel = form.querySelector('[name=to]');
            if (fromSel) fromSel.value = parts[0].split(' ')[0];
            if (toSel) toSel.value = parts[1].split(' ')[0];
            form.submit();
          }
        });
      });
    }
    if (form) {
      form.addEventListener('submit', function () {
        try {
          var from = form.querySelector('[name=from]').selectedOptions[0].textContent.trim();
          var to = form.querySelector('[name=to]').selectedOptions[0].textContent.trim();
          var label = from + ' → ' + to;
          var items = JSON.parse(localStorage.getItem(key) || '[]').filter(function (s) { return s !== label; });
          items.unshift(label);
          localStorage.setItem(key, JSON.stringify(items.slice(0, 5)));
        } catch (e) { /* storage unavailable — search still works */ }
      });
    }
    renderRecent();
  } catch (e) { /* localStorage unavailable */ }
})();
