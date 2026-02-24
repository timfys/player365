import { api } from '../lib/http.js';

/** PATCH user profile */
export async function updateUser({ endpoint, payload, signal }) {
  const res = await api.patch(endpoint, { json: payload, signal });
  return res.json().catch(() => ({}));
}

/** POST start phone verification */
export async function startPhoneVerify({ endpoint, phone, signal }) {
  const res = await api.post(endpoint, { json: { phone }, signal });
  const data = await res.json().catch(() => ({}));
  return { ok: res.ok, data };
}
