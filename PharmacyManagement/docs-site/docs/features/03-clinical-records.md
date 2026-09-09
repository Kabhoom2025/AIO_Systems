---
sidebar_position: 3
---

# Clinical Records

Doctors, patients, customers, and prescriptions — the non-financial "who" side of a pharmacy's operations.

## What it does

![Doctors](/img/screenshots/03-doctors.png)
*Doctors directory.*

![Patients](/img/screenshots/03-patients.png)
*Patient records.*

- **Doctors** — a directory of prescribing doctors (name, specialization, registration number, contact info) used when recording prescriptions.
- **Patients** — patient records (name, date of birth, contact info, address) distinct from Customers — a patient is who a prescription is written for, while a customer is who pays at the till; the same person is often both, but the system doesn't force that link.
- **Customers** — a simpler contact record used at POS for loyalty/history purposes (optional at checkout — walk-in sales don't require one).
- **Prescriptions** — links a doctor, a patient, and one or more medicine line items, used as a reference record; POS sales aren't required to reference a prescription (schedule-H drugs are a policy/paperwork concern outside this system's current scope).

## Key files

- Backend: `Pharmacy.Domain/Entities/{Doctor,Patient,Customer,Prescription,PrescriptionItem}.cs`, controllers under `Pharmacy.API/Controllers/`.
- Frontend: `client/src/app/features/{doctors,patients,customers,prescriptions}/`.

## Notes

Date-of-birth and other date fields on these entities are a recurring source of a specific bug class in this codebase: PostgreSQL's `timestamp with time zone` column type rejects .NET `DateTime` values with `DateTimeKind.Unspecified` (the default when a date comes from an HTML `<input type="date">`). Every service that accepts a date on these entities explicitly calls `DateTime.SpecifyKind(value, DateTimeKind.Utc)` before persisting.
