CREATE TABLE job_offers (
    id              uuid PRIMARY KEY,
    job_id          uuid NOT NULL REFERENCES jobs(id),
    contractor_id   uuid NOT NULL REFERENCES contractors(id),
    price           numeric(18,2) NOT NULL,
    created_at      timestamptz NOT NULL DEFAULT now()
);
