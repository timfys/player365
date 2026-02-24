export const digitsOnly = (s) => (s || '').replace(/\D+/g, '');
const getIso2 = (c) => (c && typeof c === 'object' ? c.iso2 : (typeof c === 'string' ? c : ''));
const getDial = (c) => (c && typeof c === 'object' ? c.dialCode : '');
const hasLib = !!(window.libphonenumber && window.libphonenumber.parsePhoneNumber);

export function toE164(raw, country) {
  const cleaned = (raw || '').trim();
  if (!cleaned) return '';
  if (cleaned.startsWith('+')) return '+' + digitsOnly(cleaned);
  const cc = getDial(country);
  const local = digitsOnly(cleaned);
  return cc ? `+${cc}${local}` : '';
}

export function buildUsernameFromE164(e164, dialCode) {
  const d0 = digitsOnly(e164);
  if (!d0) return '';
  const d = d0.startsWith('00') ? d0.slice(2) : d0;
  const cc = (dialCode || '');
  let rest = d.startsWith(cc) ? d.slice(cc.length) : d;
  if (rest.startsWith('0')) rest = rest.slice(1);
  return cc + rest; // “username” = digits incl. country code
}

export function normalizePhoneSafe(raw, country) {
  const candidate = toE164(raw, country);
  if (!candidate) return { valid: false, reason: 'NO_COUNTRY_CODE' };

  if (hasLib) {
    try {
      const p = window.libphonenumber.parsePhoneNumber(candidate);
      if (!p || !p.isValid()) return { valid: false, reason: 'INVALID_LIB' };
      return {
        valid: true,
        e164: p.format('E.164'),
        username: `${p.countryCallingCode}${p.nationalNumber}`,
        region: (p.country || getIso2(country) || '').toUpperCase()
      };
    } catch { return { valid: false, reason: 'PARSE_ERROR' }; }
  }

  const cc = getDial(country);
  if (!cc) return { valid: false, reason: 'NO_COUNTRY_CODE' };
  const e164 = candidate;
  const username = buildUsernameFromE164(e164, cc);
  return { valid: true, e164, username, region: (getIso2(country) || '').toUpperCase() };
}

/** Wire your <phone-number-input> custom element to state */
export function wirePhone(el, state) {
  if (!el) return;
  const getDetail = (e) => Array.isArray(e.detail) ? e.detail[0] : e.detail;

  el.addEventListener('update:modelValue', (e) => {
    state.raw = (getDetail(e) || '').toString().replace(/\s+/g, '');
    el.setAttribute('model-value', state.raw);
    state.normalized = null;
  });
  el.addEventListener('update:country', (e) => {
    state.country = getDetail(e) || null; // { name, iso2, dialCode }
    state.normalized = null;
  });
}

/** Put initial values into the custom element */
export function setInitialPhone(el, state, initCountry, initPhone) {
  if (!el) return;

  // Set country first
  if (initCountry) {
    try { el.country = initCountry; } catch {}
    try { el.setAttribute('country', initCountry); } catch {}
  }

  // After microtask, resolve country & set modelValue
  setTimeout(() => {
    const resolved = el.country || initCountry || null;
    if (resolved) state.country = resolved;

    if (initPhone) {
      let rawIn = String(initPhone).trim();
      let e164 = '';
      if (rawIn.startsWith('+')) e164 = '+' + digitsOnly(rawIn);
      else if (rawIn.startsWith('00')) e164 = '+' + digitsOnly(rawIn.slice(2));
      else {
        const n = normalizePhoneSafe(rawIn, resolved);
        if (n?.valid && n.e164) e164 = n.e164;
        else {
          const local = digitsOnly(rawIn);
          const cc = getDial(resolved);
          e164 = cc ? `+${cc}${local}` : (local ? `+${local}` : '');
        }
      }
      if (e164) {
        try { el.modelValue = e164; } catch {}
        try { el.setAttribute('model-value', e164); } catch {}
        state.raw = e164;
        state.normalized = null;
      }
    }
  }, 0);
}

/** Lazy normalization getter */
export function getNormalized(state) {
  if (!state) return null;
  if (!state.normalized) state.normalized = normalizePhoneSafe(state.raw, state.country);
  return state.normalized;
}