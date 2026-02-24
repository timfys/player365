import { api } from '../lib/http.js';

/** PATCH user profile */
export async function updateUser({ endpoint, payload, signal }) {
  try {
    const res = await api.patch(endpoint, { json: payload, signal });
    const data = await res.json().catch(() => ({}));
    return { ok: true, data };
  } catch (err) {
    const data = await err.response?.json?.().catch(() => ({})) || {};
    return { ok: false, status: err.response?.status, data };
  }
}

/** POST start phone verification */
export async function startPhoneVerify({ endpoint, phone, signal }) {
  const res = await api.post(endpoint, { json: { phone }, signal });
  const data = await res.json().catch(() => ({}));
  return { ok: res.ok, data };
}
