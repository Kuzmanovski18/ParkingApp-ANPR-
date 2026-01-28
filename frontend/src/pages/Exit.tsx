import { useState } from "react";
import { postFile } from "../api";

type ExitResult = {
  plate: string;
  result: string;
};

export default function Exit() {
  const [file, setFile] = useState<File | null>(null);
  const [resp, setResp] = useState<ExitResult | null>(null);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);

  async function send() {
    setErr("");
    setResp(null);
    if (!file) return;

    setLoading(true);
    try {
      // ✅ IMPORTANT: backend expects field name "image"
      const r = await postFile<ExitResult>("/api/anpr/exit", file);
      setResp(r);
    } catch (e: any) {
      setErr(String(e?.message || e));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="card">
      <div className="cardTitle" style={{ margin: 0 }}>ANPR Exit</div>
      <div className="muted">Upload a photo → recognize plate → decide exit</div>

      <div style={{ marginTop: 12 }}>
        <input
          className="input"
          type="file"
          accept="image/*"
          onChange={(e) => setFile(e.target.files?.[0] || null)}
        />
      </div>

      <div style={{ marginTop: 10 }} className="row">
        <button className="btn" onClick={send} disabled={!file || loading}>
          {loading ? "Processing..." : "Process"}
        </button>
        <button
          className="btn btnGhost"
          onClick={() => { setFile(null); setResp(null); setErr(""); }}
          disabled={loading}
        >
          Clear
        </button>
      </div>

      {err && <div style={{ marginTop: 12 }} className="badge badgeWarn">⚠ {err}</div>}

      {resp && (
        <div style={{ marginTop: 12 }} className="card">
          <div className="muted">Plate</div>
          <div style={{ fontWeight: 900, fontSize: 18 }}>{resp.plate ?? "-"}</div>

          <div style={{ marginTop: 8 }} className="muted">Result</div>
          <div className="badge badgeGood">{resp.result ?? "-"}</div>
        </div>
      )}
    </div>
  );
}
