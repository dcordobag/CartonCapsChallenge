# Referral Feature API Spec
From Mac/Visual Studio Code: `cd to Referrals.Api` and `dotnet run` from that path

Base URL: `http://localhost:5053/v1`

For Swagger: `http://localhost:5053/swagger`

This spec covers the **new** endpoints needed to support the referral + deferred deep link feature shown in the UI mocks.

## Terminology

- **Referrer**: existing CArton Caps user sharing an invite.
- **Referee**: new user installing the app via the invite.
- **Deferred deep link vendor**: 3rd party provider that can:
  - create short links
  - attribute installs / first opens
  - return deep link payload to the app after install
  - optionally call a backend webhook for install/open events

## Auth

- Endpoints under `/v1/referrals/*` **except** `/resolve` require `Authorization: Bearer <token>`.
- The mock implementation uses `X-Mock-UserId` instead.

## Common headers

- `Idempotency-Key` (optional, recommended for POST): caller-generated UUID.
- `X-Request-Id` (optional): echoed back for tracing.

## Common error shape

All non-2xx responses use RFC 7807 Problem Details:

```json
{
  "type": "https://cartoncaps.link/problems/<category>",
  "title": "readable title",
  "status": 400,
  "detail": "More details (safe to display).",
  "instance": "/v1/referrals/links",
  "code": "rate_limited",
  "errors": { "field": ["message"] }
}
```

---

## 1) Generate a shareable referral link

Creates a **vendor-backed deferred deep link** for sharing through Text/Email/Share Sheet.

The app can call this whenever the user taps one of the share buttons, or when the Invite screen loads (depending on caching strategy).

### `POST /v1/referrals/links`

**Auth:** required

**Request**
```json
{
  "referralCode": "XY7G4D",
  "channel": "sms",
  "locale": "en-US",
  "campaign": "invite_friends",
  "destination": "onboarding",
  "client": {
    "platform": "ios",
    "appVersion": "7.12.0"
  }
}
```

**Field rules**
- `referralCode` must match the authenticated user’s code.
- `channel`: `sms | email | share | copy`
- `destination`: an app route key (e.g., `"onboarding"`, `"signup"`). The vendor payload should carry this.

**Response 201**
```json
{
  "linkId": "rl_8f5a0b0e6f5a4f0f8c1c",
  "token": "dl_4ae9d0e1f1b34c66b6b0a1",
  "url": "https://cartncaps.link/dl_4ae9d0e1f1b34c66b6b0a1?referral_code=XY7G4D",
  "expiresAt": "2026-03-10T00:00:00Z",
  "shareTemplates": {
    "sms": "Hi! Join me ... https://cartncaps.link/dl_...?referral_code=XY7G4D ",
    "emailSubject": "You're invited to try the Carton Caps app!",
    "emailBody": "Hey!\n\nJoin me ... Download here: https://cartncaps.link/dl_...?referral_code=XY7G4D"
  }
}
```

**Errors**
- `400 invalid_request` – malformed payload / invalid enum.
- `401 unauthorized`
- `403 referral_code_mismatch` – user tried to use someone else’s referral code.
- `409 idempotency_conflict` – same idempotency key used with different payload.
- `429 rate_limited` – too many links created recently.
- `503 vendor_unavailable` – deep link vendor down.

---

## 2) List “My referrals” for the referrer

Used by the Invite Friends screen to show the list and status pills.

### `GET /v1/referrals`

**Auth:** required

**Query params**
- `status` (optional): `pending | installed | complete | rewarded`
- `limit` (optional, default 20, max 100)
- `cursor` (optional): opaque pagination cursor

**Response 200**
```json
{
  "items": [
    {
      "referralId": "ref_01J5X3D3S6P8F6",
      "createdAt": "2026-02-01T10:00:00Z",
      "status": "complete",
      "displayName": "Jenny S.",
      "lastEventAt": "2026-02-02T11:00:00Z",
      "channel": "sms"
    }
  ],
  "nextCursor": null
}
```

**Errors**
- `401 unauthorized`

---

## 3) Referral summary for Invite Friends

### `GET /v1/referrals/summary`

**Auth:** required

**Response 200**
```json
{
  "total": 3,
  "pending": 0,
  "installed": 0,
  "complete": 3,
  "rewarded": 0
}
```

---

## 4) Resolve a deferred deep link at first launch

On first app launch (or when the vendor SDK provides attribution), the app calls this endpoint to decide whether to show the *referral variant* of the auth gate and to pre-populate the referral code field.

### `POST /v1/referrals/resolve`

**Auth:** not required

**Request**
```json
{
  "token": "dl_4ae9d0e1f1b34c66b6b0a1",
  "device": {
    "deviceId": "ios:1B3C... (hashed)",
    "platform": "ios",
    "appVersion": "7.12.0"
  }
}
```

**Response 200 (referred)**
```json
{
  "isReferred": true,
  "onboardingVariant": "referred",
  "referralCode": "XY7G4D",
  "referrer": {
    "displayName": "Stephanie",
    "schoolName": "Carton Caps"
  },
  "destination": "signup"
}
```

**Response 200 (not referred)**
```json
{
  "isReferred": false,
  "onboardingVariant": "default"
}
```

**Errors**
- `400 invalid_request`
- `404 token_not_found`
- `410 token_expired`
- `429 rate_limited` – mitigate brute force guessing

---

## 5) Vendor webhook for install/open events

This is called by the deep link vendor to update referral status for the referrer UI.

### `POST /v1/webhooks/deeplink/events`

**Auth:** vendor signature header required (e.g., `X-Vendor-Signature`)

**Request**
```json
{
  "eventType": "install",
  "token": "dl_4ae9d0e1f1b34c66b6b0a1",
  "occurredAt": "2026-02-02T12:00:00Z",
  "deviceIdHash": "sha256:...",
  "metadata": { "platform": "ios" }
}
```

**Response 202**
```json
{ "accepted": true }
```

**Errors**
- `400 invalid_request`
- `401 invalid_signature`
- `404 token_not_found`

---

## Abuse mitigation recommendations

- **Rate limit** `POST /referrals/links` per user (e.g., 5/min, 50/day).
- **Idempotency-Key** for link creation.
- **Opaque token** in the shared URL.
- **Device-level rules** at redemption time (existing registration service):
  - prevent self-referral
  - block multiple rewards from the same device / payment instrument
  - fraud scoring (IP, device, velocity, jailbroken/rooted signals)
- **Webhook signature validation** for vendor events.
