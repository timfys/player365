import Alpine from 'alpinejs';
import { updateUser, startPhoneVerify } from '../api/user.js';
import { wirePhone, setInitialPhone, getNormalized } from '../lib/phone.js';

export default function accountSettings() {
  return {
    // --------- config from DOM (keeps JS Razor-free) ----------
    _cfg() {
      const root = this.$root;
      // data-config='{"userEndpoint": "...", "authPhoneEndpoint": "...", ...}'
      try { return JSON.parse(root.getAttribute('data-config') || '{}'); }
      catch { return {}; }
    },

    // --------- tabs + UI state ----------
    activeId: 'personal_information',
    loading: false,

    // status texts
    _texts() {
      const c = this._cfg();
      return {
        changed: c.changedText || 'Changed',
        saved: c.savedText || 'Saved',
        invalidPhone: c.invalidPhoneText || 'Invalid phone number',
        updateFailed: c.updateFailedText || 'Update failed'
      };
    },

    // backend error-code → user-friendly message (overridable via data-config errorTexts)
    _errorTexts() {
      const c = this._cfg();
      const custom = c.errorTexts || {};
      return {
        INVALID_EMAIL:            custom.INVALID_EMAIL            || 'The provided email is not valid',
        EMAIL_ALREADY_REGISTERED: custom.EMAIL_ALREADY_REGISTERED || 'This email is already registered',
        INVALID_PHONE_NUMBER:     custom.INVALID_PHONE_NUMBER     || 'The provided phone number is already registered',
        INVALID_CREDENTIALS:      custom.INVALID_CREDENTIALS      || 'Invalid credentials after update',
        UNEXPECTED_ERROR:         custom.UNEXPECTED_ERROR         || 'An unexpected error occurred',
        ...custom
      };
    },

    _errorMessage(code, fallbackMsg) {
      if (code) {
        const mapped = this._errorTexts()[code];
        if (mapped) return mapped;
      }
      return fallbackMsg || this._texts().updateFailed;
    },

    // phone element states
    phonePersonal: { raw: '', country: null, normalized: null },
    phoneLogin: { raw: '', country: null, normalized: null },

    // tab ordering (for syncing ?t query param)
    tabOrder: [],

    // refs
    el: {
      grid: null,
      btnPersonal: null,
      btnAddress: null,
      btnLogin: null,
      statusPersonal: null,
      statusAddress: null,
      statusLogin: null,
      btnVerifyPersonal: null,
      btnVerifyLogin: null,
      inputPersonal: null,
      inputLogin: null
    },

    // ---------- lifecycle ----------
    init() {
      // adopt initial activeId if provided as data-active-id
      const activeAttr = this.$root.getAttribute('data-active-id');
      if (activeAttr) this.activeId = activeAttr;

      // wire phone custom elements
      this.el.inputPersonal = document.getElementById('phoneInputPersonal');
      this.el.inputLogin = document.getElementById('phoneInputLogin');
      wirePhone(this.el.inputPersonal, this.phonePersonal);
      wirePhone(this.el.inputLogin, this.phoneLogin);

      // set initial phone values from config
      const cfg = this._cfg();
      setInitialPhone(this.el.inputPersonal, this.phonePersonal, cfg.initCountry, cfg.initPhone);
      setInitialPhone(this.el.inputLogin, this.phoneLogin, cfg.initCountry, cfg.initPhone);

      // buttons + statuses
      this.el.btnPersonal = document.getElementById('personal_change');
      this.el.btnAddress = document.getElementById('address_change');
      this.el.btnLogin = document.getElementById('login_change');
      this.el.statusPersonal = document.getElementById('form-valid');
      this.el.statusAddress = document.getElementById('form-valid-address');
      this.el.statusLogin = document.getElementById('form-valid-login');
      this.el.btnVerifyPersonal = document.getElementById('phone_number_not_verified');
      this.el.btnVerifyLogin = document.getElementById('phone_number_not_verified_login');

      this.tabOrder = this._scanTabOrder();

      // click handlers (keep existing markup unchanged)
      this.el.btnPersonal?.addEventListener('click', () => this.savePersonal());
      this.el.btnAddress?.addEventListener('click', () => this.saveAddress());
      this.el.btnLogin?.addEventListener('click', () => this.saveLogin());
      this.el.btnVerifyPersonal?.addEventListener('click', () => this._startPhoneVerification('personal'));
      this.el.btnVerifyLogin?.addEventListener('click', () => this._startPhoneVerification('login'));
    },

    changeTab(id, index) {
      this.activeId = id;
      const resolvedIndex = typeof index === 'number' && Number.isFinite(index)
        ? index
        : this._getTabIndex(id);
      this._updateTabQuery(resolvedIndex);
    },

    // ---------- helpers ----------
    _showStatus(el, text, ok = true) {
      if (!el) return;
      el.textContent = text;
      el.style.display = 'block';
      el.style.color = ok ? 'var(--c-green,#16a34a)' : 'var(--c-red,#dc2626)';
      clearTimeout(el._t);
      el._t = setTimeout(() => { el.style.display = 'none'; }, 4000);
    },

    _scanTabOrder() {
      return Array.from(this.$root.querySelectorAll('[data-tab-index]')).map((el) => ({
        id: el.getAttribute('id') || '',
        index: Number(el.getAttribute('data-tab-index')) || 0
      }));
    },

    _getTabIndex(id) {
      if (!id || !Array.isArray(this.tabOrder)) return null;
      const found = this.tabOrder.find((tab) => tab.id === id);
      return found?.index ?? null;
    },

    _updateTabQuery(index) {
      if (typeof window === 'undefined' || !window.history?.replaceState) return;
      try {
        const url = new URL(window.location.href);
        if (typeof index === 'number' && Number.isFinite(index)) {
          if (index === 0) url.searchParams.delete('t');
          else url.searchParams.set('t', index.toString());
        } else {
          url.searchParams.delete('t');
        }
        window.history.replaceState({}, '', url.toString());
      } catch (err) {
        console.error('Failed to sync tab query parameter', err);
      }
    },

    async _patch(payload, { btn, statusEl }) {
      const t = this._texts();
      const cfg = this._cfg();
      const label = btn?.querySelector('.button_link');
      const original = label?.textContent?.trim();

      try {
        if (btn) { btn.disabled = true; btn.classList.add('opacity-70'); if (label) label.textContent = '...'; }
        const { ok, data } = await updateUser({ endpoint: cfg.userEndpoint || 'api/user', payload });

        if (ok) {
          if (label) label.textContent = t.changed;
          this._showStatus(statusEl, t.saved, true);
          window.dispatchEvent(new CustomEvent('user:updated', { detail: data }));
        } else {
          if (label && original) label.textContent = original;
          const code = data?.code;
          const msg = this._errorMessage(code, data?.message);
          this._showStatus(statusEl, msg, false);
          console.warn('updateUser failed:', code, data);
        }
      } catch (err) {
        if (label && original) label.textContent = original;
        this._showStatus(statusEl, t.updateFailed, false);
        console.error('updateUser error:', err);
      } finally {
        if (btn) { btn.disabled = false; btn.classList.remove('opacity-70'); }
      }
    },

    // ---------- actions ----------
    async savePersonal() {
      const t = this._texts();
      const payload = {
        firstName: document.getElementById('personal_name')?.value?.trim() || undefined,
        lastName: document.getElementById('personal_last_name')?.value?.trim() || undefined,
        email: document.getElementById('personal_email')?.value?.trim() || undefined,
      };

      const bd = document.getElementById('personal_birthday')?.value?.trim();
      if (bd) { payload.birthday = bd; payload['CustomField62'] = bd; }

      const n = getNormalized(this.phonePersonal);
      if (n?.valid) {
        payload.phoneNumber = n.username;
        payload.mobile = n.username;
        payload['Mobile'] = n.username;
        payload.phoneE164 = n.e164;
        payload.countryIso = n.region || undefined;
        payload['Country'] = n.region || undefined;
      } else if (this.phonePersonal.raw || this.phonePersonal.country) {
        this._showStatus(this.el.statusPersonal, t.invalidPhone, false);
        return;
      }

      await this._patch(payload, { btn: this.el.btnPersonal, statusEl: this.el.statusPersonal });
    },

    async saveAddress() {
      const payload = {
        address: document.getElementById('personal_street')?.value?.trim() || undefined,
        city: document.getElementById('personal_city')?.value?.trim() || undefined,
        zipCode: document.getElementById('personal_post_code')?.value?.trim() || undefined,
        state: document.getElementById('personal_state')?.value?.trim() || undefined,
        countryIso: document.getElementById('personal_country')?.value
          || document.getElementById('model_country')?.value || undefined,
      };
      await this._patch(payload, { btn: this.el.btnAddress, statusEl: this.el.statusAddress });
    },

    async saveLogin() {
      const t = this._texts();
      const pass1 = document.getElementById('personal_password')?.value || '';
      const pass2 = document.getElementById('personal_re-enter_password')?.value || '';
      if (pass1 !== pass2) {
        const e = document.getElementById('password_error');
        if (e) { e.textContent = 'Passwords do not match'; e.style.display = 'block'; }
        return;
      }

      const payload = {};
      const n = getNormalized(this.phoneLogin);
      if (n?.valid) {
        payload.username = n.username;
        payload.mobile = n.username;
        payload['Mobile'] = n.username;
        payload.phoneE164 = n.e164;
      } else if (this.phoneLogin.raw || this.phoneLogin.country) {
        this._showStatus(this.el.statusLogin, t.invalidPhone, false);
        return;
      }

      if (pass1) payload.password = pass1;

      await this._patch(payload, { btn: this.el.btnLogin, statusEl: this.el.statusLogin });
    },

    async _startPhoneVerification(from /* 'personal' | 'login' */) {
      const t = this._texts();
      const cfg = this._cfg();
      const n = getNormalized(from === 'personal' ? this.phonePersonal : this.phoneLogin);

      let phonePayload = '';
      if (n?.valid) phonePayload = n.username;
      else if (cfg.initPhone) phonePayload = cfg.initPhone;

      if (!phonePayload) {
        this._showStatus(from === 'personal' ? this.el.statusPersonal : this.el.statusLogin, t.invalidPhone, false);
        return;
      }

      phonePayload = ('' + phonePayload).trim();
      if (phonePayload && !phonePayload.startsWith('+')) phonePayload = '+' + phonePayload;

      try {
        const { ok, data } = await startPhoneVerify({
          endpoint: cfg.authPhoneEndpoint || 'api/Auth/phone',
          phone: phonePayload
        });

        if (!ok) {
          const code = data?.code || data?.Extensions?.code;
          const msg = code === 'INVALID_PHONE_NUMBER' ? (data?.detail || t.invalidPhone) : (data?.detail || 'Unknown error');
          this._showStatus(from === 'personal' ? this.el.statusPersonal : this.el.statusLogin, msg, false);
          return;
        }

        let redirectUrl = '';
        if (data?.verify === true) {
          redirectUrl = data.redirectUrl;
          redirectUrl += '&r=' + encodeURIComponent(window.location.href);
        } else {
          const currentUrl = new URL(window.location.href);
          currentUrl.searchParams.set('page', '1');
          currentUrl.searchParams.set('min', 'false');
          redirectUrl = currentUrl.toString();
        }
        window.top.location.href = redirectUrl;
      } catch (err) {
        this._showStatus(from === 'personal' ? this.el.statusPersonal : this.el.statusLogin, 'Unknown error', false);
        console.error('phone verify error:', err);
      }
    }
  };
}
