import { useEffect, useState } from "react";
import { getJson } from "../api";

type PaymentDto = {
  id: string;
  plate: string;
  amount: number;
  currency: string;
  paidAtUtc: string;
  kind: string;      // Parking | Membership
  provider: string;  // Stripe
};

export default function MyPayments({
  token,
  onBack,
}: {
  token: string;
  onBack: () => void;
}) {
  const [items, setItems] = useState<PaymentDto[]>([]);
  const [err, setErr] = useState("");

  useEffect(() => {
    (async () => {
      setErr("");
      try {
        const r = await getJson<PaymentDto[]>("/api/payments/my", token);
        setItems(r);
      } catch (e: any) {
        setErr(e?.message || "Failed to load payments.");
      }
    })();
  }, [token]);

  return (
    <div className="card">
      {/* Header со Back копче */}
      <div
        className="row"
        style={{ justifyContent: "space-between", alignItems: "center" }}
      >
        <div>
          <div className="cardTitle" style={{ margin: 0 }}>
            My Payments
          </div>
          <div className="muted">History of parking & memberships</div>
        </div>

        <button className="btn btnGhost" onClick={onBack}>
          ← Back
        </button>
      </div>

      {err && (
        <div style={{ marginTop: 12 }} className="badge badgeWarn">
          ⚠ {err}
        </div>
      )}

      {!err && items.length === 0 && (
        <div style={{ marginTop: 12 }} className="muted">
          No payments yet.
        </div>
      )}

      {items.length > 0 && (
        <div style={{ marginTop: 12 }} className="stack">
          {items.map((p) => (
            <div key={p.id} className="card">
              <div
                className="row"
                style={{ justifyContent: "space-between", alignItems: "center" }}
              >
                <div>
                  <div style={{ fontWeight: 900 }}>{p.plate}</div>
                  <div className="muted">
                    {p.kind} • {p.provider}
                  </div>
                </div>

                <div style={{ textAlign: "right" }}>
                  <div style={{ fontWeight: 900 }}>
                    {p.amount} {p.currency.toUpperCase()}
                  </div>
                  <div className="muted">
                    {new Date(p.paidAtUtc).toLocaleString()}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
