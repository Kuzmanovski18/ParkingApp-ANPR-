type Tab =
  | "home"
  | "anpr"
  | "pay"
  | "membership"
  | "cars"
  | "profile"
  | "payments"
  | "register"
  | "admin";

type Props = {
  open: boolean;
  onClose: () => void;
  onNav: (tab: Tab) => void;
};

export default function SideMenu({ open, onClose, onNav }: Props) {
  return (
    <>
      {open && (
        <div
          onClick={onClose}
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(15,23,42,0.35)",
            backdropFilter: "blur(2px)",
            zIndex: 50,
          }}
        />
      )}

      <div
        style={{
          position: "fixed",
          top: 0,
          bottom: 0,
          left: 0,
          width: 300,
          transform: open ? "translateX(0)" : "translateX(-110%)",
          transition: "transform 220ms ease",
          zIndex: 60,
          padding: 16,
          background:
            "linear-gradient(135deg, rgba(91,108,255,0.98), rgba(124,58,237,0.92))",
          color: "white",
          boxShadow: "0 30px 80px rgba(15,23,42,0.35)",
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div style={{ fontWeight: 900, fontSize: 16 }}>Menu</div>
          <button
            onClick={onClose}
            className="btn btnGhost"
            style={{ padding: "8px 10px", borderRadius: 12 }}
          >
            ✕
          </button>
        </div>

        <div style={{ marginTop: 14 }} className="card">
          <div style={{ display: "flex", gap: 10, alignItems: "center" }}>
            <div className="avatar" />
            <div>
              <div style={{ fontWeight: 900 }}>Nikolas</div>
              <div className="muted">ANPR Parking user</div>
            </div>
          </div>
        </div>

        <div style={{ marginTop: 12, display: "grid", gap: 10 }}>
          <button className="btn" onClick={() => { onNav("home"); onClose(); }}>🏠 Home</button>
          <button className="btn" onClick={() => { onNav("anpr"); onClose(); }}>📷 ANPR Entry/Exit</button>
          <button className="btn" onClick={() => { onNav("pay"); onClose(); }}>💳 Pay</button>
          <button className="btn" onClick={() => { onNav("membership"); onClose(); }}>🪪 Membership</button>

          <button className="btn" onClick={() => { onNav("cars"); onClose(); }}>🚗 My Cars</button>
          <button className="btn" onClick={() => { onNav("payments"); onClose(); }}>🧾 My Payments</button>
          <button className="btn" onClick={() => { onNav("profile"); onClose(); }}>👤 My Profile</button>
        </div>

        <div style={{ position: "absolute", left: 16, right: 16, bottom: 16 }}>
          <div className="card">
            <div className="muted">Support</div>
            <div style={{ fontWeight: 900, marginTop: 4 }}>parking@demo.local</div>
          </div>
        </div>
      </div>
    </>
  );
}
