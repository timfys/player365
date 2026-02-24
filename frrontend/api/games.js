import { api } from '../lib/http.js';

/** Normalize API result from PascalCase/CamelCase */
const normalizeList = (data) => ({
  list: data?.list ?? data?.List ?? [],
  hasMore: data?.hasMore ?? data?.HasMore ?? false,
  page: data?.page ?? data?.Page ?? 1,
});

/** Fetch games for a built endpoint (category or studio) */
export async function fetchGames({ endpoint, qs, signal }) {
  const data = await api.get(endpoint, { searchParams: qs, signal }).json();
  return normalizeList(data);
}

/** Header search: GET ?name=&limit= */
export async function searchGames({ endpoint, name, limit = 10, signal }) {
  const sp = new URLSearchParams({ name, limit: String(limit) });
  const data = await api.get(endpoint, { searchParams: sp, signal }).json();
  return {
    items: Array.isArray(data?.items) ? data.items : [],
    hasMore: !!data?.hasMore
  };
}