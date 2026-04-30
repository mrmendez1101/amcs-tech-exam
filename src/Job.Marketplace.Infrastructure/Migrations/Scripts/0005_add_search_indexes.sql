-- Customers search
CREATE INDEX customers_last_name_idx ON customers (last_name);
CREATE INDEX customers_last_first_idx ON customers (last_name, first_name);

CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE INDEX customers_lastname_trgm ON customers USING gin (lower(last_name) gin_trgm_ops);

-- Contractors search
CREATE INDEX contractors_name_idx ON contractors (name);
CREATE INDEX contractors_name_trgm ON contractors USING gin (lower(name) gin_trgm_ops);

-- Jobs
CREATE INDEX jobs_customer_id_idx ON jobs (customer_id);
CREATE INDEX jobs_status_idx ON jobs (status);

-- Job offers
CREATE INDEX job_offers_job_id_idx ON job_offers (job_id);
CREATE INDEX job_offers_contractor_id_idx ON job_offers (contractor_id);
