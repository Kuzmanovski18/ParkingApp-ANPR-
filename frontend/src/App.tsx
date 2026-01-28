import { useMemo, useState, useEffect } from "react";
import SideMenu from "./components/SideMenu";

import HomeCards from "./pages/HomeCards";
import MyCars from "./pages/MyCars";

import Entry from "./pages/Entry";
import Exit from "./pages/Exit";
import Pay from "./pages/Pay";
import BuyMembership from "./pages/BuyMembership";

import Login from "./pages/Login";
import Register from "./pages/Register";
import Profile from "./pages/Profile";
import MyPayments from "./pages/MyPayments";
import Admin from "./pages/Admin";
import PaymentSuccess from "./pages/PaymentSuccess";

import { useInterval } from "./hooks/useInterval";
import { getJson, postJson } from "./api";
import { secondsBetweenUtc, formatMmSs } from "./utils/time";

type Tab =
  | "home"
  | "anpr"
  | "pay"
  | "membership"
  | "cars"
  | "profile"
  | "payments"
  | "register"
  | "admin"
  | "success";

type Role = "User" | "Admin";

const AUTH_TOKEN_KEY = "auth_token";
const ROLE_KEY = "role_cache";

function Icon({ name }: { name: "home" | "cam" | "pay" | "card" | "car" | "shield" }) {
  const style: React.CSSProperties = { width: 22, height: 22, opacity: 0.95 };
  const map: Record<string, string> = {
    home: "🏠",
    cam: "📷",
    pay: "💳",
    card: "🪪",
    car: "🚗",
    shield: "🛡️",
  };
  return <div style={style}>{map[name]}</div>;
}

type SessionDto = {
  plate: string;
  entryUtc: string;
  status: string;
  isMember: boolean;
  currentAmount: number;
  graceUntilUtc?: string | null;
};

type MeDto = {
  id: string;
  username: string;
  email?: string | null;
  createdUtc: string;
  role: Role;
};

function HomeTimerCard({
  go,
  token,
  role,
}: {
  go: (t: Tab) => void;
  token: string;
  role: Role | null;
}) {
  const [plate, setPlate] = useState("");
  const [data, setData] = useState<SessionDto | null>(null);
  const [err, setErr] = useState("");
  const [now, setNow] = useState(() => new Date());
  const [touched, setTouched] = useState(false);

  useInterval(() => setNow(new Date()), 1000);

  async function load() {
    setTouched(true);
    setErr("");
    setData(null);

    if (!plate.trim()) {
      setErr("Enter a plate first.");
      return;
    }

    try {
      const s = await getJson<SessionDto>(
        `/api/parking/active-session?plate=${encodeURIComponent(plate.trim())}`,
        token
      );
      setData(s);
    } catch (e: any) {
      setErr(String(e?.message || e || "Failed to load active session."));
    }
  }

  const elapsed = useMemo(
    () => (data ? secondsBetweenUtc(now, data.entryUtc, 9) : 0),
    [now, data]
  );

  const ringPercent = useMemo(() => {
    const max = 3600;
    return Math.min(100, Math.round((elapsed / max) * 100));
  }, [elapsed]);

  async function refreshQuote() {
    setTouched(true);
    setErr("");

    if (!plate.trim()) {
      setErr("Enter a plate first.");
      return;
    }

    try {
      const q = await postJson<any>("/api/parking/quote-and-pay", { plate }, token);

      // ако немаш data (не си Load-нал), направи минимален view
      setData((prev) => ({
        plate: (prev?.plate ?? plate).toUpperCase(),
        entryUtc: prev?.entryUtc ?? new Date().toISOString(),
        status: q.status ?? prev?.status ?? "Active",
        isMember: q.isMember ?? prev?.isMember ?? false,
        currentAmount: q.amount ?? prev?.currentAmount ?? 30, // ✅ минимум 30
        graceUntilUtc: null,
      }));
    } catch (e: any) {
      setErr(String(e?.message || "Failed to calculate price."));
    }
  }

  const displayAmount =
    data?.isMember ? 0 : Math.max(30, Number(data?.currentAmount ?? 0));

  return (
    <div className="card">
      <div className="row" style={{ alignItems: "flex-start" }}>
        <div>
          <div className="cardTitle">Active parking</div>
          <div className="muted">Track by plate</div>

          <div style={{ marginTop: 10 }}>
            <input
              className="input"
              value={plate}
              onChange={(e) => setPlate(e.target.value.toUpperCase())}
              placeholder="SK1234AB"
            />
          </div>

          <div style={{ marginTop: 10 }} className="row">
            <button className="btn btnGhost" onClick={load}>
              Load
            </button>

            <button
              className="btn"
              onClick={() => go("pay")}
              disabled={!plate.trim()}
              title={!plate.trim() ? "Enter a plate first" : "Go to payments"}
            >
              Pay
            </button>
          </div>
        </div>

        <div style={{ textAlign: "right" }}>
          <div className="muted">Rate</div>
          <div style={{ fontWeight: 900, fontSize: 18 }}>30 ден/час</div>
        </div>
      </div>

      {touched && err && (
        <div style={{ marginTop: 12 }} className="badge badgeWarn">
          ⚠ {err}
        </div>
      )}

      {data && (
        <>
          <div style={{ marginTop: 12 }} className="row">
            <div className={`badge ${data.isMember ? "badgeGood" : ""}`}>
              {data.isMember ? "✅ Member" : "— Pay as you go"}
            </div>
            <div className="badge badgeGood">{data.status}</div>
          </div>

          <div className="timerWrap">
            <div className="ring" style={{ ["--p" as any]: `${ringPercent}%` }}>
              <div className="ringInner">
                <div style={{ textAlign: "center" }}>
                  <div className="muted">Elapsed time</div>
                  <div className="bigTime">{formatMmSs(elapsed)}</div>
                </div>
              </div>
            </div>
          </div>

          <div style={{ marginTop: 10 }} className="row">
            <div className="card">
              <div className="muted">Plate</div>
              <div style={{ fontWeight: 900 }}>{data.plate}</div>
            </div>

            <div className="card" style={{ textAlign: "right" }}>
              <div className="muted">Current amount</div>
              <div style={{ fontWeight: 900 }}>{data.isMember ? "0 ден" : `${displayAmount} ден`}</div>
            </div>
          </div>

          <div style={{ marginTop: 10 }} className="row">
            <button className="btn btnGhost" onClick={refreshQuote}>
              Recalculate / Pay
            </button>
            <button className="btn btnGhost" onClick={() => go("anpr")}>
              Go to ANPR
            </button>
          </div>
        </>
      )}
    </div>
  );
}


export default function App() {
  const [tab, setTab] = useState<Tab>("home");
  const [menuOpen, setMenuOpen] = useState(false);

  const [token, setToken] = useState<string>(localStorage.getItem(AUTH_TOKEN_KEY) || "");
  const [role, setRole] = useState<Role | null>(((localStorage.getItem(ROLE_KEY) as Role) || null));

  const [me, setMe] = useState<MeDto | null>(null);

  async function refreshMe(t: string) {
    try {
      const r = await getJson<MeDto>("/api/users/me", t);
      setMe(r);
      setRole(r.role);
      localStorage.setItem(ROLE_KEY, r.role);
    } catch {
      setMe(null);
      setRole(null);
      localStorage.removeItem(ROLE_KEY);
    }
  }

  function onLoginToken(t: string) {
    setToken(t);
    localStorage.setItem(AUTH_TOKEN_KEY, t);
    refreshMe(t);
    setTab("home");
  }

  function logout() {
    setToken("");
    setRole(null);
    setMe(null);
    localStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem(ROLE_KEY);
    setTab("home");
  }

  useEffect(() => {
    if (token) refreshMe(token);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    const url = new URL(window.location.href);
    const sessionId = url.searchParams.get("session_id");
    const success = url.searchParams.get("success");
    if (sessionId || success === "1") setTab("success");
  }, []);

  function clearUrlParams() {
    window.history.replaceState({}, "", window.location.pathname);
  }

  const headerTitle = useMemo(() => {
    switch (tab) {
      case "home":
        return "Home";
      case "anpr":
        return "ANPR";
      case "pay":
        return "Payments";
      case "membership":
        return "Membership";
      case "cars":
        return "My Cars";
      case "profile":
        return "My Profile";
      case "payments":
        return "My Payments";
      case "admin":
        return "Admin";
      case "register":
        return "Register";
      case "success":
        return "Payment Success";
      default:
        return "ANPR Parking";
    }
  }, [tab]);

  return (
    <div className="appShell">
      <div className="shellWrap">
        <SideMenu open={menuOpen} onClose={() => setMenuOpen(false)} onNav={(t: Tab) => setTab(t)} />

        <div className="header">
          <div className="headerTop">
            <div className="pill" title="Menu" onClick={() => setMenuOpen(true)} style={{ cursor: "pointer" }}>
              ☰
            </div>

            <div style={{ fontWeight: 900, letterSpacing: "-0.02em" }}>{headerTitle}</div>

            <div className="row" style={{ gap: 8 }}>
              {token ? (
                <button className="btn btnGhost" onClick={logout}>
                  Logout
                </button>
              ) : (
                <button className="btn btnGhost" onClick={() => setTab("cars")}>
                  Login
                </button>
              )}

              <div className="avatar" title="Profile" onClick={() => setTab("profile")} style={{ cursor: "pointer" }} />
            </div>
          </div>

          <div className="hi">Hi, {token ? (me?.username ?? "…") : "Guest"} 👋</div>
          <div className="title">ANPR Parking</div>

          <div style={{ marginTop: 6 }} className="muted">
            {token ? `Logged in as ${role ?? "User"}` : "Not logged in"}
          </div>
        </div>

        <div className="content">
          {tab === "home" && (
            <>
              <HomeTimerCard go={setTab} token={token} role={role} />
              <div style={{ marginTop: 12 }}>
                <HomeCards onPay={() => setTab("pay")} />
              </div>
            </>
          )}

          {tab === "anpr" && (
            <>
              <Entry />
              <div style={{ marginTop: 12 }}>
                <Exit />
              </div>
            </>
          )}

          {tab === "pay" && (token ? <Pay token={token} /> : <Login onToken={onLoginToken} onGoRegister={() => setTab("register")} />)}

          {tab === "membership" &&
            (token ? <BuyMembership token={token} /> : <Login onToken={onLoginToken} onGoRegister={() => setTab("register")} />)}

          {tab === "cars" && (token ? <MyCars token={token} /> : <Login onToken={onLoginToken} onGoRegister={() => setTab("register")} />)}

          {tab === "register" && <Register onRegistered={() => setTab("cars")} />}

          {tab === "profile" &&
            (token ? <Profile token={token} onLogout={logout} onGoPayments={() => setTab("payments")} /> : <Login onToken={onLoginToken} onGoRegister={() => setTab("register")} />)}

          {tab === "payments" &&
            (token ? <MyPayments token={token} onBack={() => setTab("profile")} /> : <Login onToken={onLoginToken} onGoRegister={() => setTab("register")} />)}

          {tab === "admin" && (token && role === "Admin" ? <Admin token={token} /> : <Login onToken={onLoginToken} onGoRegister={() => setTab("register")} />)}

          {tab === "success" &&
            (token ? (
              <PaymentSuccess
                token={token}
                onDone={() => {
                  clearUrlParams();
                  setTab("profile");
                }}
                onGoPayments={() => {
                  clearUrlParams();
                  setTab("payments");
                }}
              />
            ) : (
              <Login
                onToken={(t) => {
                  onLoginToken(t);
                  setTab("success");
                }}
                onGoRegister={() => setTab("register")}
              />
            ))}
        </div>

        <div className="bottomNav" style={{ gridTemplateColumns: "repeat(5, 1fr)" }}>
          <div className={`navItem ${tab === "home" ? "navItemActive" : ""}`} onClick={() => setTab("home")}>
            <Icon name="home" />
            Home
          </div>

          <div className={`navItem ${tab === "anpr" ? "navItemActive" : ""}`} onClick={() => setTab("anpr")}>
            <Icon name="cam" />
            ANPR
          </div>

          <div className={`navItem ${tab === "pay" ? "navItemActive" : ""}`} onClick={() => setTab("pay")}>
            <Icon name="pay" />
            Pay
          </div>

          <div className={`navItem ${tab === "membership" ? "navItemActive" : ""}`} onClick={() => setTab("membership")}>
            <Icon name="card" />
            Card
          </div>

          <div className={`navItem ${tab === "cars" ? "navItemActive" : ""}`} onClick={() => setTab("cars")}>
            <Icon name="car" />
            Cars
          </div>
        </div>

        {role === "Admin" && (
          <div style={{ marginTop: 10 }} className="muted">
            Admin access enabled ✅ (open from SideMenu or navigate to Admin tab)
          </div>
        )}
      </div>
    </div>
  );
}
