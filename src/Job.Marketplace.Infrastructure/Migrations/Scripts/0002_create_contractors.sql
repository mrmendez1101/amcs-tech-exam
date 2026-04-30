CREATE TABLE contractors (
    id          uuid PRIMARY KEY,
    name        varchar(200) NOT NULL,
    rating      numeric(3,2) NOT NULL DEFAULT 0.00,
    created_at  timestamptz NOT NULL DEFAULT now()
);
