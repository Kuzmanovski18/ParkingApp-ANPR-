import { useState } from "react";
import { postFile } from "../api";

type EntryResult = {
  plate: string;
  isMember: boolean;
  sessionId: string | null;
};

export default function Entry() {
  const [file, setFile] = useState<File | null>(null);
  const [data, setData] = useState<EntryResult | null>(null);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);

  async function onSend() {
    if (!file) return;

    setErr("");
    setData(null);
    setLoading(true);

    try {
      // ✅ IMPORTANT: backend expects field name "image"
      const r = await postFile<EntryResult>("/api/anpr/entry", file);
      setData(r);
    } catch (e: any) {
      setErr(String(e?.message || e));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="card">
      <div className="cardTitle" style={{ margin: 0 }}>ANPR Entry</div>
      <div className="muted">Upload image → detect plate → create session</div>

      <div style={{ marginTop: 12 }} className="stack">
        <input
          type="file"
          accept="image/*"
          onChange={(e) => setFile(e.target.files?.[0] ?? null)}
        />

        <button className="btn" onClick={onSend} disabled={!file || loading}>
          {loading ? "Uploading..." : "Send image"}
        </button>
      </div>

      {err && <div style={{ marginTop: 12 }} className="badge badgeWarn">⚠ {err}</div>}

      {data && (
        <div style={{ marginTop: 12 }} className="card">
          <div><b>Plate:</b> {data.plate}</div>
          <div><b>Member:</b> {data.isMember ? "Yes ✅" : "No"}</div>
          <div><b>Session:</b> {data.sessionId ?? "-"}</div>
        </div>
      )}
    </div>
  );
}
