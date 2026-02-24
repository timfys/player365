export const slugify = (s) =>
  (s ?? '').toString().trim().toLowerCase()
    .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');

export const escapeHtml = (s) =>
  (s ?? '').toString()
    .replace(/&/g,'&amp;').replace(/</g,'&lt;')
    .replace(/>/g,'&gt;').replace(/"/g,'&quot;');

export const getLangPrefix = () => {
  const htmlLang = (document.documentElement.lang || '').toLowerCase();
  if (!htmlLang || htmlLang.startsWith('en')) return '';
  return `/${htmlLang.split('-')[0]}`;
};

export const detectGamesContext = () => {
  const path = window.location.pathname.replace(/\/+$/,'');
  const seg = path.split('/').filter(Boolean);
  const idx = seg.indexOf('games');
  let mode = 'category', id = null, slug = '';
  if (idx !== -1) {
    const after = seg[idx + 1];
    if (after === 's') { mode = 'studio'; id = parseInt(seg[idx + 2], 10); slug = seg[idx + 3] || ''; }
    else { mode = 'category'; id = parseInt(after, 10); slug = seg[idx + 2] || ''; }
  }
  return { mode, id, slug, langPrefix: getLangPrefix() };
};
