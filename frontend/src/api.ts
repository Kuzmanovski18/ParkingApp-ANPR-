// src/api.ts

// Prefer .env -> VITE_API_BASE, fallback to local dev
const API_BASE = (import.meta.env.VITE_API_BASE as string | undefined) ?? "https://localhost:65137";

// If you store JWT somewhere else, change this key
const TOKEN_KEY = "token";

function getToken(explicitToken?: string) {
  return explicitToken ?? localStorage.getItem(TOKEN_KEY) ?? "";
}

async function readErrorText(res: Response): Promise<string> {
  // Try JSON first (ProblemDetails), then text
  try {
    const ct = res.headers.get("content-type") || "";
    if (ct.includes("application/json")) {
      const j = await res.json().catch(() => null);
      if (j) return JSON.stringify(j);
    }
  } catch { /* ignore */ }

  return await res.text().catch(() => "");
}

// -------------------- JSON helpers --------------------
export async function getJson<T>(path: string, token?: string): Promise<T> {
  const t = getToken(token);

  const res = await fetch(API_BASE + path, {
    method: "GET",
    headers: {
      ...(t ? { Authorization: `Bearer ${t}` } : {}),
    },
  });

  if (!res.ok) {
    const txt = await readErrorText(res);
    throw new Error(`${res.status} ${res.statusText} ${txt}`.trim());
  }

  return (await res.json()) as T;
}

export async function postJson<T>(path: string, body: any, token?: string): Promise<T> {
  const t = getToken(token);

  const res = await fetch(API_BASE + path, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(t ? { Authorization: `Bearer ${t}` } : {}),
    },
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    const txt = await readErrorText(res);
    throw new Error(`${res.status} ${res.statusText} ${txt}`.trim());
  }

  return (await res.json()) as T;
}

export async function del(path: string, token?: string): Promise<void> {
  const t = getToken(token);

  const res = await fetch(API_BASE + path, {
    method: "DELETE",
    headers: {
      ...(t ? { Authorization: `Bearer ${t}` } : {}),
    },
  });

  if (!res.ok) {
    const txt = await readErrorText(res);
    throw new Error(`${res.status} ${res.statusText} ${txt}`.trim());
  }
}

// -------------------- FILE upload helper --------------------
// Backend expects: IFormFile image → formKey MUST be "image"
export async function postFile<T>(
  path: string,
  file: File,
  token?: string,
  formKey: string = "image",
  extra?: Record<string, string>
): Promise<T> {
  const t = getToken(token);

  const fd = new FormData();
  fd.append(formKey, file);

  if (extra) {
    for (const [k, v] of Object.entries(extra)) fd.append(k, v);
  }

  const res = await fetch(API_BASE + path, {
    method: "POST",
    headers: {
      ...(t ? { Authorization: `Bearer ${t}` } : {}),
      // IMPORTANT: do not set Content-Type manually for FormData
    },
    body: fd,
  });

  if (!res.ok) {
    const txt = await readErrorText(res);
    throw new Error(`${res.status} ${res.statusText} ${txt}`.trim());
  }

  return (await res.json()) as T;
}
