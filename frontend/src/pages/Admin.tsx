import { useEffect, useState } from "react";
import { getJson, postJson } from "../api";
import { secondsBetweenUtc, formatMmSs } from "../utils/time";

type OverviewDto = {
  activeSessions: number;
  members: number;
  todayPayments: number;
};

type SessionDto = {
  id: string;
  plate: string;
  entryUtc: string;
  status: string;
  isMember: boolean;
  currentAmount: number;
};

export default function Admin({ token }: { token: string }) {
  const [overview, setOverview] = useState<OverviewDto | null>(null);
  const [sessions, setSessions] = useState<SessionDto[]>([]);
  const [err, setErr] = useState("");
  const [now, setNow] = useState(() => new Date());
  const [loading, setLoading] = useState(false);

  // ⏱️ live timer tick
  useEffect(() => {
    const id = setInterval(() => setNow(new Date()), 1000);
    return () => clearInterval(id);
  }, []);

  async function loadAll() {
    setErr("");
    setLoading(true);
    try {
      const [ov, ss] = await Promise.all([
        getJson<OverviewDto>("/api/admin/overview", token),
        getJson<SessionDto[]>("/api/admin/active-sessions", token),
      ]);

      setOverview(ov);
      setSessions(ss);
    } catch (e: any) {
      setErr(e?.message || "Failed to load admin data");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadAll();
  }, [token]);

  async function forceClose(sessionId: string) {
    if (!confirm("Force close this parking session?")) return;

    try {
      await postJson(`/api/admin/close-session/${sessionId}`, {}, token);
      await loadAll();
    } catch {
      alert("Failed to close session");
    }
  }

  return (
    <div className="card">
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 10 }}>
        <div>
          <div className="cardTitle" style={{ margin: 0 }}>Admin Dashboard</div>
          <div className="muted">Live sessions • Members • Payments</div>
        </div>
        <div className="badge">🛡️ Admin</div>
      </div>

      {err && <div style={{ marginTop: 12 }} className="badge badgeWarn">⚠ {err}</div>}
      {loading && <div style={{ marginTop: 12 }} className="muted">Loading…</div>}

      {/* OVERVIEW */}
      {overview && (
        <div style={{ marginTop: 12 }} className="row">
          <div className="card">
            <div className="muted">Active sessions</div>
            <div style={{ fontWeight: 900, fontSize: 26 }}>{overview.activeSessions}</div>
          </div>
          <div className="card">
            <div className="muted">Members</div>
            <div style={{ fontWeight: 900, fontSize: 26 }}>{overview.members}</div>
          </div>
          <div className="card">
            <div className="muted">Today payments</div>
            <div style={{ fontWeight: 900, fontSize: 26 }}>{overview.todayPayments}</div>
          </div>
        </div>
      )}

      {/* ACTIVE SESSIONS TABLE */}
      <div style={{ marginTop: 16 }} className="card">
        <div className="cardTitle" style={{ margin: 0 }}>Active Parking Sessions</div>

        {sessions.length === 0 && (
          <div className="muted" style={{ marginTop: 10 }}>
            No active sessions
          </div>
        )}

        {sessions.length > 0 && (
          <div style={{ marginTop: 10, overflowX: "auto" }}>
            <table style={{ width: "100%", borderCollapse: "collapse" }}>
              <thead>
                <tr className="muted">
                  <th align="left">Plate</th>
                  <th align="left">Status</th>
                  <th align="right">Elapsed</th>
                  <th align="right">Amount</th>
                  <th align="center">Member</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {sessions.map(s => {
                  const elapsed = secondsBetweenUtc(now, s.entryUtc);
                  return (
                    <tr key={s.id} style={{ borderTop: "1px solid rgba(0,0,0,0.05)" }}>
                      <td><b>{s.plate}</b></td>
                      <td>
                        <span className={`badge ${s.status === "Grace" ? "badgeWarn" : "badgeGood"}`}>
                          {s.status}
                        </span>
                      </td>
                      <td align="right">{formatMmSs(elapsed)}</td>
                      <td align="right">
                        {s.isMember ? "0 ден" : `${s.currentAmount} ден`}
                      </td>
                      <td align="center">{s.isMember ? "✅" : "—"}</td>
                      <td align="right">
                        <button
                          className="btn btnGhost"
                          onClick={() => forceClose(s.id)}
                        >
                          Force close
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Footer */}
      <div style={{ marginTop: 12 }} className="muted">
        Dashboard auto-refreshes timers every second. Use “Force close” only if necessary.
      </div>
    </div>
  );
}
