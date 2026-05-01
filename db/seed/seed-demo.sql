-- db/seed/seed-demo.sql
-- Small demo seed for local development and curl examples.
-- Roughly: 500 customers, 50 contractors, 200 jobs, ~300 offers.
-- Idempotent-ish: wrap in a transaction and run TRUNCATE first if you want a clean slate.

BEGIN;

-- Customers --------------------------------------------------------------
INSERT INTO customers (id, first_name, last_name)
SELECT
    gen_random_uuid(),
    (ARRAY[
        'John','Jane','Bob','Alice','Carlos','Maria','Hiroshi','Yuki',
        'Pierre','Marie','Liam','Emma','Noah','Olivia','Mateo','Sofia',
        'Arjun','Priya','Chen','Mei'
    ])[1 + (i % 20)] AS first_name,
    (ARRAY[
        'Smith','Jones','Garcia','Tanaka','Dubois','Schmidt','Patel','Singh',
        'Brown','Wilson','Anderson','Nguyen','Lopez','Kim','Johansson','Rossi',
        'Murphy','Cohen','Reyes','Khan','Andersen','Walker','Hall','Young','Lee'
    ])[1 + ((i / 20) % 25)] AS last_name
FROM generate_series(1, 500) AS i;

-- Contractors ------------------------------------------------------------
INSERT INTO contractors (id, name, rating)
SELECT
    gen_random_uuid(),
    (ARRAY[
        'Acme','Bright','Stellar','Summit','Apex','Vertex','Pioneer','Nimbus',
        'Cobalt','Granite','Ironhand','Mosaic'
    ])[1 + (i % 12)]
        || ' ' ||
    (ARRAY[
        'Builders','Plumbing','Electric','Roofing','Painting','Landscaping',
        'Carpentry','Renovations','Services','Contracting','Works','Trades'
    ])[1 + ((i / 12) % 12)],
    -- Ratings between 3.0 and 5.0, rounded to one decimal.
    ROUND((3.0 + (random() * 2.0))::numeric, 1)
FROM generate_series(1, 50) AS i;

-- Jobs -------------------------------------------------------------------
-- Each job is created by a randomly chosen customer.
WITH chosen_customers AS (
    SELECT id FROM customers ORDER BY random() LIMIT 200
),
numbered AS (
    SELECT id, ROW_NUMBER() OVER () AS rn FROM chosen_customers
)
INSERT INTO jobs (id, customer_id, start_date, due_date, budget, description)
SELECT
    gen_random_uuid(),
    n.id,
    CURRENT_DATE + ((n.rn % 14))::int,                 -- starts in the next 2 weeks
    CURRENT_DATE + ((n.rn % 14) + 7 + (n.rn % 21))::int, -- due 1-4 weeks after start
    (500 + (random() * 4500))::numeric(10, 2),         -- budget 500-5000
    (ARRAY[
        'Repaint the back fence',
        'Install three ceiling fans',
        'Replace kitchen tap and check leak',
        'Mow lawn and trim hedges',
        'Patch and repaint hallway wall',
        'Service air conditioning unit',
        'Build raised garden bed',
        'Tile small bathroom floor',
        'Rewire garage lighting',
        'Pressure wash driveway'
    ])[1 + (n.rn % 10)]
FROM numbered n;

-- Job offers -------------------------------------------------------------
-- ~1-2 offers per job, from random contractors, priced near (but not exactly) the budget.
WITH job_pool AS (
    SELECT id, budget FROM jobs
),
contractor_pool AS (
    SELECT id FROM contractors
),
pairs AS (
    SELECT
        j.id      AS job_id,
        j.budget  AS budget,
        c.id      AS contractor_id,
        random()  AS r
    FROM job_pool j
    CROSS JOIN contractor_pool c
)
INSERT INTO job_offers (id, job_id, contractor_id, price, created_at)
SELECT
    gen_random_uuid(),
    p.job_id,
    p.contractor_id,
    (p.budget * (0.85 + (p.r * 0.30)))::numeric(10, 2), -- 85%-115% of budget
    now() - (random() * interval '7 days')
FROM (
    SELECT job_id, contractor_id, budget, r,
           ROW_NUMBER() OVER (PARTITION BY job_id ORDER BY r) AS rn
    FROM pairs
) p
WHERE p.rn <= 1 + (CASE WHEN random() < 0.4 THEN 1 ELSE 0 END);

COMMIT;

-- Verification (run separately to inspect)
-- SELECT COUNT(*) AS customers FROM customers;
-- SELECT COUNT(*) AS contractors FROM contractors;
-- SELECT COUNT(*) AS jobs FROM jobs;
-- SELECT COUNT(*) AS offers FROM job_offers;
