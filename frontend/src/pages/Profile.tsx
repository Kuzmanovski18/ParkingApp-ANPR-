import { useEffect, useState } from "react";
import { getJson } from "../api";

type MeDto = {
  username: string;
  role: "User" | "Admin";
};

export default function Profile({
  token,
  onLogout,
  onGoPayments,
}: {
  token: string;
  onLogout: () => void;
  onGoPayments: () => void;
}) {
  const [me, setMe] = useState<MeDto | null>(null);
  const [err, setErr] = useState("");

  useEffect(() => {
    (async () => {
      setErr("");
      try {
        const res = await getJson<MeDto>("/api/auth/me", token);
        setMe(res);
      } catch {
        setErr("Failed to load profile. Are you logged in?");
      }
    })();
  }, [token]);

  return (
    <div className="card">
      <div className="cardTitle" style={{ margin: 0 }}>My Profile</div>
      <div className="muted">Account details</div>

      {err && <div style={{ marginTop: 12 }} className="badge badgeWarn">⚠ {err}</div>}
      {!me && !err && <div style={{ marginTop: 12 }} className="muted">Loading…</div>}

      {me && (
        <>
          <div style={{ marginTop: 12 }} className="row">
            <div className="card" style={{ flex: 1 }}>
              <div className="muted">Username</div>
              <div style={{ fontWeight: 900, fontSize: 18 }}>{me.username}</div>
            </div>

            <div className="card" style={{ width: 160, textAlign: "right" }}>
              <div className="muted">Role</div>
              <div className={`badge ${me.role === "Admin" ? "badgeGood" : ""}`}>
                {me.role}
              </div>
            </div>
          </div>

          <div style={{ marginTop: 12 }} className="row">
            <button className="btn" onClick={onGoPayments}>My Payments</button>
            <button className="btn btnGhost" onClick={onLogout}>Logout</button>
          </div>
        </>
      )}
    </div>
  );
}
