export function MfaCodeInput({
  label,
  value,
  onChange,
  autoFocus = false,
}: {
  label: string;
  value: string;
  onChange: (code: string) => void;
  autoFocus?: boolean;
}) {
  return (
    <label className="grid gap-1.5 text-sm font-medium">
      {label}
      <input
        name="code"
        required
        autoFocus={autoFocus}
        inputMode="numeric"
        autoComplete="one-time-code"
        placeholder="123456"
        value={value}
        // Authenticator apps show the code as "123 456": drop everything but digits before cutting to 6.
        onChange={(event) => onChange(event.target.value.replace(/\D/g, "").slice(0, 6))}
        className="h-11 rounded-md border border-input bg-background px-3 tracking-widest outline-none focus:ring-2 focus:ring-ring"
      />
    </label>
  );
}
