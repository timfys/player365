import Alpine from 'alpinejs';
import { searchGames } from '../api/games.js';
import { slugify } from '../lib/util.js';

export default function headerSearch() {
  return {
    // config via DOM data-* to avoid Razor inside JS
    _cfg() {
      const r = this.$root;
      return {
        searchEndpoint: r.getAttribute('data-search-endpoint') || '',
        langPrefix: r.getAttribute('data-lang-prefix') || '',
        minChars: parseInt(r.getAttribute('data-min-chars') || '2', 10),
        limit: parseInt(r.getAttribute('data-limit') || '10', 10),
        unableText: r.getAttribute('data-unable-text') || 'Unable to fetch results',
        fallbackImage: r.getAttribute('data-fallback-image') || '/images/casino/studio-image.svg'
      };
    },

    // state
    term: '',
    results: [],
    open: false,
    isLoading: false,
    error: '',
    hasMore: false,
    selectedIndex: -1,
    fallbackImage: '/images/casino/studio-image.svg',
    debounceId: null,
    abortController: null,

    init() {
      const cfg = this._cfg();
      this.fallbackImage = cfg.fallbackImage;
    },

    queueSearch() {
      this.error = '';
      clearTimeout(this.debounceId);
      this.debounceId = setTimeout(() => this.performSearch(), 350);
    },

    async performSearch() {
      const cfg = this._cfg();
      const query = this.term.trim();
      if (query.length < cfg.minChars) { this.resetResults(); return; }

      this.isLoading = true;
      this.open = true;
      this.selectedIndex = -1;
      this.abortController?.abort();
      this.abortController = new AbortController();

      try {
        const { items, hasMore } = await searchGames({
          endpoint: cfg.searchEndpoint,
          name: query,
          limit: cfg.limit,
          signal: this.abortController.signal
        });

        this.results = Array.isArray(items) ? items : [];
        this.hasMore = !!hasMore;
        this.error = '';
      } catch (err) {
        if (err?.name !== 'AbortError') {
          console.error(err);
          this.error = this._cfg().unableText;
        }
      } finally {
        this.isLoading = false;
      }
    },

    onFocus() {
      if (this.results.length > 0) this.open = true;
    },

    onKeyDown(event) {
      if (!this.open || this.results.length === 0) return;
      if (event.key === 'ArrowDown') {
        event.preventDefault();
        this.selectedIndex = (this.selectedIndex + 1) % this.results.length;
        this.scrollIntoView();
      }
      if (event.key === 'ArrowUp') {
        event.preventDefault();
        this.selectedIndex = (this.selectedIndex - 1 + this.results.length) % this.results.length;
        this.scrollIntoView();
      }
      if (event.key === 'Enter' && this.selectedIndex >= 0) {
        event.preventDefault();
        window.location.href = this.buildGameUrl(this.results[this.selectedIndex]);
      }
      if (event.key === 'Escape') this.close();
    },

    scrollIntoView() {
      requestAnimationFrame(() => {
        const el = document.querySelector(`#search-results-panel li:nth-child(${this.selectedIndex + 1})`);
        el?.scrollIntoView({ block: 'nearest' });
      });
    },

    clear() { this.term = ''; this.resetResults(); },
    close() { this.open = false; },

    resetResults() {
      this.results = [];
      this.open = false;
      this.error = '';
      this.isLoading = false;
      this.hasMore = false;
      this.selectedIndex = -1;
      clearTimeout(this.debounceId);
      this.abortController?.abort();
      this.abortController = null;
    },

    resultImage(item) {
      return item?.imageUrl || this.fallbackImage;
    },

    buildGameUrl(item) {
      const cfg = this._cfg();
      const name = item?.name?.toString() ?? '';
      const slug = slugify(name) || 'game';
      return `/${cfg.langPrefix}/game/${item.id}/${slug}`.replace(/\/{2,}/g, '/');
    }
  };
}
