export function secondsBetweenUtc(now: Date, utcIso: string, offsetSeconds: number = 0) {
  const t = new Date(utcIso).getTime();
  const raw = Math.floor((now.getTime() - t) / 1000);
  return Math.max(0, raw + offsetSeconds);
}

export function formatMmSs(totalSeconds: number) {
  const mm = Math.floor(totalSeconds / 60);
  const ss = totalSeconds % 60;
  return `${mm}m : ${String(ss).padStart(2, "0")}s`;
}
