import { useMemo, useState } from "react";

export default function HomeCards({ onPay }: { onPay: () => void }) {
  const [qrOpen, setQrOpen] = useState(false);

  // ✅ demo payload (можеш да смениш што сакаш)
  const qrValue = useMemo(() => {
    const payload = {
      kind: "RESERVED_PARKING",
      zone: "Zone 1",
      spot: "32B",
      day: "Today",
      start: "10:00 AM",
      end: "11:00 PM",
      ts: new Date().toISOString(),
    };
    return JSON.stringify(payload);
  }, []);

  const qrUrl = useMemo(() => {
    return `https://api.qrserver.com/v1/create-qr-code/?size=240x240&data=${encodeURIComponent(qrValue)}`;
  }, [qrValue]);

  return (
    <>
      <div className="card">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <div className="cardTitle" style={{ margin: 0 }}>Reserved parking spaces</div>
            <div className="muted">Zone 1 • № 32B • Today</div>
          </div>

          <div
            style={{
              width: 54,
              height: 54,
              borderRadius: 16,
              background: "rgba(15,23,42,0.06)",
              border: "1px solid rgba(148,163,184,0.25)",
              display: "grid",
              placeItems: "center",
              fontWeight: 900,
              cursor: "pointer",
              userSelect: "none",
            }}
            title="QR (demo)"
            onClick={() => setQrOpen(true)}
          >
            ▣▣
          </div>
        </div>

        <div style={{ marginTop: 10, display: "flex", justifyContent: "space-between" }}>
          <div>
            <div className="muted">Start</div>
            <div style={{ fontWeight: 900 }}>10:00 AM</div>
          </div>
          <div style={{ textAlign: "right" }}>
            <div className="muted">End</div>
            <div style={{ fontWeight: 900 }}>11:00 PM</div>
          </div>
        </div>

        <div style={{ marginTop: 12 }} className="row">
          <button className="btn" onClick={onPay}>Pay now</button>
          <button className="btn btnGhost" onClick={() => setQrOpen(true)}>Show QR</button>
        </div>
      </div>

      <div className="card">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div className="cardTitle" style={{ margin: 0 }}>Previous parking</div>
          <div className="muted">View all</div>
        </div>

        <div style={{ marginTop: 10 }} className="row">
          <div className="card">
            <div className="muted">Opal Tower</div>
            <div style={{ fontWeight: 900, marginTop: 4 }}>Home</div>
            <div className="muted" style={{ marginTop: 8 }}>15 car spots</div>
          </div>
          <div className="card">
            <div className="muted">Marina Mall</div>
            <div style={{ fontWeight: 900, marginTop: 4 }}>Office</div>
            <div className="muted" style={{ marginTop: 8 }}>80 car spots</div>
          </div>
        </div>
      </div>

      {/* ✅ QR Modal */}
      {qrOpen && (
        <div
          onClick={() => setQrOpen(false)}
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(2,6,23,0.55)",
            display: "grid",
            placeItems: "center",
            zIndex: 9999,
            padding: 16,
          }}
        >
          <div
            onClick={(e) => e.stopPropagation()}
            className="card"
            style={{
              width: "min(380px, 92vw)",
              borderRadius: 18,
            }}
          >
            <div className="row" style={{ justifyContent: "space-between", alignItems: "center" }}>
              <div style={{ fontWeight: 900, fontSize: 16 }}>QR Code (demo)</div>
              <button className="btn btnGhost" onClick={() => setQrOpen(false)}>✕</button>
            </div>

            <div style={{ marginTop: 12, display: "grid", placeItems: "center" }}>
              <img
                src={qrUrl}
                alt="QR"
                style={{
                  width: 240,
                  height: 240,
                  borderRadius: 16,
                  background: "#fff",
                  border: "1px solid rgba(148,163,184,0.35)",
                }}
              />
            </div>

            <div className="muted" style={{ marginTop: 12 }}>
              This is only for UI demo. (No real validation needed.)
            </div>

            <div style={{ marginTop: 12 }} className="row">
              <button className="btn" onClick={() => setQrOpen(false)} style={{ width: "100%" }}>
                Done
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
