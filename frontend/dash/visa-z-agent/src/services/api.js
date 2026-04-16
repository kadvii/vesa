import axios from 'axios';

/** Keys shared with auth UI / 401 handler */
export const ACCESS_TOKEN_KEY = 'drs_access_token';
export const REFRESH_TOKEN_KEY = 'drs_refresh_token';
export const USER_STORAGE_KEY = 'drs_user';

const baseURL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') || 'https://localhost:7022';

const client = axios.create({
  baseURL,
  timeout: 20000,
  headers: { 'Content-Type': 'application/json' },
});

export function clearAuthStorage() {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(USER_STORAGE_KEY);
}

function isPublicAuthPath(url) {
  if (!url) return false;
  return (
    url.includes('/api/auth/login') ||
    url.includes('/api/auth/register') ||
    url.includes('/api/auth/refresh')
  );
}

client.interceptors.request.use((config) => {
  const token = localStorage.getItem(ACCESS_TOKEN_KEY);
  if (token && !isPublicAuthPath(config.url)) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ── 401 interceptor: try refresh before giving up ────────────────────────────
let _isRefreshing = false;
let _refreshQueue = []; // [{resolve, reject}] waiting for the new token

function processQueue(error, token = null) {
  _refreshQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else resolve(token);
  });
  _refreshQueue = [];
}

client.interceptors.response.use(
  (response) => response,
  async (error) => {
    const status   = error.response?.status;
    const url      = error.config?.url || '';
    const original = error.config;

    // Don't retry auth endpoints or requests that already retried
    if (
      status !== 401 ||
      original._retry ||
      isPublicAuthPath(url)
    ) {
      return Promise.reject(error);
    }

    // If a refresh is already in flight, queue this request
    if (_isRefreshing) {
      return new Promise((resolve, reject) => {
        _refreshQueue.push({ resolve, reject });
      }).then((newToken) => {
        original.headers.Authorization = `Bearer ${newToken}`;
        return client(original);
      });
    }

    original._retry  = true;
    _isRefreshing    = true;

    const storedRefresh = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!storedRefresh) {
      _isRefreshing = false;
      clearAuthStorage();
      if (!window.location.pathname.startsWith('/login')) window.location.assign('/login');
      return Promise.reject(error);
    }

    try {
      const { data } = await client.post('/api/auth/refresh', {
        refreshToken: storedRefresh,
      });

      if (!data?.success || !data.data?.accessToken) throw new Error('refresh failed');

      const newAccess  = data.data.accessToken;
      const newRefresh = data.data.refreshToken || storedRefresh;

      localStorage.setItem(ACCESS_TOKEN_KEY,  newAccess);
      localStorage.setItem(REFRESH_TOKEN_KEY, newRefresh);

      client.defaults.headers.common.Authorization = `Bearer ${newAccess}`;
      original.headers.Authorization               = `Bearer ${newAccess}`;

      processQueue(null, newAccess);
      return client(original);
    } catch (refreshErr) {
      processQueue(refreshErr, null);
      clearAuthStorage();
      if (!window.location.pathname.startsWith('/login')) window.location.assign('/login');
      return Promise.reject(refreshErr);
    } finally {
      _isRefreshing = false;
    }
  },
);

function messageFromAxiosError(error) {
  const body = error.response?.data;
  if (typeof body === 'string') return body;
  if (body?.message) return body.message;
  if (Array.isArray(body?.errors) && body.errors.length) return body.errors.join(' ');
  return error.message || 'طلب غير ناجح';
}

const visaTypeMap = {
  tourist: 1,
  business: 2,
  student: 3,
  work: 4,
  transit: 5,
  medical: 6,
};

const visaTypeLabelAr = {
  Tourist: 'سياحية',
  Business: 'أعمال',
  Student: 'دراسية',
  Work: 'عمل',
  Transit: 'عبور',
  Medical: 'علاجية',
};

/** Parses "Tag: value" segments produced by the API (ComposeApplicationNotes). */
function parseNoteTag(notes, tag) {
  if (!notes || typeof notes !== 'string') return '';
  const escaped = tag.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const re = new RegExp(`${escaped}:\\s*([^|]+?)(?=\\s*\\||$)`, 'i');
  const m = notes.match(re);
  return m ? m[1].trim() : '';
}

/**
 * Maps API `VisaApplicationResponseDto` (camelCase JSON) to the list card shape.
 * @param {object} dto
 * @param {string} [fallbackCountry] — e.g. right after create from the form
 */
function mapVisaDtoToCard(dto, fallbackCountry = '') {
  const travelFromNotes = parseNoteTag(dto.notes, 'Travel date');
  const destinationFromNotes = parseNoteTag(dto.notes, 'Destination');
  const country =
    fallbackCountry || destinationFromNotes || (dto.visaType ? String(dto.visaType) : '');
  const date =
    travelFromNotes ||
    (dto.submissionDate ? String(dto.submissionDate).slice(0, 10) : '') ||
    '';

  return {
    id: dto.id,
    country,
    date,
    status: dto.status,
    reference: `VZ-${String(dto.id).replace(/-/g, '').slice(0, 8)}`,
    createdAt: dto.submissionDate,
    userEmail: '',
    visaType: dto.visaType,
    visaTypeLabel: dto.visaType ? visaTypeLabelAr[dto.visaType] || dto.visaType : '',
  };
}

export const api = {
  auth: {
    /**
     * @returns {Promise<{ accessToken: string, refreshToken: string, user: { name: string, email: string, role: string } }>}
     */
    async login(credentials) {
      try {
        const { data } = await client.post('/api/auth/login', {
          email: credentials.email,
          password: credentials.password,
        });

        if (!data?.success || !data.data) {
          throw new Error(data?.message || 'بيانات الدخول غير صحيحة');
        }

        const d = data.data;
        return {
          accessToken: d.accessToken,
          refreshToken: d.refreshToken,
          expiresAt: d.expiresAt,
          user: {
            name: d.fullName,
            email: d.email,
            role: d.role,
          },
        };
      } catch (err) {
        if (axios.isAxiosError(err)) {
          throw new Error(messageFromAxiosError(err));
        }
        throw err;
      }
    },

    /**
     * Invalidates the refresh token on the backend.
     * Maps to POST /api/auth/logout  (Bearer token in header is enough;
     * we also send the refresh token so the server can revoke it from the DB).
     * @returns {Promise<void>}
     */
    async logout() {
      const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
      try {
        await client.post('/api/auth/logout', { refreshToken });
      } catch (err) {
        // Surface non-Axios errors in dev; swallow 4xx/5xx — caller handles cleanup.
        if (!axios.isAxiosError(err)) throw err;
      }
    },
  },

  admin: {
    /**
     * Paginated list of all visa applications (Admin JWT only — matches GET /api/visa).
     * @param {number} [page]
     * @param {number} [pageSize]
     */
    async getAllVisas(page = 1, pageSize = 50) {
      const { data } = await client.get('/api/visa', { params: { page, pageSize } }).catch((e) => {
        throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
      });
      if (data && data.success === false) {
        throw new Error(data.message || 'Forbidden or failed to load applications');
      }
      const pageData = data?.data ?? data;
      const raw = pageData?.items ?? [];
      const items = raw.map((row) => ({
        id: row.id,
        applicantName: row.applicantName || row.applicantFullName || '',
        visaType: row.visaType,
        visaTypeLabel: visaTypeLabelAr[row.visaType] || row.visaType,
        status: row.status,
        submissionDate: row.submissionDate,
        notes: row.notes,
        // Fix #6: structured field — no longer need to parse from Notes string
        destination: row.destinationCountry || parseNoteTag(row.notes, 'Destination'),
        nationality: row.nationality || '',
        intendedTravelDate: row.intendedTravelDate || '',
        reference: `VZ-${String(row.id).replace(/-/g, '').slice(0, 8)}`,
      }));
      return {
        items,
        totalCount: pageData?.totalCount ?? items.length,
        page: pageData?.page ?? page,
        pageSize: pageData?.pageSize ?? pageSize,
      };
    },

    /**
     * Maps to PUT /api/visa/{id}/approve or /reject (Admin JWT only).
     * @param {string} id Guid string
     * @param {'Approved'|'Rejected'} newStatus
     * @param {string|null} [reviewNotes]
     */
    async updateVisaStatus(id, newStatus, reviewNotes = null) {
      const s = String(newStatus).trim();
      const pathApprove = `/api/visa/${id}/approve`;
      const pathReject = `/api/visa/${id}/reject`;

      try {
        if (s === 'Approved') {
          const { data } = await client.put(pathApprove, { notes: reviewNotes });
          if (data && data.success === false) {
            throw new Error(data.message || 'Approve failed');
          }
          return data?.data ?? data;
        }
        if (s === 'Rejected') {
          const { data } = await client.put(pathReject, {
            notes: reviewNotes != null ? reviewNotes : 'Rejected by administrator.',
          });
          if (data && data.success === false) {
            throw new Error(data.message || 'Reject failed');
          }
          return data?.data ?? data;
        }
        throw new Error('newStatus must be Approved or Rejected');
      } catch (e) {
        if (axios.isAxiosError(e)) {
          throw new Error(messageFromAxiosError(e));
        }
        throw e;
      }
    },

    /**
     * Unified PATCH → /api/visa/{id}/status  (Admin JWT only — new endpoint).
     * Preferred over updateVisaStatus for new UI code.
     * @param {string} id  Guid string
     * @param {'Approved'|'Rejected'} status
     * @param {string|null} [notes]
     */
    async patchVisaStatus(id, status, notes = null) {
      try {
        const { data } = await client.patch(`/api/visa/${id}/status`, {
          status: String(status).trim(),
          notes,
        });
        if (data && data.success === false) {
          throw new Error(data.message || 'Status update failed');
        }
        return data?.data ?? data;
      } catch (e) {
        if (axios.isAxiosError(e)) throw new Error(messageFromAxiosError(e));
        throw e;
      }
    },
  },
};

/** @deprecated Prefer `api.auth.login` */
export const login = (credentials) => api.auth.login(credentials);

export async function createReservation(payload) {
  const { data } = await client.post('/api/reservations', payload).catch((e) => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function getReservations(userEmail) {
  const { data } = await client
    .get('/api/reservations', { params: userEmail ? { userEmail } : {} })
    .catch((e) => {
      throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
    });
  return data?.data ?? data;
}

export async function createVisa(payload) {
  const visaType = visaTypeMap[payload.type] ?? 1;
  /** Mirrors `CreateVisaApplicationDto` (ASP.NET binds JSON camelCase → PascalCase). */
  const body = {
    visaType,
    destinationCountry: payload.country?.trim() || undefined,
    intendedTravelDate: payload.date || undefined,
    applicantFullName: payload.fullName?.trim() || undefined,
    passportNumber: payload.passport?.trim() || undefined,
    nationality: payload.nationality?.trim() || undefined,
  };

  const { data } = await client.post('/api/visa/apply', body).catch((e) => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });

  if (data && data.success === false) {
    throw new Error(data.message || 'تعذر إنشاء الطلب');
  }

  const dto = data?.data ?? data;
  return mapVisaDtoToCard(dto, payload.country);
}

export async function getVisas(_userEmail) {
  const { data } = await client.get('/api/visa/my-requests').catch((e) => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });

  if (data && data.success === false) {
    throw new Error(data.message || 'تعذر تحميل الطلبات');
  }

  const page = data?.data ?? data;
  const items = page?.items ?? [];
  return items.map((row) => mapVisaDtoToCard(row));
}

/**
 * GET /api/visa/stats — aggregated counts for dashboard cards.
 * Requires Admin or Employee JWT.
 * @returns {Promise<{ total: number, pending: number, approved: number, rejected: number }>}
 */
export async function getVisaStats() {
  const { data } = await client.get('/api/visa/stats').catch((e) => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  // Endpoint returns the VisaStatsDto directly (not wrapped in ApiResponse)
  return data?.data ?? data;
}

/**
 * Upload a passport scan (or any document) for a visa application.
 * Maps to POST /api/documents/upload  (multipart/form-data).
 *
 * @param {File}   file            — the File object from the <input type="file">
 * @param {string} applicationId   — Guid returned by createVisa()
 * @param {number} [fileType=1]    — DocumentType enum: 1=Passport, 2=Photo, 3=SupportingDocument
 * @returns {Promise<object>}      — { id, applicationId, fileName, filePath, fileType, uploadedAt }
 */
export async function uploadPassportDocument(file, applicationId, fileType = 1) {
  // Build FormData — axios automatically sets multipart/form-data + boundary
  const form = new FormData();
  form.append('File', file);               // must match [FromForm] field name on the backend
  form.append('ApplicationId', applicationId);
  form.append('FileType', String(fileType));

  try {
    const { data } = await client.post('/api/documents/upload', form, {
      // Do NOT set Content-Type here — axios adds it with the correct boundary
      headers: { 'Content-Type': undefined },
    });

    if (data && data.success === false) {
      throw new Error(data.message || 'تعذر رفع المستند');
    }

    return data?.data ?? data;
  } catch (e) {
    if (axios.isAxiosError(e)) throw new Error(messageFromAxiosError(e));
    throw e;
  }
}

// ── Payment Gateway Integration ───────────────────────────────────────────────

/**
 * Phase 1 — Initiate a checkout session.
 * Maps to POST /api/payments/checkout
 *
 * @param {{ applicationId: string, amount: number, currency?: string, method?: number }} payload
 *   method: 1=CreditCard, 2=DebitCard, 3=BankTransfer, 4=OnlineWallet (default 1)
 * @returns {Promise<{ paymentId: string, sessionToken: string, amount: number, currency: string, description: string, expiresAt: string }>}
 */
export async function initiateCheckout(payload) {
  try {
    const { data } = await client.post('/api/payments/checkout', {
      applicationId: payload.applicationId,
      amount:        payload.amount,
      currency:      payload.currency ?? 'USD',
      method:        payload.method   ?? 1,      // default: CreditCard
      notes:         payload.notes    ?? null,
    });
    if (data && data.success === false)
      throw new Error(data.message || 'تعذر بدء عملية الدفع');
    return data?.data ?? data;
  } catch (e) {
    if (axios.isAxiosError(e)) throw new Error(messageFromAxiosError(e));
    throw e;
  }
}

/**
 * Phase 2 — Confirm payment via the simulated webhook.
 * In production the payment gateway calls this directly; in dev the frontend calls it
 * after the user "completes" the checkout modal.
 *
 * Maps to POST /api/payments/webhook/confirm  (no JWT required — auth via sessionToken)
 *
 * @param {{ paymentId: string, sessionToken: string, status: 'Paid'|'Failed', gatewayReference?: string }} payload
 * @returns {Promise<PaymentResponseDto>}
 */
export async function confirmPayment(payload) {
  try {
    const { data } = await client.post('/api/payments/webhook/confirm', {
      paymentId:        payload.paymentId,
      sessionToken:     payload.sessionToken,
      status:           payload.status,
      gatewayReference: payload.gatewayReference ?? null,
    });
    if (data && data.success === false)
      throw new Error(data.message || 'فشل تأكيد الدفع');
    return data?.data ?? data;
  } catch (e) {
    if (axios.isAxiosError(e)) throw new Error(messageFromAxiosError(e));
    throw e;
  }
}

/**
 * Poll payment status after the checkout completes.
 * Maps to GET /api/payments/{paymentId}
 *
 * @param {string} paymentId
 * @returns {Promise<PaymentResponseDto>}
 */
export async function pollPaymentStatus(paymentId) {
  try {
    const { data } = await client.get(`/api/payments/${paymentId}`);
    return data?.data ?? data;
  } catch (e) {
    if (axios.isAxiosError(e)) throw new Error(messageFromAxiosError(e));
    throw e;
  }
}

// ── Countries Settings (Admin) ──────────────────────────────────────────────

export async function getCountries() {
  const { data } = await client.get('/api/countries').catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function createCountry(isoCode, name, description = '', isActive = true) {
  const { data } = await client.post('/api/countries', { isoCode, name, description, isActive }).catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function updateCountry(isoCode, name, description = '', isActive = true) {
  const { data } = await client.put(`/api/countries/${isoCode}`, { isoCode, name, description, isActive }).catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function deleteCountry(id) {
  const { data } = await client.delete(`/api/countries/${id}`).catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

// ── Pricing Settings (Admin) ────────────────────────────────────────────────

export async function getPrices() {
  const { data } = await client.get('/api/prices').catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function createPrice(isoCode, amountUsd, description = '') {
  const { data } = await client.post('/api/prices', { isoCode, amountUsd, description }).catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function updatePrice(isoCode, amountUsd, description = '') {
  const { data } = await client.put(`/api/prices/${isoCode}`, { isoCode, amountUsd, description }).catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function deletePrice(id) {
  const { data } = await client.delete(`/api/prices/${id}`).catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

// ── Required Documents Settings (Admin) ─────────────────────────────────────

export async function getRequiredDocs() {
  const { data } = await client.get('/api/reqdocs').catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function createRequiredDoc(slug, name, description = '', isMandatory = true) {
  const { data } = await client.post('/api/reqdocs', { slug, name, description, isMandatory }).catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function updateRequiredDoc(slug, name, description = '', isMandatory = true) {
  const { data } = await client.put(`/api/reqdocs/${slug}`, { slug, name, description, isMandatory }).catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export async function deleteRequiredDoc(id) {
  const { data } = await client.delete(`/api/reqdocs/${id}`).catch(e => {
    throw new Error(axios.isAxiosError(e) ? messageFromAxiosError(e) : e.message);
  });
  return data?.data ?? data;
}

export default client;
