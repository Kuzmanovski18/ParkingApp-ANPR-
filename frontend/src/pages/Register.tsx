import { useMemo, useState } from "react";
import { postJson } from "../api";

type Props = {
  onRegistered: () => void; // после register ќе те префрли на login
};

function isValidEmail(s: string) {
  const v = s.trim();
  return v.includes("@") && v.includes(".") && v.length >= 6;
}

export default function Register({ onRegistered }: Props) {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState("");
  const [ok, setOk] = useState("");

  const canSubmit = useMemo(() => {
    return (
      !busy &&
      username.trim().length >= 3 &&
      isValidEmail(email) &&
      password.trim().length >= 6
    );
  }, [busy, username, email, password]);

  async function submit() {
    setErr("");
    setOk("");

    if (!canSubmit) {
      setErr("Please fill all fields correctly.");
      return;
    }

    setBusy(true);
    try {
      await postJson("/api/auth/register", {
        username: username.trim(),
        email: email.trim(),
        password,
      });

      setOk("✅ Account created. Please login.");
      setTimeout(() => onRegistered(), 500);
    } catch (e: any) {
      setErr(e?.message || "Register failed.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="card">
      <div className="cardTitle" style={{ margin: 0 }}>Create account</div>
      <div className="muted">Register to manage cars, payments and membership.</div>

      {err && <div style={{ marginTop: 12 }} className="badge badgeWarn">⚠ {err}</div>}
      {ok && <div style={{ marginTop: 12 }} className="badge badgeGood">{ok}</div>}

      <div style={{ marginTop: 14 }} className="stack">
        <div>
          <div className="muted">Username</div>
          <input
            className="input"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="nikolas"
            disabled={busy}
          />
          {username.trim().length > 0 && username.trim().length < 3 && (
            <div className="muted" style={{ marginTop: 6 }}>
              Username must be at least 3 characters.
            </div>
          )}
        </div>

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
            autoComplete="new-password"
            disabled={busy}
          />
          {password.trim().length > 0 && password.trim().length < 6 && (
            <div className="muted" style={{ marginTop: 6 }}>
              Password must be at least 6 characters.
            </div>
          )}
        </div>

        <button className="btn" disabled={!canSubmit} onClick={submit}>
          {busy ? "Creating..." : "Register"}
        </button>

        <button className="btn btnGhost" onClick={onRegistered} disabled={busy}>
          Back to login
        </button>
      </div>
    </div>
  );
}
