import { useState, type FormEvent } from "react";
import { QRCodeSVG } from "qrcode.react";
import { ShieldCheck } from "lucide-react";
import { Button } from "@/components/ui/button";
import { MfaCodeInput } from "@/components/Account/MfaCodeInput";
import { disableMfa, enableMfa, setupMfa, type MfaSetup } from "@/api/mfa";

export function TwoFactorSection({
  enabled,
  onChanged,
}: {
  enabled: boolean;
  onChanged: (enabled: boolean) => void;
}) {
  const [setup, setSetup] = useState<MfaSetup | null>(null);
  const [disabling, setDisabling] = useState(false);
  const [code, setCode] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const run = async (action: () => Promise<void>) => {
    setError(null);
    setSuccess(null);
    setLoading(true);
    try {
      await action();
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Something went wrong");
    } finally {
      setCode("");
      setLoading(false);
    }
  };

  const reset = () => {
    setSetup(null);
    setDisabling(false);
    setCode("");
    setError(null);
  };

  const handleStart = () => run(async () => setSetup(await setupMfa()));

  const handleEnable = (event: FormEvent) => {
    event.preventDefault();
    void run(async () => {
      await enableMfa(code);
      setSetup(null);
      setSuccess("Two-step verification is now on.");
      onChanged(true);
    });
  };

  const handleDisable = (event: FormEvent) => {
    event.preventDefault();
    void run(async () => {
      await disableMfa(code);
      setDisabling(false);
      setSuccess("Two-step verification is now off.");
      onChanged(false);
    });
  };

  return (
    <section className="mt-8 rounded-lg border border-border bg-card p-6 sm:p-10">
      <div className="flex items-center gap-2">
        <ShieldCheck className="size-5 text-primary" />
        <h2 className="font-display text-2xl">Two-step verification</h2>
      </div>
      <p className="mt-2 text-sm text-muted-foreground">
        {enabled
          ? "On. Logging in requires a code from your authenticator app."
          : "Off. Protect your account with a code from an authenticator app, in addition to your password."}
      </p>

      {!enabled && !setup && (
        <Button className="mt-6" disabled={loading} onClick={() => void handleStart()}>
          {loading ? "Preparing…" : "Turn on"}
        </Button>
      )}

      {!enabled && setup && (
        <form className="mt-6 grid max-w-md gap-4" onSubmit={handleEnable}>
          <p className="text-sm">
            1. Scan this QR code with Google Authenticator, Microsoft Authenticator or a similar
            app.
          </p>
          <div className="w-fit rounded-md bg-white p-4">
            <QRCodeSVG value={setup.otpAuthUri} size={192} title="Authenticator setup QR code" />
          </div>
          <p className="text-sm text-muted-foreground">
            Can&apos;t scan it? Enter this key manually:
            <code className="mt-1 block break-all font-mono text-foreground">{setup.secret}</code>
          </p>
          <MfaCodeInput
            label="2. Enter the 6-digit code from the app"
            value={code}
            onChange={setCode}
          />
          <div className="flex flex-wrap gap-2">
            <Button type="submit" disabled={loading || code.length !== 6}>
              {loading ? "Checking…" : "Confirm"}
            </Button>
            <Button type="button" variant="outline" disabled={loading} onClick={reset}>
              Cancel
            </Button>
          </div>
        </form>
      )}

      {enabled && !disabling && (
        <Button
          variant="outline"
          className="mt-6 text-destructive hover:text-destructive"
          onClick={() => {
            setSuccess(null);
            setDisabling(true);
          }}
        >
          Turn off
        </Button>
      )}

      {enabled && disabling && (
        <form className="mt-6 grid max-w-md gap-4" onSubmit={handleDisable}>
          <MfaCodeInput
            label="Enter a code from your authenticator app to turn this off"
            value={code}
            onChange={setCode}
          />
          <div className="flex flex-wrap gap-2">
            <Button type="submit" variant="destructive" disabled={loading || code.length !== 6}>
              {loading ? "Checking…" : "Turn off"}
            </Button>
            <Button type="button" variant="outline" disabled={loading} onClick={reset}>
              Cancel
            </Button>
          </div>
        </form>
      )}

      {error && (
        <p
          role="alert"
          className="mt-4 max-w-md rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive"
        >
          {error}
        </p>
      )}
      {success && (
        <p
          role="status"
          className="mt-4 max-w-md rounded-md bg-primary/10 px-3 py-2 text-sm text-primary"
        >
          {success}
        </p>
      )}
    </section>
  );
}
