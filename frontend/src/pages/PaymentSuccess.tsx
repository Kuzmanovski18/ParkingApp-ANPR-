import { useEffect, useState } from "react";
import { postJson } from "../api";

type ConfirmResponse = {
  kind?: string;
  plate?: string;
  isMember?: boolean;
  amount?: number;
  graceUntilUtc?: string | null;
  sessionId?: string | null;
};

export default function PaymentSuccess({
  token,
  onDone,
  onGoPayments,
}: {
  token: string;
  onDone: () => void;
  onGoPayments: () => void;
}) {
  const [status, setStatus] = useState<"loading" | "ok" | "err">("loading");
  const [msg, setMsg] = useState("Confirming payment...");
  const [err, setErr] = useState("");
  const [data, setData] = useState<ConfirmResponse | null>(null);

  useEffect(() => {
    (async () => {
      setErr("");
      setData(null);
      setStatus("loading");
      setMsg("Confirming payment...");

      try {
        const url = new URL(window.location.href);
        const sessionId = url.searchParams.get("session_id");

        if (!sessionId) {
          setStatus("err");
          setErr("Missing session_id in URL.");
          return;
        }

        // ✅ Confirm payment (backend reads Stripe session + metadata)
        // IMPORTANT: this matches the endpoint I suggested: POST /api/stripe/confirm?sessionId=...
        const r = await postJson<ConfirmResponse>(
          `/api/stripe/confirm?sessionId=${encodeURIComponent(sessionId)}`,
          {},
          token
        );

        setData(r ?? null);
        setStatus("ok");
        setMsg("Payment confirmed ✅");
      } catch (e: any) {
        setStatus("err");
        setErr(String(e?.message || e));
      }
    })();
  }, [token]);

  return (
    <div className="card">
      <div className="cardTitle" style={{ margin: 0 }}>Payment Success</div>
      <div className="muted">
        {status === "loading" ? "Finalizing your purchase..." : msg}
      </div>

      {status === "loading" && (
        <div style={{ marginTop: 12 }} className="muted">
          Please wait…
        </div>
      )}

      {status === "ok" && (
        <>
          <div style={{ marginTop: 12 }} className="badge badgeGood">
            ✅ Your payment was successful.
          </div>

          {/* Optional details (nice for parking) */}
          {data?.kind === "Parking" && (
            <div style={{ marginTop: 12 }} className="card">
              <div className="row">
                <div>
                  <div className="muted">Plate</div>
                  <div style={{ fontWeight: 900 }}>{data.plate ?? "-"}</div>
                </div>
                <div style={{ textAlign: "right" }}>
                  <div className="muted">Amount</div>
                  <div style={{ fontWeight: 900, fontSize: 18 }}>
                    {data.isMember ? "0 ден" : `${data.amount ?? 0} ден`}
                  </div>
                </div>
              </div>

              <div style={{ marginTop: 10 }} className="row">
                <div>
                  <div className="muted">Member</div>
                  <div className={`badge ${data.isMember ? "badgeGood" : ""}`}>
                    {data.isMember ? "✅ Active membership" : "— Not a member"}
                  </div>
                </div>
                <div style={{ textAlign: "right" }}>
                  <div className="muted">Grace until (UTC)</div>
                  <div style={{ fontWeight: 800 }}>{data.graceUntilUtc ?? "-"}</div>
                </div>
              </div>
            </div>
          )}
        </>
      )}

      {status === "err" && (
        <div style={{ marginTop: 12 }} className="badge badgeWarn">
          ⚠ {err || "Failed to confirm payment."}
        </div>
      )}

      <div style={{ marginTop: 12 }} className="row">
        <button className="btn btnGhost" onClick={onDone}>
          Go to Profile
        </button>
        <button className="btn" onClick={onGoPayments}>
          View Payments
        </button>
      </div>
    </div>
  );
}
