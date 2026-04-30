CREATE TABLE customers (
    id          uuid PRIMARY KEY,
    first_name  varchar(100) NOT NULL,
    last_name   varchar(100) NOT NULL,
    created_at  timestamptz NOT NULL DEFAULT now()
);
