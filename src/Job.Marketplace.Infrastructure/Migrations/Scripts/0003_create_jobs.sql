CREATE TABLE jobs (
    id                  uuid PRIMARY KEY,
    customer_id         uuid NOT NULL REFERENCES customers(id),
    start_date          date NOT NULL,
    due_date            date NOT NULL,
    budget              numeric(18,2) NOT NULL,
    description         varchar(500) NOT NULL,
    accepted_offer_id   uuid,
    created_at          timestamptz NOT NULL DEFAULT now()
);
