import { useState } from "react";
import { postJson } from "../api";

type CheckoutResponse =
  | { url: string; sessionId?: string }
  | { Url: string; SessionId?: string };

function getUrl(r: any) {
  return r?.url ?? r?.Url ?? null;
}

export default function Pay({ token }: { token: string }) {
  const [plate, setPlate] = useState("");
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);

  async function onPay() {
    setErr("");
    setLoading(true);

    try {
      const p = plate.trim().toUpperCase();
      if (p.length < 5) {
        setErr("Enter a valid plate.");
        return;
      }

      // ✅ NEW: backend creates Stripe Checkout Session and returns URL
      const r = await postJson<CheckoutResponse>(
        "/api/parking/checkout",
        { plate: p },
        token
      );

      const url = getUrl(r);
      if (!url) {
        setErr("No Stripe checkout URL returned from server.");
        return;
      }

      // ✅ redirect to Stripe
      window.location.href = url;
    } catch (e: any) {
      setErr(String(e?.message || e));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="card">
      <div className="cardTitle" style={{ margin: 0 }}>Pay by plate</div>
      <div className="muted">30 ден/час (по започнат час)</div>

      <div style={{ marginTop: 12 }}>
        <input
          className="input"
          value={plate}
          onChange={(e) => setPlate(e.target.value.toUpperCase())}
          placeholder="SK1234AB"
          disabled={loading}
        />
      </div>

      <div style={{ marginTop: 10 }} className="row">
        <button className="btn" onClick={onPay} disabled={loading || plate.trim().length < 5}>
          {loading ? "Redirecting..." : "Pay with Stripe"}
        </button>

        <button
          className="btn btnGhost"
          onClick={() => { setPlate(""); setErr(""); }}
          disabled={loading}
        >
          Clear
        </button>
      </div>

      {err && (
        <div style={{ marginTop: 12 }} className="badge badgeWarn">
          ⚠ {err}
        </div>
      )}
    </div>
  );
}
