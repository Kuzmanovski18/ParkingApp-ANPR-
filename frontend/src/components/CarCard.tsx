export type CarItem = {
  plate: string;
  model?: string;
  color?: string;
  member?: boolean;
};

export default function CarCard({ car }: { car: CarItem }) {
  return (
    <div className="card" style={{ display: "flex", alignItems: "center", gap: 12 }}>
      <div
        style={{
          width: 52,
          height: 52,
          borderRadius: 16,
          background: "rgba(91,108,255,0.14)",
          border: "1px solid rgba(91,108,255,0.18)",
          display: "grid",
          placeItems: "center",
          fontSize: 22,
        }}
      >
        🚗
      </div>

      <div style={{ flex: 1 }}>
        <div style={{ fontWeight: 900 }}>{car.model || "Vehicle"}</div>
        <div className="muted">{car.plate}{car.color ? ` • ${car.color}` : ""}</div>
      </div>

      <div className={`badge ${car.member ? "badgeGood" : ""}`}>
        {car.member ? "Member" : "Pay-as-you-go"}
      </div>
    </div>
  );
}
