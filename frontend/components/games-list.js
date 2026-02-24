import Alpine from 'alpinejs';
import { fetchGames } from '../api/games.js';
import { detectGamesContext, getLangPrefix, slugify, escapeHtml } from '../lib/util.js';

const ctx = detectGamesContext();

export default function gamesList() {
  return {
    // init from data-initial='{...}' to keep JS Razor-free
    _initFromDom() {
      try {
        const raw = this.$root.getAttribute('data-initial') || '{}';
        const init = JSON.parse(raw);
        this.q = init.q ?? '';
        this.sort = init.sort ?? '';
        this.dir = init.dir ?? '';
        this.page = Math.max(1, parseInt(init.page || 1, 10));
        this.pageSize = Math.max(1, parseInt(init.pageSize || 30, 10));
        this.sortLabel = this._labelForSort(this.sort);

        this.device = (init.device === 'mobile' || init.device === 'desktop') ? init.device : 'desktop';

        this.initialSort = this.sort;
        this.initialDir = this.dir;
        this.initialDevice = this.device;
        this.initialPage = this.page;
        this.currentPage = this.page;
      } catch { /* defaults */ }
    },

    // UI state you asked to keep
    sortOpen: false,
    sort: '',
    sortLabel: 'Newest Added',
    dir: '',

    device: '',
    initialSort: '',
    initialDir: '',
    initialDevice: '',
    initialPage: 1,
    currentPage: 1,

    toggleDir() {
      if (this.dir === '') { this.dir = 'asc'; return; }
      if (this.dir === 'asc') { this.dir = 'desc'; return; }
      this.dir = '';
    },

    toggleDevice() {
      this.setDevice(this.device === 'desktop' ? 'mobile' : 'desktop');
    },

    setDevice(d) {
      if (d !== 'desktop' && d !== 'mobile') return;
      this.device = d;
      this.refresh();
    },

    // data
    q: '',
    items: [],
    page: 1,
    pageSize: 30,
    hasMore: false,
    loading: false,
    error: null,

    // refs
    gridEl: null,
    loadMoreBtn: null,

    // internals
    aborter: null,

    async init() {
      this._initFromDom();
      this.gridEl = document.getElementById('games-grid');
      this.loadMoreBtn = document.getElementById('load-more');

      const ps = parseInt(this.loadMoreBtn?.dataset.pagesize || this.pageSize, 10);
      if (!Number.isNaN(ps) && ps > 0) this.pageSize = ps;

      window.gamesOnImgError = (img, name) => {
        img.outerHTML = `
        <div class="w-full h-full flex flex-col items-center justify-center rounded-xl p-2
                    bg-[#022b32] border-2 border-yellow-400 text-yellow-100 
                    text-center font-semibold text-lg">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"
               class="w-12 h-12 mb-2 opacity-70">
            <path d="M21 19V5a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2ZM8 11l3 4 2-3 3 4H6l2-5Z"/>
          </svg>
          <span class="uppercase">${name}</span>
        </div>
      `;
      };

      await this.refresh(this.initialPage);
    },

    _labelForSort(s) { return s === 'name' ? 'Name' : s === 'provider' ? 'Provider' : 'Newest Added'; },
    setSort(s) { this.sort = s; this.sortLabel = this._labelForSort(s); this.sortOpen = false; this.dir = ''; this.refresh(); },

    _endpoint() {
      if (ctx.mode === 'studio')
        return `/api/games/studios/${ctx.id}/${ctx.slug || ''}`.replace(/\/{2,}/g, '/');
      return `/api/games/categories/${ctx.id}/${ctx.slug || ''}`.replace(/\/{2,}/g, '/');
    },

    _endpointWithLang() {
      const lang = ctx.langPrefix;
      if (ctx.mode === 'studio')
        return `${lang}/api/games/studios/${ctx.id}/${ctx.slug || ''}`.replace(/\/{2,}/g, '/');
      return `${lang}/api/games/categories/${ctx.id}/${ctx.slug || ''}`.replace(/\/{2,}/g, '/');
    },


    _qs() {
      const qs = { q: this.q || '', page: this.page, pageSize: this.pageSize, sort: this.sort || 'id' };
      if (this.dir) qs.dir = this.dir;
      if (this.device) qs.device = this.device;
      return qs;
    },

    async refresh(startPage = 1) {
      this.page = Math.max(1, parseInt(startPage, 10) || 1);
      await this._fetch(true);
      this._pushQueryToUrl();
    },

    async loadMore() {
      if (this.loading || !this.hasMore) return;
      await this._fetch(false);
    },

    async _fetch(reset) {
      if (this.loading) { try { this.aborter?.abort(); } catch { } }
      this.loading = true; this.error = null;
      const controller = new AbortController(); this.aborter = controller;

      try {
        const { list, hasMore, page } = await fetchGames({
          endpoint: this._endpoint(),
          qs: this._qs(),
          signal: controller.signal
        });

        this.items = reset ? list : this.items.concat(list);
        this.hasMore = !!hasMore;
        const current = parseInt(page, 10);
        this.currentPage = Number.isFinite(current) && current > 0 ? current : Math.max(1, this.page || 1);
        this.page = this.currentPage + 1;

        this._render(reset);
      } catch (e) {
        if (e?.name !== 'AbortError') {
          this.error = e?.message || 'Request failed';
          console.error(e);
        }
      } finally {
        this.loading = false;
        this._updateLoadMoreBtn();
      }
    },

    _render(reset = false) {
      if (!this.gridEl) return;
      const lang = getLangPrefix();
      const sourceList = this.items || [];
      const listToRender = this.hasMore ? this._trimToFullRows(sourceList) : sourceList;
      const html = listToRender.map(g => {
        const id = g.gameId ?? g.Id;
        const name = (g.game_name ?? g.Name ?? '').toString();
        const img = g.game_image ?? g.ImageUrl ?? '';
        const href = `${lang}/game/${id}/${slugify(name)}`.replace(/\/{2,}/g, '/');
        return `
      <a data-game-item data-game-id="${id}" href="${href}"
         class="block rounded-xl shadow-sm hover:shadow transition overflow-hidden bg-white/25">
        <div class="aspect-[3/4]">
          <img src="${img}" alt="${escapeHtml(name)}"
               class="w-full h-full object-fill" loading="lazy"
               onerror="window.gamesOnImgError?.(this, '${escapeHtml(name)}')" />
        </div>
      </a>`;
      }).join('');

      this.gridEl.innerHTML = html;
    },

    _trimToFullRows(list) {
      const items = Array.isArray(list) ? list : [];
      if (items.length === 0) return items;

      const cols = this._currentColumnCount();
      if (cols <= 1) return items;
      if (items.length <= cols) return items;

      const remainder = items.length % cols;
      if (remainder === 0) return items;
      return items.slice(0, items.length - remainder);
    },

    _currentColumnCount() {
      if (!this.gridEl || typeof window === 'undefined') return 1;
      try {
        const style = window.getComputedStyle(this.gridEl);
        let template = style?.gridTemplateColumns || '';
        if (!template || template === 'none') return 1;

        template = template.replace(/\s+/g, ' ').trim();
        const repeatMatch = template.match(/repeat\((\d+)/i);
        if (repeatMatch) {
          const count = parseInt(repeatMatch[1], 10);
          if (!Number.isNaN(count) && count > 0) return count;
        }

        const explicitCols = template.split(' ').filter(Boolean).length;
        return explicitCols || 1;
      } catch {
        return 1;
      }
    },

    _updateLoadMoreBtn() {
      if (!this.loadMoreBtn) return;
      this.loadMoreBtn.hidden = !this.hasMore;
      this.loadMoreBtn.disabled = this.loading || !this.hasMore;
      this.loadMoreBtn.textContent = this.loading ? 'Loading...' : 'Load more';
      this.loadMoreBtn.dataset.page = String(this.page);
      this.loadMoreBtn.dataset.pagesize = String(this.pageSize);
    },

    _pushQueryToUrl() {
      try {
        const url = new URL(window.location.href);
        const setOrDel = (k, v) => {
          if (v === undefined || v === null || v === '') url.searchParams.delete(k);
          else url.searchParams.set(k, v);
        };
        const setIfChanged = (k, v, initial) => {
          if (v === initial) return;
          if (v === undefined || v === null || v === '') url.searchParams.delete(k);
          else url.searchParams.set(k, v);
        };
        setOrDel('q', this.q);
        setOrDel('page', this.currentPage > 1 ? this.currentPage : '');
        setIfChanged('sort', this.sort, this.initialSort);
        setIfChanged('dir', this.dir, this.initialDir);
        setIfChanged('device', this.device, this.initialDevice);
        history.replaceState(null, '', url.toString());
      } catch { }
    }
  };
}
