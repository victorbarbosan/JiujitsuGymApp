# Stripe Subscriptions - Local Setup & Remaining Work

This file tracks the manual setup steps needed to run/test the Stripe
subscription integration locally, plus what's still outstanding before
production.

## 1. Local webhook testing with the Stripe CLI

Webhooks are how Stripe tells **us** that a payment/subscription event
happened (`StripeWebhookController`, route `POST /webhooks/stripe`). Stripe's
servers can't reach `localhost` directly, so the Stripe CLI forwards events
from your Stripe test account to your local app over an authenticated tunnel.

### One-time install

```powershell
winget install Stripe.StripeCLI
```

(or download from https://github.com/stripe/stripe-cli/releases)

### Log in (one time per machine)

```powershell
stripe login
```

This opens a browser to link the CLI to your Stripe account (test mode).

### Start forwarding events to your local app

With the app running (`https://localhost:7106` per `launchSettings.json`),
in a separate terminal:

```powershell
stripe listen --forward-to https://localhost:7106/webhooks/stripe --skip-verify --all-snapshot
```

- `--skip-verify` is needed because the ASP.NET Core dev-cert is not trusted
  by the CLI's own HTTP client; it's fine for local-only testing.
- `--all-snapshot` is required on newer CLI versions (they now ship two event
  shapes: classic "snapshot" events with the full object payload, and newer
  "thin" events with just an ID you'd fetch yourself). `StripeWebhookController`
  pattern-matches on `stripeEvent.Data.Object is Session` /
  `is Subscription`, which only works with snapshot events, so always pass
  `--all-snapshot` (or `--events checkout.session.completed,customer.subscription.updated,customer.subscription.deleted`
  to forward only the three events we actually handle).
- On startup the CLI prints a line like:

  ```
  Ready! Your webhook signing secret is whsec_XXXXXXXXXXXXXXXXXXXXXXXX (^C to quit)
  ```

  This secret is **ephemeral** - it's generated per `stripe listen` session
  and only valid while that command is running. Copy it.

### Store the secret via user-secrets (never commit it)

```powershell
cd JiujitsuGymApp
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_XXXXXXXXXXXXXXXXXXXXXXXX"
```

Restart the app after setting it so `IOptions<StripeOptions>` picks up the
new value.

> Note: every time you restart `stripe listen`, it mints a **new** secret.
> Update the user-secret again if it changes, or pin one with
> `stripe listen --forward-to ... --latest` (CLI docs vary by version - check
> `stripe listen --help`).

### Test end-to-end

1. Run the app (`https://localhost:7106`) and `stripe listen` side by side.
2. Sign in, go to `/Subscriptions`, pick a plan, complete the embedded
   Checkout form using a [Stripe test card](https://docs.stripe.com/testing)
   (e.g. `4242 4242 4242 4242`, any future expiry, any CVC).
3. Watch the `stripe listen` terminal - you should see
   `checkout.session.completed` forwarded with a `200` response from your app.
4. Confirm in the database (or via EF/psql) that the `Subscriptions` row for
   that user now has `StripeCustomerId`, `StripeSubscriptionId`, and
   `Status = Active` populated.
5. Optionally trigger other events manually without re-running Checkout:

   ```powershell
   stripe trigger customer.subscription.updated
   stripe trigger customer.subscription.deleted
   ```

   (These use Stripe's own fixture data, so they won't correlate to a real
   local subscription unless you pass `--add-override`; mainly useful to
   confirm the endpoint parses/signs/responds correctly.)

### Troubleshooting

- **400 Bad Request from the webhook endpoint**: signature mismatch - usually
  means the `Stripe:WebhookSecret` user-secret is stale (CLI was restarted) or
  the raw body was altered (check nothing in the pipeline is buffering/
  rewriting the request body before `StripeWebhookController` reads it).
- **CLI shows `connection refused`**: app isn't running, or the port in
  `--forward-to` doesn't match `launchSettings.json`.
- **Nothing happens after payment**: confirm `stripe listen` is still running
  and that the Checkout Session was created in the same Stripe account/mode
  (test mode) the CLI is forwarding from.

## 2. Remaining work (tracked separately, not yet implemented)

- [ ] Customer Portal integration for self-service cancellation/plan changes.
- [ ] Production webhook endpoint registration in the Stripe Dashboard
	  (Developers -> Webhooks -> Add endpoint, pointing at the deployed
	  `/webhooks/stripe` URL) and storing that endpoint's signing secret via
	  production environment variables (`Stripe__WebhookSecret`), not
	  user-secrets.
- [ ] Automated tests for `SubscriptionService` webhook handlers (signature
	  verification is hard to unit test directly - focus on
	  `HandleCheckoutSessionCompletedAsync` /
	  `HandleSubscriptionUpdatedAsync` / `HandleSubscriptionDeletedAsync`
	  business logic with constructed Stripe model objects).
- [ ] Decide on handling for invoice payment failures
	  (`invoice.payment_failed`) - currently not handled, so a card decline on
	  renewal won't update local status until Stripe itself cancels the
	  subscription after retries.
