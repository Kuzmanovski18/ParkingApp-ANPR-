import { useMemo, useState } from "react";
import { postJson } from "../api";

type Props = {
  onToken: (t: string) => void;
  onGoRegister: () => void;
};

type LoginResp =
  | { token: string }
  | { Token: string };

function isValidEmail(s: string) {
  const v = s.trim();
  return v.includes("@") && v.includes(".") && v.length >= 6;
}

export default function Login({ onToken, onGoRegister }: Props) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState("");

  const canSubmit = useMemo(() => {
    return !busy && isValidEmail(email) && password.trim().length >= 6;
  }, [busy, email, password]);

  async function submit() {
    setErr("");
    setBusy(true);
    try {
      const res = await postJson<LoginResp>("/api/auth/login", {
        email: email.trim(),
        password,
      });

      const token = (res as any).token ?? (res as any).Token;
      if (!token) {
        setErr("No token returned from server.");
        return;
      }

      onToken(token);
    } catch (e: any) {
      setErr(e?.message || "Login failed.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="card">
      <div className="cardTitle" style={{ margin: 0 }}>Login</div>
      <div className="muted">Sign in to continue.</div>

      {err && <div style={{ marginTop: 12 }} className="badge badgeWarn">⚠ {err}</div>}

      <div style={{ marginTop: 14 }} className="stack">
        <div>
          <div className="muted">Email</div>
          <input
            className="input"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="you@email.com"
            autoComplete="email"
            disabled={busy}
          />
          {!isValidEmail(email) && email.trim().length > 0 && (
            <div className="muted" style={{ marginTop: 6 }}>
              Please enter a valid email.
            </div>
          )}
        </div>

        <div>
          <div className="muted">Password</div>
          <input
            className="input"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            autoComplete="current-password"
            disabled={busy}
          />
          {password.trim().length > 0 && password.trim().length < 6 && (
            <div className="muted" style={{ marginTop: 6 }}>
              Password must be at least 8 characters.
            </div>
          )}
        </div>

        <button className="btn" disabled={!canSubmit} onClick={submit}>
          {busy ? "Signing in..." : "Login"}
        </button>

        <button className="btn btnGhost" onClick={onGoRegister} disabled={busy}>
          Create account
        </button>
      </div>
    </div>
  );
}
