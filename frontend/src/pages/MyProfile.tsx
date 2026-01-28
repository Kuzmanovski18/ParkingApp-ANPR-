import { useEffect, useState } from "react";
import { getJson } from "../api";

type MeDto = {
  id: string;
  username: string;
  email?: string | null;
  createdUtc: string;
  role: "User" | "Admin";
};

export default function MyProfile({ token, onLogout }: { token: string; onLogout: () => void }) {
  const [me, setMe] = useState<MeDto | null>(null);
  const [err, setErr] = useState("");

  useEffect(() => {
    (async () => {
      setErr("");
      try {
        const r = await getJson<MeDto>("/api/users/me", token);
        setMe(r);
      } catch (e: any) {
        setErr(e?.message || "Failed to load profile.");
      }
    })();
  }, [token]);

  return (
    <div className="card">
      <div className="row" style={{ justifyContent: "space-between", alignItems: "center" }}>
        <div>
          <div className="cardTitle" style={{ margin: 0 }}>My Profile</div>
          <div className="muted">Account details</div>
        </div>
        <button className="btn btnGhost" onClick={onLogout}>Logout</button>
      </div>

      {err && <div style={{ marginTop: 12 }} className="badge badgeWarn">⚠ {err}</div>}
      {!me && !err && <div style={{ marginTop: 12 }} className="muted">Loading...</div>}

      {me && (
        <div style={{ marginTop: 12 }} className="stack">
          <div className="card">
            <div className="muted">Username</div>
            <div style={{ fontWeight: 900 }}>{me.username}</div>
          </div>

          <div className="card">
            <div className="muted">Email</div>
            <div style={{ fontWeight: 900 }}>{me.email || "—"}</div>
          </div>

          <div className="row">
            <div className="card" style={{ flex: 1 }}>
              <div className="muted">Role</div>
              <div style={{ fontWeight: 900 }}>{me.role}</div>
            </div>
            <div className="card" style={{ flex: 1 }}>
              <div className="muted">Created</div>
              <div style={{ fontWeight: 900 }}>{new Date(me.createdUtc).toLocaleString()}</div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
