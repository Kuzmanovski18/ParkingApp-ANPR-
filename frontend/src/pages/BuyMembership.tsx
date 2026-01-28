import { useMemo, useState } from "react";
import { postJson } from "../api";

type Plan = "monthly" | "yearly";

type CheckoutResponse =
  | { url: string }
  | { Url: string };

function isValidEmail(s: string) {
  const v = s.trim();
  return v.includes("@") && v.includes(".") && v.length >= 6;
}

export default function BuyMembership({ token }: { token: string }) {
  const [plate, setPlate] = useState("");
  const [plan, setPlan] = useState<Plan>("monthly");
  const [ownerName, setOwnerName] = useState("");
  const [ownerEmail, setOwnerEmail] = useState("");
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);

  const price = useMemo(() => (plan === "monthly" ? 1500 : 12000), [plan]);
  const typeLabel = useMemo(() => (plan === "monthly" ? "Monthly" : "Yearly"), [plan]);

  async function buy() {
    setErr("");
    setLoading(true);

    try {
      const payload = {
        plate: plate.trim().toUpperCase(),
        type: typeLabel,                      // ✅ "Monthly" | "Yearly"
        ownerName: ownerName.trim() || "Customer",
        ownerEmail: ownerEmail.trim(),
      };

      const r = await postJson<CheckoutResponse>("/api/memberships/checkout", payload, token);

      const url = (r as any).url ?? (r as any).Url;
      if (!url) {
        setErr("No checkout url returned from server.");
        return;
      }

      window.location.href = url;
    } catch (e: any) {
      setErr(String(e?.message || e));
    } finally {
      setLoading(false);
    }
  }

  const canBuy =
    !loading &&
    plate.trim().length >= 5 &&
    isValidEmail(ownerEmail);

  return (
    <div className="card">
      <div className="cardTitle" style={{ margin: 0 }}>Membership</div>
      <div className="muted">
        Monthly: 1500 ден • Yearly: 12000 ден • Plate stays in DB during membership
      </div>

      <div style={{ marginTop: 12 }} className="stack">
        <input
          className="input"
          value={plate}
          onChange={(e) => setPlate(e.target.value.toUpperCase())}
          placeholder="Plate (e.g. SK1234AB)"
          disabled={loading}
        />

        <input
          className="input"
          value={ownerName}
          onChange={(e) => setOwnerName(e.target.value)}
          placeholder="Owner name"
          disabled={loading}
        />

        <input
          className="input"
          value={ownerEmail}
          onChange={(e) => setOwnerEmail(e.target.value)}
          placeholder="Owner email (required)"
          disabled={loading}
        />
        {!isValidEmail(ownerEmail) && ownerEmail.trim().length > 0 && (
          <div className="muted" style={{ marginTop: 6 }}>
            Please enter a valid email.
          </div>
        )}
      </div>

      <div style={{ marginTop: 12 }} className="row">
        <button
          className={`btn ${plan === "monthly" ? "" : "btnGhost"}`}
          onClick={() => setPlan("monthly")}
          disabled={loading}
        >
          Monthly • 1500
        </button>
        <button
          className={`btn ${plan === "yearly" ? "" : "btnGhost"}`}
          onClick={() => setPlan("yearly")}
          disabled={loading}
        >
          Yearly • 12000
        </button>
      </div>

      <div style={{ marginTop: 12 }} className="card">
        <div className="muted">Selected</div>
        <div style={{ display: "flex", justifyContent: "space-between", marginTop: 6 }}>
          <div style={{ fontWeight: 900 }}>{typeLabel}</div>
          <div style={{ fontWeight: 900 }}>{price} ден</div>
        </div>
        <div className="muted" style={{ marginTop: 6 }}>
          Members do not pay hourly. Their plate is not deleted on exit.
        </div>
      </div>

      <div style={{ marginTop: 12 }}>
        <button className="btn" onClick={buy} disabled={!canBuy}>
          {loading ? "Redirecting..." : "Continue to payment"}
        </button>
      </div>

      {err && <div style={{ marginTop: 12 }} className="badge badgeWarn">⚠ {err}</div>}
    </div>
  );
}
