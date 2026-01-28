import { useEffect, useState } from "react";
import { del, getJson, postJson } from "../api";

type CarDto = { id: string; plate: string; label?: string | null; createdUtc: string };

export default function MyCars({ token }: { token: string }) {
  const [cars, setCars] = useState<CarDto[]>([]);
  const [plate, setPlate] = useState("");
  const [label, setLabel] = useState("");
  const [err, setErr] = useState("");
  const [busy, setBusy] = useState(false);

  async function load() {
    setErr("");
    const r = await getJson<CarDto[]>("/api/cars/my", token);
    setCars(r);
  }

  useEffect(() => { load().catch(e => setErr(String(e?.message || e))); }, [token]);

  async function addCar() {
    setErr("");
    const p = plate.trim().toUpperCase();
    if (p.length < 3) { setErr("Invalid plate."); return; }

    setBusy(true);
    try {
      await postJson("/api/cars", { plate: p, label: label.trim() || null }, token);
      setPlate(""); setLabel("");
      await load();
    } catch (e: any) {
      setErr(e?.message || "Failed to add car.");
    } finally {
      setBusy(false);
    }
  }

  async function removeCar(id: string) {
    setErr("");
    setBusy(true);
    try {
      await del(`/api/cars/${id}`, token);
      await load();
    } catch (e: any) {
      setErr(e?.message || "Failed to delete car.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="card">
      <div className="cardTitle" style={{ margin: 0 }}>My Cars</div>
      <div className="muted">Saved plates for quick access</div>

      {err && <div style={{ marginTop: 12 }} className="badge badgeWarn">⚠ {err}</div>}

      <div style={{ marginTop: 12 }} className="card">
        <div className="muted">Add new car</div>
        <div className="row" style={{ marginTop: 8 }}>
          <input className="input" value={plate} onChange={(e) => setPlate(e.target.value)} placeholder="SK1234AB" />
          <input className="input" value={label} onChange={(e) => setLabel(e.target.value)} placeholder="Label (optional)" />
        </div>
        <div style={{ marginTop: 10 }}>
          <button className="btn" disabled={busy} onClick={addCar}>{busy ? "Saving..." : "Add car"}</button>
        </div>
      </div>

      <div style={{ marginTop: 12 }} className="stack">
        {cars.map(c => (
          <div key={c.id} className="card">
            <div className="row" style={{ justifyContent: "space-between", alignItems: "center" }}>
              <div>
                <div style={{ fontWeight: 900 }}>{c.plate}</div>
                <div className="muted">{c.label || "—"}</div>
              </div>
              <button className="btn btnGhost" disabled={busy} onClick={() => removeCar(c.id)}>Delete</button>
            </div>
          </div>
        ))}
        {cars.length === 0 && <div className="muted">No cars yet.</div>}
      </div>
    </div>
  );
}
